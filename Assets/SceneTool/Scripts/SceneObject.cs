using UnityEngine;
using UnityEditor;
using System.Linq;

namespace SceneTool
{
    // This class checks SceneAsset file's path every serialization call by UnityEditor. Hence always keeps valid scenepath.  
    // This class leaves a ghost instance (not null but empty) in class that deserializes it. UnityEditor can not set non-asset, non-gameobject type to null.
    [System.Serializable]
    public class SceneObject : ISerializationCallbackReceiver
    {
    #if UNITY_EDITOR
        [SerializeField] private SceneAsset sceneAsset = null; // What we use in editor to select the scene
    #endif

        public bool IsValid() => !string.IsNullOrEmpty(path);

        [SerializeField]
        private string path = string.Empty; // This should only ever be set during serialization!
        public  string Path
        {
            get
            {
            #if UNITY_EDITOR
                if (sceneAsset == null)
                    return string.Empty;

                return AssetDatabase.GetAssetPath(sceneAsset);
            #else
                return path;
            #endif
            }
        }

        public void OnBeforeSerialize()
        {
        #if UNITY_EDITOR
            if (sceneAsset != null)
                path = AssetDatabase.GetAssetPath(sceneAsset);
            else
                path = string.Empty;
        #endif
        }

        public void OnAfterDeserialize()
        {
        #if UNITY_EDITOR
            EditorApplication.update += WarnBuildSetting;
        #endif
        }

        #if UNITY_EDITOR
        private void WarnBuildSetting()
        {
            EditorApplication.update -= WarnBuildSetting;

            // Get scenepath after deserialization in case sceneAsset file change directory
            if (sceneAsset != null)
                path = AssetDatabase.GetAssetPath(sceneAsset);

            if (string.IsNullOrEmpty(path))
                return;

            if (!IsIncludedInBuildSetting())
                Debug.LogWarning(path + " not included in build setting.");
        }

        public bool IsIncludedInBuildSetting() => EditorBuildSettings.scenes.Any(scene => scene.path.Equals(this.path));
        #endif
    }
}
