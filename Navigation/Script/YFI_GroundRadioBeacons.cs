
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.BasicFlightData;

namespace YuxiFlightInstruments.Navigation
{
    public enum BeaconType
    {
        NDB,
        VOR,
        VORDME,
        ILS
    }

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class YFI_GroundRadioBeacons : UdonSharpBehaviour
    {

        public string beaconName = "VAU";
        public float beaconFrequency = 114.5f;
        public BeaconType beaconType = BeaconType.VORDME;
        [Tooltip("m")]
        public float serviceRange = 74080f;
        public bool isAvailable = true;
        
        [Header("如果是ILS")]
        [Tooltip("下滑道位置，z指向跑道方向")]
        public Transform glideSlopeStation;
        [Tooltip("跑道航向")]
        public float runwayHeading = 0;
         
        void Start()
        {

        }
    }
}
