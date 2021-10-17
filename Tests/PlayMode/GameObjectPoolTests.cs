using NUnit.Framework;
using UnityEngine;

namespace Tests.PlayMode
{
    public class GameObjectPoolTests
    {
        [Test]
        public void CreateAndCloneTest()
        {
            // Use the Assert class to test conditions
            var testParameter1 = Random.Range(int.MinValue, int.MaxValue);
            var poolCapacity = (ushort) Random.Range(0, 20);
            var prefab = new GameObject("TestObj").AddComponent<GameObjectPoolTestsComponent>();
            prefab.TestParameter1 = testParameter1;
            var pool = new GameObjectPool<GameObjectPoolTestsComponent>(prefab, poolCapacity);
            Object.Destroy(prefab.gameObject);
            //init
            pool.Initialize();
            //test clones
            var passed = true;
            for (var i = 0; i < poolCapacity; i++)
            {
                if (pool.Objects[i].ComponentData.TestParameter1 == testParameter1) continue;
                passed = false;
                break;
            }

            Assert.IsTrue(passed);
            pool.Dispose();
        }

        [Test]
        public void SteppingCheck()
        {
            var poolCapacity = (ushort) Random.Range(0, 20);
            var prefab = new GameObject("TestObj").AddComponent<GameObjectPoolTestsComponent>();
            var pool = new GameObjectPool<GameObjectPoolTestsComponent>(prefab, poolCapacity);
            //init
            pool.Initialize();
            //test clones
            for (var i = 0; i < poolCapacity; i++)
            {
                var obj = pool.GetNext();
                obj.ComponentData.TestParameter1 = i;
            }

            var passed = true;
            for (var i = 0; i < poolCapacity; i++)
            {
                if (pool.ActiveNext().TestParameter1 == i) continue;
                passed = false;
            }

            Assert.IsTrue(passed);
            pool.Dispose();
        }
    }
}