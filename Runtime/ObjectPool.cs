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

    public ObjectPool(T objectPrefab, ushort capacity = 1, int initializeFrameStep = 0)
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
        if (initializeFrameStep > 0) Initialize(initializeFrameStep);
    }

    // ReSharper disable once MemberCanBeProtected.Global
    public ObjectPool(T[] objectPrefab, ushort capacity = 1, int initializeFrameStep = 0)
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
        if (initializeFrameStep > 0) Initialize(initializeFrameStep);
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

    // ReSharper disable once MemberCanBePrivate.Global
    public void Initialize(int frameStep = 0)
    {
        ObjectPoolBehaviour.Singletone.StartCoroutine(InitializeOnFrameDelayEnumerator(frameStep));
    }

    private IEnumerator InitializeOnFrameDelayEnumerator(int frameStep)
    {
        if (_objectPrefab == null || _objectPrefab.Length < 1) yield break;
        for (var i = 0; i < Capacity; i++)
        {
            var randomPrefab = _objectPrefab[Random.Range(0, _objectPrefab.Length)];
            if (randomPrefab == null) yield break;
            if (Objects[i] != null) Objects[i].Dispose();
            Objects[i] = randomPrefab.Clone() as T;
            for (var c = 0; c < frameStep; c++) yield return null;
        }
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