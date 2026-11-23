using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Voices.Audio;

namespace Voices.UI
{
    // Title screen: start the game, options (volume), quit.
    // The texts live in the MainMenu scene and can be edited in the Inspector.
    public class MainMenu : MonoBehaviour
    {
        public string firstScene = "Session";
        public Button startButton;
        public Button optionsButton;
        public Button quitButton;

        [Header("Options")]
        public GameObject mainPanel;
        public GameObject optionsPanel;
        public Slider musicSlider;
        public Slider soundsSlider;
        public Button backButton;

        void Awake()
        {
            startButton.onClick.AddListener(StartGame);
            quitButton.onClick.AddListener(Quit);
            optionsButton.onClick.AddListener(() => ShowOptions(true));
            backButton.onClick.AddListener(() => ShowOptions(false));

            musicSlider.SetValueWithoutNotify(GameVolume.Music);
            soundsSlider.SetValueWithoutNotify(GameVolume.Sounds);
            musicSlider.onValueChanged.AddListener(v => GameVolume.Music = v);
            soundsSlider.onValueChanged.AddListener(v => GameVolume.Sounds = v);
            ShowOptions(false);
        }

        void ShowOptions(bool show)
        {
            mainPanel.SetActive(!show);
            optionsPanel.SetActive(show);
            (show ? musicSlider.gameObject : startButton.gameObject).GetComponent<Selectable>().Select();
        }

        public void StartGame()
        {
            Voices.Core.SessionController.NewGame();
            SceneManager.LoadScene(firstScene);
        }

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
