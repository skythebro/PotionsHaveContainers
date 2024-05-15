using Bloodstone.API;
using HarmonyLib;
using PotionsHaveContainers.Systems;
using ProjectM;
using Unity.Collections;

namespace PotionsHaveContainers.Hooks
{
    [HarmonyPatch]
    public static class BuffSystemHook
    {
        [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
        [HarmonyPrefix]
        private static void OnUpdate(BuffSystem_Spawn_Server __instance)
        {
            if (!VWorld.IsServer || __instance.__query_401358634_0.IsEmpty) return;

            if (!Settings.ENABLE_MOD.Value) return;
            //var entityManager = __instance.EntityManager;

            var entities = __instance.__query_401358634_0.ToEntityArray(Allocator.Temp);
            foreach (var entity in entities)
            {
                //PrefabGUID buffGUID = entityManager.GetComponentData<PrefabGUID>(entity);
                ConsumableToContainerSystem.ProcessConsumable(entity);
            }
        }
    }
}