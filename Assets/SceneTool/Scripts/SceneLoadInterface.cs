using System.Collections.Generic;

namespace SceneTool
{
    // Support fluent syntax for better readibility and flexible parameters
    // Store mutable setting before intancing SceneArg to pass to SceneLoader
    public class SceneLoadInterface
    {
        // Cache mutable setting
        #region Fields & Properties
        private string[] scenePathsToLoad   = null;
        private string[] scenePathsToUnload = null;

        private string transitionScenePath  = null;
        private string scenePathToSetActive = null;

        private bool allowSceneActivation = true;
        private bool automaticallyUnloadTransitionScene = true;
        #endregion

        #region Fluent Interfaces
        public SceneLoadInterface AddScenesToLoad(params string[] scenePathsToLoad)
        {
            this.scenePathsToLoad = scenePathsToLoad;

            return this;
        }

        public SceneLoadInterface AddScenesToLoad(params SceneObject[] scenesToLoad)
        {
            if (scenesToLoad != null)
            {
                List<string> scenePaths = new List<string>();

                foreach (var scene in scenesToLoad)
                    scenePaths.Add(scene.Path);

                this.scenePathsToLoad = scenePaths.ToArray();
            }

            return this;
        }

        public SceneLoadInterface ThenUnloadScenes(params string[] scenePathsToUnload)
        {
            this.scenePathsToUnload = scenePathsToUnload;

            return this;
        }

        public SceneLoadInterface ThenUnloadScenes(params SceneObject[] scenesToUnload)
        {
            if (scenesToUnload != null)
            {
                List<string> scenePaths = new List<string>();

                foreach (var scene in scenesToUnload)
                    scenePaths.Add(scene.Path);

                this.scenePathsToUnload = scenePaths.ToArray();
            }

            return this;
        }

        public SceneLoadInterface AllowSceneActivation(bool value)
        {
            this.allowSceneActivation = value;

            return this;
        }

        public SceneLoadInterface HasTransitionScene(string transitionScenePath)
        {
            this.transitionScenePath = transitionScenePath;

            return this;
        }

        public SceneLoadInterface HasTransitionScene(SceneObject transtionScene)
        {
            if (transtionScene != null)
                this.transitionScenePath = transtionScene.Path;

            return this;
        }

        public SceneLoadInterface AutomaticallyUnloadTransitionScene(bool value)
        {
            this.automaticallyUnloadTransitionScene = value;

            return this;
        }

        public SceneLoadInterface SetActiveScene(string scenePathToSetActive)
        {
            this.scenePathToSetActive = scenePathToSetActive;

            return this;
        }

        public SceneLoadInterface SetActiveScene(SceneObject sceneToSetActive)
        {
            if (sceneToSetActive != null)
                this.scenePathToSetActive = sceneToSetActive.Path;

            return this;
        }
        #endregion

        #region Scene Load Delegation
        public void StartLoadingSceneAsync()
        {
            SceneLoader.LoadSceneAsync(new SceneLoadArg(scenePathsToLoad, 
                                                        scenePathsToUnload, 
                                                        allowSceneActivation, 
                                                        scenePathToSetActive, 
                                                        transitionScenePath, 
                                                        automaticallyUnloadTransitionScene));
        }

        public void StartLoadingAdditiveSceneAsync()
        {
            SceneLoader.LoadAdditiveSceneAsync(new SceneLoadArg(scenePathsToLoad,
                                                                scenePathsToUnload,
                                                                allowSceneActivation,
                                                                scenePathToSetActive,
                                                                transitionScenePath,
                                                                automaticallyUnloadTransitionScene));
        }
        #endregion
    }

    public class SceneUnloadInterface
    {
        // Cache mutable setting
        #region Fields & Properties
        private string[] scenePathsToUnload = null;
        #endregion

        #region Fluent Interfaces
        public SceneUnloadInterface AddScenesToUnload(params string[] scenePathsToUnload)
        {
            this.scenePathsToUnload = scenePathsToUnload;

            return this;
        }

        public SceneUnloadInterface AddScenesToUnload(params SceneObject[] scenesToUnload)
        {
            if (scenesToUnload != null)
            {
                List<string> scenePaths = new List<string>();

                foreach (var scene in scenesToUnload)
                    scenePaths.Add(scene.Path);

                this.scenePathsToUnload = scenePaths.ToArray();
            }

            return this;
        }
        #endregion

        #region Scene Unload Delegation
        public void StartUnloadingSceneAsync()
        {
            SceneLoader.UnloadSceneAsync(scenePathsToUnload);
        }
        #endregion
    }
}
