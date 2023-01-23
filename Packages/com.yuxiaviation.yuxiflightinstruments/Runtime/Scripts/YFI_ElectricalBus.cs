using UnityEngine;
using UdonSharp;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;
using SaccFlightAndVehicles;

namespace YuxiFlightInstruments.ElectricalBus
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
    public class YFI_ElectricalBus : UdonSharpBehaviour
    {
        /*2023-01-23
         * 模拟的部分、电瓶、地面电源、APU发电机与发动机发电机
         * 需要显示的参数先给整出来
         * 然后之后的仪表从这里读数据，生成transform
         */

        public SaccEntity entityControl;
        public SaccAirVehicle SAVControl;
        [Tooltip("Debug Output Text")]
        public Text DebugOutput;

        [System.NonSerialized] public bool masterSwitch = true;
        //TODO:电瓶电量模拟，但是不想再加Update力
        [System.NonSerialized] [UdonSynced] public bool batteryOn = false;
        [System.NonSerialized] [UdonSynced] public bool APUGeneratorOn = true;
        [System.NonSerialized] [UdonSynced] public bool engineGeneratorOn = true;
        [System.NonSerialized] [UdonSynced] public bool externalPowerOn = false;//EXT
        [System.NonSerialized] [UdonSynced] public bool EmergencyGeneratorOn = true;//RAT

        //是否具备条件
        [System.NonSerialized] [UdonSynced] public bool BatteryAviliable = true;
        [System.NonSerialized] [UdonSynced] public bool APUGeneratorAviliable = false;
        [System.NonSerialized] [UdonSynced] public int engineGeneratorAviliable = 0;
        [System.NonSerialized] [UdonSynced] public bool externalPowerAviliable = false;
        [System.NonSerialized] [UdonSynced] public bool EmergencyGeneratorAviliable = false;

        [System.NonSerialized] public bool hasPower = false;

        [Tooltip("Will be enable when masterSwitch and has power")]
        public GameObject[] avionicsEquipments = { };

        public void SFEXT_G_APUStarted()
        {
            APUGeneratorAviliable = true;
            CheckIfHasPower();
        }
        public void SFEXT_G_APUShutDown()
        { 
            APUGeneratorAviliable = false;
            CheckIfHasPower();
        }
        public void SFEXT_G_EngineStarted()
        {
            CheckIfHasPower();
            engineGeneratorAviliable += 1;
        }
        public void SFEXT_G_EngineShutDown()
        {
            CheckIfHasPower();
            engineGeneratorAviliable -= 1;
        }

        public void ToggleMasterSwitchLocal()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleMasterSwitch));    
        }

        public void ToggleBatteryLocal()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleBattery));
        }

        public void ToggleMasterSwitch()
        {
            masterSwitch = !masterSwitch;
            CheckIfHasPower();
        }

        public void ToggleBattery()
        {
            batteryOn = !batteryOn;
            CheckIfHasPower();
        }


        public override void OnDeserialization()
        {
            CheckIfHasPower();
        }

        public void CheckIfHasPower()
        {
            if (masterSwitch)
            {
                if (batteryOn && BatteryAviliable)
                    hasPower = true;
                else if (APUGeneratorOn && APUGeneratorAviliable)
                    hasPower = true;
                else if (engineGeneratorOn && engineGeneratorAviliable>0)
                    hasPower = true;
                else if (externalPowerOn && externalPowerAviliable)
                    hasPower = true;
                else if (EmergencyGeneratorOn && EmergencyGeneratorAviliable)
                    hasPower = true;
                else hasPower = false;
            }
            else hasPower = false;

            foreach (var item in avionicsEquipments)
            {
                item.SetActive(hasPower);
            }

        }
    }
}
