using Bloodstone.API;
using HarmonyLib;
using PotionsHaveContainers.Systems;
using ProjectM;
using Unity.Collections;

namespace PotionsHaveContainers.Hooks;

[HarmonyPatch]
public static class AbilityCastStarted_SpawnPrefabSystem_ServerHook
{
    [HarmonyPatch(typeof(AbilityCastStarted_SpawnPrefabSystem_Server), nameof(AbilityCastStarted_SpawnPrefabSystem_Server.OnUpdate))]
    [HarmonyPrefix]
    private static void OnUpdate(AbilityCastStarted_SpawnPrefabSystem_Server __instance)
    {
        if (!VWorld.IsServer || __instance.__query_577032082_0.IsEmpty) return;

        if (!Settings.ENABLE_MOD.Value) return;

        var entities = __instance.__query_577032082_0.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            ConsumableToContainerSystem.ProcessAbilityUseEvent(entity);
        }
    }
}