
using UdonSharp;
using UnityEngine;
using YuxiFlightInstruments.BasicFlightData;

namespace YuxiFlightInstruments.AnimationDriver
{
    //TODO:不用每一帧都更新，设置一个可以指定更新频率的参数
    public class YFI_AnimatorController : UdonSharpBehaviour
    {
        [Tooltip("Flight Data Interface")]
        public YFI_FlightDataInterface FlightData;
        [Tooltip("仪表的动画控制器")]
        public Animator IndicatorAnimator;
        [Header("下面的量程都是单侧的")]
        [Tooltip("速度表最大量程(节)")]
        public float MAXSPEED = 500f;
        [Tooltip("俯仰角最大量程(度)")]
        public float MAXPITCH = 20f;
        [Tooltip("滚转角最大量程(度)")]
        public float MAXBANK = 45f;
        [Tooltip("高度表最大量程(英尺)")]
        public float MAXALT = 10000f;
        [Header("对于数字每一位都需要单独动画的仪表")]
        public bool altbybit = false;
        //[Tooltip("高度表万位最大示数")]
        //public int MAXALT10000 = 10;
        [Tooltip("一些罗盘动画起始角度并不为0")]
        public int HDGoffset = 0;
        [Tooltip("爬升率最大量程(英尺/分钟)")]
        public int MAXVS = 16000;
        //侧滑这个数值先固定着
        [Tooltip("侧滑角量程")]
        public int MAXSLIPANGLE = 2;
        //暂时不考虑航向表有量程
        [Tooltip("如果用的是球形陀螺仪，在此放置球的transform(弃用了)")]
        public Transform GyroBall;
        [Tooltip("0苏式；1西方")]
        public int GyroBallmodel = 0;

        private float altitude = 0f;
        //animator strings that are sent every frame are converted to int for optimization
        private int AIRSPEED_HASH = Animator.StringToHash("AirSpeedNormalize");
        private int PITCH_HASH = Animator.StringToHash("PitchAngleNormalize");
        private int BANK_HASH = Animator.StringToHash("BankAngleNormalize");
        private int ALT_HASH = Animator.StringToHash("AltitudeNormalize");
        private int ALT10_HASH = Animator.StringToHash("Altitude10Normalize");
        private int ALT100_HASH = Animator.StringToHash("Altitude100Normalize");
        private int ALT1000_HASH = Animator.StringToHash("Altitude1000Normalize");
        private int ALT10000_HASH = Animator.StringToHash("Altitude10000Normalize");
        private int ROC_HASH = Animator.StringToHash("VerticalSpeedNormalize");
        private int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
        private int SLIPANGLE_HASH = Animator.StringToHash("SlipAngleNormalize");
        //set default ball rotation here
        private Vector3 GyroBallRotationDefault;
        private float[] GyroBallFacotr = { -2f, 0f };
   
        private float PitchAngle = 0f;
        private float BankAngle = 0f;
        private float HeadingAngle = 0f;

        private void Start()
        {
            //初始化陀螺仪旋转姿态
            if (GyroBall != null)
            {
             GyroBallRotationDefault = GyroBall.eulerAngles;
            }

        }
        private void LateUpdate()
        {
                UpdateAnimation();
        }

        private void UpdateAnimation()
        {
                //这里可以用来做仪表更新延迟之类的逻辑
                PitchAngle = FlightData.pitch;
                BankAngle = FlightData.bank;
                HeadingAngle = FlightData.magneticHeading;
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
            //TODO:如何不显示航向？
            GyroBall.eulerAngles = GyroBallRotationDefault + new Vector3(GyroBallFacotr[GyroBallmodel] * PitchAngle, HeadingAngle, 0);
        }
        private void UpdateAirspeed()
        {
            IndicatorAnimator.SetFloat(AIRSPEED_HASH, FlightData.TAS / MAXSPEED);
        }
        private void UpdateAltitude()
        {

            //默认都会写Altitude
            altitude = FlightData.altitude;
            IndicatorAnimator.SetFloat(ALT_HASH, (altitude / MAXALT));
            if (altbybit)
            {
                IndicatorAnimator.SetFloat(ALT10_HASH, (altitude % 100) / 100f);
                IndicatorAnimator.SetFloat(ALT100_HASH, ((int)(altitude/100f) % 10) / 10f);
                IndicatorAnimator.SetFloat(ALT1000_HASH, ((int)(altitude/1000f) % 10) / 10f);
                IndicatorAnimator.SetFloat(ALT10000_HASH, ((int)(altitude/10000f) % 10) / 10f);
            }
        
        }
        private void UpdateVerticalSpeed()
        {
                float VerticalSpeedNormal = Remap01(FlightData.verticalSpeed, -MAXVS, MAXVS);
                IndicatorAnimator.SetFloat(ROC_HASH, VerticalSpeedNormal);
        }
        private void UpdateHeading()
        {
            IndicatorAnimator.SetFloat(HEADING_HASH, (HeadingAngle- HDGoffset) / 360f);
        }
        private void UpdatePitch()
        {
            //玄学问题，Pitch 跟 Bank 调用不了Remap01??
            float PitchAngleNormal = Mathf.Clamp01((PitchAngle + MAXPITCH) / (MAXPITCH + MAXPITCH));
            PitchAngleNormal = PitchAngleNormal == 1 ? 0.99999f : PitchAngleNormal;
            IndicatorAnimator.SetFloat(PITCH_HASH, PitchAngleNormal);
        }
        private void UpdateBank()
        {
            float BankAngleNormal = Mathf.Clamp01((BankAngle + MAXBANK) / (MAXBANK + MAXBANK));
            BankAngleNormal = BankAngleNormal == 1 ? 0.99999f : BankAngleNormal;
            IndicatorAnimator.SetFloat(BANK_HASH, BankAngleNormal);
        }
        private void UpdateSlip()
        {
            IndicatorAnimator.SetFloat(SLIPANGLE_HASH, Mathf.Clamp01((FlightData.SlipAngle + MAXSLIPANGLE) / (MAXSLIPANGLE + MAXSLIPANGLE)));
        }
        private float Remap01(float value, float valueMin, float valueMax)
            {
                value = Mathf.Clamp01((value - valueMin) / (valueMax - valueMin));
                return value;
            }
        }
}
