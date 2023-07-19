using BepInEx.Unity.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using BepInEx;
using PotionsHaveContainers.Systems;
using Bloodstone.API;

namespace PotionsHaveContainers
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    [BepInDependency("gg.deca.Bloodstone")]
    public class Plugin : BasePlugin, IRunOnInitialized
    {
        private Harmony _harmony;

        public static ManualLogSource LogInstance { get; private set; }

        public override void Load()
        {
            LogInstance = Log;
            Settings.Initialize(Config);

            if (!VWorld.IsServer)
            {
                Log.LogWarning("This plugin is a server-only plugin.");
            }
        }

        public void OnGameInitialized()
        {
            if (VWorld.IsClient)
            {
                return;
            }
            
            _harmony = Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        }

        public override bool Unload()
        {
            Config.Clear();
            _harmony.UnpatchSelf();
            ConsumableToContainerSystem.Deinitialize();
            return true;
        }
    }
}