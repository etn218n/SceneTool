using UnityEngine;

namespace SceneTool
{
    public enum SceneBehaviourType { None, UnloadSceneAsync, UnloadSelfAsync, LoadSceneAsync, LoadAdditiveSceneAsync, LoadPreviousSceneAsync }

    [DisallowMultipleComponent]
    public class SceneLoaderBehaviour : MonoBehaviour
    {
        public SceneBehaviourType LoadType = SceneBehaviourType.None;

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
            if (LoadType == SceneBehaviourType.None)
                return;

            if (!HasTransitionScene) 
                transitionScene = null; // nullify SceneObject ghost instance by Unity custom editor

            if (sceneToSetActive != null && !sceneToSetActive.IsValid())
                sceneToSetActive = null;

            switch (LoadType)
            {
                case SceneBehaviourType.LoadSceneAsync: LoadSceneAsync(); break;
                case SceneBehaviourType.LoadAdditiveSceneAsync: LoadAdditiveSceneAsync(); break;
                case SceneBehaviourType.LoadPreviousSceneAsync: LoadPreviousSceneAsync(); break;

                case SceneBehaviourType.UnloadSceneAsync: UnloadSceneAsync(); break;
                case SceneBehaviourType.UnloadSelfAsync:  UnloadSelfAsync();  break;

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

        private void UnloadSelfAsync()
        {
            SceneLoader.AddScenesToUnload(this.gameObject.scene.path)
                       .UnloadUnusedAsset(UnloadUnusedAssets)
                       .StartUnloadingSceneAsync();
        }
    }
}
