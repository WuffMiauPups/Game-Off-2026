using System.Collections;
using UnityEngine;

namespace Voices.Audio
{
    // Scales this AudioSource by the music or sounds volume from the options menu.
    // The volume set in the Inspector stays the mix level.
    [RequireComponent(typeof(AudioSource))]
    public class VolumeChannel : MonoBehaviour
    {
        public enum Channel { Music, Sounds }
        public Channel channel = Channel.Sounds;

        AudioSource source;
        float mixVolume;
        float fade = 1f;

        void Awake()
        {
            source = GetComponent<AudioSource>();
            mixVolume = source.volume;
            Apply();
            GameVolume.Changed += Apply;

            if (channel == Channel.Music && nextMusicFadeIn > 0f)
            {
                float seconds = nextMusicFadeIn;
                nextMusicFadeIn = 0f;
                FadeIn(seconds);
            }
        }

        static float nextMusicFadeIn;

        // The next music source that wakes up (in the scene being loaded) rises slowly instead of starting at full volume.
        public static void FadeInNextMusic(float seconds) => nextMusicFadeIn = seconds;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => nextMusicFadeIn = 0f;

        // Keeps playing across the scene change, fades to silence, then removes itself.
        public void FadeOutAndDestroy(float seconds)
        {
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);
            StopAllCoroutines();
            StartCoroutine(FadeOutRoutine(seconds));
        }

        IEnumerator FadeOutRoutine(float seconds)
        {
            float from = fade;
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                fade = Mathf.Lerp(from, 0f, t / seconds);
                Apply();
                yield return null;
            }
            Destroy(gameObject);
        }

        void OnDestroy() => GameVolume.Changed -= Apply;

        void Apply() => source.volume = mixVolume * fade * (channel == Channel.Music ? GameVolume.Music : GameVolume.Sounds);

        // Starts the source silent and lets it rise slowly to its normal volume.
        public void FadeIn(float seconds)
        {
            StopAllCoroutines();
            StartCoroutine(FadeRoutine(seconds));
        }

        IEnumerator FadeRoutine(float seconds)
        {
            fade = 0f;
            Apply();
            if (!source.isPlaying) source.Play();
            for (float t = 0f; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = t / seconds;
                fade = k * k;
                Apply();
                yield return null;
            }
            fade = 1f;
            Apply();
        }
    }
}
