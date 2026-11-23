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

        void Awake()
        {
            source = GetComponent<AudioSource>();
            mixVolume = source.volume;
            Apply();
            GameVolume.Changed += Apply;
        }

        void OnDestroy() => GameVolume.Changed -= Apply;

        void Apply() => source.volume = mixVolume * (channel == Channel.Music ? GameVolume.Music : GameVolume.Sounds);
    }
}
