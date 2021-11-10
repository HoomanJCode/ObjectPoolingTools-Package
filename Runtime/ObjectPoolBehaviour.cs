using UnityEngine;

public class ObjectPoolBehaviour : MonoBehaviour
{
    // ReSharper disable once IdentifierTypo
    private static ObjectPoolBehaviour _singletone;

    // ReSharper disable once IdentifierTypo
    public static ObjectPoolBehaviour Singletone =>
        _singletone ??= new GameObject(nameof(ObjectPoolBehaviour)).AddComponent<ObjectPoolBehaviour>();
}