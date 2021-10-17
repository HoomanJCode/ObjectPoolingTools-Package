using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable once UnusedType.Global
public class GameObjectPool : GameObjectPool<Transform>
{
    public GameObjectPool(GameObject objectPrefab, ushort capacity = 1, int initializeFrameStep = 0) : base(
        objectPrefab.transform, capacity, initializeFrameStep)
    {
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    public GameObjectPool(GameObject[] objectPrefab, ushort capacity = 1, int initializeFrameStep = 0) : base(
        objectPrefab.Select(x => x.transform).ToArray(), capacity, initializeFrameStep)
    {
    }
}

public class GameObjectPool<TComponent> : ObjectPool<GameObjectPool<TComponent>.ObjectPoolItem>
    where TComponent : Component
{
    // ReSharper disable once MemberCanBeProtected.Global
    public GameObjectPool(TComponent objectPrefab, ushort capacity = 1, int initializeFrameStep = 0) : base(
        new ObjectPoolItem(objectPrefab), capacity, initializeFrameStep)
    {
        if (objectPrefab) return;
        Debug.LogError($"{nameof(objectPrefab)} Can not be null!");
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public GameObjectPool(IEnumerable<TComponent> objectPrefab, ushort capacity = 1, int initializeFrameStep = 0) :
        base(objectPrefab.Select(x => new ObjectPoolItem(x)).ToArray(), capacity, initializeFrameStep)
    {
    }


    private static IEnumerator DeActive(GameObject obj, float deActiveAfter)
    {
        yield return new WaitForSeconds(deActiveAfter);
        obj.SetActive(false);
    }


    public TComponent ActiveNext(Vector3 position, float deActiveAfter = 0)
    {
        return ActiveNext(true, position, deActiveAfter);
    }

    public TComponent ActiveNext(float deActiveAfter = 0)
    {
        return ActiveNext(false, default, deActiveAfter);
    }

    private TComponent ActiveNext(bool setNewPosition, Vector3 position, float deActiveAfter)
    {
        for (var c = 0; c < Capacity; c++)
        {
            var nextItem = GetNext();
            if (!nextItem.ComponentData.gameObject.activeInHierarchy) continue;
            if (setNewPosition) nextItem.ComponentData.transform.position = position;
            nextItem.ComponentData.gameObject.SetActive(true);
            if (deActiveAfter > 0)
                BehaviourObject.StartCoroutine(DeActive(nextItem.ComponentData.gameObject, deActiveAfter));
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

    public class ObjectPoolItem : IDisposable, ICloneable
    {
        public readonly TComponent ComponentData;
        public readonly GameObject GameObject;
        public readonly Transform Transform;

        public ObjectPoolItem(TComponent componentData)
        {
            ComponentData = componentData;
            GameObject = componentData.gameObject;
            Transform = componentData.transform;
        }

        public object Clone()
        {
            return new ObjectPoolItem(Object.Instantiate(ComponentData.gameObject, ComponentData.transform.parent)
                .GetComponent<TComponent>());
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            Object.Destroy(ComponentData.gameObject);
        }

        ~ObjectPoolItem()
        {
            ReleaseUnmanagedResources();
        }
    }
}