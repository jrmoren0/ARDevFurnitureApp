using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

/// <summary>
/// Based off Unity's PrefabImagePairManager Sample.
///     Fixed errors.
///     Added functionality for OnImageFound/OnImageLost events on the TrackedImagePrefab script.
///     Allows GameObjects from scene instead of just prefabs.
/// </summary>
namespace CircuitStream.ARFoundation
{
    /// <summary>
    /// This component listens for images detected by the <c>XRImageTrackingSubsystem</c>
    /// and overlays some prefabs on top of the detected image.
    /// </summary>
    [RequireComponent(typeof(ARTrackedImageManager))]
    public class ObjectImagePairManager : MonoBehaviour, ISerializationCallbackReceiver
    {
        /// <summary>
        /// Used to associate an `XRReferenceImage` with a Prefab by using the `XRReferenceImage`'s guid as a unique identifier for a particular reference image.
        /// </summary>
        [Serializable]
        struct ImageObjectPair
        {
            // System.Guid isn't serializable, so we store the Guid as a string. At runtime, this is converted back to a System.Guid
            public string imageGuid;
            public ObjectOrPrefab imageGameObject;

            public ImageObjectPair(Guid guid, ObjectOrPrefab prefab)
            {
                imageGuid = guid.ToString();
                imageGameObject = prefab;
            }
        }

        [Serializable]
        public struct ObjectOrPrefab
        {
            public ObjectImagePair trackedImageComponent;
            public bool instantiate;

            public ObjectOrPrefab(ObjectImagePair prefab, bool shouldInstantiate)
            {
                trackedImageComponent = prefab;
                instantiate = shouldInstantiate;
            }
        }

        [SerializeField]
        [HideInInspector]
        List<ImageObjectPair> m_PrefabsList = new List<ImageObjectPair>();

        Dictionary<Guid, ObjectOrPrefab> m_PrefabsDictionary = new Dictionary<Guid, ObjectOrPrefab>();
        Dictionary<Guid, ObjectImagePair> m_Instantiated = new Dictionary<Guid, ObjectImagePair>();
        ARTrackedImageManager m_TrackedImageManager;

        [SerializeField]
        [Tooltip("Reference Image Library")]
        XRReferenceImageLibrary m_ImageLibrary;

        /// <summary>
        /// Get the <c>XRReferenceImageLibrary</c>
        /// </summary>
        public XRReferenceImageLibrary imageLibrary
        {
            get => m_ImageLibrary;
            set => m_ImageLibrary = value;
        }

        public void OnBeforeSerialize()
        {
            m_PrefabsList.Clear();
            foreach (var kvp in m_PrefabsDictionary)
            {
                m_PrefabsList.Add(
                    new ImageObjectPair(kvp.Key,
                    new ObjectOrPrefab(kvp.Value.trackedImageComponent, kvp.Value.instantiate)));
            }
        }

        public void OnAfterDeserialize()
        {
            m_PrefabsDictionary = new Dictionary<Guid, ObjectOrPrefab>();
            foreach (var entry in m_PrefabsList)
            {
                m_PrefabsDictionary.Add(Guid.Parse(entry.imageGuid), new ObjectOrPrefab(entry.imageGameObject.trackedImageComponent, entry.imageGameObject.instantiate));
            }
        }

        void Awake()
        {
            m_TrackedImageManager = GetComponent<ARTrackedImageManager>();
        }

        void OnEnable()
        {
            m_TrackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
        }

        void OnDisable()
        {
            m_TrackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
        }

        void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
        {
            foreach (var trackedImage in eventArgs.added)
            {
                // Give the initial image a reasonable default scale
                //var minLocalScalar = Mathf.Min(trackedImage.size.x, trackedImage.size.y) / 2;
                //trackedImage.transform.localScale = new Vector3(minLocalScalar, minLocalScalar, minLocalScalar);
                AssignObject(trackedImage);
            }

            foreach (var trackedImage in eventArgs.updated)
            {
                if (!TryGetObject(trackedImage.referenceImage.guid, out ObjectOrPrefab prefab))
                    return;

                if (prefab.instantiate)
                    m_Instantiated[trackedImage.referenceImage.guid].UpdateImageTracking(trackedImage.trackingState);
                else
                    prefab.trackedImageComponent.UpdateImageTracking(trackedImage.trackingState);
            }

            foreach (var trackedImage in eventArgs.removed)
            {
                if (!TryGetObject(trackedImage.referenceImage.guid, out ObjectOrPrefab prefab))
                    return;

                if (prefab.instantiate)
                    m_Instantiated[trackedImage.referenceImage.guid].UpdateImageTracking(TrackingState.None);
                else
                    prefab.trackedImageComponent.UpdateImageTracking(TrackingState.None);
            }
        }

        void AssignObject(ARTrackedImage trackedImage)
        {
            if (!TryGetObject(trackedImage.referenceImage.guid, out ObjectOrPrefab prefab))
                return;

            if (prefab.instantiate)
            {
                ObjectImagePair copy = Instantiate(prefab.trackedImageComponent, trackedImage.transform);
                copy.transform.localPosition = Vector3.zero;
                copy.transform.localRotation = Quaternion.identity;
                m_Instantiated[trackedImage.referenceImage.guid] = copy;
                copy.UpdateImageTracking(TrackingState.Tracking);
            }
            else
            {
                prefab.trackedImageComponent.transform.SetParent(trackedImage.transform);
                prefab.trackedImageComponent.transform.localPosition = Vector3.zero;
                prefab.trackedImageComponent.transform.localRotation = Quaternion.identity;
                prefab.trackedImageComponent.UpdateImageTracking(TrackingState.Tracking);
            }
        }

        public bool TryGetObject(Guid imageID, out ObjectOrPrefab prefab)
        {
            return m_PrefabsDictionary.TryGetValue(imageID, out prefab);
        }

        public ObjectOrPrefab GetObjectForReferenceImage(XRReferenceImage referenceImage)
            => m_PrefabsDictionary.TryGetValue(referenceImage.guid, out var prefab) ? prefab : new ObjectOrPrefab(null, true);

        public void SetObjectForReferenceImage(XRReferenceImage referenceImage, ObjectImagePair alternativePrefab)
        {
            var pair = m_PrefabsDictionary[referenceImage.guid];
            pair.trackedImageComponent = alternativePrefab;
            m_PrefabsDictionary[referenceImage.guid] = pair;
            if (m_Instantiated.TryGetValue(referenceImage.guid, out var instantiatedPrefab))
            {
                ObjectImagePair prefab = Instantiate(alternativePrefab, instantiatedPrefab.transform.parent);
                m_Instantiated[referenceImage.guid] = prefab;
                prefab.transform.localPosition = Vector3.zero;
                prefab.transform.localRotation = Quaternion.identity;
                Destroy(instantiatedPrefab);
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// This customizes the inspector component and updates the prefab list when
        /// the reference image library is changed.
        /// </summary>
        [CustomEditor(typeof(ObjectImagePairManager))]
        class ObjectImagePairManagerInspector : Editor
        {
            List<XRReferenceImage> m_ReferenceImages = new List<XRReferenceImage>();
            bool m_IsExpanded = true;

            bool HasLibraryChanged(XRReferenceImageLibrary library)
            {
                if (library == null)
                    return m_ReferenceImages.Count != 0;

                if (m_ReferenceImages.Count != library.count)
                    return true;

                for (int i = 0; i < library.count; i++)
                {
                    if (m_ReferenceImages[i] != library[i])
                        return true;
                }

                return false;
            }

            public override void OnInspectorGUI()
            {
                //customized inspector
                var behaviour = serializedObject.targetObject as ObjectImagePairManager;

                serializedObject.Update();
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Script"));
                }

                var libraryProperty = serializedObject.FindProperty(nameof(m_ImageLibrary));
                EditorGUILayout.PropertyField(libraryProperty);
                var library = libraryProperty.objectReferenceValue as XRReferenceImageLibrary;

                //check library changes
                if (HasLibraryChanged(library))
                {
                    if (library)
                    {
                        var tempDictionary = new Dictionary<Guid, ObjectOrPrefab>();
                        foreach (var referenceImage in library)
                        {
                            tempDictionary.Add(referenceImage.guid, behaviour.GetObjectForReferenceImage(referenceImage));
                        }
                        behaviour.m_PrefabsDictionary = tempDictionary;
                    }
                }

                if (library == null)
                {
                    serializedObject.ApplyModifiedProperties();
                    return;
                }

                // update current
                m_ReferenceImages.Clear();
                if (library)
                {
                    foreach (var referenceImage in library)
                    {
                        m_ReferenceImages.Add(referenceImage);
                    }
                }

                //show prefab list
                m_IsExpanded = EditorGUILayout.Foldout(m_IsExpanded, "Object List");
                if (m_IsExpanded)
                {
                    using (new EditorGUI.IndentLevelScope())
                    {
                        EditorGUI.BeginChangeCheck();

                        var tempDictionary = new Dictionary<Guid, ObjectOrPrefab>();
                        foreach (var image in library)
                        {
                            var pair = GetPrefab(behaviour.m_PrefabsDictionary, image.guid);
                            ObjectImagePair prefab = (ObjectImagePair)EditorGUILayout.ObjectField(image.name, pair.trackedImageComponent, typeof(ObjectImagePair), true);
                            tempDictionary.Add(image.guid, new ObjectOrPrefab(prefab, IsAPrefab(prefab)));
                        }

                        if (EditorGUI.EndChangeCheck())
                        {
                            Undo.RecordObject(target, "Update Object");
                            behaviour.m_PrefabsDictionary = tempDictionary;
                            EditorUtility.SetDirty(target);
                        }
                    }
                }

                serializedObject.ApplyModifiedProperties();
            }

            ObjectOrPrefab GetPrefab(Dictionary<Guid, ObjectOrPrefab> dict, Guid id) => dict.TryGetValue(id, out ObjectOrPrefab value) ? value : new ObjectOrPrefab(null, true);

            /// <summary>
            /// Returns if referencing a prefab in the project window.
            /// </summary>
            public static bool IsAPrefab(ObjectImagePair obj)
            {
                if (obj == null)
                    return true;

                bool isAPrefab = PrefabUtility.GetPrefabAssetType(obj) != PrefabAssetType.NotAPrefab;
                bool isInScene = PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.Connected;
                return isAPrefab && !isInScene;
            }
        }
#endif
    }
}