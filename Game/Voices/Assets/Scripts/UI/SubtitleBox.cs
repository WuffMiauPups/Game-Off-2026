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
        [Tooltip("Face of whoever is speaking. Hidden when the speaker has no face.")]
        public Image portrait;
        [Tooltip("Box around the face, shown and hidden together with it")]
        public GameObject portraitFrame;
        [Tooltip("Face size: 1 pixel of the face image = this many UI units")]
        public float portraitScale = 4f;
        [Tooltip("How far the text moves right while a face is shown")]
        public float portraitIndent = 190f;

        float speakerLeft;
        float bodyLeft;

        void Awake()
        {
            speakerLeft = speaker.rectTransform.offsetMin.x;
            bodyLeft = body.target.rectTransform.offsetMin.x;
            SetPortrait(null);
        }

        void SetPortrait(Sprite face)
        {
            if (!portrait) return;
            portrait.sprite = face;
            portrait.enabled = face;
            if (face) portrait.rectTransform.sizeDelta = face.rect.size * portraitScale;
            if (portraitFrame) portraitFrame.SetActive(face);
            float indent = face ? portraitIndent : 0f;
            SetLeft(speaker.rectTransform, speakerLeft + indent);
            SetLeft(body.target.rectTransform, bodyLeft + indent);
        }

        static void SetLeft(RectTransform rt, float left)
        {
            var min = rt.offsetMin;
            min.x = left;
            rt.offsetMin = min;
        }

        public void Show() => group.alpha = 1f;

        public void Hide()
        {
            group.alpha = 0f;
            body.Stop();
        }

        public void Play(string who, string text, float charsPerSecond = 0f, Sprite face = null)
        {
            SetPortrait(face);
            speaker.text = who ?? "";
            speaker.enabled = !string.IsNullOrEmpty(who);
            body.Play(text, charsPerSecond);
            SetHint(false);
        }

        public void SetHint(bool on)
        {
            if (continueHint) continueHint.SetActive(on);
        }
    }
}
