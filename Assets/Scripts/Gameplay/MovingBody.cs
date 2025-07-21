using UnityEngine;

public class MovingBody : MonoBehaviour
{
    protected bool canMove = true;

    private void FreezeMovement(OnEndLevel evt)
    {
        canMove = false;
    }

    protected virtual void Awake()
    {
        EventsManager.AddSubscriber<OnEndLevel>(FreezeMovement);
    }

    protected virtual void OnDestroy()
    {
        EventsManager.RemoveSubscriber<OnEndLevel>(FreezeMovement);
    }
}
