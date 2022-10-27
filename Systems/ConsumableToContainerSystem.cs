using PotionsHaveContainers.Utils;
using ProjectM;
using ProjectM.Scripting;
using System.Collections.Generic;
using Unity.Entities;

namespace PotionsHaveContainers.Systems
{
    public static class ConsumableToContainerSystem
    {
        private static readonly Dictionary<PrefabGUID, (PrefabGUID itemGUID, int stackCount)> ConsumableToEmptyContainer = new();

        private static readonly Dictionary<PrefabGUID, PrefabGUID> RecipeItemToReturnedItemMapping = new()
        {
            // -- Water-filled Canteen -> Empty Canteen
            [new PrefabGUID(-1322000172)] = new PrefabGUID(-810738866),

            // -- Water-filled Bottle -> Empty Glass Bottle
            [new PrefabGUID(-1382451936)] = new PrefabGUID(-437611596)
        };

        public static void ProcessConsumable(Entity entity, PrefabGUID buffGUID)
        {
            var server = Plugin.World;
            var entityManager = server.EntityManager;

            if (!entityManager.HasComponent<EntityCreator>(entity) || !entityManager.HasComponent<EntityOwner>(entity))
            {
                return;
            }

            var entityOwner = entityManager.GetComponentData<EntityOwner>(entity);
            var entityCreator = entityManager.GetComponentData<EntityCreator>(entity);
            var entityCreatorEntity = entityCreator.Creator._Entity;
            if (!entityManager.HasComponent<AbilityOwner>(entityCreatorEntity))
            {
                return;
            }

            var abilityOwner = entityManager.GetComponentData<AbilityOwner>(entityCreatorEntity);
            var abilityGroupEntity = abilityOwner.AbilityGroup._Entity;
            if (!entityManager.HasComponent<PrefabGUID>(abilityGroupEntity))
            {
                return;
            }

            if(ConsumableToEmptyContainer.Count == 0)
            {
                CreateConsumableContainers();
            }

            var abilityGroupPrefabGUID = entityManager.GetComponentData<PrefabGUID>(abilityGroupEntity);
            if (!ConsumableToEmptyContainer.TryGetValue(abilityGroupPrefabGUID, out var returnData))
            {
                return;
            }

            var gameDataSystem = server.GetExistingSystem<ServerScriptMapper>()._GameDataSystem;

            Helper.AddItemToInventory(entityManager, gameDataSystem.ItemHashLookupMap, entityOwner.Owner, returnData.itemGUID, returnData.stackCount, out _, out _, dropRemainder: true);
        }

        public static void CreateConsumableContainers()
        {
            ConsumableToEmptyContainer.Clear();

            var server = Plugin.World;
            var entityManager = server.EntityManager;
            var gameDataSystem = server.GetExistingSystem<ServerScriptMapper>()._GameDataSystem;
            var prefabCollectionSystem = server.GetExistingSystem<PrefabCollectionSystem>();

            var duplicateConsumables = new List<PrefabGUID>();

            foreach (var recipeKvp in gameDataSystem.RecipeHashLookupMap)
            {
                var recipeEntity = recipeKvp.Value.Entity;
                if (!entityManager.HasComponent<RecipeRequirementBuffer>(recipeEntity) || !entityManager.HasComponent<RecipeOutputBuffer>(recipeEntity))
                {
                    continue;
                }

                // -- Find the returned item
                PrefabGUID? returnItemGUID = null;
                int returnItemStackCount = 0;
                var requirementBuffer = entityManager.GetBuffer<RecipeRequirementBuffer>(recipeEntity);
                foreach (var requirement in requirementBuffer)
                {
                    if (RecipeItemToReturnedItemMapping.TryGetValue(requirement.Guid, out var prefabGUID))
                    {
                        returnItemGUID = prefabGUID;
                        returnItemStackCount = requirement.Stacks;
                        break;
                    }
                }

                // -- Check if we've found a returnable item
                if (!returnItemGUID.HasValue)
                {
                    continue;
                }

                // -- Find the buff that belongs to this item
                var outputBuffer = entityManager.GetBuffer<RecipeOutputBuffer>(recipeEntity);
                foreach (var output in outputBuffer)
                {
                    var outputEntity = prefabCollectionSystem.PrefabLookupMap[output.Guid];
                    if (!entityManager.HasComponent<CastAbilityOnConsume>(outputEntity))
                    {
                        continue;
                    }

                    var castAbilityOnConsuime = entityManager.GetComponentData<CastAbilityOnConsume>(outputEntity);
                    var abilityGUID = castAbilityOnConsuime.AbilityGuid;

                    Plugin.Logger.LogMessage($"[Matched consumable with available container {prefabCollectionSystem.PrefabNameLookupMap[abilityGUID]} -> {prefabCollectionSystem.PrefabNameLookupMap[returnItemGUID.Value]}]");

                    if (ConsumableToEmptyContainer.ContainsKey(abilityGUID))
                    {
                        Plugin.Logger.LogWarning($"[Found duplicate consumable {prefabCollectionSystem.PrefabNameLookupMap[abilityGUID]} and removed it.]");
                        duplicateConsumables.Add(abilityGUID);
                    }
                    ConsumableToEmptyContainer[abilityGUID] = (returnItemGUID.Value, returnItemStackCount);
                }
            }

            duplicateConsumables.ForEach(x => ConsumableToEmptyContainer.Remove(x));
        }
        public static void Deinitialize()
        {
            ConsumableToEmptyContainer.Clear();
        }
    }
}
