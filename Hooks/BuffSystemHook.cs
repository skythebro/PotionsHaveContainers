using HarmonyLib;
using PotionsHaveContainers.Systems;
using ProjectM;
using Unity.Collections;
namespace PotionsHaveContainers.Shared
{
    [HarmonyPatch]
    public static class BuffSystemHook
    {
        [HarmonyPatch(typeof(BuffSystem_Spawn_Server), nameof(BuffSystem_Spawn_Server.OnUpdate))]
        [HarmonyPrefix]
        private static void OnUpdate(BuffSystem_Spawn_Server __instance)
        {
            if (!Plugin.IsServer || __instance.__OnUpdate_LambdaJob0_entityQuery == null) { return; }

            if(Plugin._EnableMod)
            {
                var entityManager = __instance.EntityManager;

                var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
                foreach (var entity in entities)
                {
                    PrefabGUID buffGUID = entityManager.GetComponentData<PrefabGUID>(entity);
                    ConsumableToContainerSystem.ProcessConsumable(entity, buffGUID);
                }
            }
        }
    }
}
