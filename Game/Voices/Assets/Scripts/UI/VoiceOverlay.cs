using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Voices.UI
{
    public class VoiceOverlay : MonoBehaviour
    {
        public CanvasGroup group;
        public RawImage face;
        [Tooltip("Placeholder text inside the face box, hidden once a voice has a real face")]
        public GameObject faceLabel;
        public Text voiceName;
        public TypewriterText answer;
        public Text shiftHint;
        public RectTransform timerFill;

        public Color timerNormal = new Color32(0x7a, 0x72, 0x72, 0xff);
        public Color timerDanger = new Color32(0x9e, 0x1b, 0x22, 0xff);
        [Range(0f, 1f)] public float dangerThreshold = 0.3f;
        public float glitchSeconds = 0.15f;
        public float glitchStep = 12f;

        Image timerImage;
        Vector2 faceHome;
        Color faceColor;
        Color placeholderColor;
        Rect faceUv = new Rect(0f, 0f, 1f, 1f);
        Coroutine glitch;

        void Awake()
        {
            timerImage = timerFill.GetComponent<Image>();
            faceHome = face.rectTransform.anchoredPosition;
            faceColor = placeholderColor = face.color;
        }

        public void Show() => group.alpha = 1f;

        public void Hide()
        {
            group.alpha = 0f;
            answer.Stop();
            SetShiftHint(false);
        }

        public void SetShiftHint(bool visible) => shiftHint.enabled = visible;

        public void SetTimerVisible(bool visible) => timerFill.parent.gameObject.SetActive(visible);

        // faceSprite may be null while a voice has no art yet; the grey placeholder box stays visible.
        public void SetVoice(string displayName, Sprite faceSprite, string text, bool withGlitch)
        {
            voiceName.text = displayName;
            if (glitch != null) StopCoroutine(glitch);
            glitch = null;
            face.rectTransform.anchoredPosition = faceHome;
            face.texture = faceSprite ? faceSprite.texture : null;
            // The sprite is only a part of its texture, so show just that part.
            faceUv = new Rect(0f, 0f, 1f, 1f);
            if (faceSprite)
            {
                var r = faceSprite.textureRect;
                faceUv = new Rect(r.x / faceSprite.texture.width, r.y / faceSprite.texture.height, r.width / faceSprite.texture.width, r.height / faceSprite.texture.height);
            }
            face.uvRect = faceUv;
            faceColor = faceSprite ? Color.white : placeholderColor;
            face.color = faceColor;
            if (faceLabel) faceLabel.SetActive(!faceSprite);
            answer.Play(text);
            if (!withGlitch) return;
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
                face.uvRect = new Rect(faceUv.x + Random.Range(-0.04f, 0.04f) * faceUv.width, faceUv.y, faceUv.width, faceUv.height);
                face.color = Random.value < 0.3f ? faded : faceColor;
                yield return null;
            }
            rt.anchoredPosition = faceHome;
            face.uvRect = faceUv;
            face.color = faceColor;
            glitch = null;
        }
    }
}
