
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
//TODO:不用每一帧都更新，设置一个可以指定更新频率的参数
public class YFI_AnimatorController : UdonSharpBehaviour
{
    [Tooltip("Flight Data Interface")]
    public UdonSharpBehaviour YFI_FlightDataInterface;
    [Tooltip("仪表的动画控制器")]
    public Animator IndicatorAnimator;

    [Tooltip("速度表最大量程(节)")]
    public int MAXSPEED = 500;
    [Tooltip("俯仰角最大量程(度)")]
    public int MAXPITCH = 20;
    [Tooltip("滚转角最大量程(度)")]
    public int MAXBANK = 45;
    [Tooltip("高度表最大量程(英尺)")]
    public int MAXALT = 10000;
    [Tooltip("高度表万位最大量程(英尺)")]
    public int MAXALT10000 = 100000;
    [Tooltip("爬升率最大量程(英尺/分钟)")]
    public int MAXVS = 16000;
    //侧滑这个数值先固定着
    [Tooltip("侧向G力最大数值")]
    public int MAXSIDEG = 2;
    //暂时不考虑航向表有量程
    [Tooltip("如果用的是球形陀螺仪，在此放置球的transform")]
    public Transform GyroBall;
    [Tooltip("0苏式；1西方")]
    public int GyroBallmodel = 0;

    //一些需要提前算好的参数
    private float pitchFixValue = 3.5f;
    private float bankFixValue = 0.5f;
    private float MAXPITCH_double = 40f;
    private float MAXVS_double = 32000f;
    private float MAXBANK_double = 90f;
    private float MAXSIDEG_double = 4f;
    //animator strings that are sent every frame are converted to int for optimization
    private int AIRSPEED_HASH = Animator.StringToHash("AirSpeedNormalize");
    private int PITCH_HASH = Animator.StringToHash("PitchAngelNormalize");
    private int BANK_HASH = Animator.StringToHash("BankAngelNormalize");
    private int ALT_HASH = Animator.StringToHash("AltitudeNormalize");
    private int ALT10000_HASH = Animator.StringToHash("Altitude10000Normalize");
    private int ROC_HASH = Animator.StringToHash("VerticalSpeedNormalize");
    private int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
    private int SIDEG_HASH = Animator.StringToHash("SideGNormalize");
    //set default ball rotation here
    private Vector3 GyroBallRotationDefault;
    private float[] GyroBallFacotr = { -2f, 0f };
   
    private float PitchAngle = 0f;
    private float BankAngle = 0f;
    private float HeadingAngle = 0f;

    private float PitchAngle90 = 0f;
    private float BankAngle90 = 0f;

    private void Start()
    {
        MAXPITCH_double = 2f * MAXPITCH;
        MAXBANK_double = 2f * MAXBANK;
        MAXVS_double = 2f * MAXVS;

        pitchFixValue = (90f - MAXPITCH)/ MAXPITCH_double;
        bankFixValue = (90f - MAXBANK) / MAXBANK_double;

        MAXSIDEG_double = 2f * MAXSIDEG;

        //初始化陀螺仪旋转姿态
        if (GyroBall != null)
        {
         GyroBallRotationDefault = GyroBall.eulerAngles;
         //  Debug.Log(string.Concat((GyroBallRotationDefault.x)));
         //  Debug.Log(string.Concat((GyroBallRotationDefault.y)));
         //  Debug.Log(string.Concat((GyroBallRotationDefault.z)));
        }

    }
    private void LateUpdate()
    {
        //这里可以用来做仪表更新延迟之类的逻辑
        PitchAngle = (float)YFI_FlightDataInterface.GetProgramVariable("Pitch");
        BankAngle = (float)YFI_FlightDataInterface.GetProgramVariable("Bank");
        HeadingAngle = (float)YFI_FlightDataInterface.GetProgramVariable("Heading");
        //AirSpeed
        UpdateAirspeed();
        //Altitude
        UpdateAltitude();
        //VS
        UpdateVerticalSpeed();
        //Heading
        UpdateHeading();
        //Bank
        UpdateBank();
        //Slip
        UpdateSlip();
        //goryball
        //Pitch
        if (GyroBall != null)
        {

            UpdateGyroBall();
        }
        else
        {
            UpdatePitch();
        }


    }
    private void UpdateGyroBall()
    {
        GyroBall.eulerAngles = GyroBallRotationDefault + new Vector3(GyroBallFacotr[GyroBallmodel] * PitchAngle, HeadingAngle, 0);
    }
    private void UpdateAirspeed()
    {
        IndicatorAnimator.SetFloat(AIRSPEED_HASH, (float)YFI_FlightDataInterface.GetProgramVariable("TAS") / MAXSPEED);
    }
    private void UpdateAltitude()
    {
        IndicatorAnimator.SetFloat(ALT_HASH, (float)YFI_FlightDataInterface.GetProgramVariable("Altitude") / MAXALT);
        IndicatorAnimator.SetFloat(ALT10000_HASH, (float)YFI_FlightDataInterface.GetProgramVariable("Altitude") / MAXALT10000);
    }
    private void UpdateVerticalSpeed()
    {
        float VerticalSpeedNormal = (float)YFI_FlightDataInterface.GetProgramVariable("RateOfClimb") / MAXVS_double + 0.5f;
        if (0.0f < VerticalSpeedNormal && VerticalSpeedNormal < 1.0f)
        {
            IndicatorAnimator.SetFloat(ROC_HASH, VerticalSpeedNormal);
        }
    }
    private void UpdateHeading()
    {
        IndicatorAnimator.SetFloat(HEADING_HASH, HeadingAngle / 360f);
    }
    private void UpdatePitch()
    {
        PitchAngle90 = (PitchAngle < 270f) ?
            (PitchAngle + 90f) :
            (PitchAngle - 270f);
        float PitchNormal = PitchAngle90 / MAXPITCH_double - pitchFixValue;
        if (0.0f < PitchNormal && PitchNormal < 1.0f)
        {
            IndicatorAnimator.SetFloat(PITCH_HASH, PitchNormal);
        }
    }
    private void UpdateBank()
    {
        BankAngle90 = (BankAngle < 270f) ?
            (BankAngle + 90f) :
            (BankAngle - 270f);
        float BankNormal = BankAngle90 / MAXBANK_double - bankFixValue;
        if (0.0f < BankNormal && BankNormal < 1.0f)
        {
            IndicatorAnimator.SetFloat(BANK_HASH, BankNormal);
        }
    }
    private void UpdateSlip()
    {
        float SideG = (float)YFI_FlightDataInterface.GetProgramVariable("SideG");
        float SideGNormal = SideG / MAXSIDEG_double + 0.5f;
        if (0.0f < SideGNormal && SideGNormal < 1.0f)
        {
            IndicatorAnimator.SetFloat(SIDEG_HASH, SideGNormal);
        }
    }
}
