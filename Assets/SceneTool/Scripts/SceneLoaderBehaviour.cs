using UnityEngine;

namespace SceneTool
{
    public enum SceneActionType { None, UnloadSceneAsync, UnloadSelfAsync, LoadScene, LoadAdditiveScene, LoadSceneAsync, LoadAdditiveSceneAsync }

    [DisallowMultipleComponent]
    public class SceneLoaderBehaviour : MonoBehaviour
    {
        public SceneActionType LoadType = SceneActionType.None;

        public bool AllowSceneActivation  = true;
        public bool HasTransitionScene    = false;
        public bool UnloadScenesAfterLoad = false;
        public bool AutomaticallyUnloadTransitionScene = true;

        [SerializeField]
        private SceneObject[] scenesToLoad = null;

        [SerializeField]
        private SceneObject[] scenesToUnload = null;

        [SerializeField]
        private SceneObject[] scenesToUnloadAfterLoad = null;

        [SerializeField]
        private SceneObject transitionScene = null;

        [SerializeField]
        private SceneObject sceneToSetActive = null;

        public void Execute()
        {
            if (LoadType == SceneActionType.None)
                return;

            if (!HasTransitionScene) 
                transitionScene = null; // nullify SceneObject ghost instance by Unity custom editor

            if (!UnloadScenesAfterLoad)
                scenesToUnloadAfterLoad = null; 

            if (!sceneToSetActive.IsValid())
                sceneToSetActive = null;

            switch (LoadType)
            {
                case SceneActionType.LoadScene: LoadScene(); break;
                case SceneActionType.LoadSceneAsync: LoadSceneAsync(); break;

                case SceneActionType.LoadAdditiveScene: LoadAdditiveScene(); break;
                case SceneActionType.LoadAdditiveSceneAsync: LoadAdditiveSceneAsync(); break;

                case SceneActionType.UnloadSceneAsync: UnloadSceneAsync(); break;
                case SceneActionType.UnloadSelfAsync:  UnloadSelfAsync();  break;

                default: break;
            }
        }

        private void LoadScene() => SceneLoader.LoadScene(scenesToLoad[0]);
        private void LoadAdditiveScene() => SceneLoader.LoadAdditiveScene(scenesToLoad[0]);

        private void LoadSceneAsync() => SceneLoader.AddScenesToLoad(scenesToLoad).AllowSceneActivation(AllowSceneActivation)
                                                    .SetActiveScene(sceneToSetActive)
                                                    .HasTransitionScene(transitionScene).AutomaticallyUnloadTransitionScene(AutomaticallyUnloadTransitionScene)
                                                    .StartLoadingSceneAsync();

        private void LoadAdditiveSceneAsync() => SceneLoader.AddScenesToLoad(scenesToLoad).AllowSceneActivation(AllowSceneActivation)
                                                            .SetActiveScene(sceneToSetActive)
                                                            .HasTransitionScene(transitionScene).AutomaticallyUnloadTransitionScene(AutomaticallyUnloadTransitionScene)
                                                            .ThenUnloadScenes(scenesToUnloadAfterLoad)
                                                            .StartLoadingAdditiveSceneAsync();

        private void UnloadSceneAsync() => SceneLoader.UnloadSceneAsync(scenesToUnload);
        private void UnloadSelfAsync()  => SceneLoader.UnloadSceneAsync(this.gameObject.scene.path);

    }
}
