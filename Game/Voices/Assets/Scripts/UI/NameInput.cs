using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Voices.UI
{
    // The player types a name; Enter confirms. Opened by "# input: name" in Ink.
    public class NameInput : MonoBehaviour
    {
        public CanvasGroup group;
        public Text field;
        public int maxLength = 16;

        string typed = "";
        bool open;

        public bool Done { get; private set; }
        public string Value => typed.Trim();

        void Awake() => group.alpha = 0f;

        public void Open()
        {
            typed = "";
            Done = false;
            open = true;
            group.alpha = 1f;
            if (Keyboard.current != null) Keyboard.current.onTextInput += OnChar;
        }

        public void Close()
        {
            open = false;
            group.alpha = 0f;
            if (Keyboard.current != null) Keyboard.current.onTextInput -= OnChar;
        }

        void OnDisable()
        {
            if (open) Close();
        }

        void OnChar(char c)
        {
            if (!open || Done || typed.Length >= maxLength) return;
            if (char.IsLetterOrDigit(c) || c == '-' || c == '\'' || (c == ' ' && typed.Length > 0)) typed += c;
        }

        void Update()
        {
            if (!open) return;
            var kb = Keyboard.current;
            if (kb != null && !Done)
            {
                if (kb.backspaceKey.wasPressedThisFrame && typed.Length > 0) typed = typed.Substring(0, typed.Length - 1);
                if ((kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame) && Value.Length > 0) Done = true;
            }
            bool cursor = Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f;
            field.text = typed + (cursor ? "_" : " ");
        }
    }
}
