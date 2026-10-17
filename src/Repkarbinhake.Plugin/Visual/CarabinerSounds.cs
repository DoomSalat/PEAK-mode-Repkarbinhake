using Repkarbinhake.Plugin.Data;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class CarabinerSounds
    {
        private const float SoundRange = 40f;
        private const float PitchVariation = 0.08f;

        private readonly SFX_Instance _lock;
        private readonly SFX_Instance _unlock;
        private readonly RepkaConfig _config;

        public CarabinerSounds(RepkaAssets assets, RepkaConfig config)
        {
            _config = config;
            _lock = Create(assets.LockClip);
            _unlock = Create(assets.UnlockClip);
        }

        public void PlayLock(Vector3 position)
        {
            Play(_lock, position);
        }

        public void PlayUnlock(Vector3 position)
        {
            Play(_unlock, position);
        }

        private void Play(SFX_Instance sound, Vector3 position)
        {
            if (SFX_Player.instance == null)
                return;

            sound.settings.volume = _config.SoundVolume.Value;
            SFX_Player.instance.PlaySFX(sound, position);
        }

        private SFX_Instance Create(AudioClip clip)
        {
            SFX_Instance sound = ScriptableObject.CreateInstance<SFX_Instance>();
            sound.name = clip.name;
            sound.clips = new[] { clip };
            sound.settings = new SFX_Settings
            {
                volume = _config.SoundVolume.Value,
                volume_Variation = 0f,
                pitch_Variation = PitchVariation,
                range = SoundRange,
            };
            Object.DontDestroyOnLoad(sound);
            return sound;
        }
    }
}
