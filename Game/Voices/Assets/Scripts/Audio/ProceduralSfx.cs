using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Voices.Audio
{
    // Placeholder sounds generated in code until real audio exists.
    public class ProceduralSfx : MonoBehaviour
    {
        [Range(0f, 1f)] public float tickVolume = 0.5f;
        [Range(0f, 1f)] public float blipVolume = 0.25f;
        [Range(0f, 1f)] public float glitchVolume = 0.4f;

        const int Rate = 44100;

        AudioSource timerSource;
        AudioSource voiceSource;
        AudioClip tick;
        AudioClip blip;
        AudioClip glitch;

        void Awake()
        {
            timerSource = gameObject.AddComponent<AudioSource>();
            voiceSource = gameObject.AddComponent<AudioSource>();
            timerSource.playOnAwake = false;
            voiceSource.playOnAwake = false;

            var rng = new System.Random(7);
            tick = Make("tick", 0.035f, (t, i) => Mathf.Sin(2f * Mathf.PI * 1600f * t) * Mathf.Exp(-t * 160f));

            float lowpass = 0f;
            blip = Make("blip", 0.06f, (t, i) =>
            {
                float noise = (float)rng.NextDouble() * 2f - 1f;
                lowpass += (noise - lowpass) * 0.08f;
                return lowpass * 3f * Mathf.Sin(Mathf.PI * t / 0.06f);
            });

            glitch = Make("glitch", 0.14f, (t, i) =>
            {
                float crushed = Mathf.Round(((float)rng.NextDouble() * 2f - 1f) * 3f) / 3f;
                return (i / 400) % 2 == 0 ? crushed * 0.6f : 0f;
            });
        }

        static AudioClip Make(string name, float seconds, Func<float, int, float> sample)
        {
            int count = (int)(Rate * seconds);
            var data = new float[count];
            for (int i = 0; i < count; i++) data[i] = Mathf.Clamp(sample((float)i / Rate, i), -1f, 1f);
            var clip = AudioClip.Create(name, count, 1, Rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        public void Tick(float timeLeftFraction)
        {
            timerSource.pitch = Mathf.Lerp(1.5f, 1f, timeLeftFraction);
            timerSource.PlayOneShot(tick, tickVolume * Mathf.Lerp(1f, 0.5f, timeLeftFraction));
        }

        public void Blip(char c, float pitch)
        {
            if (char.IsWhiteSpace(c)) return;
            voiceSource.pitch = pitch * Random.Range(0.85f, 1.15f);
            voiceSource.PlayOneShot(blip, blipVolume);
        }

        public void Glitch()
        {
            voiceSource.pitch = 1f;
            voiceSource.PlayOneShot(glitch, glitchVolume);
        }
    }
}
