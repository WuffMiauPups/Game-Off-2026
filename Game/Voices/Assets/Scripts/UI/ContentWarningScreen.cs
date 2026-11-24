using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Voices.UI
{
    // Black screen with the content warning, shown once per launch before the main menu.
    // Any key or a mouse click continues. The text lives in the MainMenu scene.
    public class ContentWarningScreen : MonoBehaviour
    {
        public Selectable selectAfter;
        public float minSeconds = 1f;

        static bool shown;
        float elapsed;
        bool closing;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetShown() => shown = false;

        void OnEnable()
        {
            if (shown) gameObject.SetActive(false);
        }

        void Update()
        {
            if (closing) return;

            // Keep the menu behind from reacting to the key that closes this screen.
            var es = EventSystem.current;
            if (es && es.currentSelectedGameObject) es.SetSelectedGameObject(null);

            elapsed += Time.unscaledDeltaTime;
            if (elapsed < minSeconds) return;

            var kb = Keyboard.current;
            var mouse = Mouse.current;
            bool pressed = (kb != null && kb.anyKey.wasPressedThisFrame)
                           || (mouse != null && mouse.leftButton.wasPressedThisFrame);
            if (pressed) StartCoroutine(Close());
        }

        IEnumerator Close()
        {
            closing = true;
            shown = true;
            yield return null;
            if (selectAfter) selectAfter.Select();
            gameObject.SetActive(false);
        }
    }
}
