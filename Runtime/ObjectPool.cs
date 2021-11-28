using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class ObjectPool<T> : IDisposable where T : class, IDisposable, ICloneable
{
    private readonly bool _cleanable;
    private readonly T[] _objectPrefab;
    protected readonly int Capacity;
    public readonly T[] Objects;

    public ObjectPool(T objectPrefab, ushort capacity = 1)
    {
        if (capacity < 1)
        {
            Debug.LogError($"{nameof(capacity)} should be 1 or higher.");
            return;
        }

        _objectPrefab = new[] {objectPrefab};
        Capacity = capacity;
        Objects = new T[capacity];
        _cleanable = _objectPrefab.GetType().IsSubclassOf(typeof(IResetAble));
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public ObjectPool(T[] objectPrefab, ushort capacity = 1)
    {
        if (objectPrefab == null || objectPrefab.Length < 1)
        {
            Debug.LogError($"{nameof(objectPrefab)} Can not be null!");
            return;
        }

        if (capacity < 1)
        {
            Debug.LogError($"{nameof(capacity)} should be 1 or higher.");
            return;
        }

        _objectPrefab = objectPrefab;
        Capacity = capacity;
        Objects = new T[capacity];
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected int CurrentPosition { get; set; } = -1;

    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public void ResetPoolPosition()
    {
        CurrentPosition = -1;
    }

    public void Initialize()
    {
        if (_objectPrefab == null || _objectPrefab.Length < 1) return;
        for (var i = 0; i < Capacity; i++)
            if (InitializeOnIndex(i))
                return;
    }

    public void InitializeConcurrent(Action onInitEnded = null)
    {
        if (_objectPrefab == null || _objectPrefab.Length < 1) return;
        ObjectPoolBehaviour.Singletone.StartCoroutine(InitializeOnFrameDelayEnumerator(onInitEnded));
    }

    private IEnumerator InitializeOnFrameDelayEnumerator(Action onInitEnded)
    {
        for (var i = 0; i < Capacity; i++)
            if (InitializeOnIndex(i)) yield return null;
            else yield break;
        onInitEnded?.Invoke();
    }

    private bool InitializeOnIndex(int i)
    {
        var randomPrefab = _objectPrefab[Random.Range(0, _objectPrefab.Length)];
        if (randomPrefab == null) return false;
        if (Objects[i] != null) Objects[i].Dispose();
        Objects[i] = randomPrefab.Clone() as T;
        return true;
    }


    public T GetNext()
    {
        //check is there any inactive
        var index = ++CurrentPosition % Capacity;
        var nextOne = Objects[index];
        if (nextOne == null) return Objects[index] = _objectPrefab[Random.Range(0, _objectPrefab.Length)].Clone() as T;
        // ReSharper disable once SuspiciousTypeConversion.Global
        if (_cleanable) (nextOne as IResetAble)?.Reset();
        return nextOne;
    }

    private void ReleaseUnmanagedResources()
    {
        for (var i = 0; i < Capacity; i++)
            if (Objects[i] != null)
                Objects[i].Dispose();
    }

    ~ObjectPool()
    {
        Dispose();
    }
}