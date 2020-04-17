using UnityEngine;
using UnityEngine.Assertions;

public class CameraEntity : MonoBehaviour
{
    public CameraManager Manager;

    private Camera thisCamera;
    private AudioListener audioListener;

    private void OnValidate()
    {
        Assert.IsNotNull(Manager);
    }

    private void Awake()
    {
        thisCamera = GetComponent<Camera>();
    }

    private void OnEnable()
    {
        Manager.Add(thisCamera);
    }

    private void OnDisable()
    {
        Manager.Remove(thisCamera);
    }
}
