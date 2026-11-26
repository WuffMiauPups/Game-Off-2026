using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Voices.Audio;

namespace Voices.UI
{
    // Title screen: start the game, options (volume, display), quit.
    // The texts live in the MainMenu scene and can be edited in the Inspector.
    public class MainMenu : MonoBehaviour
    {
        public string firstScene = "Session";
        public Button startButton;
        public Button optionsButton;
        public Button quitButton;

        [Header("Options (the panel has an OptionsPanel component)")]
        public GameObject mainPanel;
        public GameObject optionsPanel;

        [Header("Music")]
        [Tooltip("Fades out when the game starts, while the game's music fades in.")]
        public VolumeChannel menuMusic;
        public float musicCrossfadeSeconds = 4f;

        OptionsPanel options;

        void Awake()
        {
            startButton.onClick.AddListener(StartGame);
            quitButton.onClick.AddListener(Quit);
            optionsButton.onClick.AddListener(() => ShowOptions(true));
            options = optionsPanel.GetComponent<OptionsPanel>();
            options.onBack = () => ShowOptions(false);
            ShowOptions(false);
        }

        void ShowOptions(bool show)
        {
            mainPanel.SetActive(!show);
            optionsPanel.SetActive(show);
            (show ? options.musicSlider.gameObject : startButton.gameObject).GetComponent<Selectable>().Select();
        }

        public void StartGame()
        {
            if (menuMusic) menuMusic.FadeOutAndDestroy(musicCrossfadeSeconds);
            VolumeChannel.FadeInNextMusic(musicCrossfadeSeconds);
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
