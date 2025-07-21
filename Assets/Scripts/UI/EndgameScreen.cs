using UnityEngine;

public class EndgameScreen : MonoBehaviour
{
    [SerializeField] private GameObject content;

    private void Awake()
    {
        EventsManager.AddSubscriber<OnEndLevel>(DisplayScreen);
    }

    private void DisplayScreen(OnEndLevel evt)
    {
        // TODO: Set differences based on evt.haveSucceded
        content.SetActive(true);
    }
}
