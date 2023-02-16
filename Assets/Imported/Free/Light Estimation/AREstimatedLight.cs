using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace CircuitStream.ARLightEstimation
{
    [RequireComponent(typeof(Light))]
    public class AREstimatedLight : BaseARLightEstimation
    {
        [SerializeField]
        private bool _enableBrightness = true;
        [SerializeField]
        private bool _enableColor = true;
        [SerializeField]
        private bool _enableDirection = true;

        private Light _mainLight;

        protected override void Awake()
        {
            base.Awake();
            _mainLight = GetComponent<Light>();
        }

        protected override void FrameReceived(ARCameraFrameEventArgs args)
        {
            ARLightEstimationData lightEstimationData = args.lightEstimation;

            // Brightness
            if (_enableBrightness)
            {
                if (lightEstimationData.averageMainLightBrightness.HasValue)
                    _mainLight.intensity = lightEstimationData.averageMainLightBrightness.Value;
                else if (lightEstimationData.averageBrightness.HasValue)
                    _mainLight.intensity = lightEstimationData.averageBrightness.Value;
            }

            // Color
            if (_enableColor)
            {
                if (lightEstimationData.mainLightColor.HasValue)
                {
#if PLATFORM_ANDROID
                    // ARCore needs to apply energy conservation term (1 / PI) and be placed in gamma
                    _mainLight.color = lightEstimationData.mainLightColor.Value / Mathf.PI;
                    _mainLight.color = _mainLight.color.gamma;
#else
                    _mainLight.color = lightEstimationData.mainLightColor.Value;
#endif
                }
                else if (lightEstimationData.colorCorrection.HasValue)
                    _mainLight.color = lightEstimationData.colorCorrection.Value;

                if (lightEstimationData.averageColorTemperature.HasValue)
                    _mainLight.colorTemperature = lightEstimationData.averageColorTemperature.Value;
            }

            // Direction
            if (_enableDirection && lightEstimationData.mainLightDirection.HasValue)
                _mainLight.transform.rotation = Quaternion.LookRotation(lightEstimationData.mainLightDirection.Value);
        }
    }
}