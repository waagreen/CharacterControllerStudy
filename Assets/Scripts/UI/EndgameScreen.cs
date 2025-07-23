using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;

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

    public void ResetLevel()
    {
        // Reload current scene
        EventsManager.ClearAll();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#endif
#if UNITY_STANDALONE
        Application.Quit();
#endif
    }
}
