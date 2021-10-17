using System;
using NUnit.Framework;
using UnityEngine;
using Random = UnityEngine.Random;

// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Tests.Runtime
{
    public class ObjectPoolTest
    {
        [Test]
        public void CreateAndCloneTest()
        {
            var testParameter1 = Random.Range(int.MinValue, int.MaxValue);
            var poolCapacity = (ushort) Random.Range(0, 20);
            var pool = new ObjectPool<PoolTestClassItem>(new PoolTestClassItem
            {
                TestParameter1 = testParameter1
            }, poolCapacity);
            //init
            pool.Initialize();
            //test clones
            var passed = true;
            for (var i = 0; i < poolCapacity; i++)
            {
                if (pool.Objects[i].TestParameter1 == testParameter1) continue;
                passed = false;
                break;
            }

            Assert.IsTrue(passed);
        }

        [Test]
        public void SteppingCheck()
        {
            var poolCapacity = (ushort) Random.Range(0, 20);
            var pool = new ObjectPool<PoolTestClassItem>(new PoolTestClassItem
            {
                TestParameter1 = 0
            }, poolCapacity);
            //init
            pool.Initialize();
            //test clones
            for (var i = 0; i < poolCapacity; i++)
            {
                var obj = pool.GetNext();
                obj.TestParameter1 = i;
            }

            var passed = true;
            for (var i = 0; i < poolCapacity; i++)
            {
                if (pool.GetNext().TestParameter1 == i) continue;
                passed = false;
                Debug.Log(i);
                break;
            }

            Assert.IsTrue(passed);
        }

        private class PoolTestClassItem : IDisposable, ICloneable
        {
            public float TestParameter1;

            public object Clone()
            {
                return new PoolTestClassItem {TestParameter1 = TestParameter1};
            }

            public void Dispose()
            {
            }
        }
    }
}