using System.IO;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Repkarbinhake.Plugin.Data
{
    public sealed class RepkaAssets
    {
        private const string ResourcePrefix = "Repkarbinhake.Assets.";
        private const string ReticleFile = "carabiner_reticle.png";
        private const string HookIndicatorFile = "hook_indicator.png";
        private const string HookRetractFile = "hook_retract.png";
        private const string LockFile = "carabiner_lock.wav";
        private const string UnlockFile = "carabiner_unlock.wav";
        private const string CarabinerFile = "carabiner.obj";
        private const string BodyMaterial = "Carabiner_Metal";
        private const string GateMaterial = "Carabiner_Gate";
        private const string SleeveMaterial = "Carabiner_Sleeve";
        private const string BoltMaterial = "Carabiner_Bolt";
        private const float CenterPivot = 0.5f;

        private readonly Assembly _assembly;

        public RepkaAssets(Assembly assembly)
        {
            _assembly = assembly;
            WavReader wav = new WavReader();
            ReticleSprite = LoadSprite(ReticleFile);
            HookIndicatorSprite = LoadSprite(HookIndicatorFile);
            HookRetractSprite = LoadSprite(HookRetractFile);
            LockClip = wav.Read(ReadResource(LockFile), Path.GetFileNameWithoutExtension(LockFile));
            UnlockClip = wav.Read(ReadResource(UnlockFile), Path.GetFileNameWithoutExtension(UnlockFile));
            string carabiner = Encoding.UTF8.GetString(ReadResource(CarabinerFile));
            string[][] submeshes = { new[] { BodyMaterial }, new[] { GateMaterial, SleeveMaterial }, new[] { BoltMaterial } };
            Carabiner = new CarabinerModel(new ObjMeshReader().Read(carabiner, Path.GetFileNameWithoutExtension(CarabinerFile), submeshes));
        }

        public Sprite ReticleSprite { get; }
        public Sprite HookIndicatorSprite { get; }
        public Sprite HookRetractSprite { get; }
        public AudioClip LockClip { get; }
        public AudioClip UnlockClip { get; }
        public CarabinerModel Carabiner { get; }

        private Sprite LoadSprite(string file)
        {
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false) { name = Path.GetFileNameWithoutExtension(file) };
            texture.LoadImage(ReadResource(file));
            return Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(CenterPivot, CenterPivot));
        }

        private byte[] ReadResource(string file)
        {
            using (Stream stream = _assembly.GetManifestResourceStream(ResourcePrefix + file))
            {
                if (stream == null)
                    throw new FileNotFoundException($"{nameof(RepkaAssets)}: embedded asset is missing: {file}");

                using (MemoryStream copy = new MemoryStream())
                {
                    stream.CopyTo(copy);
                    return copy.ToArray();
                }
            }
        }
    }
}
