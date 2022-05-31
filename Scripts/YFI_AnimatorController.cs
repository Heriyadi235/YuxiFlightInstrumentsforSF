
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
    [Tooltip("爬升率最大量程(英尺/分钟)")]
    public int MAXVS = 16000;
    //暂时不考虑航向表有量程

    //侧滑这个数值先固定着
    public int MAXSIDEG = 2;
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
    private int ROC_HASH = Animator.StringToHash("VerticalSpeedNormalize");
    private int HEADING_HASH = Animator.StringToHash("HeadingNormalize");
    private int SIDEG_HASH = Animator.StringToHash("SideGNormalize");

    private void Start()
    {
        MAXPITCH_double = 2f * MAXPITCH;
        MAXBANK_double = 2f * MAXBANK;
        MAXVS_double = 2f * MAXVS;

        pitchFixValue = (90f - MAXPITCH)/ MAXPITCH_double;
        bankFixValue = (90f - MAXBANK) / MAXBANK_double;

        MAXSIDEG_double = 2f * MAXSIDEG;
    }
    private void LateUpdate()
    {
        //AirSpeed
        IndicatorAnimator.SetFloat(AIRSPEED_HASH, (float)YFI_FlightDataInterface.GetProgramVariable("TAS")/MAXSPEED);
        
        //Altitude
        IndicatorAnimator.SetFloat(ALT_HASH, (float)YFI_FlightDataInterface.GetProgramVariable("Altitude") / MAXALT);

        //VS
        float VerticalSpeedNormal = (float)YFI_FlightDataInterface.GetProgramVariable("RateOfClimb") / MAXVS_double + 0.5f;
        if (0.0f < VerticalSpeedNormal && VerticalSpeedNormal < 1.0f)
        { 
            IndicatorAnimator.SetFloat(ROC_HASH, VerticalSpeedNormal); 
        }

        //Heading
        IndicatorAnimator.SetFloat(HEADING_HASH, (float)YFI_FlightDataInterface.GetProgramVariable("Heading") / 360f);
        
        //Pitch
        float PitchAngle90 = ((float)YFI_FlightDataInterface.GetProgramVariable("Pitch")  < 270f) ?
            ((float)YFI_FlightDataInterface.GetProgramVariable("Pitch") + 90f) :
            ((float)YFI_FlightDataInterface.GetProgramVariable("Pitch") - 270f);

        float PitchNormal = PitchAngle90 / MAXPITCH_double - pitchFixValue;
        if (0.0f < PitchNormal && PitchNormal < 1.0f)
        { 
            IndicatorAnimator.SetFloat(PITCH_HASH, PitchNormal); 
        }

        //Bank
        float BankAngle90 = ((float)YFI_FlightDataInterface.GetProgramVariable("Bank") < 270f) ?
            ((float)YFI_FlightDataInterface.GetProgramVariable("Bank") + 90f) :
            ((float)YFI_FlightDataInterface.GetProgramVariable("Bank") - 270f);

        float BankNormal = BankAngle90 / MAXBANK_double - bankFixValue;
        if (0.0f < BankNormal && BankNormal < 1.0f)
        {    
            IndicatorAnimator.SetFloat(BANK_HASH, BankNormal); 
        }

        //Slip
        float SideG = (float)YFI_FlightDataInterface.GetProgramVariable("SideG");
        float SideGNormal = SideG / MAXSIDEG_double + 0.5f;
        if (0.0f < SideGNormal && SideGNormal < 1.0f)
        {
            IndicatorAnimator.SetFloat(SIDEG_HASH, SideGNormal);
        }

    }

}
