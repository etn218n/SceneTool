using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace SceneTool
{
    [CreateAssetMenu(fileName = "SceneBuildSetting", menuName = "SceneLoader/SceneBuildSetting")]
    public class SceneBuildSetting : ScriptableObject
    {
    #if UNITY_EDITOR
        [SerializeField]
        private SceneAsset preloadScene = null;
        public  SceneAsset PreloadScene { get => preloadScene; }

        [SerializeField]
        private List<SceneAsset> scenes = new List<SceneAsset>();
        public  List<SceneAsset> Scenes { get => scenes; }
    #endif
    }
}
