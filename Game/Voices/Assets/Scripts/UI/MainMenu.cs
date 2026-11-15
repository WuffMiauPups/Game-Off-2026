using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Voices.UI
{
    // Title screen: start the game or quit. The texts live in the MainMenu scene and can be edited in the Inspector.
    public class MainMenu : MonoBehaviour
    {
        public string firstScene = "Session";
        public Button startButton;
        public Button quitButton;

        void Awake()
        {
            startButton.onClick.AddListener(StartGame);
            quitButton.onClick.AddListener(Quit);
        }

        public void StartGame() => SceneManager.LoadScene(firstScene);

        public void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
