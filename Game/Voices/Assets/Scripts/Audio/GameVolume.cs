using System;
using UnityEngine;

namespace Voices.Audio
{
    // Volume set in the options menu, remembered between starts.
    public static class GameVolume
    {
        static float music = -1f;
        static float sounds = -1f;

        public static event Action Changed;

        public static float Music
        {
            get => music < 0f ? music = PlayerPrefs.GetFloat("volume_music", 1f) : music;
            set { music = Mathf.Clamp01(value); PlayerPrefs.SetFloat("volume_music", music); Changed?.Invoke(); }
        }

        public static float Sounds
        {
            get => sounds < 0f ? sounds = PlayerPrefs.GetFloat("volume_sounds", 1f) : sounds;
            set { sounds = Mathf.Clamp01(value); PlayerPrefs.SetFloat("volume_sounds", sounds); Changed?.Invoke(); }
        }
    }
}
