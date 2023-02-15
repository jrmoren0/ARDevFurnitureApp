using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace CircuitStream.ARLightEstimation
{
    public abstract class BaseARLightEstimation : MonoBehaviour
    {
        [SerializeField]
        protected ARCameraManager arCameraManager;

        protected virtual void Awake()
        {
            if (arCameraManager == null)
                arCameraManager = FindObjectOfType<ARCameraManager>();
        }

        protected void OnEnable() => arCameraManager.frameReceived += FrameReceived;
        protected void OnDisable() => arCameraManager.frameReceived -= FrameReceived;

        protected abstract void FrameReceived(ARCameraFrameEventArgs args);

#if UNITY_EDITOR
        private void OnValidate()
        {
            var cm = FindObjectOfType<ARCameraManager>();
            if (cm == null)
            {
                Debug.LogError("No Camera Manager found!");
                return;
            }

            if (cm.requestedLightEstimation == LightEstimation.None)
            {
                Debug.LogError("Light Estimation is disabled on the Camera Manager!");
            }
        }
#endif
    }
}