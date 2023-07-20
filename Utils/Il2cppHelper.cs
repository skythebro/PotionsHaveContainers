using Il2CppInterop.Runtime;
using Il2CppSystem;
using Unity.Entities;

namespace PotionsHaveContainers.Utils;

public static class Il2cppHelper
{
    private static Type GetType<T>() => Il2CppType.Of<T>();
    
    public static unsafe T GetComponentDataAOT<T>(this EntityManager entityManager, Entity entity) where T : unmanaged
    {
        var type = TypeManager.GetTypeIndex(GetType<T>());
        var result = (T*)entityManager.GetComponentDataRawRW(entity, type);
        return *result;
    }
}