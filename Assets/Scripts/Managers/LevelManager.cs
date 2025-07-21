using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Character player;
    [SerializeField] private Chaser chaser;
    [SerializeField] private OrbitCamera mainCamera;
    [SerializeField] private Transform playerSpawnPoint, chaserSpawnPoint, cameraSpawnPoint;

    private void Awake()
    {
        EventsManager.AddSubscriber<OnPlayerCaught>(EndGame);
    }

    private void OnDestroy()
    {
        EventsManager.RemoveSubscriber<OnPlayerCaught>(EndGame);
    }

    private void Start()
    {
        Character p = Instantiate(player, playerSpawnPoint.position, Quaternion.identity);
        Chaser c = Instantiate(chaser, chaserSpawnPoint.position, Quaternion.identity);
        OrbitCamera cam = Instantiate(mainCamera, cameraSpawnPoint.position, cameraSpawnPoint.rotation);

        c.SetTarget(p.transform);
        cam.SetFocus(p.transform);
    }

    private void EndGame(OnPlayerCaught evt)
    {
        Debug.Log("END GAME");
        EventsManager.Broadcast(new OnEndLevel() { haveSucceded = false });
    }
}
