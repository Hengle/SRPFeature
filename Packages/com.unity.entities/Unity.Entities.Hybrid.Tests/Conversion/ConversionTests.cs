using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;
using Unity.Collections;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.TestTools;
using static Unity.Entities.GameObjectConversionUtility;
using UnityObject = UnityEngine.Object;

namespace Unity.Entities.Tests.Conversion
{
    class ConversionTests : ConversionTestFixtureBase
    {
        [Test]
        public void ConversionIgnoresMissingMonoBehaviour()
        {
            LogAssert.Expect(LogType.Warning, new Regex("missing"));

            var entity = ConvertGameObjectHierarchy(LoadPrefab("Prefab_MissingMB"), MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager, EntityMatch.Any(entity));
        }

        [Test]
        public void ConversionOfGameObject()
        {
            var gameObject = CreateGameObject();
            var entity = ConvertGameObjectHierarchy(gameObject, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager, EntityMatch.Exact(entity, k_RootComponents));
        }

        [Test]
        public void ConversionOfStaticGameObject()
        {
            var gameObject = CreateGameObject("", typeof(StaticOptimizeEntity));
            var entity = ConvertGameObjectHierarchy(gameObject, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager, EntityMatch.Exact<Static, LocalToWorld, LinkedEntityGroup>(entity));
        }

        [Test]
        public void ConversionOfInactiveStaticGameObject()
        {
            var gameObject = CreateGameObject("", typeof(StaticOptimizeEntity));
            var child = CreateGameObject("");
            child.transform.parent = gameObject.transform;

            gameObject.SetActive(false);
            var entity = ConvertGameObjectHierarchy(gameObject, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<Disabled, Static, LocalToWorld, LinkedEntityGroup>(entity),
                EntityMatch.Exact<Disabled, Static, LocalToWorld>());
        }

        [Test]
        public void ConversionIsBuildingForEditor()
        {
            var gameObject = CreateGameObject("", typeof(TestComponentAuthoringIsBuildingForEditor));
            ConvertGameObjectHierarchy(gameObject, MakeDefaultSettings());
            EntitiesAssert.ContainsOnly(m_Manager, EntityMatch.Partial(new IntTestData(1)));
        }

        [Test]
        public void ConversionOfPrefabIsEntityPrefab()
        {
            var entity = ConvertGameObjectHierarchy(LoadPrefab("Prefab"), MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<Prefab, MockData>(entity, k_RootComponents));
        }

        [Test]
        public void ConversionOfNullGameObjectReference()
        {
            var go = CreateGameObject();
            go.AddComponent<EntityRefTestDataAuthoring>();

            var entity = ConvertGameObjectHierarchy(go, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<LocalToWorld, Translation, Rotation, LinkedEntityGroup>(entity, new EntityRefTestData()));
        }

        [Test]
        public void ConversionOfPrefabReferenceOtherPrefab()
        {
            var go = CreateGameObject();
            go.AddComponent<EntityRefTestDataAuthoring>().Value = LoadPrefab("Prefab_Reference_Prefab");

            var entity = ConvertGameObjectHierarchy(go, MakeDefaultSettings());
            var referenced = m_Manager.GetComponentData<EntityRefTestData>(entity).Value;
            var referenced2 = m_Manager.GetComponentData<EntityRefTestData>(referenced).Value;

            EntitiesAssert.ContainsOnly(m_Manager,
                // gameobject created above
                EntityMatch.Exact<EntityRefTestData>(entity, k_RootComponents),
                // Prefab_Reference_Prefab.prefab
                EntityMatch.Exact<EntityRefTestData, Prefab>(referenced, new MockData(1), k_RootComponents),
                // Prefab.prefab
                EntityMatch.Exact<Prefab>(referenced2, new MockData(), k_RootComponents));
        }

        [Test, Ignore("DOTS-2092 - Not implemented")]
        public void ConversionOfScriptableObjectReferenceOtherScriptableObject()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void ConversionOfPrefabSelfReference()
        {
            var go = CreateGameObject();
            go.AddComponent<EntityRefTestDataAuthoring>().Value = LoadPrefab("Prefab_Reference_Self");

            var entity = ConvertGameObjectHierarchy(go, MakeDefaultSettings());
            var referenced = m_Manager.GetComponentData<EntityRefTestData>(entity).Value;

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<EntityRefTestData>(entity, k_RootComponents),
                EntityMatch.Exact<Prefab, MockData>(referenced, new EntityRefTestData { Value = referenced }, k_RootComponents));
        }

        [Test, Ignore("DOTS-2092 - Not implemented")]
        public void ConversionOfScriptableObjectSelfReference()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void GameObjectReferenceOutsideConvertedGroupWarning()
        {
            LogAssert.Expect(LogType.Warning, new Regex("not included in the conversion"));
            var go = CreateGameObject();

            var notIncluded = CreateGameObject();
            go.AddComponent<EntityRefTestDataAuthoring>().Value = notIncluded;

            var entity = ConvertGameObjectHierarchy(go, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact(entity, new EntityRefTestData(), k_RootComponents));
        }

        [Test, Ignore("DOTS-2092 - Not implemented")]
        public void AssetReferenceOutsideConvertedGroupWarning()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void SetEnabledOnPrefabOnCompleteSet()
        {
            var entity = ConvertGameObjectHierarchy(LoadPrefab("Prefab_Hierarchy"), MakeDefaultSettings());
            var instance = m_Manager.Instantiate(entity);

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<Prefab>(new MockData(100), k_RootComponents, entity),
                EntityMatch.Exact<Prefab>(new MockData(101), k_ChildComponents),
                EntityMatch.Exact(new MockData(100), k_RootComponents, instance),
                EntityMatch.Exact(new MockData(101), k_ChildComponents));

            m_Manager.SetEnabled(instance, false);

            EntitiesAssert.Contains(m_Manager,
                EntityMatch.Exact<Disabled, MockData>(k_RootComponents, instance),
                EntityMatch.Exact<Disabled, MockData>(k_ChildComponents));

            m_Manager.SetEnabled(instance, true);

            EntitiesAssert.Contains(m_Manager,
                EntityMatch.Exact<MockData>(k_RootComponents, instance),
                EntityMatch.Exact<MockData>(k_ChildComponents));
        }

        [Test]
        public void DestroyEntity_WithInstantiatedPrefabHierarchy_DestroysEntireHierarchy()
        {
            var entity = ConvertGameObjectHierarchy(LoadPrefab("Prefab_Hierarchy"), MakeDefaultSettings());
            var instance = m_Manager.Instantiate(entity);

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<Prefab>(new MockData(100), k_RootComponents, entity),
                EntityMatch.Exact<Prefab>(new MockData(101), k_ChildComponents),
                EntityMatch.Exact(new MockData(100), k_RootComponents, instance),
                EntityMatch.Exact(new MockData(101), k_ChildComponents));

            m_Manager.DestroyEntity(instance);

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<Prefab>(new MockData(100), k_RootComponents, entity),
                EntityMatch.Exact<Prefab>(new MockData(101), k_ChildComponents));
        }

        [Test]
        public void InactiveHierarchyBecomesPartOfLinkedEntityGroupSet()
        {
            var go = CreateGameObject();
            var child = CreateGameObject();
            var childChild = CreateGameObject();

            child.SetActive(false);
            go.AddComponent<EntityRefTestDataAuthoring>().Value = child;
            child.transform.parent = go.transform;
            childChild.transform.parent = child.transform;

            var entity = ConvertGameObjectHierarchy(go, MakeDefaultSettings());
            var childEntity = m_Manager.GetComponentData<EntityRefTestData>(entity).Value;

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<EntityRefTestData, LinkedEntityGroup>(k_CommonComponents, entity),
                EntityMatch.Exact<Disabled,          LinkedEntityGroup>(k_ChildComponents, childEntity),
                EntityMatch.Exact<Disabled>(k_ChildComponents));

            // Conversion will automatically add a LinkedEntityGroup to all inactive children
            // so that when enabling them, the whole hierarchy will get enabled
            m_Manager.SetEnabled(m_Manager.GetComponentData<EntityRefTestData>(entity).Value, true);

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<EntityRefTestData, LinkedEntityGroup>(k_CommonComponents, entity),
                EntityMatch.Exact<LinkedEntityGroup>(k_ChildComponents, childEntity),
                EntityMatch.Exact(k_ChildComponents));
        }

        [Test]
        public void InactiveConversion()
        {
            var gameObject = CreateGameObject();
            var child = CreateGameObject();
            child.transform.parent = gameObject.transform;
            gameObject.gameObject.SetActive(false);

            var entity = ConvertGameObjectHierarchy(gameObject, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<Disabled>(entity, k_RootComponents),
                EntityMatch.Exact<Disabled>(k_ChildComponents));

            Assert.That(Entities.WithAll<Translation>().ToEntityQuery().CalculateEntityCount(), Is.Zero);
        }

        [Test]
        public void DisabledBehaviourStripping()
        {
            var gameObject = new GameObject();
            gameObject.AddComponent<MockDataAuthoring>().enabled = false;
            gameObject.AddComponent<EntityRefTestDataAuthoring>().enabled = false;

            var strippedEntity = ConvertGameObjectHierarchy(gameObject, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager, EntityMatch.Exact(strippedEntity, k_RootComponents));
        }

        [Test]
        public void DuplicateComponentOnRootGameObject()
        {
            var gameObject = new GameObject();

            gameObject.AddComponent<EntityRefTestDataAuthoring>();
            gameObject.AddComponent<EntityRefTestDataAuthoring>();

            var convertedEntity = ConvertGameObjectHierarchy(gameObject, MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<EntityRefTestData>(convertedEntity, k_RootComponents));
        }

        [UpdateInGroup(typeof(GameObjectAfterConversionGroup))]
        internal class TestConversionCleanup : GameObjectConversionSystem
        {
            protected override void OnUpdate()
            {
                Entities.ForEach((Transform transform) =>
                {
                    var entity = GetPrimaryEntity(transform);
                    if (DstEntityManager.HasComponent<Parent>(entity))
                        DstEntityManager.DestroyEntity(entity);
                });
            }
        }

        [Test]
        public void DeletingEntitiesOfConvertedPrefab_DoesNotThrow()
        {
            var prefabEntity = Entity.Null;

            var settings = MakeDefaultSettings();

            settings.ConversionWorldCreated = CreateSystem;
            settings.ConversionWorldPreDispose = DestroySystem;

            Assert.DoesNotThrow(() => prefabEntity = ConvertGameObjectHierarchy(LoadPrefab("Prefab_Hierarchy"), settings));

            Assert.DoesNotThrow(() => m_Manager.Instantiate(prefabEntity));

            void CreateSystem(World world)
            {
                var gameObjectAfterConversionGroup = world.GetExistingSystem<GameObjectAfterConversionGroup>();
                var testConversionCleanup = world.GetOrCreateSystem<TestConversionCleanup>();

                gameObjectAfterConversionGroup.AddSystemToUpdateList(testConversionCleanup);
            }

            void DestroySystem(World world)
            {
                var testConversionCleanup = world.GetOrCreateSystem<TestConversionCleanup>();
                world.DestroySystem(testConversionCleanup);
            }
        }

        struct TestEntityData : IComponentData
        {
            public int Index;

            public TestEntityData(int index)
            {
                Index = index;
            }
        }

        class CreateAdditionalEntitySystem : GameObjectConversionSystem
        {
            protected override void OnUpdate()
            {
                Entities.ForEach((Transform t) =>
                {
                    var e = CreateAdditionalEntity(t);
                    DstEntityManager.AddComponent<TestEntityData>(e);
                });
            }
        }

        class CreateAdditionalEntityMultipleSystem : GameObjectConversionSystem
        {
            protected override void OnUpdate()
            {
                Entities.ForEach((Transform t) =>
                {
                    var entities = new NativeArray<Entity>(3, Allocator.Temp);
                    CreateAdditionalEntity(t, entities);
                    for (int i = 0; i < entities.Length; i++)
                        DstEntityManager.AddComponentData(entities[i], new TestEntityData(i));
                });
            }
        }

        [Test]
        public void CreateAdditionalEntity_CreatesAdditionalEntity()
        {
            var go = CreateGameObject();
            var entity = ConvertGameObjectHierarchy(go, MakeDefaultSettings().WithExtraSystem<CreateAdditionalEntitySystem>());
            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact(entity, k_RootComponents),
                EntityMatch.Exact(new TestEntityData(0))
            );
        }

        [Test]
        public void CreateAdditionalEntity_WithMultiple_CreatesAdditionalEntity()
        {
            var go = CreateGameObject();
            var entity = ConvertGameObjectHierarchy(go, MakeDefaultSettings().WithExtraSystem<CreateAdditionalEntityMultipleSystem>());
            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact(entity, k_RootComponents),
                EntityMatch.Exact(new TestEntityData(0)),
                EntityMatch.Exact(new TestEntityData(1)),
                EntityMatch.Exact(new TestEntityData(2))
            );
        }

        [Test]
        public void PrefabHierarchy_WithStaticOptimizeEntity_ActuallyIsStatic()
        {
            var entity = ConvertGameObjectHierarchy(LoadPrefab("Prefab_Hierarchy_With_StaticOptimizeEntity"), MakeDefaultSettings());

            EntitiesAssert.ContainsOnly(m_Manager,
                EntityMatch.Exact<Prefab, Disabled>(entity, k_StaticRootComponents),
                EntityMatch.Exact<Prefab, Disabled>(k_StaticComponents));
        }
    }
}
