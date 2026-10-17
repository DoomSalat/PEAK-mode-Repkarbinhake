using System.IO;
using BepInEx;
using HarmonyLib;
using Repkarbinhake.Plugin.Data;
using Repkarbinhake.Plugin.Patches;
using UnityEngine;

namespace Repkarbinhake.Plugin
{
    [BepInPlugin(Guid, Name, Version)]
    public sealed class RepkaPlugin : BaseUnityPlugin
    {
        public const string Guid = "repkarbinhake";
        public const string Name = "Repkarbinhake";
        public const string Version = "0.5.6";

        private Harmony _harmony;

        private void Awake()
        {
            RepkaConfig config = new RepkaConfig(Config);
            RepkaAssets assets = new RepkaAssets(Path.GetDirectoryName(Info.Location));

            _harmony = new Harmony(Guid);
            _harmony.PatchAll(typeof(RepkaPlugin).Assembly);

            GameObject host = new GameObject(Name);
            host.SetActive(false);
            DontDestroyOnLoad(host);
            host.hideFlags = HideFlags.HideAndDontSave;
            RepkaBack back = host.AddComponent<RepkaBack>();
            back.Initialize(config, assets, Logger);
            host.SetActive(true);
            Logger.LogInfo($"{nameof(RepkaPlugin)} {Version} loaded");
        }

        private void OnDestroy()
        {
            _harmony?.UnpatchSelf();
        }
    }
}
