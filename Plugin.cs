using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using Unity.Entities;
using UnityEngine;
using System.IO;
using System;
using ProjectM;
using PotionsHaveContainers.Systems;
using BepInEx.Configuration;

namespace PotionsHaveContainers
{
    [BepInPlugin(BuildConfig.PackageID, BuildConfig.Name, BuildConfig.Version)]
    public class Plugin : BasePlugin
    { 
        private Harmony harmony;

        private static ConfigEntry<bool> EnableMod;
        public static bool _EnableMod;
        
        public static string ModFolder = "BepInEx/config/" + BuildConfig.PackageID + "";

        public static bool isInitialized = false;

        public static ManualLogSource Logger;

        private static World _serverWorld;
        public static World World
        {
            get
            {
                if (_serverWorld != null) return _serverWorld;

                _serverWorld = GetWorld("Server")
                    ?? throw new Exception("There is no Server world (yet). Did you install a server mod on the client?");
                return _serverWorld;
            }
        }

        public static bool IsServer => Application.productName == "VRisingServer";

        private static World GetWorld(string name)
        {
            foreach (var world in World.s_AllWorlds)
            {
                if (world.Name == name)
                {
                    return world;
                }
            }

            return null;
        }

        public void InitConfig()
        {
            EnableMod = Config.Bind("Config", "Enable Mod", true, "Enable or Disable PotionsHaveContainers");
        }

        public override void Load()
        {
            InitConfig();

            Logger = Log;
            harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

            Log.LogInfo($"Plugin {BuildConfig.Name}-v{BuildConfig.Version} is loaded!");
        }

        public override bool Unload()
        {
            Config.Clear();
            harmony.UnpatchSelf();
            ConsumableToContainerSystem.Deinitialize();
            return true;
        }

        [HarmonyPatch(typeof(GameBootstrap), nameof(GameBootstrap.Start))]
        public static class GameBootstrapStart_Patch
        {
            public static void Postfix()
            {
                var Plugin = new Plugin();
                Plugin.OnGameInitialized();
            }
        }

        public void OnGameInitialized()
        {
            if (isInitialized) return;
            _EnableMod = EnableMod.Value;

            isInitialized = true;
        }
    }
}
