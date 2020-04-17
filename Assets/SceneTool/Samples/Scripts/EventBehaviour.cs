using UnityEngine;
using UnityEngine.Events;

public class EventBehaviour : MonoBehaviour
{
    public UnityEvent OnAwake  = new UnityEvent();
    public UnityEvent OnStart  = new UnityEvent();
    public UnityEvent OnUpdate = new UnityEvent();
    public UnityEvent OnFixedUpdate = new UnityEvent();
    public UnityEvent OnDestroyed   = new UnityEvent();

    private void Awake()  => OnAwake.Invoke();
    private void Start()  => OnStart.Invoke();
    private void Update() => OnUpdate.Invoke();
    private void FixedUpdate() => OnFixedUpdate.Invoke();
    private void OnDestroy()   => OnDestroyed.Invoke();
}
