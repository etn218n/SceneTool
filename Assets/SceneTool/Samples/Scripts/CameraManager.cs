using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraManager", menuName = "Managers/CameraManager")]
public class CameraManager : ScriptableObject
{
    [SerializeField]
    private List<Camera> Cameras = new List<Camera>();

    [SerializeField]
    private List<AudioListener> AudioListeners  = new List<AudioListener>();

    public Dictionary<Camera, AudioListener> CameraDict = new Dictionary<Camera, AudioListener>();

    public void Add(Camera camera)
    {
        Cameras.Add(camera);
        AudioListeners.Add(camera.GetComponent<AudioListener>());

        OnAdded();
    }

    public void Remove(Camera camera)
    {
        Cameras.Remove(camera);
        AudioListeners.Remove(camera.GetComponent<AudioListener>());

        OnRemoved();
    }

    private void OnAdded()
    {
        if (Cameras.Count > 1)
        {
            Cameras[Cameras.Count - 1].enabled = false;
            AudioListeners[Cameras.Count - 1].enabled = false;
        }
    }

    private void OnRemoved()
    {
        if (Cameras.Count > 0)
        {
            Cameras[0].enabled = true;
            AudioListeners[0].enabled = true;
        }
    }
}
