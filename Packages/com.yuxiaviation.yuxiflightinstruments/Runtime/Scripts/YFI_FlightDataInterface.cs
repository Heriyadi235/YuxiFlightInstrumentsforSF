using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using SaccFlightAndVehicles;

namespace YuxiFlightInstruments.BasicFlightData
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YFI_FlightDataInterface : UdonSharpBehaviour
    {
        /*2022-05-28
         * 我寻思先写一个接口从AVController或者eneity里面把仪表
         * 需要显示的参数先给整出来
         * 然后之后的仪表从这里读数据，生成transform
         */

        /*2022-12-23
         * 大重构
         * 支持了航迹计算
         * 加入了命名空间
         * 现在作为SF_EXT
         * TODO: add owml support
         */

        public SaccEntity entityControl;
        public SaccAirVehicle SAVControl;
        public Transform OWMLMap;
        [Tooltip("Debug Output Text")]
        public Text DebugOutput;

        private Transform VehicleTransform;
        //下面定义了关键的飞行参数们
        //飞行参数
        [System.NonSerialized] public float altitude = 0; //barometric altitude in ft = center of mass position.y - sealevel 
        [System.NonSerialized] public float groundSpeed = 0; //center of mass .velocity
        [System.NonSerialized] public float TAS = 0; //center of mass .velocity - wind.velocity
        [System.NonSerialized] public float heading = 0; // 0-360 center of mass .eulerAngles.y 
        [System.NonSerialized] public float magneticHeading = 0;// 0-360 center of mass .eulerAngles.y - magnetic Declination
        [System.NonSerialized] public float pitch = 0;//-180-0-180 抬头为正 center of mass .eulerAngles.z
        [System.NonSerialized] public float bank = 0; //-180-0-180 右滚为正 center of mass .eulerAngles.x
        [System.NonSerialized] public float verticalSpeed = 0; //爬升率 in ft/min
        [System.NonSerialized] public float verticalG = 0; //垂直G
        [System.NonSerialized] public float angleOfAttack = 0; //水平攻角与垂直攻角中间的大值
        [System.NonSerialized] public float mach = 0; //水平攻角与垂直攻角中间的大值

        //航迹信息
        [System.NonSerialized] public float trackPitchAngle = 0; //航迹俯仰角 向上为正
        [System.NonSerialized] public float SlipAngle = 0; //侧滑角 向右为正


        [Tooltip("磁偏角")]
        public float magneticDeclination = 0;//西偏于真北为负 

        private Vector3 currentVelocity = Vector3.zero; //航迹矢量

        private bool PlayerInVehicle = false;

        //方法
        private void Start()
        {
            var MapObject = GameObject.Find("/MapObject");
            if(MapObject) OWMLMap = MapObject.transform;
        }
        public void SFEXT_O_PilotEnter()
        {
            PlayerInVehicle = SAVControl.Piloting || SAVControl.Passenger;
        }
        public void SFEXT_O_PilotExit()
        {
            PlayerInVehicle = SAVControl.Piloting || SAVControl.Passenger;
        }
        public void SFEXT_P_PassengerEnter()
        {
            PlayerInVehicle = SAVControl.Piloting || SAVControl.Passenger;
        }
        public void SFEXT_P_PassengerExit()
        {
            PlayerInVehicle = SAVControl.Piloting || SAVControl.Passenger;
        }
        public void SFEXT_L_EntityStart()
        {
            entityControl = GetComponentInParent<SaccEntity>();
            SAVControl = (SaccAirVehicle)entityControl.GetExtention(GetUdonTypeName<SaccAirVehicle>());
            VehicleTransform = entityControl.transform;
            PlayerInVehicle = false;
            FlightDataUpdate();
        }

        private void FlightDataUpdate()
        {
            //可以直接从SAV获取的参数
            //TODO:地速与TAS包含了垂直速度？
            //地速
            groundSpeed = SAVControl.Speed * 1.94384f;
            //TAS
            TAS = SAVControl.AirSpeed * 1.94384f;
            //垂直G力
            verticalG = SAVControl.VertGs;
            //攻角
            angleOfAttack = SAVControl.AngleOfAttack;
            //获取航迹矢量
            currentVelocity = SAVControl.CurrentVel.magnitude > 0.5f ? SAVControl.CurrentVel : Vector3.zero;

            //需要稍微算一下的参数
            //航向
            heading = SAVControl.CenterOfMass.eulerAngles.y;
            heading = (heading + 360) % 360;
            magneticHeading = (heading - magneticDeclination + 360) % 360;
            //俯仰
            pitch = -SAVControl.CenterOfMass.eulerAngles.x;
            pitch = pitch < -180 ? (360 + pitch) : pitch;
            //坡度
            bank = -SAVControl.CenterOfMass.eulerAngles.z;
            bank = bank < -180 ? (360 + bank) : bank;
            //高度 英尺
            //altitude = (SAVControl.CenterOfMass.position.y - SAVControl.SeaLevel) * 3.28084f;
            altitude = (SAVControl.CenterOfMass.position.y - SAVControl.SeaLevel - (OWMLMap != null ? OWMLMap.position.y : 0)) * 3.28084f;
            //垂直速度 英尺/分钟
            verticalSpeed = currentVelocity.y * 60 * 3.28084f;

            //马赫数 11000米之后的数值不准确
            if (altitude / 3.28084f < 11000)
                mach = SAVControl.AirSpeed/ (20.05f * Mathf.Sqrt(288f - (altitude / 3.28084f) * 0.65f / 100f));
            else
                mach = SAVControl.AirSpeed / 295f;

           //航迹参数计算
           Vector3 vecForward = VehicleTransform.forward;
            trackPitchAngle = -Vector3.SignedAngle(vecForward, Vector3.ProjectOnPlane(currentVelocity, VehicleTransform.right), VehicleTransform.right);
            SlipAngle = Vector3.SignedAngle(vecForward, Vector3.ProjectOnPlane(currentVelocity, VehicleTransform.up), VehicleTransform.up);
        }

        private void OnEnable()
        {
            return;
        }

        private void Update()
        {
            if (PlayerInVehicle)
            {
                FlightDataUpdate();
                /*
                if (DebugOutput)
                {
                    DebugOutput.text = string.Concat("Headin: ", heading.ToString(),
                    "\nPitch: ", pitch.ToString(),
                    "\nBank: ", bank.ToString(),
                    "\nTRKPitch: ", trackPitchAngle.ToString(),
                    "\nSlipAngel: ", SlipAngle.ToString(),
                    "\nMach: ", mach.ToString(),
                    "\nVS: ", verticalSpeed.ToString(),
                    "\nAltitude: ", altitude.ToString(),
                    "\nGS: ", groundSpeed.ToString(),
                    "\nTAS: ", TAS.ToString());
                }
                */
            }
        }
    }
}
