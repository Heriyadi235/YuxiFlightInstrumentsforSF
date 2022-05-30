using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class YFI_FlightDataInterface : UdonSharpBehaviour
{
    /*2022-05-28
     * 我寻思先写一个接口从AVController或者eneity里面把仪表
     * 需要显示的参数先给整出来
     * 然后之后的仪表从这里读数据，生成transform
     */
    [Tooltip("这里是SAVControl")]
    public UdonSharpBehaviour SAVControl;
    //[Tooltip("用动画器做仪表变形的控制")]
    [Tooltip("Debug Output Text")]
    public Text DebugOut;
    
    private SaccEntity EntityControl;
    private Transform VehicleTransform;
    VRCPlayerApi localPlayer;

    private Transform CenterOfMass;
    //下面定义了关键的飞行参数们
    private Vector3 VelocityVector;
    private float GValue;
    private float AngleOfAttack;
    private float GS;
    private float Mach;
    private float TAS;
    private float IAS;
    private float RateOfClimb;
    private float Altitude;
    private float Heading;
    private float Pitch;
    private float Bank;
    private float SeaLevel; 


    private float DeltaTime;
    private Vector3 DeltaVelocityVector;
    private Vector3 VelocityVectorBefore;
    private Vector3 PerdCurrentVelocityVector;
    private Vector3 LerpCurrentVelocityVector;
    private float TimeBefore;

    //方法
    private void Start()
    {

        EntityControl = (SaccEntity)SAVControl.GetProgramVariable("EntityControl");
        SeaLevel = (float)SAVControl.GetProgramVariable("SeaLevel");
        localPlayer = Networking.LocalPlayer;
        CenterOfMass = EntityControl.CenterOfMass;
    }

    private void OnEnable()
    {
        return;
    }
    private void Update()
    {
        //TODO:如果性能出现问题，限制此处的更新频率
        //我觉得时间平滑不用在这里实现，但是肯定要考虑不是每一个tick都调用以减少开销
        //float SmoothDeltaTime = Time.smoothDeltaTime;

        //获取速度向量
        //SAV中CurrentVel是飞行器速度向量，本质上小飞机所在位置就是1s(?)后的飞机位置
        Vector3 CurrentVelocityVector = (Vector3)SAVControl.GetProgramVariable("CurrentVel");

        //(在绘制AHI时再做投影)
        //低速时，让箭头垂直向下
        if (CurrentVelocityVector.magnitude < 2)
        {
            CurrentVelocityVector = Vector3.down * 2;//straight down instead of spazzing out when moving very slow
        }
        else 
        {
            //插值操作，做平滑
            if (CurrentVelocityVector != VelocityVectorBefore)
            {
                float CurrentTime = Time.time;
                DeltaTime = CurrentTime - TimeBefore;
                DeltaVelocityVector = (CurrentVelocityVector - VelocityVectorBefore) / DeltaTime;

                VelocityVectorBefore = CurrentVelocityVector;
                TimeBefore = CurrentTime;
            }
            //VelocityVector = CurrentVelocityVector + DeltaVelocityVector * (Time.time- TimeBefore);
            PerdCurrentVelocityVector = CurrentVelocityVector + DeltaVelocityVector * (DeltaTime);
        }

        //TODO:这玩意是否应该在仪表脚本里(如果有)完成
        //TODO:是否需要正则化？
        //可能需要(如果要投影在一个机械的陀螺仪（球)上)
        if ((bool)SAVControl.GetProgramVariable("IsOwner"))
        {
            VelocityVector = PerdCurrentVelocityVector;
        }
        else
        {
            LerpCurrentVelocityVector = Vector3.Lerp(LerpCurrentVelocityVector, PerdCurrentVelocityVector, 9f * Time.smoothDeltaTime);
            VelocityVector = LerpCurrentVelocityVector;
        }

        //////////

        Vector3 VehicleEuler = EntityControl.transform.rotation.eulerAngles;
        //航向
        //Heading = (Quaternion.Euler(new Vector3(0, -VehicleEuler.y, 0)));
        //float angle = Quaternion.Angle(transform.rotation, target.rotation);
        Heading = VehicleEuler.y;
        //俯仰
        //Pitch = Quaternion.Euler(new Vector3(0, VehicleEuler.y, 0));
        Pitch = VehicleEuler.x;
        //坡度
        //Bank = Quaternion.Euler(new Vector3(0, 0, -VehicleEuler.z));
        Bank = VehicleEuler.z;
        //过载
        GValue = ((float)SAVControl.GetProgramVariable("VertGs"));
        //攻角
        AngleOfAttack = ((float)SAVControl.GetProgramVariable("AngleOfAttack"));
        //马赫数
        Mach = ((float)SAVControl.GetProgramVariable("Speed")) / 343f;
        //爬升率
        RateOfClimb = ((Vector3)SAVControl.GetProgramVariable("CurrentVel")).y * 60 * 3.28084f;
        //高度
        Altitude = (CenterOfMass.position.y - SeaLevel) * 3.28084f;

        //SAV里的speed是地速 airspeed是TAS
        //SAV里使用的是IS，所以这里要把秒换算为节
        //地速
        GS = ((float)SAVControl.GetProgramVariable("Speed")) *1.9438445f;
        //真空速
        TAS = ((float)SAVControl.GetProgramVariable("AirSpeed")) *1.9438445f;

        if (DebugOut)
        {
            //HUDText_G.text = string.Concat(((float)SAVControl.GetProgramVariable("VertGs")).ToString("F1"), "\n", maxGs.ToString("F1"));
            DebugOut.text = string.Concat("Headin: ", Heading.ToString(), 
            "\nPitch: ", Pitch.ToString(),
            "\nBank: ", Bank.ToString(),
            "\nG: ", GValue.ToString(),
            "\nAOA: ", AngleOfAttack.ToString(),
            "\nMach: ", Mach.ToString(),
            "\nROC: ", RateOfClimb.ToString(),
            "\nAltitude: ", Altitude.ToString(),
            "\nGS: ", GS.ToString(),
            "\nTAS: ", TAS.ToString());
        }
    }
}
