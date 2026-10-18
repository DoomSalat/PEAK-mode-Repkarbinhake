using Repkarbinhake.Plugin.Data;
using UnityEngine;

namespace Repkarbinhake.Plugin.Visual
{
    public sealed class CarabinerSounds
    {
        private const float SoundRange = 40f;
        private const float PitchVariation = 0.08f;
        private const float HookPitch = 0.88f;
        private const float HookVolume = 0.6f;

        private readonly SFX_Instance _lock;
        private readonly SFX_Instance _unlock;
        private readonly SFX_Instance _hookLock;
        private readonly SFX_Instance _hookUnlock;
        private readonly RepkaConfig _config;

        public CarabinerSounds(RepkaAssets assets, RepkaConfig config)
        {
            _config = config;
            _lock = Create(assets.LockClip, 1f);
            _unlock = Create(assets.UnlockClip, 1f);
            _hookLock = Create(assets.LockClip, HookPitch);
            _hookUnlock = Create(assets.UnlockClip, HookPitch);
        }

        public void PlayLock(Vector3 position)
        {
            Play(_lock, position);
        }

        public void PlayUnlock(Vector3 position)
        {
            Play(_unlock, position);
        }

        public void PlayHookLock(Vector3 position)
        {
            Play(_hookLock, position, HookVolume);
        }

        public void PlayHookUnlock(Vector3 position)
        {
            Play(_hookUnlock, position, HookVolume);
        }

        private void Play(SFX_Instance sound, Vector3 position, float volumeScale = 1f)
        {
            if (SFX_Player.instance == null)
                return;

            sound.settings.volume = _config.SoundVolume.Value * volumeScale;
            SFX_Player.instance.PlaySFX(sound, position);
        }

        private SFX_Instance Create(AudioClip clip, float pitch)
        {
            SFX_Instance sound = ScriptableObject.CreateInstance<SFX_Instance>();
            sound.name = clip.name;
            sound.clips = new[] { clip };
            sound.settings = new SFX_Settings
            {
                volume = _config.SoundVolume.Value,
                volume_Variation = 0f,
                pitch = pitch,
                pitch_Variation = PitchVariation,
                range = SoundRange,
            };
            Object.DontDestroyOnLoad(sound);
            return sound;
        }
    }
}
