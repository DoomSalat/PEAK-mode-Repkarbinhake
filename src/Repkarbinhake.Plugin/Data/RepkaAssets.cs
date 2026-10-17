using System.IO;
using UnityEngine;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class RepkaAssets
    {
        private const string AssetsFolder = "Assets";
        private const string ReticleFile = "carabiner_reticle.png";
        private const string HookIndicatorFile = "hook_indicator.png";
        private const string LockFile = "carabiner_lock.wav";
        private const string UnlockFile = "carabiner_unlock.wav";
        private const float CenterPivot = 0.5f;

        public RepkaAssets(string pluginFolder)
        {
            string folder = Path.Combine(pluginFolder, AssetsFolder);
            WavReader wav = new WavReader();
            ReticleSprite = LoadSprite(Path.Combine(folder, ReticleFile));
            HookIndicatorSprite = LoadSprite(Path.Combine(folder, HookIndicatorFile));
            LockClip = wav.Read(Path.Combine(folder, LockFile));
            UnlockClip = wav.Read(Path.Combine(folder, UnlockFile));
        }

        public Sprite ReticleSprite { get; }
        public Sprite HookIndicatorSprite { get; }
        public AudioClip LockClip { get; }
        public AudioClip UnlockClip { get; }

        private Sprite LoadSprite(string path)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = Path.GetFileNameWithoutExtension(path) };
            texture.LoadImage(File.ReadAllBytes(path));
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(CenterPivot, CenterPivot));
        }
    }
}
