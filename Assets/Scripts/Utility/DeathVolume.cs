using UnityEngine;

public class DeathVolume : MonoBehaviour
{
    [SerializeField] private LayerMask blackList = -1;

    private bool triggered = false;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if ((blackList & (1 << other.gameObject.layer)) == 0) return;

        triggered = true;
        Debug.Log("Player caught!");
        EventsManager.Broadcast(new OnPlayerCaught());
    }
}
