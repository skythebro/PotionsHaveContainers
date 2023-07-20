
using System;
using ProjectM;
using BepInEx.Logging;
using Bloodstone.API;
using Il2CppInterop.Runtime;
using Unity.Collections;
using Unity.Entities;

namespace PotionsHaveContainers.Utils
{
    public static class Helper
    {
        private static ManualLogSource _log => Plugin.LogInstance;

        public static void AddItemToInventory(Entity recipient, PrefabGUID itemType, int amount)
        {
            try
            {
                var gameData = VWorld.Server.GetExistingSystem<GameDataSystem>();
                var itemSettings = AddItemSettings.Create(VWorld.Server.EntityManager, gameData.ItemHashLookupMap);
                //_log.LogDebug($"Trying to add item with GUID: {itemType} to {recipient} with amount {amount}");
                InventoryUtilitiesServer.TryAddItem(itemSettings, recipient, itemType, amount);
            }
            catch (Exception e)
            {
                _log.LogError(e.Message);
            }
        }
    }
}