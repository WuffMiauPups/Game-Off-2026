using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Voices.Audio;

namespace Voices.UI
{
    // Black screen with the content warning, shown once per launch before the main menu.
    // It stays for a few seconds, fades out by itself, then the menu music fades in.
    // The text lives in the MainMenu scene.
    [RequireComponent(typeof(CanvasGroup))]
    public class ContentWarningScreen : MonoBehaviour
    {
        public float showSeconds = 3f;
        public float fadeSeconds = 1f;
        public Selectable selectAfter;

        [Header("Menu music")]
        public VolumeChannel music;
        public float musicFadeSeconds = 4f;

        static bool shown;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetShown() => shown = false;

        IEnumerator Start()
        {
            if (shown) { Finish(); yield break; }

            var group = GetComponent<CanvasGroup>();
            group.alpha = 1f;
            yield return new WaitForSecondsRealtime(showSeconds);

            for (float t = 0f; t < fadeSeconds; t += Time.unscaledDeltaTime)
            {
                group.alpha = 1f - t / fadeSeconds;
                yield return null;
            }

            shown = true;
            Finish();
        }

        void Update()
        {
            // Keep the menu behind from reacting to keys while the warning is up.
            var es = EventSystem.current;
            if (es && es.currentSelectedGameObject) es.SetSelectedGameObject(null);
        }

        void Finish()
        {
            if (music) music.FadeIn(musicFadeSeconds);
            if (selectAfter) selectAfter.Select();
            gameObject.SetActive(false);
        }
    }
}
