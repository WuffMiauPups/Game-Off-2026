using System.Collections.Generic;
using UnityEngine;

namespace Voices.Audio
{
    // Plays the recorded sounds: the Shift sound, the heartbeat timer, the gong at the end of a day,
    // and anything Ink asks for with "# sfx: name".
    public class SoundPlayer : MonoBehaviour
    {
        public AudioSource oneShots;
        public AudioSource heartbeat;
        public AudioClip shift;
        public AudioClip dayEnd;

        [Tooltip("Sounds Ink can play with '# sfx: name'. 'knock' plays sfx_knock; 'sigh' picks one of the sfx_sigh_* sounds at random.")]
        public AudioClip[] library;

        bool heartbeatStarted;

        public void PlayShift() => oneShots.PlayOneShot(shift);

        public void PlayDayEnd()
        {
            if (dayEnd) oneShots.PlayOneShot(dayEnd);
        }

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
            if (pick) oneShots.PlayOneShot(pick);
            else Debug.LogWarning($"Sound '{name}' not found. Add it to the Library list on the Audio object.");
        }

        static string Strip(string n) => n.StartsWith("sfx_") ? n.Substring(4) : n;

        public bool HasHeartbeat => heartbeat && heartbeat.clip;

        public void StopHeartbeat()
        {
            heartbeatStarted = false;
            if (heartbeat) heartbeat.Stop();
        }

        // Starts the heartbeat late enough that its last, fastest beats land on the end of the timer.
        public void UpdateHeartbeat(float secondsLeft)
        {
            if (heartbeatStarted || !HasHeartbeat) return;
            float length = heartbeat.clip.length;
            if (secondsLeft > length) return;
            heartbeat.Play();
            heartbeat.time = Mathf.Clamp(length - secondsLeft, 0f, length - 0.05f);
            heartbeatStarted = true;
        }
    }
}
