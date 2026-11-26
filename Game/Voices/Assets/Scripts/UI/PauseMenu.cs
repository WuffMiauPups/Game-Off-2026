using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Voices.UI
{
    // Escape pauses the game: time, timer and all sounds stop. From there: resume, or back to the main menu.
    // Nothing is saved; going to the main menu ends the run.
    public class PauseMenu : MonoBehaviour
    {
        // While true the game ignores Space, Enter, Shift and typing.
        public static bool IsPaused { get; private set; }

        public GameObject panel;
        public Button resumeButton;
        public Button menuButton;
        public string menuScene = "MainMenu";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => IsPaused = false;

        void Awake()
        {
            IsPaused = false;
            panel.SetActive(false);
            resumeButton.onClick.AddListener(Resume);
            menuButton.onClick.AddListener(QuitToMenu);
        }

        void Update()
        {
            var kb = Keyboard.current;
            if (kb == null || !kb.escapeKey.wasPressedThisFrame) return;
            if (IsPaused) Resume();
            else Pause();
        }

        void Pause()
        {
            IsPaused = true;
            Time.timeScale = 0f;
            AudioListener.pause = true;
            panel.SetActive(true);
            resumeButton.Select();
        }

        public void Resume()
        {
            if (!IsPaused || !panel.activeSelf) return;
            StartCoroutine(Close());
        }

        IEnumerator Close()
        {
            panel.SetActive(false);
            Time.timeScale = 1f;
            AudioListener.pause = false;
            // One frame later, so the key that pressed "Resume" does not also answer in the game.
            yield return null;
            IsPaused = false;
        }

        void QuitToMenu()
        {
            Unpause();
            Voices.Core.SessionController.NewGame();
            SceneManager.LoadScene(menuScene);
        }

        void OnDestroy() => Unpause();

        static void Unpause()
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            IsPaused = false;
        }
    }
}
