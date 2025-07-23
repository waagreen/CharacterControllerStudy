using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class MovingBody : MonoBehaviour
{
    protected Rigidbody rb;

    private void FreezeMovement(OnEndLevel evt)
    {
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody>();
        EventsManager.AddSubscriber<OnEndLevel>(FreezeMovement);
    }

    protected virtual void OnDestroy()
    {
        EventsManager.RemoveSubscriber<OnEndLevel>(FreezeMovement);
    }
}
