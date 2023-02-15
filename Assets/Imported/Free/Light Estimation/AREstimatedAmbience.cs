using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace CircuitStream.ARLightEstimation
{
    public class AREstimatedAmbience : BaseARLightEstimation
    {
        [SerializeField]
        private bool _enableFlatBrightness = true;

        [SerializeField]
        private bool _enableFlatColor = true;

        protected override void FrameReceived(ARCameraFrameEventArgs args)
        {
            ARLightEstimationData lightData = args.lightEstimation;

            if (lightData.ambientSphericalHarmonics.HasValue)
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Skybox;
                RenderSettings.ambientProbe = lightData.ambientSphericalHarmonics.Value;
            }
            else
            {
                RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;

                // Color
                if (_enableFlatColor)
                {
                    if (lightData.colorCorrection.HasValue)
                        RenderSettings.ambientLight = lightData.colorCorrection.Value;
                    else if (lightData.mainLightColor.HasValue)
                    {
#if PLATFORM_ANDROID
                        //ARCore needs to apply energy conservation term(1 / PI) and be placed in gamma
                        Color fixedColor = lightData.mainLightColor.Value / Mathf.PI;
                        RenderSettings.ambientLight = fixedColor.gamma;
#else
                    RenderSettings.ambientLight = lightData.mainLightColor.Value;
#endif
                    }
                }

                // Brightness
                if (_enableFlatBrightness)
                {
                    if (lightData.averageBrightness.HasValue)
                        ApplyAmbientIntensity(lightData.averageBrightness.Value);
                    else if (lightData.averageIntensityInLumens.HasValue)
                        ApplyAmbientIntensity(lightData.averageIntensityInLumens.Value);
                    else if (lightData.averageMainLightBrightness.HasValue)
                        ApplyAmbientIntensity(lightData.averageMainLightBrightness.Value);
                    else if (lightData.mainLightIntensityLumens.HasValue)
                        ApplyAmbientIntensity(lightData.mainLightIntensityLumens.Value);
                }
            }
        }

        private void ApplyAmbientIntensity(float value)
        {
            RenderSettings.ambientLight *= value;
        }
    }
}