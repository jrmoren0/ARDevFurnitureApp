using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARSubsystems;

namespace CircuitStream.ARFoundation
{
    public class ObjectImagePair : MonoBehaviour
    {
        public UnityEvent OnImageTracked;
        public UnityEvent OnImageLost;

        public bool autoEnableAndDisable = true;

        public TrackingState CurrentTrackingState { get; private set; } = TrackingState.None;

        internal void UpdateImageTracking(TrackingState state)
        {
            if (state == CurrentTrackingState)
                return;

            CurrentTrackingState = state;

            switch (state)
            {
                case TrackingState.Limited:
                case TrackingState.None:
                    OnImageLost?.Invoke();
                    if (autoEnableAndDisable)
                        gameObject.SetActive(false);
                    break;

                case TrackingState.Tracking:
                    OnImageTracked?.Invoke();
                    if (autoEnableAndDisable)
                        gameObject.SetActive(true);
                    break;
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(ObjectImagePair))]
        class ObjectImagePairInspector : Editor
        {
            public override void OnInspectorGUI()
            {
                var trackedEvent = serializedObject.FindProperty(nameof(OnImageTracked));
                EditorGUILayout.PropertyField(trackedEvent);
                var lostEvent = serializedObject.FindProperty(nameof(OnImageLost));
                EditorGUILayout.PropertyField(lostEvent);
                var autoEnableAndDisableBool = serializedObject.FindProperty(nameof(autoEnableAndDisable));
                EditorGUILayout.PropertyField(autoEnableAndDisableBool);

                if (GUILayout.Button("Create Reference Image"))
                {
                    var pairManager = FindObjectOfType<ObjectImagePairManager>();
                    if (pairManager == null || pairManager.imageLibrary == null) return;

                    var objectPair = serializedObject.targetObject as ObjectImagePair;
                    if (!TryGetTexture(pairManager, objectPair, out XRReferenceImage libraryImage))
                        return;
                    
                    var refImageTransform = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
                    refImageTransform.SetParent(objectPair.transform);
                    refImageTransform.localPosition = Vector3.zero;
                    refImageTransform.localRotation = Quaternion.Euler(new Vector3(90, 0, 0));
                    var physicalSize = libraryImage.specifySize ? libraryImage.size : Vector2.one;
                    refImageTransform.localScale = new Vector3(physicalSize.x, physicalSize.y, 1);
                    refImageTransform.name = "Reference Image";

                    var texturePath = AssetDatabase.GUIDToAssetPath(new GUID(libraryImage.textureGuid.ToString().Replace("-", "")));
                    var textureNameLength = (Path.GetFileName(Application.dataPath.Replace("\\", "/") + texturePath)).Length;
                    var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                    var textureDirectory = texturePath.Substring(0, texturePath.Length - textureNameLength);

                    if (!AssetDatabase.IsValidFolder(textureDirectory + "Materials"))
                        AssetDatabase.CreateFolder(textureDirectory.Trim('/'), "Materials");

                    string assetPath = textureDirectory + "Materials/" + libraryImage.name + "Material.mat";
                    var renderer = refImageTransform.GetComponent<MeshRenderer>();
                    AssetDatabase.CreateAsset(new Material(renderer.sharedMaterial.shader), assetPath);

                    var mat = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                    mat.mainTexture = texture;
                    renderer.sharedMaterial = mat;
                }
            }

            private bool TryGetTexture(ObjectImagePairManager pairManager, ObjectImagePair imagePair, out XRReferenceImage libraryImage)
            {
                foreach (var item in pairManager.imageLibrary)
                {
                    if (!pairManager.TryGetObject(item.guid, out ObjectImagePairManager.ObjectOrPrefab prefab))
                        continue;

                    if (prefab.trackedImageComponent == imagePair)
                    {
                        libraryImage = item;

                        if (prefab.instantiate)
                        {
                            Debug.LogError("Create Reference Button will only work for Objects in the Scene View.");
                            return false;
                        }

                        return true;
                    }
                }

                libraryImage = default;
                Debug.LogError("Could not find Object linked to the ObjectImagePairManager");
                return false;
            }
        }
#endif
    }
}