using System;
using UnityEngine;

public class ObjectPoolBehaviour : MonoBehaviour
{
    // ReSharper disable once IdentifierTypo
    private static ObjectPoolBehaviour _singletone;

    // ReSharper disable once IdentifierTypo
    public static ObjectPoolBehaviour Singletone
    {
        get
        {
            if (!_singletone)
                _singletone = new GameObject(nameof(ObjectPoolBehaviour)).AddComponent<ObjectPoolBehaviour>();
            return _singletone;
        }
    }

    private void LateUpdate()
    {
        OnFrame?.Invoke();
    }

    public static event Action OnFrame;
}