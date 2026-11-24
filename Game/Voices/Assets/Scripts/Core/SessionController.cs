using System.Collections;
using System.Collections.Generic;
using Ink.Runtime;
using Ink.UnityIntegration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Voices.Audio;
using Voices.Data;
using Voices.UI;

namespace Voices.Core
{
    // Plays one session (one day) from an Ink story (preferred) or from the older JSON format:
    // lines, events and Shift-voice questions.
    //
    // Ink conventions (see Assets/Resources/Ink/scene1.ink):
    //   "Name: text"       speaker + line; "playerthoughts:" / "description:" are narration
    //   "* [Voice: text]"  one answer per voice, cycled with Shift; "Silence:" is taken on timeout
    //   # event: fade_in | doctors_enter
    //   # input: name      after this line the player types a name; saved in the Ink variable player_name,
    //                      use it in text as {player_name}
    //   # time: 8          seconds for the next question
    //   # timer: off | on  questions from here on have no timer (intro day) / have one again
    //   # sfx: knock       plays sfx_knock; "sigh" picks one of the sfx_sigh_* sounds (see SoundPlayer)
    //   # sfx_timer: chair written above the answers: plays a few seconds after the voices open,
    //                      only while the timer is running
    //   # game_over        the game ends after this day (back to the menu) instead of going to the next day
    // The end of the story (-> END) is the end of the day: text goes away, gong, slow fade to black.
    public class SessionController : MonoBehaviour
    {
        // Days are played in order from Resources/Ink: scene1 (intro), then day1, day2, ... as long as a file exists.
        // Ink variables (player_name, reports, clues) are carried from one day to the next.
        static int dayIndex;
        static readonly Dictionary<string, object> carried = new();

        public static void NewGame()
        {
            dayIndex = 0;
            carried.Clear();
            SoundPlayer.NewGame();
        }

        static string DayResource(int index) => index == 0 ? "Ink/scene1" : "Ink/day" + index;

        [Header("Data (Ink wins over JSON; empty = Resources/Ink/scene1, then day1, day2, ...)")]
        public InkFile inkFile;
        public TextAsset sessionJson;
        public TextAsset voicesJson;

        [Header("Scene")]
        public VoiceOverlay overlay;
        public SubtitleBox subtitle;
        public ProceduralSfx sfx;
        public SoundPlayer sounds;
        public NameInput nameInput;
        public CanvasGroup blackScreen;
        public SpriteRenderer[] doctors;

        [System.Serializable]
        public class Portrait
        {
            [Tooltip("Speaker name as written in Ink, e.g. HeadDoctor. Spaces and capitals don't matter.")]
            public string speaker;
            public Sprite face;
        }

        [Header("Faces shown next to the text while that person speaks")]
        public Portrait[] portraits;

        [Header("Faces, named face_<voiceId> (empty = placeholder box)")]
        public Texture2D[] faces;

        [Header("Timing")]
        [Tooltip("Screen stays black this long before waking up")]
        public float blackHoldSeconds = 2f;
        [Tooltip("Length of the final, full eye opening")]
        public float fadeInSeconds = 3.5f;
        [Tooltip("Typing speed of thoughts and descriptions (lines without a speaker)")]
        public float thoughtCharsPerSecond = 16f;
        [Tooltip("Pause between a doctor's question and the voices opening")]
        public float questionPauseSeconds = 2f;
        public float dayEndPauseSeconds = 2.5f;
        public float dayEndFadeSeconds = 5f;
        [Tooltip("Loaded after the day has faded out (until more days exist)")]
        public string sceneAfterDay = "MainMenu";
        public float doctorFadeSeconds = 1f;
        public float defaultQuestionSeconds = 15f;

        bool shiftUsed;
        bool gameOver;
        readonly List<string> timerSounds = new();
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
            var ink = dayIndex == 0 && inkFile ? inkFile : Resources.Load<InkFile>(DayResource(dayIndex));
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
            foreach (var pair in carried)
                if (story.variablesState.GlobalVariableExistsWithName(pair.Key)) story.variablesState[pair.Key] = pair.Value;

            float nextQuestionSeconds = 0f;
            string pendingInput = null;
            bool questionShown = false;
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
                            case "sfx_timer": timerSounds.Add(value); break;
                            case "game_over": gameOver = true; break;
                            case "input": pendingInput = value; break;
                            case "scene": break; // author's note, nothing to show
                            default: Debug.Log($"Ink tag ignored: {tag}"); break;
                        }
                    }
                    if (raw.Length == 0) continue;

                    // "(Choice)" is the author's marker above the answers. The question is the line before it,
                    // which is already on screen.
                    bool marker = raw.Equals("(Choice)", System.StringComparison.OrdinalIgnoreCase);
                    if (marker && story.canContinue) continue;
                    questionShown = marker;
                    if (!marker) SplitSpeaker(raw, out lastSpeaker, out lastText);
                    // A line directly followed by choices is the question: show it, then the answers.
                    bool isQuestion = !story.canContinue && story.currentChoices.Count > 0;
                    if (marker && !isQuestion) continue;
                    // A day without "# event: fade_in" simply fades in before its first line.
                    if (blackScreen.alpha > 0.99f) yield return FadeBlack(1f, 0f, fadeInSeconds);
                    if (isQuestion)
                    {
                        if (pendingInput != null) yield return RunInput(story, pendingInput);
                        pendingInput = null;
                        break;
                    }
                    yield return RunLine(lastSpeaker, lastText, pendingInput == null);
                    if (pendingInput != null) yield return RunInput(story, pendingInput);
                    pendingInput = null;
                }

                if (story.currentChoices.Count == 0)
                {
                    yield return RunDayEnd(story);
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
                yield return RunQuestion(step, i => chosenIndex = i, questionShown);
                questionShown = false;

                if (chosenIndex < 0)
                {
                    // Timeout: take a "Silence:" answer if the author wrote one, otherwise the first one, uncounted.
                    chosenIndex = System.Array.FindIndex(answers, a => a.voice == "silence");
                    if (chosenIndex < 0) chosenIndex = 0;
                }
                story.ChooseChoiceIndex(chosenIndex);
            }
        }

        // The player types into the name field; the result goes into an Ink variable.
        IEnumerator RunInput(Story story, string what)
        {
            string variable = what == "name" ? "player_name" : what;
            if (!nameInput)
            {
                Debug.LogWarning("SessionController: no NameInput assigned, skipping '# input'.");
                yield break;
            }
            subtitle.SetHint(false);
            nameInput.Open();
            while (!nameInput.Done) yield return null;
            nameInput.Close();
            yield return null;
            if (story.variablesState.GlobalVariableExistsWithName(variable)) story.variablesState[variable] = nameInput.Value;
            else Debug.LogWarning($"Ink has no variable '{variable}'. Add 'VAR {variable} = \"\"' at the top of the file.");
        }

        IEnumerator RunDayEnd(Story story)
        {
            foreach (string name in story.variablesState)
            {
                object value = story.variablesState[name];
                if (value is int || value is float || value is string || value is bool) carried[name] = value;
            }

            subtitle.Hide();
            if (sounds)
            {
                sounds.StopRandomSounds();
                StartCoroutine(sounds.FadeOutMusic(dayEndPauseSeconds + dayEndFadeSeconds));
            }
            yield return new WaitForSeconds(dayEndPauseSeconds);
            if (sounds) sounds.PlayDayEnd();
            for (float t = 0f; t < dayEndFadeSeconds; t += Time.deltaTime)
            {
                blackScreen.alpha = Mathf.SmoothStep(0f, 1f, t / dayEndFadeSeconds);
                yield return null;
            }
            blackScreen.alpha = 1f;
            yield return new WaitForSeconds(3f);
            dayIndex++;
            if (!gameOver && Resources.Load<InkFile>(DayResource(dayIndex)))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
            else
            {
                NewGame();
                if (!string.IsNullOrEmpty(sceneAfterDay)) SceneManager.LoadScene(sceneAfterDay);
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
                    // Waking up: black for a moment, one heavy blink, then the eyes stay open.
                    blackScreen.alpha = 1f;
                    yield return new WaitForSeconds(blackHoldSeconds);
                    yield return FadeBlack(1f, 0.4f, 1.6f);
                    yield return FadeBlack(0.4f, 1f, 0.7f);
                    yield return new WaitForSeconds(0.5f);
                    yield return FadeBlack(1f, 0f, fadeInSeconds);
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

        IEnumerator FadeBlack(float from, float to, float seconds)
        {
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                blackScreen.alpha = Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds));
                yield return null;
            }
            blackScreen.alpha = to;
        }

        // Thoughts and descriptions (no speaker) type more slowly than spoken lines.
        // "Head Doctor", "HeadDoctor" and "HEADDOCTOR (whispers)" all find the same face.
        Sprite FaceFor(string speaker)
        {
            if (portraits == null || string.IsNullOrEmpty(speaker)) return null;
            string id = VoiceId(speaker);
            foreach (var p in portraits)
                if (p.face && !string.IsNullOrEmpty(p.speaker) && id.StartsWith(VoiceId(p.speaker))) return p.face;
            return null;
        }

        float SpeedFor(string speaker) => string.IsNullOrEmpty(speaker) ? thoughtCharsPerSecond : 0f;

        IEnumerator RunLine(string speaker, string text, bool waitForConfirm = true)
        {
            subtitle.Show();
            subtitle.Play(speaker, text, SpeedFor(speaker), FaceFor(speaker));
            yield return WaitTyping(subtitle.body);
            if (!waitForConfirm) yield break;
            subtitle.SetHint(true);
            yield return WaitConfirm();
            subtitle.SetHint(false);
        }

        IEnumerator RunQuestion(Step step, System.Action<int> onChosen = null, bool alreadyShown = false)
        {
            subtitle.Show();
            if (!alreadyShown)
            {
                subtitle.Play(step.speaker, step.text, SpeedFor(step.speaker), FaceFor(step.speaker));
                yield return WaitTyping(subtitle.body);
            }
            // Let the question sink in before the voices open.
            yield return new WaitForSeconds(questionPauseSeconds);

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
            Coroutine timerSfx = timed && sounds && timerSounds.Count > 0 ? StartCoroutine(PlayTimerSounds(timerSounds.ToArray())) : null;
            timerSounds.Clear();

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
            if (timerSfx != null) StopCoroutine(timerSfx);
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

        IEnumerator PlayTimerSounds(string[] names)
        {
            foreach (string name in names)
            {
                yield return new WaitForSeconds(Random.Range(2f, 4f));
                sounds.Play(name);
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
