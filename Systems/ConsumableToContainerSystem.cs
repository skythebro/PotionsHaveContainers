using System;
using ProjectM;
using ProjectM.Scripting;
using System.Collections.Generic;
using System.Threading;
using BepInEx.Logging;
using Bloodstone.API;
using PotionsHaveContainers.Utils;
using Stunlock.Core;
using Unity.Entities;

namespace PotionsHaveContainers.Systems
{
    public static class ConsumableToContainerSystem
    {
        private static readonly Dictionary<PrefabGUID, (PrefabGUID itemGUID, int stackCount)>
            ConsumableToEmptyContainer = new();

        private static readonly Dictionary<PrefabGUID, (PrefabGUID itemGUID, int stackCount)>
            WaterOrBloodConsumableToEmptyContainer = new();

        private static readonly World Server = VWorld.Server;

        private static EntityManager _entityManager = Server.EntityManager;

        private static ManualLogSource _log => Plugin.LogInstance;

        private static readonly Dictionary<PrefabGUID, PrefabGUID> RecipeItemToReturnedItemMapping = new()
        {
            // -- Water-filled Canteen -> Empty Canteen
            [new PrefabGUID(-1322000172)] = new PrefabGUID(-810738866),

            // -- Enchanted Brew -> Empty Canteen 
            [new PrefabGUID(248289327)] = new PrefabGUID(-810738866),

            // -- Blood Rose Brew -> Empty Canteen
            [new PrefabGUID(800879747)] = new PrefabGUID(-810738866),

            // -- Brew of Ferocity -> Empty Canteen
            [new PrefabGUID(-269326085)] = new PrefabGUID(-810738866),

            // -- Fire Resistance Brew -> Empty Canteen
            [new PrefabGUID(970650569)] = new PrefabGUID(-810738866),

            // -- Garlic Fever Antidote -> Empty Canteen (seems to be unused and removed from the game in 1.0)
            [new PrefabGUID(-606793991)] = new PrefabGUID(-810738866),

            // -- Minor Garlic Resistance Brew -> Empty Canteen
            [new PrefabGUID(423790753)] = new PrefabGUID(-810738866),

            // -- Minor Sun Resistance Brew -> Empty Canteen
            [new PrefabGUID(-38051433)] = new PrefabGUID(-810738866),

            // -- Silver Resistance Brew -> Empty Canteen
            [new PrefabGUID(272647158)] = new PrefabGUID(-810738866),

            // -- Water-filled Bottle -> Empty Glass Bottle
            [new PrefabGUID(-1382451936)] = new PrefabGUID(-437611596),

            // -- Blood Rose Potion -> Empty Glass Bottle
            [new PrefabGUID(429052660)] = new PrefabGUID(-437611596),

            // -- Blood Potion -> Empty Glass Bottle
            [new PrefabGUID(2058060497)] = new PrefabGUID(-437611596),

            // -- Garlic Resistance Potion -> Empty Glass Bottle
            [new PrefabGUID(-2139183850)] = new PrefabGUID(-437611596),

            // -- Holy Resistance Potion -> Empty Glass Bottle
            [new PrefabGUID(890484447)] = new PrefabGUID(-437611596),

            // -- Holy Resistance Flask -> Empty Glass Bottle
            [new PrefabGUID(639992282)] = new PrefabGUID(-437611596),

            // -- Potion of Rage -> Empty Glass Bottle
            [new PrefabGUID(-1568756102)] = new PrefabGUID(-437611596),

            // -- Silver Resistance Potion -> Empty Glass Bottle
            [new PrefabGUID(2107622409)] = new PrefabGUID(-437611596),

            // -- Witch Potion -> Empty Glass Bottle
            [new PrefabGUID(1510182325)] = new PrefabGUID(-437611596),

            // -- Wrangler’s Potion -> Empty Glass Bottle
            [new PrefabGUID(541321301)] = new PrefabGUID(-437611596),

            // -- Extracted Blood Potion (fakeitem) -> Empty Glass Bottle
            [new PrefabGUID(-1871776321)] = new PrefabGUID(-437611596),
            
            // -- Prison potion -> empty glass bottle
            [new PrefabGUID(828432508)] = new PrefabGUID(-437611596),

            // -- Extracted bloodwine (blood merlot) -> Empty Glass Bottle
            [new PrefabGUID(1223264867)] = new PrefabGUID(-437611596),

            // -- spell leech potion T1 (name pending) -> empty glass bottle
            [new PrefabGUID(-2102469163)] = new PrefabGUID(-437611596),
            
            // -- Water-filled Crystal Flask -> Empty Crystal Flask (not used in game anymore, removed in 1.0?)
            //[new PrefabGUID(225917880)] = new PrefabGUID(1675503103)
        };

        public static void ProcessAbilityUseEvent(Entity entity)
        {
            //var prefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();
            var abEvent =
                _entityManager.GetComponentDataAOT<AbilityCastStartedEvent>(entity);
            var ability = abEvent.Ability;
            var character = abEvent.Character;
            var abilityGroup = abEvent.AbilityGroup;
            _entityManager.TryGetComponentData<PrefabGUID>(ability, out var abilityguid);
            //_log.LogDebug($"[Ability][{prefabCollectionSystem.PrefabGuidToNameDictionary[abilityguid]}]");
            //_entityManager.TryGetComponentData<PrefabGUID>(character, out var characterguid);
            //_log.LogDebug($"[Character][{prefabCollectionSystem.PrefabGuidToNameDictionary[characterguid]}]");
            _entityManager.TryGetComponentData<PrefabGUID>(abilityGroup, out var abilityGroupguid);
            //_log.LogDebug($"[AbilityGroup][{prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGroupguid]}]");


            if (abilityguid != new PrefabGUID(-159878578) && abilityguid != new PrefabGUID(204798150) &&
                abilityguid != new PrefabGUID(1538048498))
            {
                return;
            }

            //_log.LogWarning($"AbilityGroupId! {prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGroupguid]}");
            if (ConsumableToEmptyContainer.Count == 0)
            {
                CreateConsumableContainers();
            }

            var hasReturnData = ConsumableToEmptyContainer.TryGetValue(abilityGroupguid, out var returnData);
            var hasWaterOrBloodReturnData =
                WaterOrBloodConsumableToEmptyContainer.TryGetValue(abilityGroupguid, out var waterOrBloodReturnData);
            if (!hasReturnData)
            {
                //_log.LogDebug($"Consumable not found in original dictionary! {prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGroupguid]}");
            }
            else
            {
                Helper.AddItemToInventory(character, returnData.itemGUID, returnData.stackCount);
                return;
            }

            if (!hasWaterOrBloodReturnData)
            {
                //_log.LogDebug($"Consumable not found in second dictionary either! {prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGroupguid]}");
                return;
            }

            Thread.Sleep(200);
            Helper.AddItemToInventory(character, waterOrBloodReturnData.itemGUID, waterOrBloodReturnData.stackCount);
        }

        public static void ProcessConsumable(Entity entity)
        {
            try
            {
                //var prefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();
                var hasCreator = _entityManager.TryGetComponentData<EntityCreator>(entity, out var entityCreator);
                var hasOwner = _entityManager.TryGetComponentData<EntityOwner>(entity, out var entityOwner);
                //_entityManager.TryGetComponentData<PrefabGUID>(entity, out var entityguid);
                //_log.LogDebug($"testing if {prefabCollectionSystem.PrefabGuidToNameDictionary[entityguid]} has creatorComponent");
                if (!hasCreator || !hasOwner)
                {
                    return;
                }

                var entityCreatorEntity = entityCreator.Creator._Entity;
                var hasAbilityOwner =
                    _entityManager.TryGetComponentData<AbilityOwner>(entityCreatorEntity, out var abilityOwner);
                //_entityManager.TryGetComponentData<PrefabGUID>(entityCreatorEntity, out var entitycreatorguid);
                //_log.LogDebug($"testing if {prefabCollectionSystem.PrefabGuidToNameDictionary[entitycreatorguid]} has abilityownerComponent");
                if (!hasAbilityOwner)
                {
                    return;
                }

                var abilityGroupEntity = abilityOwner.AbilityGroup._Entity;
                var hasPrefabGuid =
                    _entityManager.TryGetComponentData<PrefabGUID>(abilityGroupEntity, out var abilityGroupPrefabGuid);
                //_log.LogDebug($"testing if {prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGroupPrefabGuid]} has abilityownerComponent");
                if (!hasPrefabGuid)
                {
                    //_log.LogDebug($"Consumable does not have a PrefabGUID! {abilityGroupEntity.ToString()}");
                    return;
                }

                if (ConsumableToEmptyContainer.Count == 0)
                {
                    CreateConsumableContainers();
                }

                //_log.LogDebug($"testing if {prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGroupPrefabGuid]} has return data (is it in the dictionary?)");
                var hasReturnData = ConsumableToEmptyContainer.TryGetValue(abilityGroupPrefabGuid, out var returnData);
                if (!hasReturnData)
                {
                    //_log.LogDebug($"Consumable not found in dictionary! {prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGroupPrefabGuid]}");
                    return;
                }
                
                Helper.AddItemToInventory(entityOwner.Owner, returnData.itemGUID, returnData.stackCount);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private static void CreateConsumableContainers()
        {
            ConsumableToEmptyContainer.Clear();

            var gameDataSystem = Server.GetExistingSystemManaged<ServerScriptMapper>()._GameDataSystem;
            var prefabCollectionSystem = Server.GetExistingSystemManaged<PrefabCollectionSystem>();

            var duplicateConsumables = new List<PrefabGUID>();

            foreach (var recipeKvp in gameDataSystem.RecipeHashLookupMap)
            {
                var recipeEntity = recipeKvp.Value.Entity;
                var hasRecipeRequirementBuffer =
                    _entityManager.TryGetBuffer<RecipeRequirementBuffer>(recipeEntity, out var requirementBuffer);
                var hasRecipeOutputBuffer =
                    _entityManager.TryGetBuffer<RecipeOutputBuffer>(recipeEntity, out var outputBuffer);
                if (!hasRecipeRequirementBuffer || !hasRecipeOutputBuffer)
                {
                    continue;
                }

                // -- Find the returned item
                PrefabGUID? returnItemGuid = null;
                int returnItemStackCount = 0;
                foreach (var requirement in requirementBuffer)
                {
                    //_log.LogDebug($"Item required for recipe guid: {prefabCollectionSystem.PrefabGuidToNameDictionary[requirement.Guid]} amount: {requirement.Amount}");
                    if (RecipeItemToReturnedItemMapping.TryGetValue(requirement.Guid, out var prefabGUID))
                    {
                        //_log.LogDebug($"Found a returnable item: {prefabCollectionSystem.PrefabGuidToNameDictionary[requirement.Guid]}");
                        returnItemGuid = prefabGUID;
                        returnItemStackCount = requirement.Amount;
                        //_log.LogDebug($"returnItemGuid: {prefabCollectionSystem.PrefabGuidToNameDictionary[prefabGUID]} Amount of the item to return: {returnItemStackCount}");
                        break;
                    }
                }

                // -- Check if we've found a returnable item
                if (!returnItemGuid.HasValue)
                {
                    continue;
                }

                // -- Find the buff that belongs to this item
                foreach (var output in outputBuffer)
                {
                    var outputEntity = prefabCollectionSystem.PrefabLookupMap[output.Guid];
                    //_log.LogDebug("------------------------------------------------------");
                    //_log.LogDebug($"Output entity: {prefabCollectionSystem.PrefabGuidToNameDictionary[output.Guid]}");


                    var hasCastAbilityOnConsume =
                        _entityManager.TryGetComponentData<CastAbilityOnConsume>(outputEntity,
                            out var castAbilityOnConsume);
                    if (!hasCastAbilityOnConsume)
                    {
                        //_log.LogDebug($"Couldnt match cast ability on consume for {prefabCollectionSystem.PrefabGuidToNameDictionary[output.Guid]}");
                        continue;
                    }


                    var abilityGuid = castAbilityOnConsume.AbilityGuid;
                    //_log.LogDebug($"Found ability guid: {prefabCollectionSystem.PrefabGuidToNameDictionary[abilityGuid]}");
                    //_log.LogDebug("------------------------------------------------------");
                    if (ConsumableToEmptyContainer.ContainsKey(abilityGuid))
                    {
                        //_log.LogDebug($"[Found duplicate consumable {prefabCollectionSystem._PrefabGuidToNameDictionary[abilityGuid]} and removed it.]");
                        duplicateConsumables.Add(abilityGuid);
                    }

                    ConsumableToEmptyContainer[abilityGuid] = (returnItemGuid.Value, returnItemStackCount);


                    //_log.LogDebug($"[Added consumable {prefabCollectionSystem._PrefabGuidToNameDictionary[abilityGuid]} -> {prefabCollectionSystem._PrefabGuidToNameDictionary[returnItemGuid.Value]} Count {returnItemStackCount} to dictionary ]");
                }
            }
            // Do the blood potions, water potions and garlic cure(idk if this is actually ingame) manually
            if (Settings.GIVE_BOTTLES.Value)
            {
                
                //WaterOrBloodConsumableToEmptyContainer[new PrefabGUID(1743540461)] =
                //(new PrefabGUID(-437611596), 1);
                WaterOrBloodConsumableToEmptyContainer[new PrefabGUID(1743540461)] =
                    (new PrefabGUID(-437611596), 1);

                WaterOrBloodConsumableToEmptyContainer[new PrefabGUID(974235336)] =
                    (new PrefabGUID(-437611596), 1);
            }
            else
            {
                WaterOrBloodConsumableToEmptyContainer[new PrefabGUID(1743540461)] =
                    (new PrefabGUID(-810738866), 1);

                WaterOrBloodConsumableToEmptyContainer[new PrefabGUID(974235336)] =
                    (new PrefabGUID(-437611596), 1);
            }

            WaterOrBloodConsumableToEmptyContainer[new PrefabGUID(-194817723)] =
                (new PrefabGUID(1743540461), 1);
            duplicateConsumables.ForEach(x => ConsumableToEmptyContainer.Remove(x));
        }

        public static void Deinitialize()
        {
            ConsumableToEmptyContainer.Clear();
        }
    }
}