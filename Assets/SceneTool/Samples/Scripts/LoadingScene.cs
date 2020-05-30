using UnityEngine;
using UnityEngine.UI;
using SceneTool;

public class LoadingScene : MonoBehaviour
{
    [SerializeField]
    private Text progressText = null;

    [SerializeField]
    private GameObject pressAnyKeyText = null;

    private void Awake()
    {
        pressAnyKeyText.SetActive(false);

        SceneLoader.Updated.AddListener(OnProgressUpdated);
    }

    private void Update()
    {
        if (Input.anyKeyDown)
            SceneLoader.ActivateScenes();
    }

    private void OnDestroy()
    {
        SceneLoader.Updated.RemoveListener(OnProgressUpdated);
    }

    private void OnProgressUpdated(float progress)
    {
        progressText.text = (progress * 100).ToString("0.0") + "%";

        if (!SceneLoader.AllowSceneActivation && progress == 0.9f)
            pressAnyKeyText.SetActive(true);
    }
}
