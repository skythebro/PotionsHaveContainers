using BepInEx.Configuration;

namespace PotionsHaveContainers
{
	public static class Settings
	{
		public static ConfigEntry<bool> ENABLE_MOD;
		
		internal static void Initialize(ConfigFile config)
		{
			ENABLE_MOD = config.Bind("Config", "Enable Mod", true, "Enable or Disable PotionsHaveContainers");
		}

	}
}
