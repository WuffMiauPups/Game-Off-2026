using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Voices.UI
{
    public class VoiceOverlay : MonoBehaviour
    {
        public CanvasGroup group;
        public RawImage face;
        public Text voiceName;
        public TypewriterText answer;
        public RectTransform timerFill;

        public Color timerNormal = new Color32(0x7a, 0x72, 0x72, 0xff);
        public Color timerDanger = new Color32(0x9e, 0x1b, 0x22, 0xff);
        [Range(0f, 1f)] public float dangerThreshold = 0.3f;
        public float glitchSeconds = 0.15f;
        public float glitchStep = 10f;

        Image timerImage;
        Vector2 faceHome;
        Color faceColor;
        Coroutine glitch;

        void Awake()
        {
            timerImage = timerFill.GetComponent<Image>();
            faceHome = face.rectTransform.anchoredPosition;
            faceColor = face.color;
        }

        public void Show() => group.alpha = 1f;

        public void Hide()
        {
            group.alpha = 0f;
            answer.Stop();
        }

        // faceTexture may be null while there is no art yet; the placeholder box stays visible.
        public void SetVoice(string displayName, Texture faceTexture, string text, bool withGlitch)
        {
            voiceName.text = displayName;
            face.texture = faceTexture;
            answer.Play(text);
            if (!withGlitch) return;
            if (glitch != null) StopCoroutine(glitch);
            glitch = StartCoroutine(Glitch());
        }

        public void SetTimer(float fraction)
        {
            timerFill.localScale = new Vector3(fraction, 1f, 1f);
            timerImage.color = fraction < dangerThreshold ? timerDanger : timerNormal;
        }

        IEnumerator Glitch()
        {
            var rt = face.rectTransform;
            var faded = faceColor;
            faded.a *= 0.4f;
            for (float t = 0f; t < glitchSeconds; t += Time.deltaTime)
            {
                rt.anchoredPosition = faceHome + new Vector2(Random.Range(-1, 2) * glitchStep, 0f);
                face.uvRect = new Rect(Random.Range(-0.04f, 0.04f), 0f, 1f, 1f);
                face.color = Random.value < 0.3f ? faded : faceColor;
                yield return null;
            }
            rt.anchoredPosition = faceHome;
            face.uvRect = new Rect(0f, 0f, 1f, 1f);
            face.color = faceColor;
            glitch = null;
        }
    }
}
