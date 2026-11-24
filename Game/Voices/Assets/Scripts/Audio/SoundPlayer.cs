using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Voices.Audio
{
    // Plays the recorded sounds: the Shift sound, the heartbeat timer, the gong at the end of a day,
    // and anything Ink asks for with "# sfx: name". Every sound gets a slightly different pitch each time.
    public class SoundPlayer : MonoBehaviour
    {
        public AudioSource oneShots;
        public AudioSource heartbeat;
        [Tooltip("Fades out when the day ends. Empty = the child object 'Music'.")]
        public AudioSource music;
        public AudioClip shift;
        [Range(0f, 1f)] public float shiftVolume = 0.45f;
        public AudioClip dayEnd;

        [Header("Random background sounds (rare)")]
        [Tooltip("Played now and then on their own, never the same twice in a row")]
        public AudioClip[] randomSounds;
        [Tooltip("Like the random sounds, but each of these plays only one single time in the whole game")]
        public AudioClip[] onceSounds;
        [Tooltip("Shortest and longest wait between two random sounds, in seconds")]
        public float randomMinSeconds = 35f;
        public float randomMaxSeconds = 90f;
        [Range(0f, 1f)] public float randomVolume = 0.5f;

        [Header("Heartbeat timer")]
        [Tooltip("The heartbeat is slowed down at most this far so it covers the whole timer. 1 = never slowed.")]
        [Range(0.4f, 1f)] public float heartbeatMinPitch = 0.6f;

        [Tooltip("Sounds Ink can play with '# sfx: name'. 'knock' plays sfx_knock; 'sigh' picks one of the sfx_sigh_* sounds at random.")]
        public AudioClip[] library;

        [Tooltip("Each sound picks one of these pitches at random, never the same twice in a row. 1 = original.")]
        public float[] pitchSteps = { 0.88f, 0.94f, 1f, 1.06f, 1.12f };

        const int Voices = 6;
        readonly List<AudioSource> pool = new();
        int next;
        int lastStep = -1;
        bool heartbeatStarted;
        AudioClip lastRandom;
        Coroutine randomLoop;
        static readonly HashSet<string> oncePlayed = new();

        public static void NewGame() => oncePlayed.Clear();

        void Start() => randomLoop = StartCoroutine(RandomLoop());

        public void StopRandomSounds()
        {
            if (randomLoop != null) StopCoroutine(randomLoop);
            randomLoop = null;
        }

        IEnumerator RandomLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(randomMinSeconds, randomMaxSeconds));
                var choices = new List<AudioClip>();
                if (randomSounds != null)
                    foreach (var c in randomSounds) if (c && c != lastRandom) choices.Add(c);
                if (onceSounds != null)
                    foreach (var c in onceSounds) if (c && !oncePlayed.Contains(c.name)) choices.Add(c);
                if (choices.Count == 0) continue;
                lastRandom = choices[Random.Range(0, choices.Count)];
                oncePlayed.Add(lastRandom.name);
                PlayClip(lastRandom, randomVolume);
            }
        }

        void Awake()
        {
            if (!music)
            {
                var child = transform.Find("Music");
                if (child) music = child.GetComponent<AudioSource>();
            }
            // Separate sources so overlapping sounds can each have their own pitch.
            for (int i = 0; i < Voices; i++)
            {
                var src = oneShots.gameObject.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 0f;
                src.outputAudioMixerGroup = oneShots.outputAudioMixerGroup;
                pool.Add(src);
            }
        }

        void PlayClip(AudioClip clip, float volume = 1f)
        {
            if (!clip) return;
            var src = pool[next];
            next = (next + 1) % pool.Count;
            src.clip = clip;
            src.volume = oneShots.volume * volume;
            src.pitch = NextPitch();
            src.Play();
        }

        float NextPitch()
        {
            if (pitchSteps == null || pitchSteps.Length == 0) return 1f;
            int step = Random.Range(0, pitchSteps.Length);
            if (step == lastStep && pitchSteps.Length > 1) step = (step + 1 + Random.Range(0, pitchSteps.Length - 1)) % pitchSteps.Length;
            lastStep = step;
            return pitchSteps[step];
        }

        // Slowly fades the music out after the last line of the day.
        public IEnumerator FadeOutMusic(float seconds)
        {
            if (!music) yield break;
            float from = music.volume;
            for (float t = 0f; t < seconds; t += Time.deltaTime)
            {
                music.volume = Mathf.Lerp(from, 0f, t / seconds);
                yield return null;
            }
            music.volume = 0f;
        }

        public void PlayShift() => PlayClip(shift, shiftVolume);

        public void PlayDayEnd() => PlayClip(dayEnd);

        public void Play(string name)
        {
            string key = Strip(name.Trim().ToLowerInvariant());
            AudioClip exact = null;
            var variants = new List<AudioClip>();
            foreach (var clip in library)
            {
                if (!clip) continue;
                string n = Strip(clip.name.ToLowerInvariant());
                if (n == key) exact = clip;
                else if (n.StartsWith(key)) variants.Add(clip);
            }
            var pick = exact ? exact : variants.Count > 0 ? variants[Random.Range(0, variants.Count)] : null;
            if (pick) PlayClip(pick);
            else Debug.LogWarning($"Sound '{name}' not found. Add it to the Library list on the Audio object.");
        }

        static string Strip(string n) => n.StartsWith("sfx_") ? n.Substring(4) : n;

        public bool HasHeartbeat => heartbeat && heartbeat.clip;

        public void StopHeartbeat()
        {
            heartbeatStarted = false;
            if (heartbeat) heartbeat.Stop();
        }

        // The heartbeat starts with the timer and is slowed down so its last, fastest beats land on the end.
        // On a very long timer it waits until the slowest allowed speed fits; on a short one it skips ahead.
        public void UpdateHeartbeat(float secondsLeft)
        {
            if (heartbeatStarted || !HasHeartbeat || secondsLeft <= 0f) return;
            float length = heartbeat.clip.length;
            float pitch = length / secondsLeft;
            if (pitch < heartbeatMinPitch) return;
            heartbeat.pitch = Mathf.Min(pitch, 1f);
            heartbeat.Play();
            heartbeat.time = pitch > 1f ? Mathf.Clamp(length - secondsLeft, 0f, length - 0.05f) : 0f;
            heartbeatStarted = true;
        }
    }
}
