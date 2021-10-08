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
            if (!nextItem.ComponentObject.gameObject.activeInHierarchy) continue;
            if (setNewPosition) nextItem.ComponentObject.transform.position = position;
            nextItem.ComponentObject.gameObject.SetActive(true);
            if (deActiveAfter > 0)
                BehaviourObject.StartCoroutine(DeActive(nextItem.ComponentObject.gameObject, deActiveAfter));
            return nextItem.ComponentObject;
        }

        throw new IndexOutOfRangeException("Pool is Small for your usage, all GameObjects is in use!");
    }

    public void DeActiveAll()
    {
        foreach (var monoBehaviour in Objects)
            if (monoBehaviour.ComponentObject)
                monoBehaviour.ComponentObject.gameObject.SetActive(false);
        ResetPoolPosition();
    }

    public class ObjectPoolItem : ICloneable<ObjectPoolItem>, IDisposable
    {
        public readonly TComponent ComponentObject;

        public ObjectPoolItem(TComponent componentObject)
        {
            ComponentObject = componentObject;
        }

        public ObjectPoolItem Clone()
        {
            return new ObjectPoolItem(Object.Instantiate(ComponentObject.gameObject, ComponentObject.transform.parent)
                .GetComponent<TComponent>());
        }

        public void Dispose()
        {
            ReleaseUnmanagedResources();
            GC.SuppressFinalize(this);
        }

        private void ReleaseUnmanagedResources()
        {
            Object.Destroy(ComponentObject.gameObject);
        }

        ~ObjectPoolItem()
        {
            ReleaseUnmanagedResources();
        }
    }
}