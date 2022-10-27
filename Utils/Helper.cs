using ProjectM;
using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Entities;
using Wetstone.API;

namespace PotionsHaveContainers.Utils
{
    public static class Helper
    {
        public static Entity TempEntity = new Entity();
        public static EntityManager em = Plugin.World.EntityManager;
        private static System.Random rand = new System.Random();
        private static Entity empty_entity = new Entity();

        public static PrefabGUID GetGUIDFromName(string name)
        {
            GameDataSystem gameDataSystem = VWorld.Server.GetExistingSystem<GameDataSystem>();
            ManagedDataRegistry managed = gameDataSystem.ManagedDataRegistry;

            foreach (Unity.Collections.LowLevel.Unsafe.KeyValue<PrefabGUID, ItemData> entry in gameDataSystem.ItemHashLookupMap)
            {
                try
                {
                    ManagedItemData item = managed.GetOrDefault<ManagedItemData>(entry.Key);
                    if (item.PrefabName.StartsWith("Item_VBloodSource") || item.PrefabName.StartsWith("GM_Unit_Creature_Base") || item.PrefabName == "Item_Cloak_ShadowPriest") continue;
                    if (item.Name.ToString().ToLower() == name.ToLower())
                    {
                        return entry.Key;
                    }
                }
                catch { }
            }

            return new PrefabGUID(0);
        }
        public static bool AddItemToInventory(EntityManager entityManager, NativeHashMap<PrefabGUID, ItemData> itemDataMap, Entity target, PrefabGUID itemType, int itemStacks, out int remainingStacks, out Entity newEntity, bool dropRemainder = false)
        {
            if (!Plugin.IsServer)
            {
                remainingStacks = itemStacks;
                newEntity = Entity.Null;
                return false;
            }
            itemDataMap ??= Plugin.World.GetExistingSystem<GameDataSystem>().ItemHashLookupMap;

            unsafe
            {
                // Some hacky code to create a null-able that won't be GC'ed by the IL2CPP domain.
                var bytes = stackalloc byte[Marshal.SizeOf<FakeNull>()];
                var bytePtr = new IntPtr(bytes);
                Marshal.StructureToPtr<FakeNull>(new()
                {
                    value = 7,
                    has_value = true
                }, bytePtr, false);
                var boxedBytePtr = IntPtr.Subtract(bytePtr, 0x10);
                var fakeInt = new Il2CppSystem.Nullable<int>(boxedBytePtr);

                return InventoryUtilitiesServer.TryAddItem(entityManager, itemDataMap, target, itemType, itemStacks, out remainingStacks, out newEntity, startIndex: fakeInt, dropRemainder: dropRemainder);
            }
        }

        public static PrefabGUID GetPrefabGUID(Entity entity)
        {
            var entityManager = Plugin.World.EntityManager;
            PrefabGUID guid;
            try
            {
                guid = entityManager.GetComponentData<PrefabGUID>(entity);
            }
            catch
            {
                guid.GuidHash = 0;
            }
            return guid;
        }

        public static string GetPrefabName(PrefabGUID hashCode)
        {
            var s = Plugin.World.GetExistingSystem<PrefabCollectionSystem>();
            string name = "Nonexistent";
            if (hashCode.GuidHash == 0)
            {
                return name;
            }
            try
            {
                name = s.PrefabNameLookupMap[hashCode].ToString();
            }
            catch
            {
                name = "NoPrefabName";
            }
            return name;
        }

        struct FakeNull
        {
            public int value;
            public bool has_value;
        }
    }
}