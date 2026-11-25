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

        [Header("Options")]
        public GameObject mainPanel;
        public GameObject optionsPanel;
        public Slider musicSlider;
        public Slider soundsSlider;
        public Button backButton;

        [Header("Music")]
        [Tooltip("Fades out when the game starts, while the game's music fades in.")]
        public VolumeChannel menuMusic;
        public float musicCrossfadeSeconds = 4f;

        // The three display rows are built at start as copies of the Back button and a slider label,
        // and the options are laid out here (positions in game pixels from the top of the 320 x 180 screen).
        [Header("Display options")]
        public string displayLabel = "Display";
        public string fullscreenText = "Fullscreen";
        public string windowedText = "Windowed";
        public string sizeLabel = "Size";
        public string fpsLabel = "Max FPS";
        public string vsyncText = "VSync";
        public string unlimitedText = "Unlimited";

        Button sizeButton;
        Text displayValue, sizeValue, fpsValue;

        void Awake()
        {
            BuildDisplayRows();

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

        void BuildDisplayRows()
        {
            var panel = optionsPanel.transform;
            var musicLabel = panel.Find("MusicSliderLabel");
            var soundsLabel = panel.Find("SoundsSliderLabel");
            var labelTemplate = musicLabel ? musicLabel.GetComponent<Text>() : null;
            if (!labelTemplate)
            {
                Debug.LogWarning("MainMenu: no 'MusicSliderLabel' in the options panel, display options are not shown.");
                return;
            }

            MoveTop(labelTemplate.rectTransform, 70f);
            MoveTop((RectTransform)musicSlider.transform, 70f);
            if (soundsLabel) MoveTop((RectTransform)soundsLabel, 82f);
            MoveTop((RectTransform)soundsSlider.transform, 82f);

            var displayButton = MakeRow(labelTemplate, displayLabel, 95f, out displayValue);
            sizeButton = MakeRow(labelTemplate, sizeLabel, 111f, out sizeValue);
            var fpsButton = MakeRow(labelTemplate, fpsLabel, 127f, out fpsValue);
            MoveTop((RectTransform)backButton.transform, 145f);

            displayButton.onClick.AddListener(() => { DisplaySettings.Fullscreen = !DisplaySettings.Fullscreen; RefreshDisplay(); });
            sizeButton.onClick.AddListener(() =>
            {
                int zoom = DisplaySettings.WindowZoom + 1;
                DisplaySettings.WindowZoom = zoom > DisplaySettings.MaxZoom ? DisplaySettings.MinZoom : zoom;
                RefreshDisplay();
            });
            fpsButton.onClick.AddListener(() =>
            {
                int i = System.Array.IndexOf(DisplaySettings.FpsChoices, DisplaySettings.FpsLimit);
                DisplaySettings.FpsLimit = DisplaySettings.FpsChoices[(i + 1) % DisplaySettings.FpsChoices.Length];
                RefreshDisplay();
            });
            RefreshDisplay();
        }

        // One row: a label on the left and a button that shows the current value and steps to the next one.
        Button MakeRow(Text labelTemplate, string label, float topPixel, out Text value)
        {
            var text = Instantiate(labelTemplate, optionsPanel.transform);
            text.name = label + "Label";
            text.text = label;
            MoveTop(text.rectTransform, topPixel + 3f);

            var button = Instantiate(backButton, optionsPanel.transform);
            button.name = label + "Button";
            button.onClick.RemoveAllListeners();
            var rt = (RectTransform)button.transform;
            rt.offsetMax = new Vector2(192f, UnitsFromTop(topPixel));
            rt.offsetMin = new Vector2(-168f, UnitsFromTop(topPixel + 14f));
            value = button.GetComponentInChildren<Text>(true);
            return button;
        }

        void RefreshDisplay()
        {
            if (!displayValue) return;
            displayValue.text = DisplaySettings.Fullscreen ? fullscreenText : windowedText;
            sizeValue.text = DisplaySettings.WindowSizeText(DisplaySettings.WindowZoom);
            sizeButton.interactable = !DisplaySettings.Fullscreen;
            int fps = DisplaySettings.FpsLimit;
            fpsValue.text = fps == 0 ? vsyncText : fps < 0 ? unlimitedText : fps.ToString();
        }

        // 6 UI units = 1 game pixel; the options are anchored to the middle of the 180 pixel high screen.
        static float UnitsFromTop(float pixel) => (90f - pixel) * 6f;

        static void MoveTop(RectTransform rt, float topPixel)
        {
            float height = rt.offsetMax.y - rt.offsetMin.y;
            float top = UnitsFromTop(topPixel);
            rt.offsetMax = new Vector2(rt.offsetMax.x, top);
            rt.offsetMin = new Vector2(rt.offsetMin.x, top - height);
        }

        void ShowOptions(bool show)
        {
            mainPanel.SetActive(!show);
            optionsPanel.SetActive(show);
            (show ? musicSlider.gameObject : startButton.gameObject).GetComponent<Selectable>().Select();
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
