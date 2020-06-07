using System;
using UnityEngine;

namespace SceneTool
{
    public enum SceneActionType { None, UnloadSceneAsync, LoadSceneAsync, LoadAdditiveSceneAsync, LoadPreviousSceneAsync }

    [CreateAssetMenu(fileName = "SceneLoaderAction", menuName = "SceneLoader/SceneLoaderAction")]
    public class SceneLoaderAction : ScriptableObject
    {
        public SceneActionType LoadType = SceneActionType.None;

        public bool AllowSceneActivation  = true;
        public bool HasTransitionScene    = false;
        public bool UnloadUnusedAssets    = false;
        public bool UnloadScenesAfterLoad = false;
        public bool AutomaticallyUnloadTransitionScene = true;

        [SerializeField] private SceneObject[] scenesToLoad   = null;
        [SerializeField] private SceneObject[] scenesToUnload = null;

        [SerializeField] private SceneObject transitionScene  = null;
        [SerializeField] private SceneObject sceneToSetActive = null;

        public void Execute()
        {
            if (LoadType == SceneActionType.None)
                return;

            if (!HasTransitionScene)
                transitionScene = null; // nullify SceneObject ghost instance by Unity custom editor

            if (sceneToSetActive != null && !sceneToSetActive.IsValid())
                sceneToSetActive = null;

            switch (LoadType)
            {
                case SceneActionType.LoadSceneAsync: LoadSceneAsync(); break;
                case SceneActionType.LoadAdditiveSceneAsync: LoadAdditiveSceneAsync(); break;
                case SceneActionType.LoadPreviousSceneAsync: LoadPreviousSceneAsync(); break;

                case SceneActionType.UnloadSceneAsync: UnloadSceneAsync(); break;

                default: break;
            }
        }

        private void LoadSceneAsync()
        {
            SceneLoader.AddScenesToLoad(scenesToLoad).AllowSceneActivation(AllowSceneActivation)
                       .SetActiveScene(sceneToSetActive)
                       .HasTransitionScene(transitionScene).AutomaticallyUnloadTransitionScene(AutomaticallyUnloadTransitionScene)
                       .StartLoadingSceneAsync();
        }

        private void LoadAdditiveSceneAsync()
        {
            SceneLoader.AddScenesToLoad(scenesToLoad).AllowSceneActivation(AllowSceneActivation)
                                                              .SetActiveScene(sceneToSetActive)
                                                              .HasTransitionScene(transitionScene).AutomaticallyUnloadTransitionScene(AutomaticallyUnloadTransitionScene)
                                                              .StartLoadingAdditiveSceneAsync();

            if (UnloadScenesAfterLoad)
            {
                SceneLoader.AddScenesToUnload(scenesToUnload)
                           .UnloadUnusedAsset(UnloadUnusedAssets)
                           .StartUnloadingSceneAsync();
            }
        }

        private void LoadPreviousSceneAsync()
        {
            SceneLoader.AddScenesToLoad(SceneLoader.PreviousScene).AllowSceneActivation(AllowSceneActivation)
                       .HasTransitionScene(transitionScene).AutomaticallyUnloadTransitionScene(AutomaticallyUnloadTransitionScene)
                       .StartLoadingSceneAsync();
        }

        private void UnloadSceneAsync()
        {
            SceneLoader.AddScenesToUnload(scenesToUnload)
                       .UnloadUnusedAsset(UnloadUnusedAssets)
                       .StartUnloadingSceneAsync();
        }
    }
}
