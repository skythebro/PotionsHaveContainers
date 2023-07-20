using BepInEx.Configuration;

namespace PotionsHaveContainers
{
	public static class Settings
	{
		public static ConfigEntry<bool> ENABLE_MOD;
		public static ConfigEntry<bool> GIVE_BOTTLES;
		
		internal static void Initialize(ConfigFile config)
		{
			ENABLE_MOD = config.Bind("Config", "Enable Mod", true, "Enable or Disable PotionsHaveContainers");
			GIVE_BOTTLES = config.Bind("Config", "Give Bottles", false, "Always give canteens(false) or always give bottles(true) for both water containers.");
		}

	}
}
