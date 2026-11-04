using UnityEngine;
using UnityEngine.UI;

namespace Voices.UI
{
    public class SubtitleBox : MonoBehaviour
    {
        public CanvasGroup group;
        public Text speaker;
        public TypewriterText body;
        public GameObject continueHint;

        public void Show() => group.alpha = 1f;

        public void Hide()
        {
            group.alpha = 0f;
            body.Stop();
        }

        public void Play(string who, string text)
        {
            speaker.text = who ?? "";
            speaker.enabled = !string.IsNullOrEmpty(who);
            body.Play(text);
            SetHint(false);
        }

        public void SetHint(bool on)
        {
            if (continueHint) continueHint.SetActive(on);
        }
    }
}
