using UnityEngine;

namespace SceneTool
{
    public enum SceneActionType { None, UnloadSceneAsync, LoadScene, LoadAdditiveScene, LoadSceneAsync, LoadAdditiveSceneAsync }

    [CreateAssetMenu(fileName = "SceneLoaderAction", menuName = "SceneLoader/SceneLoaderAction")]
    public class SceneLoaderAction : ScriptableObject
    {
        public SceneActionType LoadType = SceneActionType.None;

        public bool AllowSceneActivation  = true;
        public bool HasTransitionScene    = false;
        public bool UnloadUnusedAssets    = false;
        public bool UnloadScenesAfterLoad = false;
        public bool AutomaticallyUnloadTransitionScene = true;

        [SerializeField] private SceneObject[] scenesToLoad = null;
        [SerializeField] private SceneObject[] scenesToUnload = null;

        [SerializeField] private SceneObject transitionScene = null;
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
                case SceneActionType.LoadScene: LoadScene(); break;
                case SceneActionType.LoadSceneAsync: LoadSceneAsync(); break;

                case SceneActionType.LoadAdditiveScene: LoadAdditiveScene(); break;
                case SceneActionType.LoadAdditiveSceneAsync: LoadAdditiveSceneAsync(); break;

                case SceneActionType.UnloadSceneAsync: UnloadSceneAsync(); break;

                default: break;
            }
        }

        private void LoadScene() => SceneLoader.LoadScene(scenesToLoad[0]);
        private void LoadAdditiveScene() => SceneLoader.LoadAdditiveScene(scenesToLoad[0]);

        private void LoadSceneAsync() => SceneLoader.AddScenesToLoad(scenesToLoad).AllowSceneActivation(AllowSceneActivation)
                                                    .SetActiveScene(sceneToSetActive)
                                                    .HasTransitionScene(transitionScene).AutomaticallyUnloadTransitionScene(AutomaticallyUnloadTransitionScene)
                                                    .StartLoadingSceneAsync();

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

        private void UnloadSceneAsync() => SceneLoader.AddScenesToUnload(scenesToUnload)
                                                      .UnloadUnusedAsset(UnloadUnusedAssets)
                                                      .StartUnloadingSceneAsync();
    }
}
