
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using TMPro;

namespace YuxiFlightInstruments
{
    //TODO:我也不知道这么更新对不对
    //原则上可以不用更新，但是希望实现一个其他人也能看到ap上文字的效果
    [DefaultExecutionOrder(1000)]
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class YFI_AutoPilot : UdonSharpBehaviour
    {

        public YFI_SPDControl SPDControl;
        public UdonSharpBehaviour HDGControl;
        public UdonSharpBehaviour ALTControl;

        public TextMeshPro SPDText;
        public TextMeshPro HDGText;
        public TextMeshPro ALTText;
        public TextMeshPro VSText;

        public GameObject SPDEnableIndicator;
        public GameObject HDGEnableIndicator;
        public GameObject ALTEnableIndicator;

        public float SPDCommand = 200f;
        public float HDGCommand = 10f;
        public float ALTCommand = 2000f;
        public float VSCommand = 1500f;

        public float minALT = 500f;
        public float maxALT = 50000f;
        public float ALTStep = 500f;

        public float minSPD = 30f;
        public float maxSPD = 400f;
        public float SPDStep = 25f;

        private bool isAPEnable = false; //处于某些原因先放一个总开关在这
        private bool isSPDEnable = false;
        private bool isHDGEnable = false;
        private bool isALTEnable = false;
        //之后会加入的显示格式化功能
        //public bool overrideFrequencyFormat = false;
        //public string frequencyFormat = "{0:#00.00#}";
        void Start()
        {
            UpdateIndicator();
            UpdateDisplay();
            if (SPDControl)
            {
                SPDControl.SetSpeed = SPDCommand;
                SPDControl.gameObject.SetActive(isALTEnable);
            }
        }

        private void UpdateIndicator()
        {
            if (SPDEnableIndicator != null)
                SPDEnableIndicator.SetActive(isSPDEnable);
            if (HDGEnableIndicator != null)
                HDGEnableIndicator.SetActive(isHDGEnable);
            if (ALTEnableIndicator != null)
                ALTEnableIndicator.SetActive(isALTEnable);
        }

        private void UpdateDisplay()
        {
            //if (frequencyText && !overrideFrequencyFormat) frequencyFormat = frequencyText.text;
            if (SPDText)
                SPDText.text = string.Format("{0:###}", SPDCommand);
            if (ALTText)
                ALTText.text = string.Format("{0:#####}", ALTCommand);
            if (VSText)
                VSText.text = string.Format("{0:####}", VSCommand);
            if (HDGText)
                HDGText.text = string.Format("{0:###}", HDGCommand);

        }

        public void _TakeOwnership()
        {
            if (Networking.IsOwner(gameObject)) return;
            Networking.SetOwner(Networking.LocalPlayer, gameObject);
        }

        public void _ToggleALT()
        {
            _TakeOwnership();
            if(ALTControl)
                ALTControl.gameObject.SetActive(isALTEnable);
            isALTEnable = !isALTEnable;
            UpdateIndicator();
            RequestSerialization();
        }
        public void _SetALT(float altitude)
        {
            _TakeOwnership();
            altitude = altitude > maxALT ? minALT : (altitude < minALT ? maxALT : altitude);
            RequestSerialization();
        }
        public void _ALTIncrease() => _SetALT(ALTCommand + ALTStep);
        public void _ALTDecrease() => _SetALT(ALTCommand - ALTStep);

        public void _ToggleSPD()
        {
            _TakeOwnership();
            isSPDEnable = !isSPDEnable;
            if (SPDControl)
            {
                SPDControl.gameObject.SetActive(isALTEnable);
                SPDControl.SetSpeed = SPDCommand / 1.9438445f;
                if (isSPDEnable)
                    SPDControl.SetCruiseOn();
                else
                    SPDControl.SetCruiseOff();
            }
            UpdateIndicator();
            RequestSerialization();
        }
        public void _SetSPD(float speed)
        {
            _TakeOwnership();
            SPDCommand = speed > maxSPD ? minSPD : (speed < minSPD ? maxSPD : speed);
            UpdateDisplay();
            //SPDControl.SetProgramVariable("SetSpeed", SPDCommand);
            SPDControl.SetSpeed = SPDCommand / 1.9438445f;
            RequestSerialization();
        }
        public void _SPDIncrease() => _SetSPD(SPDCommand + SPDStep);
        public void _SPDDecrease() => _SetSPD(SPDCommand - SPDStep);

        //一堆事件处理函数

    }
}
