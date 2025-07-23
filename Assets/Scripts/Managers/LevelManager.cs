using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private Character playerPrefab;
    [SerializeField] private Chaser chaserPrefab;
    [SerializeField] private OrbitCamera cameraPrefab;
    [SerializeField] private Transform playerSpawnPoint, chaserSpawnPoint, cameraSpawnPoint;
    private InputManager input;

    private void Awake()
    {
        input = gameObject.AddComponent<InputManager>();
        EventsManager.AddSubscriber<OnPlayerCaught>(EndGame);
    }

    private void OnDestroy()
    {
        EventsManager.RemoveSubscriber<OnPlayerCaught>(EndGame);
    }

    private void Start()
    {
        Character player = Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        Chaser chaser = Instantiate(chaserPrefab, chaserSpawnPoint.position, Quaternion.identity);
        OrbitCamera cam = Instantiate(cameraPrefab, cameraSpawnPoint.position, cameraSpawnPoint.rotation);

        chaser.SetTarget(player.transform);
        cam.SetFocus(player.transform, input);
        player.Setup(input, cam.transform);
    }

    private void EndGame(OnPlayerCaught evt)
    {
        Debug.Log("END GAME");
        EventsManager.Broadcast(new OnEndLevel() { haveSucceded = false });
    }
}
