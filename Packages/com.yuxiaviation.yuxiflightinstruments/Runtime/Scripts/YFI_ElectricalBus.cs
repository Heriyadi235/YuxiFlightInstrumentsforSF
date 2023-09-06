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

        //A have no idea when to use main switch
        [System.NonSerialized] public bool masterSwitch = true;

        //switches
        [System.NonSerialized] [UdonSynced] public bool batteryOn = false;
        [System.NonSerialized] [UdonSynced] public bool APUGeneratorOn = true;//havent use
        [System.NonSerialized] [UdonSynced] public bool engineGeneratorOn = true;//havent use
        [System.NonSerialized] [UdonSynced] public bool externalPowerOn = false;//EXT //havent use
        [System.NonSerialized] [UdonSynced] public bool EmergencyGeneratorOn = true;//RAT //havent use

        //condications
        [System.NonSerialized] [UdonSynced] public bool BatteryAviliable = true;//havent use
        [System.NonSerialized] [UdonSynced] public bool APUGeneratorAviliable = false;
        [System.NonSerialized] [UdonSynced] public sbyte engineGeneratorAviliable = 0;
        [System.NonSerialized] [UdonSynced] public bool externalPowerAviliable = false;//havent use
        [System.NonSerialized] [UdonSynced] public bool EmergencyGeneratorAviliable = false;//havent use

        [System.NonSerialized] public bool hasPower = false;

        [Tooltip("Will be enable when masterSwitch and has power")]
        public GameObject[] avionicsEquipments = { };
        public GameObject[] disableOnEletriced = { };
        //我愿称之为重置三件套
        public void SFEXT_L_EntityStart() => ResetElectrical();
        public void SFEXT_G_Explode() => ResetElectrical();
        public void SFEXT_G_RespawnButton() => ResetElectrical();

        public void SFEXT_O_OnPlayerJoined() => RequestSerialization();
        public void SFEXT_L_APUStarted()//havent use
        {
            APUGeneratorAviliable = true;
            UpdatePower();
        }
        public void SFEXT_L_APUShutDown()//havent use
        { 
            APUGeneratorAviliable = false;
            UpdatePower();
        }
        public void SFEXT_L_EngineStarted()
        {
            UpdatePower();
            engineGeneratorAviliable += 1;
        }
        public void SFEXT_L_EngineShutDown()
        {
            UpdatePower();
            engineGeneratorAviliable -= 1;
        }

        public void OnToggleMasterSwitch()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleMasterSwitch));    
        }

        public void OnToggleBattery()
        {
            SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(ToggleBattery));
        }

        public void SFEXT_O_PilotEnter() => ObjectSetActive();

        public void SFEXT_P_PassengerEnter() => ObjectSetActive();
        public void SFEXT_O_PilotExit()
        {
            foreach (var item in disableOnEletriced)
            {
                item.SetActive(true);
            }
        }
        public void SFEXT_P_PassengerExit()
        {
            foreach (var item in disableOnEletriced)
            {
                item.SetActive(true);
            }
        }

        public void ToggleMasterSwitch()
        {
            masterSwitch = !masterSwitch;
            UpdatePower();
        }

        public void ToggleBattery()
        {
            batteryOn = !batteryOn;
            UpdatePower();
        }

        public override void OnDeserialization()
        {
            UpdatePower();
        }

        public void UpdatePower()
        {
            hasPower = false;
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
            ObjectSetActive();
        }

        public void ResetElectrical()
        {
            batteryOn = false;
            APUGeneratorOn = true;
            engineGeneratorOn = true;
            externalPowerOn = false;//EXT
            EmergencyGeneratorOn = true;//RAT

            BatteryAviliable = true;
            APUGeneratorAviliable = false;
            engineGeneratorAviliable = 0;
            externalPowerAviliable = false;
            EmergencyGeneratorAviliable = false;

            UpdatePower();
        }

        private void ObjectSetActive()
        {
            foreach (var item in avionicsEquipments)
            {
                item.SetActive(hasPower);
            }

            foreach (var item in disableOnEletriced)
            {
                item.SetActive(!hasPower);
            }
        }

    }
}
