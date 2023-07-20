using BepInEx.Logging;
using Bloodstone.API;
using HarmonyLib;
using PotionsHaveContainers.Systems;
using PotionsHaveContainers.Utils;
using ProjectM;
using ProjectM.Gameplay.Systems;
using Unity.Collections;

namespace PotionsHaveContainers.Hooks;

[HarmonyPatch]
public static class AbilityCastStarted_SpawnPrefabSystem_ServerHook
{
    [HarmonyPatch(typeof(AbilityCastStarted_SpawnPrefabSystem_Server), nameof(AbilityCastStarted_SpawnPrefabSystem_Server.OnUpdate))]
    [HarmonyPrefix]
    private static void OnUpdate(AbilityCastStarted_SpawnPrefabSystem_Server __instance)
    {
        if (!VWorld.IsServer || __instance.__OnUpdate_LambdaJob0_entityQuery.IsEmpty) return;

        if (!Settings.ENABLE_MOD.Value) return;

        var entities = __instance.__OnUpdate_LambdaJob0_entityQuery.ToEntityArray(Allocator.Temp);
        foreach (var entity in entities)
        {
            ConsumableToContainerSystem.ProcessAbilityUseEvent(entity);
        }
    }
}