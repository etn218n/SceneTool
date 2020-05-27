using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SceneTool
{
    public class SceneLoader : MonoBehaviour
    {
        #region Fields & Properties
        public class UnityFloatEvent : UnityEvent<float> { };

        public static UnityFloatEvent Updated { get; private set; } = new UnityFloatEvent();
        public static UnityEvent    Completed { get; private set; } = new UnityEvent();

        public static float Progress { get; private set; } = 0.0f;

        public static bool IsLoading { get; private set; } = false;
        public static bool AllowSceneActivation { get; private set; } = false;
        public static void ActivateScenes()   => AllowSceneActivation = true;

        private static List<AsyncOperation>   asyncOperations = new List<AsyncOperation>();
        private static Queue<IEnumerator> sceneOperationQueue = new Queue<IEnumerator>();

        internal static SceneLoader Instance { get; private set; } = null;
        #endregion

        #region Unity Callbacks
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogError("Attemp to instantiate an instance of already existing SceneLoader.");

                Destroy(this.gameObject);
                return;
            }

            IsLoading = true;
            Instance  = this;

            DontDestroyOnLoad(Instance.gameObject);
        }

        private void Start()
        {
            StartCoroutine(FetchOperations());
        }

        private void OnDestroy()
        {
            IsLoading = false;
            Progress  = 0.0f;
        }

        private IEnumerator FetchOperations()
        {
            while (sceneOperationQueue.Any())
            {
                Progress = 0.0f;

                asyncOperations.Clear();

                var nextOperation = sceneOperationQueue.Dequeue();

                yield return StartCoroutine(nextOperation);
            }

            Destroy(Instance.gameObject);
        }
        #endregion

        #region Wrapper Functions
        private void SetActiveScene(Scene scene) => SceneManager.SetActiveScene(scene);
        private void SetActiveScene(int buildIndex) => SceneManager.SetActiveScene(SceneManager.GetSceneByBuildIndex(buildIndex));
        private void SetActiveScene(string scenePath) => SceneManager.SetActiveScene(SceneManager.GetSceneByPath(scenePath));
        #endregion

        #region Asynchronous Scene Load
        public static void LoadSceneAsync(SceneLoadArg arg)
        {
            if (!arg.IsValid)
            {
                Debug.LogError("Scene operation cancelled.");
                return;
            }

            if (!IsLoading)
            {
                new GameObject("Scene Progress").AddComponent<SceneLoader>();
            }

            sceneOperationQueue.Enqueue(Instance.LoadSceneAsyncCoroutine(arg));
        }

        public static void LoadAdditiveSceneAsync(SceneLoadArg arg)
        {
            if (!arg.IsValid)
            {
                Debug.LogError("Scene operation cancelled.");
                return;
            }

            if (!IsLoading)
            {
                new GameObject("Scene Progress").AddComponent<SceneLoader>();
            }

            sceneOperationQueue.Enqueue(Instance.LoadAdditiveSceneAsyncCoroutine(arg));
        }

        public static void LoadSceneAsync(params string[] scenePathsToLoad)  => LoadSceneAsync(new SceneLoadArg(scenePathsToLoad));
        public static void LoadSceneAsync(params SceneObject[] scenesToLoad) => LoadSceneAsync(new SceneLoadArg(scenesToLoad));

        public static void LoadAdditiveSceneAsync(params string[] scenePathsToLoad)  => LoadAdditiveSceneAsync(new SceneLoadArg(scenePathsToLoad));
        public static void LoadAdditiveSceneAsync(params SceneObject[] scenesToLoad) => LoadAdditiveSceneAsync(new SceneLoadArg(scenesToLoad));

        public static SceneLoadInterface AddScenesToLoad(params string[] scenePathsToLoad)  => new SceneLoadInterface().AddScenesToLoad(scenePathsToLoad);
        public static SceneLoadInterface AddScenesToLoad(params SceneObject[] scenesToLoad) => new SceneLoadInterface().AddScenesToLoad(scenesToLoad);
        #endregion

        #region Asynchronous Scene Unload
        public static void UnloadSceneAsync(SceneUnloadArg arg)
        {
            if (!arg.IsValid)
            {
                Debug.LogError("Scene operation cancelled.");
                return;
            }

            if (!IsLoading)
            {
                new GameObject("Scene Progress").AddComponent<SceneLoader>();
            }

            sceneOperationQueue.Enqueue(Instance.UnloadSceneAsyncCoroutine(arg));
        }

        public static void UnloadSceneAsync(params string[] scenePathsToUnload)  => UnloadSceneAsync(new SceneUnloadArg(scenePathsToUnload, false));
        public static void UnloadSceneAsync(params SceneObject[] scenesToUnload) => UnloadSceneAsync(new SceneUnloadArg(scenesToUnload, false));

        public static SceneUnloadInterface AddScenesToUnload(params string[] scenePathsToUnload)  => new SceneUnloadInterface().AddScenesToUnload(scenePathsToUnload);
        public static SceneUnloadInterface AddScenesToUnload(params SceneObject[] scenesToUnload) => new SceneUnloadInterface().AddScenesToUnload(scenesToUnload);
        #endregion

        #region Synchronous Scene Load
        private static void LoadScene(string scenePathToLoad, LoadSceneMode mode)
        {
            if (IsLoading)
                return;

            SceneManager.LoadScene(scenePathToLoad, mode);

            Destroy(Instance.gameObject);
        }

        private static void LoadScene(SceneObject sceneToLoad, LoadSceneMode mode)
        {
            LoadScene(sceneToLoad.Path, mode);
        }

        public static void LoadScene(string scenePathToload)  => LoadScene(scenePathToload, LoadSceneMode.Single);
        public static void LoadScene(SceneObject sceneToLoad) => LoadScene(sceneToLoad, LoadSceneMode.Single);

        public static void LoadAdditiveScene(string scenePathToload)  => LoadScene(scenePathToload, LoadSceneMode.Additive);
        public static void LoadAdditiveScene(SceneObject sceneToLoad) => LoadScene(sceneToLoad, LoadSceneMode.Additive);
        #endregion

        #region Coroutines
        private IEnumerator LoadAdditiveSceneAsyncCoroutine(SceneLoadArg arg)
        {
            if (arg.HasTransitionScene)
                yield return StartCoroutine(LoadAdditiveCoroutine(arg.TransitionScenePath));

            AllowSceneActivation = arg.AllowSceneActivation;

            foreach (var scenePath in arg.ScenePathsToLoad)
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

                operation.allowSceneActivation = false;

                asyncOperations.Add(operation);

                while (!operation.isDone)
                {
                    float currentProgress = operation.progress;

                    Updated.Invoke(currentProgress / arg.NumberOfScenesToLoad + Progress);

                    if (currentProgress == 0.9f)
                        break;

                    yield return null;
                }

                Progress += (0.9f / arg.NumberOfScenesToLoad);
            }

            while (!asyncOperations.TrueForAll(operation => operation.isDone))
            {
                if (AllowSceneActivation)
                {
                    for (int i = 0; i < asyncOperations.Count; i++)
                    {
                        asyncOperations[i].allowSceneActivation = true;

                        while (!asyncOperations[i].isDone)
                            yield return null;

                        if (arg.HasSceneToSetActive)
                        {
                            if (i == arg.ActiveSceneIndex)
                                SetActiveScene(arg.ScenePathsToLoad[i]);
                        }

                        Progress += (0.1f / arg.NumberOfScenesToLoad);
                        Updated.Invoke(Progress);
                    }
                }

                yield return null;
            }

            Completed.Invoke();

            if (arg.HasTransitionScene && arg.AutomaticallyUnloadTransitionScene)
                yield return StartCoroutine(UnloadSceneAsyncCoroutine(false, arg.TransitionScenePath));
        }

        private IEnumerator LoadSceneAsyncCoroutine(SceneLoadArg arg)
        {
            List<string> scenePathsToUnload = new List<string>();

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);

                if (scene.isLoaded)
                    scenePathsToUnload.Add(scene.path);
            }

            Scene tempScene = SceneManager.CreateScene("__Temp__");
            SetActiveScene(tempScene);

            if (arg.HasTransitionScene)
                yield return StartCoroutine(LoadAdditiveCoroutine(arg.TransitionScenePath));

            yield return StartCoroutine(UnloadSceneAsyncCoroutine(true, scenePathsToUnload.ToArray()));

            AllowSceneActivation = arg.AllowSceneActivation;

            foreach (var scenePath in arg.ScenePathsToLoad)
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Additive);

                operation.allowSceneActivation = false;

                asyncOperations.Add(operation);

                while (!operation.isDone)
                {
                    float currentProgress = operation.progress;

                    Updated.Invoke(currentProgress / arg.NumberOfScenesToLoad + Progress);

                    if (currentProgress == 0.9f)
                        break;

                    yield return null;
                }

                Progress += (0.9f / arg.NumberOfScenesToLoad);
            }

            while (!asyncOperations.TrueForAll(operation => operation.isDone))
            {
                if (AllowSceneActivation)
                {
                    for (int i = 0; i < asyncOperations.Count; i++)
                    {
                        asyncOperations[i].allowSceneActivation = true;

                        while (!asyncOperations[i].isDone)
                            yield return null;

                        if (arg.HasSceneToSetActive)
                        {
                            if (i == arg.ActiveSceneIndex)
                                SetActiveScene(arg.ScenePathsToLoad[i]);
                        }
                        else if (i == 0)
                        {
                            SetActiveScene(arg.ScenePathsToLoad[0]);
                        }

                        Progress += (0.1f / arg.NumberOfScenesToLoad);
                        Updated.Invoke(Progress);
                    }
                }

                yield return null;
            }

            Completed.Invoke();

            yield return SceneManager.UnloadSceneAsync(tempScene);

            if (arg.HasTransitionScene && arg.AutomaticallyUnloadTransitionScene)
                yield return StartCoroutine(UnloadSceneAsyncCoroutine(false, arg.TransitionScenePath));
        }

        private IEnumerator LoadAdditiveCoroutine(string loadingScenePath)
        {
            AsyncOperation sceneLoadOperation = SceneManager.LoadSceneAsync(loadingScenePath, LoadSceneMode.Additive);

            sceneLoadOperation.allowSceneActivation = true;

            while (!sceneLoadOperation.isDone)
                yield return null;
        }

        private IEnumerator UnloadSceneAsyncCoroutine(SceneUnloadArg arg)
        {
            yield return StartCoroutine(UnloadSceneAsyncCoroutine(arg.UnloadUnusedAssets, arg.ScenePathsToUnload));
        }

        private IEnumerator UnloadSceneAsyncCoroutine(bool unloadUnusedAssets, params string[] scenePathsToUnload)
        {
            foreach (var scenePath in scenePathsToUnload)
            {
                if (!IsLoaded(scenePath))
                    continue;

                AsyncOperation sceneUnloadOperation = SceneManager.UnloadSceneAsync(scenePath);

                while (!sceneUnloadOperation.isDone)
                    yield return null;
            }

            if (unloadUnusedAssets)
            {
                Debug.Log("Unload unused assets.");
                yield return Resources.UnloadUnusedAssets();
            }
        }
        #endregion

        #region Validation
        public static bool IsLoaded(params string[] scenePaths)
        {
            foreach (var scenePath in scenePaths)
            {
                Scene scene = SceneManager.GetSceneByPath(scenePath);

                if (!scene.isLoaded)
                {
                    Debug.Log(scenePath + " is not loaded.");
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}
