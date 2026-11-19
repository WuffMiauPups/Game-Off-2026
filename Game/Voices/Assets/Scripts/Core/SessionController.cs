using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using Ink.UnityIntegration;
using UnityEngine;
using UnityEngine.InputSystem;
using Voices.Audio;
using Voices.Data;
using Voices.UI;

namespace Voices.Core
{
    // Plays one session (one day) from an Ink story (preferred) or from the older JSON format:
    // lines, events and Shift-voice questions.
    //
    // Ink conventions (see Assets/Resources/Ink/introscene.ink):
    //   "Name: text"       speaker + line; "playerthoughts:" / "description:" are narration
    //   "* [Voice: text]"  one answer per voice, cycled with Shift; "Silence:" is taken on timeout
    //   # event: fade_in | doctors_enter
    //   # input: name      player types a name (not built yet, logged)
    //   # time: 8          seconds for the next question
    //   # timer: off | on  questions from here on have no timer (intro day) / have one again
    //   # sfx: knock       plays sfx_knock; "sigh" picks one of the sfx_sigh_* sounds (see SoundPlayer)
    // The end of the story (-> END) is the end of the day and plays the gong.
    public class SessionController : MonoBehaviour
    {
        [Header("Data (Ink wins over JSON; empty = Resources/Ink/introscene)")]
        public InkFile inkFile;
        public TextAsset sessionJson;
        public TextAsset voicesJson;

        [Header("Scene")]
        public VoiceOverlay overlay;
        public SubtitleBox subtitle;
        public ProceduralSfx sfx;
        public SoundPlayer sounds;
        public CanvasGroup blackScreen;
        public SpriteRenderer[] doctors;

        [Header("Faces, named face_<voiceId> (empty = placeholder box)")]
        public Texture2D[] faces;

        [Header("Timing")]
        public float fadeInSeconds = 2.5f;
        public float doctorFadeSeconds = 1f;
        public float defaultQuestionSeconds = 15f;

        bool shiftUsed;
        bool timerOn = true;
        readonly Dictionary<string, string> voiceNames = new();
        readonly Dictionary<string, Texture2D> faceById = new();
        readonly Dictionary<string, int> timesChosen = new();

        public IReadOnlyDictionary<string, int> TimesChosen => timesChosen;

        IEnumerator Start()
        {
            if (voicesJson)
                foreach (var v in JsonUtility.FromJson<VoiceList>(voicesJson.text).voices) voiceNames[v.id] = v.name;
            foreach (var f in faces)
                if (f) faceById[f.name.Replace("face_", "")] = f;

            subtitle.body.onChar = c => sfx.Blip(c, 0.7f);
            overlay.answer.onChar = c => sfx.Blip(c, 1f);

            overlay.Hide();
            subtitle.Hide();
            blackScreen.alpha = 1f;
            foreach (var d in doctors) SetAlpha(d, 0f);

            // Ink integration 2.x imports .ink as an InkFile that holds the compiled JSON (no separate .json file).
            var ink = inkFile ? inkFile : Resources.Load<InkFile>("Ink/introscene");
            if (ink && ink.isCompiled)
            {
                yield return RunInk(ink.storyJson);
                yield break;
            }
            if (ink) Debug.LogError($"SessionController: '{ink.name}' is not compiled, check the Ink file for errors. Falling back to JSON.");

            if (!sessionJson)
            {
                Debug.LogError("SessionController: no Ink story and no session JSON assigned.");
                yield break;
            }
            var session = JsonUtility.FromJson<SessionData>(sessionJson.text);
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


        // ---------- Ink ----------

        IEnumerator RunInk(string json)
        {
            var story = new Story(json);
            story.onError += (msg, type) => Debug.LogError($"Ink {type}: {msg}");

            float nextQuestionSeconds = 0f;
            string lastSpeaker = "";
            string lastText = "";

            while (true)
            {
                while (story.canContinue)
                {
                    string raw = story.Continue().Trim();
                    foreach (var tag in story.currentTags)
                    {
                        var (key, value) = SplitTag(tag);
                        switch (key)
                        {
                            case "event": yield return RunEvent(value); break;
                            case "time": float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out nextQuestionSeconds); break;
                            case "timer": timerOn = !value.Equals("off", System.StringComparison.OrdinalIgnoreCase); break;
                            case "sfx": if (sounds) sounds.Play(value); break;
                            case "input": Debug.LogWarning($"Ink tag '# input: {value}' is not implemented yet, skipping."); break;
                            case "scene": break; // author's note, nothing to show
                            default: Debug.Log($"Ink tag ignored: {tag}"); break;
                        }
                    }
                    if (raw.Length == 0) continue;

                    SplitSpeaker(raw, out lastSpeaker, out lastText);
                    // A line directly followed by choices is the question: show it, then the answers.
                    bool isQuestion = !story.canContinue && story.currentChoices.Count > 0;
                    if (isQuestion) break;
                    yield return RunLine(lastSpeaker, lastText);
                }

                if (story.currentChoices.Count == 0)
                {
                    if (sounds) sounds.PlayDayEnd();
                    yield break;
                }

                var answers = new Answer[story.currentChoices.Count];
                for (int i = 0; i < answers.Length; i++)
                {
                    SplitSpeaker(story.currentChoices[i].text, out var voice, out var text);
                    answers[i] = new Answer { voice = VoiceId(voice), text = text };
                }

                var step = new Step { speaker = lastSpeaker, text = lastText, answers = answers, time = nextQuestionSeconds };
                nextQuestionSeconds = 0f;
                int chosenIndex = -1;
                yield return RunQuestion(step, i => chosenIndex = i);

                if (chosenIndex < 0)
                {
                    // Timeout: take a "Silence:" answer if the author wrote one, otherwise the first one, uncounted.
                    chosenIndex = System.Array.FindIndex(answers, a => a.voice == "silence");
                    if (chosenIndex < 0) chosenIndex = 0;
                }
                story.ChooseChoiceIndex(chosenIndex);
            }
        }

        static (string key, string value) SplitTag(string tag)
        {
            int colon = tag.IndexOf(':');
            if (colon < 0) return (tag.Trim().ToLowerInvariant(), "");
            return (tag.Substring(0, colon).Trim().ToLowerInvariant(), tag.Substring(colon + 1).Trim());
        }

        // "Doctor 1: Hello" -> ("DOCTOR 1", "Hello"); narration prefixes and plain lines -> ("", text).
        static void SplitSpeaker(string line, out string speaker, out string text)
        {
            int colon = line.IndexOf(':');
            if (colon > 0 && colon <= 40)
            {
                string who = line.Substring(0, colon).Trim();
                string lower = who.ToLowerInvariant();
                if (lower == "playerthoughts" || lower == "description" || lower == "narrator")
                {
                    speaker = "";
                    text = line.Substring(colon + 1).Trim();
                    return;
                }
                speaker = who.ToUpperInvariant();
                text = line.Substring(colon + 1).Trim();
                return;
            }
            speaker = "";
            text = line;
        }

        // "Yes-Man" -> "yesman", "Paranoid" -> "paranoid"
        static string VoiceId(string label)
        {
            var sb = new System.Text.StringBuilder();
            foreach (char c in label.ToLowerInvariant())
                if (char.IsLetterOrDigit(c)) sb.Append(c);
            return sb.Length > 0 ? sb.ToString() : "unknown";
        }

        // ---------- JSON steps ----------

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

        IEnumerator RunQuestion(Step step, System.Action<int> onChosen = null)
        {
            subtitle.Show();
            subtitle.Play(step.speaker, step.text);
            yield return WaitTyping(subtitle.body);

            var answers = step.answers;
            int index = 0;
            overlay.Show();
            ShowVoice(answers[index], false);

            // Without a timer (intro day) the question waits until the player answers.
            bool timed = timerOn;
            overlay.SetTimerVisible(timed);
            float total = step.time > 0f ? step.time : defaultQuestionSeconds;
            float left = total;
            float nextTick = 0f;
            Answer chosen = null;
            int chosenIndex = -1;

            while (!timed || left > 0f)
            {
                if (timed)
                {
                    left -= Time.deltaTime;
                    float fraction = Mathf.Clamp01(left / total);
                    overlay.SetTimer(fraction);

                    if (sounds && sounds.HasHeartbeat)
                    {
                        sounds.UpdateHeartbeat(left);
                    }
                    else
                    {
                        nextTick -= Time.deltaTime;
                        if (nextTick <= 0f)
                        {
                            sfx.Tick(fraction);
                            nextTick = Mathf.Lerp(0.22f, 1f, fraction);
                        }
                    }
                }

                if (ShiftPressed())
                {
                    shiftUsed = true;
                    index = (index + 1) % answers.Length;
                    ShowVoice(answers[index], true);
                }
                else if (ConfirmPressed())
                {
                    chosen = answers[index];
                    chosenIndex = index;
                    break;
                }
                // On screen from the moment the voices open, until the player has shifted once.
                overlay.SetShiftHint(!shiftUsed);
                yield return null;
            }
            if (sounds) sounds.StopHeartbeat();
            yield return null;
            overlay.Hide();

            onChosen?.Invoke(chosenIndex);

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
            if (!glitch) return;
            if (sounds && sounds.shift) sounds.PlayShift();
            else sfx.Glitch();
        }

        string VoiceName(string id) => voiceNames.TryGetValue(id, out var n) ? n : id.ToUpperInvariant();

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
