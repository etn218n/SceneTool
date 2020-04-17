using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SceneTool
{
    public class SceneArg
    {
        #region Validation
        public virtual bool IsValid { get; } = false;

        protected bool IsValidPath(params string[] scenePaths)
        {
            if (!scenePaths.Any())
            {
                Debug.LogError("ScenePaths array is empty.");
                return false;
            }

            foreach (var path in scenePaths)
            {
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError("ScenePath is null or empty.");
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Get Scene Path
        protected string[] GetPathFrom(SceneObject[] sceneObjects)
        {
            if (sceneObjects == null)
                return null;

            List<string> scenePaths = new List<string>();

            foreach (var sceneObject in sceneObjects)
            {
                if (!sceneObject.IsValid())
                {
                    Debug.LogError("SceneObject.Path is null or not included in build setting.");
                    return null;
                }

                scenePaths.Add(sceneObject.Path);
            }

            return scenePaths.ToArray();
        }

        protected string GetPathFrom(SceneObject sceneObject)
        {
            if (!sceneObject.IsValid())
            {
                Debug.LogError("SceneObject.Path is null or not included in build setting.");
                return null;
            }

            return sceneObject.Path;
        }
        #endregion
    }

    public class SceneLoadArg : SceneArg
    {
        #region Fields & Properties
        private readonly string[] scenePathsToLoad   = null; // Scenepaths to load in both modes (Single & Additive)
        private readonly string[] scenePathsToUnload = null; // Scenepaths to unload after finishing loading @scenePathsToLoad scenes (only applicable for Additive load mode)

        private readonly string scenePathToSetActive = null; // Scenepath to be set active right after it finishes loading resource & initialize gameobject (must be one of @scenePathsToLoad)
        private readonly string transitionScenePath  = null; // Scenepath to be loaded in between scene loads

        private readonly bool allowSceneActivation = true;   // Activate scene for initialization process when all @scenePathsToLoad finish loading resource 
                                                             // (not to be confused with active scene)

        private readonly bool automaticallyUnloadTransitionScene = true; // Automatically unload @transitionScenePath after finishing loading @scenePathsToLoad (in both modes)
                                                                         // or after unloading @scenePathsToUnload (Additive mode only)

        public string[] ScenePathsToLoad   { get => scenePathsToLoad;   }
        public string[] ScenePathsToUnload { get => scenePathsToUnload; }

        public string TransitionScenePath  { get => transitionScenePath;  }
        public string ScenePathToSetActive { get => scenePathToSetActive; }

        public bool AllowSceneActivation { get => allowSceneActivation; }
        public bool AutomaticallyUnloadTransitionScene { get => automaticallyUnloadTransitionScene; }
        #endregion

        #region Constructors
        public SceneLoadArg(SceneObject[] scenesToLoad,
                            SceneObject[] scenesToUnload,
                            bool allowSceneActivation,
                            SceneObject sceneToSetActive,
                            SceneObject transitionScene,
                            bool automaticallyUnloadTransitionScene)
        {
            this.scenePathsToLoad     = GetPathFrom(scenesToLoad);
            this.scenePathsToUnload   = GetPathFrom(scenesToUnload);
            this.allowSceneActivation = allowSceneActivation;
            this.scenePathToSetActive = GetPathFrom(sceneToSetActive);
            this.transitionScenePath  = GetPathFrom(transitionScene);

            this.automaticallyUnloadTransitionScene = automaticallyUnloadTransitionScene;
        }

        public SceneLoadArg(SceneObject[] scenesToLoad)
        {
            this.scenePathsToLoad = GetPathFrom(scenesToLoad);
        }

        public SceneLoadArg(string[] scenePathsToLoad,
                            string[] scenePathsToUnload,
                            bool allowSceneActivation,
                            string scenePathToSetActive,
                            string transitionScenePath,
                            bool automaticallyUnloadTransitionScene)
        {
            if (scenePathsToLoad != null)
            {
                this.scenePathsToLoad = new string[scenePathsToLoad.Length]; // Scene load happens across multiple frames so it's best to make readonly copy of original array 
                                                                             // (await for ImmutableArray brought into Unity)
                scenePathsToLoad.CopyTo(this.scenePathsToLoad, 0);
            }

            if (scenePathsToUnload != null)
            {
                this.scenePathsToUnload = new string[scenePathsToUnload.Length]; // Scene unload happens across multiple frames so it's best to make readonly copy of original array 
                                                                                 // (await for ImmutableArray brought into Unity)
                scenePathsToUnload.CopyTo(this.scenePathsToUnload, 0);
            }

            this.allowSceneActivation = allowSceneActivation;
            this.scenePathToSetActive = scenePathToSetActive;
            this.transitionScenePath  = transitionScenePath;

            this.automaticallyUnloadTransitionScene = automaticallyUnloadTransitionScene;
        }

        public SceneLoadArg(string[] scenePathsToLoad)
        {
            if (scenePathsToLoad != null)
            {
                this.scenePathsToLoad = new string[scenePathsToLoad.Length];
                scenePathsToLoad.CopyTo(this.scenePathsToLoad, 0);
            }
        }
        #endregion

        #region Validation
        public override bool IsValid
        {
            get
            {
                if (scenePathsToLoad != null)
                {
                    if (!IsValidPath(scenePathsToLoad))
                        return false;
                }

                if (scenePathsToUnload != null)
                {
                    if (!IsValidPath(scenePathsToUnload))
                        return false;
                }

                if (transitionScenePath != null)
                {
                    if (!IsValidPath(transitionScenePath))
                        return false;
                }

                if (scenePathToSetActive != null)
                {
                    if (!IsValidPath(scenePathToSetActive))
                        return false;

                    if (!IsValidToSetActive(scenePathToSetActive)) // Check if @scenePathToSetActive is contained in @scenePathsToLoad
                        return false;
                }

                return true;
            }
        }

        private bool IsValidToSetActive(string scenePath)
        {
            if (!scenePathsToLoad.Contains(scenePathToSetActive))
            {
                Debug.LogError(scenePath + " to be set active not included in list of scenepath to load.");
                return false;
            }

            return true;
        }
        #endregion

        #region Wrapper Functions
        public bool HasTransitionScene  { get => !string.IsNullOrEmpty(transitionScenePath);  }
        public bool HasSceneToSetActive { get => !string.IsNullOrEmpty(scenePathToSetActive); }

        public bool UnloadScenesAfterLoad { get => scenePathsToUnload != null; }

        public int NumberOfScenesToLoad { get => scenePathsToLoad.Length; }
        public int ActiveSceneIndex { get => Array.IndexOf(scenePathsToLoad, scenePathToSetActive); }
        #endregion
    }

    public class SceneUnloadArg : SceneArg
    {
        #region Fields & Properties
        private readonly string[] scenePathsToUnload = null;

        public string[] ScenePathsToUnload { get => scenePathsToUnload; }
        #endregion

        #region Constructors
        public SceneUnloadArg(string[] scenePathsToUnload)
        {
            if (scenePathsToUnload != null)
            {
                this.scenePathsToUnload = new string[scenePathsToUnload.Length];
                scenePathsToUnload.CopyTo(this.scenePathsToUnload, 0);
            }
        }

        public SceneUnloadArg(SceneObject[] scenesToUnload)
        {
            this.scenePathsToUnload = GetPathFrom(scenesToUnload);
        }
        #endregion

        #region Validation
        public override bool IsValid
        {
            get
            {
                if (scenePathsToUnload != null)
                {
                    if (!IsValidPath(scenePathsToUnload))
                        return false;
                }

                return true;
            }
        }
        #endregion
    }
}
