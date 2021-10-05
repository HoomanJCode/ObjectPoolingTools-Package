using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class ObjectPool : ObjectPool<Transform>
{
    public ObjectPool(GameObject objectPrefab, ushort capacity = 1, int initializeFrameStep = 0) : base(
        objectPrefab.transform, capacity, initializeFrameStep)
    {
    }

    // ReSharper disable once ParameterTypeCanBeEnumerable.Local
    public ObjectPool(GameObject[] objectPrefab, ushort capacity = 1, int initializeFrameStep = 0) : base(
        objectPrefab.Select(x => x.transform).ToArray(), capacity, initializeFrameStep)
    {
    }
}

public class ObjectPool<TComponent> : IDisposable where TComponent : Component
{
    private readonly int _capacity;
    private readonly TComponent[] _objectPrefab;

    // ReSharper disable once MemberCanBePrivate.Global
    public readonly ObjectPoolBehaviour BehaviourObject;
    public readonly TComponent[] Objects;
    private int _currentPosition = -1;

    public ObjectPool(TComponent objectPrefab, ushort capacity = 1, int initializeFrameStep = 0)
    {
        if (!objectPrefab)
        {
            Debug.LogError($"{nameof(objectPrefab)} Can not be null!");
            return;
        }

        if (capacity < 1)
        {
            Debug.LogError($"{nameof(capacity)} should be 1 or higher.");
            return;
        }

        BehaviourObject = new GameObject($"{nameof(ObjectPool<TComponent>)} of {objectPrefab.name}")
            .AddComponent<ObjectPoolBehaviour>();
        _objectPrefab = new[] {objectPrefab};
        _capacity = capacity;
        Objects = new TComponent[capacity];
        if (initializeFrameStep > 0) Initialize(initializeFrameStep);
    }

    public ObjectPool(TComponent[] objectPrefab, ushort capacity = 1, int initializeFrameStep = 0)
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

        BehaviourObject = new GameObject($"{nameof(ObjectPool<TComponent>)} of {objectPrefab[0].name},...")
            .AddComponent<ObjectPoolBehaviour>();
        _objectPrefab = objectPrefab;
        _capacity = capacity;
        Objects = new TComponent[capacity];
        if (initializeFrameStep > 0) Initialize(initializeFrameStep);
    }


    public void Dispose()
    {
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public void Initialize(int frameStep = 0)
    {
        BehaviourObject.StartCoroutine(InitializeOnFrameDelayEnumerator(frameStep));
    }

    private IEnumerator InitializeOnFrameDelayEnumerator(int frameStep)
    {
        for (var i = 0; i < _capacity; i++)
        {
            if (Objects[i]) Object.Destroy(Objects[i].gameObject);
            if (_objectPrefab == null || _objectPrefab.Length < 1) yield break;
            var randomPrefab = _objectPrefab[Random.Range(0, _objectPrefab.Length)];
            if (!randomPrefab) yield break;
            Objects[i] = Object.Instantiate(randomPrefab.gameObject, BehaviourObject.transform)
                .GetComponent<TComponent>();
            Objects[i].gameObject.SetActive(false);
            for (var c = 0; c < frameStep; c++) yield return null;
        }
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
        TComponent MakeReadyCurrentPosition(int index)
        {
            if (setNewPosition) Objects[index].transform.position = position;
            Objects[index].gameObject.SetActive(true);
            if (deActiveAfter > 0)
                BehaviourObject.StartCoroutine(DeActive(Objects[index].gameObject, deActiveAfter));
            return Objects[index];
        }

        //check is there any inactive
        var tempPosition = _currentPosition;
        for (var c = 0; c < _capacity; c++)
        {
            tempPosition++;
            var index = tempPosition % _capacity;
            if (!Objects[index])
            {
                var randomPrefab = _objectPrefab[Random.Range(0, _objectPrefab.Length)];
                Objects[index] = Object.Instantiate(randomPrefab.gameObject, BehaviourObject.transform)
                    .GetComponent<TComponent>();
                Objects[index].gameObject.SetActive(false);
            }

            if (Objects[index].gameObject.activeInHierarchy) continue;
            _currentPosition = tempPosition;
            return MakeReadyCurrentPosition(index);
        }

        //get next active if not found
        for (var c = 0; c < _capacity; c++)
        {
            var index = ++_currentPosition % _capacity;
            if (!Objects[index]) continue;
            Objects[index].gameObject.SetActive(false);
            return MakeReadyCurrentPosition(index);
        }

        return null;
    }

    public void DeActiveAll()
    {
        foreach (var monoBehaviour in Objects)
            if (monoBehaviour)
                monoBehaviour.gameObject.SetActive(false);
        _currentPosition = -1;
    }

    private void ReleaseUnmanagedResources()
    {
        if (BehaviourObject) Object.Destroy(BehaviourObject.gameObject);
        for (var i = 0; i < _capacity; i++)
            if (Objects[i])
                Object.Destroy(Objects[i].gameObject);
    }

    ~ObjectPool()
    {
        Dispose();
    }
}