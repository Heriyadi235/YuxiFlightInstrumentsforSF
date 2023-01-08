
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using YuxiFlightInstruments.BasicFlightData;

#if !COMPILER_UDONSHARP && UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UdonSharpEditor;
#endif

namespace YuxiFlightInstruments.Navigation
{
    [UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]//不同步，频率同步在其上一层实现
    public class YFI_NavigationReceiver : UdonSharpBehaviour
    {
        public float frequency = 114.5f;
        [UdonSynced] public int beaconIndex = 0; //随便一写，从1开始
        public YFI_GroundRadioBeacons[] Beacons;
        public YFI_FlightDataInterface flightData;
        public bool autoSetupBeforeSave = true;

        //[HeaderAttribute("导航参数")]
        [System.NonSerialized] public float distance = 10f; //距离m DME
        [System.NonSerialized] public float azimuth = 0f; //偏离角，对于NDB/HSI,通过-180~+180表示在航向左侧还是右侧，对于VOR表示在OB左侧或右侧
        [System.NonSerialized] public float VORazimuth = 0f; //VOR偏离角，-90-0-90
        [System.NonSerialized] public bool isFlyTo = true; //向台or背台
        [System.NonSerialized] public bool isHSI = true; //是否自动判断向台背台
        [System.NonSerialized] public float omnibearing = 0; //模拟改变OBS时，VOR方位角变化，此时有 azimuthofPlane=-magneticDeclination-omnibearing =azimuthofPlane，HSI下omnibearing = magneticHeading"
        [System.NonSerialized] public float GSAngle = 0; //下滑道偏差角, 飞机偏上时为负
        [System.NonSerialized] public bool GSCapture = false;
        //public string DebugOut;
        public bool hasBeaconSelected = false;
        public YFI_GroundRadioBeacons SelectedBeacon = null;

        [Tooltip("second")]
        public float updateInterval = 0.3f;
        private float sinceLastUpdate = 0;
        void Start()
        {
            ValidateFrequency();
        }

        private void Update()
        {
            sinceLastUpdate += Time.deltaTime;
            bool updateFlag = sinceLastUpdate > updateInterval;
            if (hasBeaconSelected && updateFlag)
            {
                sinceLastUpdate = 0;

                //接收机会计算出所有参数，显示时再判断类型决定输出哪些参数
                //计算距离
                distance = (gameObject.transform.position - SelectedBeacon.transform.position).magnitude;
                //计算方位角
                var PlaneBeaconVector3 = SelectedBeacon.transform.position - gameObject.transform.position;
                var PlaneBeaconVector2 = new Vector2(PlaneBeaconVector3.x * Mathf.Cos(flightData.magneticDeclination * Mathf.PI / 180) - PlaneBeaconVector3.z * Mathf.Sin(flightData.magneticDeclination * Mathf.PI / 180), 
                                                    PlaneBeaconVector3.x * Mathf.Sin(flightData.magneticDeclination * Mathf.PI / 180) + PlaneBeaconVector3.z * Mathf.Cos(flightData.magneticDeclination * Mathf.PI / 180));//转换到磁北坐标系的导航台-飞机向量
                //var PlaneBeaconVector2 = new Vector2(PlaneBeaconVector3.x, PlaneBeaconVector3.z);
                //NDB或HSI的情况，不需要考虑omnibearing
                //TODO:NDB的干扰可以通过旋转omnibearing实现
                if (SelectedBeacon.beaconType == BeaconType.ILS)
                {
                    omnibearing = SelectedBeacon.runwayHeading;
                    //下滑道计算
                    GSAngle = Vector3.SignedAngle(Vector3.up,Vector3.ProjectOnPlane(SelectedBeacon.glideSlopeStation.position - gameObject.transform.position, SelectedBeacon.glideSlopeStation.right), SelectedBeacon.glideSlopeStation.right);
                    GSAngle = (90 + 3) - GSAngle; //先写死一个3度下滑角
                    if (Mathf.Abs(GSAngle) > 90) GSCapture = false;
                    else GSCapture = true;
                }
                else if (isHSI || SelectedBeacon.beaconType == BeaconType.NDB) omnibearing = flightData.magneticHeading;
                var omnibearingVector = new Vector2(Mathf.Sin(omnibearing * Mathf.PI / 180), Mathf.Cos(omnibearing * Mathf.PI / 180));
                var azimuthtoOmniBearing = Vector2.SignedAngle(omnibearingVector, PlaneBeaconVector2);
                //DebugOut = PlaneBeaconVector2.x.ToString("f1") + "\t" + PlaneBeaconVector2.y.ToString("f1") + "\t" +  omnibearingVector.x.ToString("f1")+ "\t" + omnibearingVector.y.ToString("f1");
                VORazimuth = azimuth = -azimuthtoOmniBearing;
                isFlyTo = Mathf.Abs(azimuth) < 90f;
                if (!isFlyTo)
                {
                    VORazimuth = azimuth < 0 ? azimuth + 180 : azimuth - 180;
                }
                //计算下滑道
            }

        }

        public void OnPrevFreq()
        {
            beaconIndex = beaconIndex - 1 < 0 ? 0 : beaconIndex - 1;
            ValidateFrequency();
            RequestSerialization();
        }

        public void OnNextFreq()
        {
            beaconIndex = beaconIndex + 1 >= Beacons.Length ? Beacons.Length : beaconIndex + 1;
            ValidateFrequency();
            RequestSerialization();
        }
        public void SFEXT_L_EntityStart()
        {
            gameObject.SetActive(false);
        }
        public void SFEXT_G_PilotEnter()
        {
            gameObject.SetActive(true);
        }
        private void ValidateFrequency()
        {
            if (Beacons.Length != 0)
            {
                if (Beacons[beaconIndex] != null && Beacons[beaconIndex].beaconType != BeaconType.NULL)
                {
                    hasBeaconSelected = true;
                    SelectedBeacon = Beacons[beaconIndex];
                    frequency = SelectedBeacon.beaconFrequency;
                }
                else hasBeaconSelected = false;
            }
            else
                Debug.Log("Beacons.Length is 0");
        }

        public override void OnDeserialization()
        {
            ValidateFrequency();
        }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
        private static IEnumerable<T> GetUdonSharpComponentsInScene<T>() where T : UdonSharpBehaviour
        {
            return FindObjectsOfType<UdonBehaviour>()
                .Where(UdonSharpEditorUtility.IsUdonSharpBehaviour)
                .Select(UdonSharpEditorUtility.GetProxyBehaviour)
                .Select(u => u as T)
                .Where(u => u != null);
        }
        public void Setup()
        {
            //NOT WORK WHY?
            Beacons = GetUdonSharpComponentsInScene<YFI_GroundRadioBeacons>().ToArray();

            EditorUtility.SetDirty(UdonSharpEditorUtility.GetBackingUdonBehaviour(this));
        }

#endif
    }

#if !COMPILER_UDONSHARP && UNITY_EDITOR
    [CustomEditor(typeof(YFI_NavigationReceiver))]
    public class YFI_NavigationReceiverEditor : Editor
    {
        private static IEnumerable<T> GetUdonSharpComponentsInScene<T>() where T : UdonSharpBehaviour
        {
            return FindObjectsOfType<UdonBehaviour>()
                .Where(UdonSharpEditorUtility.IsUdonSharpBehaviour)
                .Select(UdonSharpEditorUtility.GetProxyBehaviour)
                .Select(u => u as T)
                .Where(u => u != null);
        }

        public override void OnInspectorGUI()
        {
            if (UdonSharpGUI.DrawDefaultUdonSharpBehaviourHeader(target)) return;
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            var receiver = target as YFI_NavigationReceiver;
            if (GUILayout.Button("Setup"))
            {
                receiver.Setup();
            }
        }

        [InitializeOnLoadMethod]
        static public void RegisterCallback()
        {
            EditorSceneManager.sceneSaving += (_, __) => SetupBeaconList();
        }

        private static void SetupBeaconList()
        {
            var receivers = GetUdonSharpComponentsInScene<YFI_NavigationReceiver>();
            foreach (var receiver in receivers)
            {
                if (receiver?.autoSetupBeforeSave != true) continue;
                Debug.Log($"[{receiver.gameObject.name}] Auto setup");
                receiver.Setup();
            }
        }
    }

#endif
}
