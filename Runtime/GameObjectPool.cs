﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class GameObjectPool : GameObjectPool<Transform>
{
    public GameObjectPool(GameObject objectPrefab, ushort capacity = 1, Transform parent = null) : base(
        objectPrefab.transform, capacity, parent)
    {
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    public GameObjectPool(GameObject[] objectPrefab, ushort capacity = 1, Transform parent = null) : base(
        objectPrefab.Select(x => x.transform).ToArray(), capacity, parent)
    {
    }
}

public class GameObjectPool<TComponent> : ObjectPool<GameObjectPool<TComponent>.ObjectPoolItem>
    where TComponent : Component
{
    // ReSharper disable once MemberCanBeProtected.Global
    public GameObjectPool(TComponent objectPrefab, ushort capacity = 1, Transform parent = null) : base(
        new ObjectPoolItem(objectPrefab, parent ? parent : ObjectPoolBehaviour.Singletone.transform), capacity)
    {
        if (objectPrefab) return;
        Debug.LogError($"{nameof(objectPrefab)} Can not be null!");
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public GameObjectPool(IEnumerable<TComponent> objectPrefab, ushort capacity = 1, Transform parent = null) : base(
        objectPrefab.Select(x => new ObjectPoolItem(x, parent ? parent : ObjectPoolBehaviour.Singletone.transform))
            .ToArray(), capacity)
    {
    }


    private static IEnumerator DeActive(GameObject obj, float deActiveAfter)
    {
        yield return new WaitForSeconds(deActiveAfter);
        obj.SetActive(false);
    }


    public TComponent ActiveNext(Vector3 position, Quaternion rotation, float deActiveAfter = 0)
    {
        return ActiveNext(true, position, rotation, deActiveAfter);
    }

    public TComponent ActiveNext(float deActiveAfter = 0)
    {
        return ActiveNext(false, default, default, deActiveAfter);
    }

    private TComponent ActiveNext(bool setNewPosition, Vector3 position, Quaternion rotation, float deActiveAfter)
    {
        for (var c = 0; c < Capacity; c++)
        {
            var nextItem = GetNext();
            if (nextItem.ComponentData.gameObject.activeInHierarchy) continue;
            if (setNewPosition)
            {
                var transform = nextItem.ComponentData.transform;
                transform.position = position;
                transform.rotation = rotation;
            }

            nextItem.ComponentData.gameObject.SetActive(true);
            if (deActiveAfter > 0)
                ObjectPoolBehaviour.Singletone.StartCoroutine(
                    DeActive(nextItem.ComponentData.gameObject, deActiveAfter));
            return nextItem.ComponentData;
        }

        throw new IndexOutOfRangeException("Pool is Small for your usage, all GameObjects is in use!");
    }

    public void DeActiveAll()
    {
        foreach (var monoBehaviour in Objects)
            if (monoBehaviour.ComponentData)
                monoBehaviour.ComponentData.gameObject.SetActive(false);
        ResetPoolPosition();
    }

    public static void DeActive(TComponent targetObject)
    {
        targetObject.gameObject.SetActive(false);
    }

    public static void DeActive(GameObject otherGameObject)
    {
        otherGameObject.SetActive(false);
    }

    public class ObjectPoolItem : IDisposable, ICloneable
    {
        private readonly Transform _parent;
        public readonly TComponent ComponentData;

        public ObjectPoolItem(TComponent componentData, Transform parent)
        {
            ComponentData = componentData;
            _parent = parent;
        }

        public object Clone()
        {
            var component = Object
                .Instantiate(ComponentData.gameObject, _parent ? _parent : ComponentData.transform.parent)
                .GetComponent<TComponent>();
            component.gameObject.SetActive(false);
            return new ObjectPoolItem(component, _parent);
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            if (ComponentData)
                Object.Destroy(ComponentData.gameObject);
        }

        ~ObjectPoolItem()
        {
            ReleaseUnmanagedResources();
        }
    }
}