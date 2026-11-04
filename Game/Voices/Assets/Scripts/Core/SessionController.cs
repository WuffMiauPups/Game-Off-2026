using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Voices.Audio;
using Voices.Data;
using Voices.UI;

namespace Voices.Core
{
    // Plays one session (one day) from JSON: lines, events and Shift-voice questions.
    public class SessionController : MonoBehaviour
    {
        [Header("Data")]
        public TextAsset sessionJson;
        public TextAsset voicesJson;

        [Header("Scene")]
        public VoiceOverlay overlay;
        public SubtitleBox subtitle;
        public ProceduralSfx sfx;
        public CanvasGroup blackScreen;
        public SpriteRenderer[] doctors;

        [Header("Faces, named face_<voiceId> (empty = placeholder box)")]
        public Texture2D[] faces;

        [Header("Timing")]
        public float fadeInSeconds = 2.5f;
        public float doctorFadeSeconds = 1f;
        public float defaultQuestionSeconds = 15f;

        readonly Dictionary<string, string> voiceNames = new();
        readonly Dictionary<string, Texture2D> faceById = new();
        readonly Dictionary<string, int> timesChosen = new();

        public IReadOnlyDictionary<string, int> TimesChosen => timesChosen;

        IEnumerator Start()
        {
            var session = JsonUtility.FromJson<SessionData>(sessionJson.text);
            foreach (var v in JsonUtility.FromJson<VoiceList>(voicesJson.text).voices) voiceNames[v.id] = v.name;
            foreach (var f in faces)
                if (f) faceById[f.name.Replace("face_", "")] = f;

            subtitle.body.onChar = c => sfx.Blip(c, 0.7f);
            overlay.answer.onChar = c => sfx.Blip(c, 1f);

            overlay.Hide();
            subtitle.Hide();
            blackScreen.alpha = 1f;
            foreach (var d in doctors) SetAlpha(d, 0f);

            foreach (var step in session.steps)
            {
                switch (step.type)
                {
                    case "event": yield return RunEvent(step.eventName); break;
                    case "line": yield return RunLine(step.speaker, step.text); break;
                    case "question": yield return RunQuestion(step); break;
                    default: Debug.LogWarning($"Unknown step type '{step.type}'"); break;
                }
            }
        }

        IEnumerator RunEvent(string eventName)
        {
            switch (eventName)
            {
                case "fade_in":
                    for (float t = 0f; t < fadeInSeconds; t += Time.deltaTime)
                    {
                        blackScreen.alpha = 1f - t / fadeInSeconds;
                        yield return null;
                    }
                    blackScreen.alpha = 0f;
                    break;
                case "doctors_enter":
                    for (float t = 0f; t < doctorFadeSeconds; t += Time.deltaTime)
                    {
                        foreach (var d in doctors) SetAlpha(d, t / doctorFadeSeconds);
                        yield return null;
                    }
                    foreach (var d in doctors) SetAlpha(d, 1f);
                    break;
                default:
                    Debug.LogWarning($"Unknown event '{eventName}'");
                    break;
            }
        }

        IEnumerator RunLine(string speaker, string text)
        {
            subtitle.Show();
            subtitle.Play(speaker, text);
            yield return WaitTyping(subtitle.body);
            subtitle.SetHint(true);
            yield return WaitConfirm();
            subtitle.SetHint(false);
        }

        IEnumerator RunQuestion(Step step)
        {
            subtitle.Show();
            subtitle.Play(step.speaker, step.text);
            yield return WaitTyping(subtitle.body);

            var answers = step.answers;
            int index = 0;
            overlay.Show();
            ShowVoice(answers[index], false);

            float total = step.time > 0f ? step.time : defaultQuestionSeconds;
            float left = total;
            float nextTick = 0f;
            Answer chosen = null;

            while (left > 0f)
            {
                left -= Time.deltaTime;
                float fraction = Mathf.Clamp01(left / total);
                overlay.SetTimer(fraction);

                nextTick -= Time.deltaTime;
                if (nextTick <= 0f)
                {
                    sfx.Tick(fraction);
                    nextTick = Mathf.Lerp(0.22f, 1f, fraction);
                }

                if (ShiftPressed())
                {
                    index = (index + 1) % answers.Length;
                    ShowVoice(answers[index], true);
                }
                else if (ConfirmPressed())
                {
                    chosen = answers[index];
                    break;
                }
                yield return null;
            }
            yield return null;
            overlay.Hide();

            if (chosen != null)
            {
                timesChosen.TryGetValue(chosen.voice, out int count);
                timesChosen[chosen.voice] = count + 1;
                yield return RunLine(VoiceName(chosen.voice), chosen.text);
                if (!string.IsNullOrEmpty(chosen.reaction)) yield return RunLine("", chosen.reaction);
            }
            else
            {
                yield return RunLine("", "...");
                if (!string.IsNullOrEmpty(step.timeoutReaction)) yield return RunLine("", step.timeoutReaction);
            }
        }

        void ShowVoice(Answer answer, bool glitch)
        {
            faceById.TryGetValue(answer.voice, out var face);
            overlay.SetVoice(VoiceName(answer.voice), face, answer.text, glitch);
            if (glitch) sfx.Glitch();
        }

        string VoiceName(string id) => voiceNames.TryGetValue(id, out var n) ? n : id;

        static IEnumerator WaitTyping(TypewriterText typewriter)
        {
            while (!typewriter.IsDone)
            {
                if (ConfirmPressed()) typewriter.Complete();
                yield return null;
            }
            yield return null;
        }

        static IEnumerator WaitConfirm()
        {
            while (!ConfirmPressed()) yield return null;
            yield return null;
        }

        static bool ConfirmPressed()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.spaceKey.wasPressedThisFrame || kb.enterKey.wasPressedThisFrame || kb.numpadEnterKey.wasPressedThisFrame);
        }

        static bool ShiftPressed()
        {
            var kb = Keyboard.current;
            return kb != null && (kb.leftShiftKey.wasPressedThisFrame || kb.rightShiftKey.wasPressedThisFrame);
        }

        static void SetAlpha(SpriteRenderer sr, float a)
        {
            var c = sr.color;
            c.a = a;
            sr.color = c;
        }
    }
}
