using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace Railgame.Tests
{
    public sealed class TeamFeatureRegressionTests
    {
        [Test]
        public void Bogyeong_RailDirectionsRemainOrthogonalAndReversible()
        {
            Type directionType = FindType("GridDir");
            Type extensionsType = FindType("GridDirExtensions");
            MethodInfo toOffset = extensionsType.GetMethod("ToOffset", BindingFlags.Public | BindingFlags.Static);
            MethodInfo opposite = extensionsType.GetMethod("Opposite", BindingFlags.Public | BindingFlags.Static);
            Assert.That(toOffset, Is.Not.Null);
            Assert.That(opposite, Is.Not.Null);

            AssertOffset("North", new Vector2Int(0, 1));
            AssertOffset("East", new Vector2Int(1, 0));
            AssertOffset("South", new Vector2Int(0, -1));
            AssertOffset("West", new Vector2Int(-1, 0));

            foreach (object direction in Enum.GetValues(directionType))
            {
                object reverse = opposite.Invoke(null, new[] { direction });
                object original = opposite.Invoke(null, new[] { reverse });
                Assert.That(original, Is.EqualTo(direction));
            }

            void AssertOffset(string name, Vector2Int expected)
            {
                object direction = Enum.Parse(directionType, name);
                Assert.That((Vector2Int)toOffset.Invoke(null, new[] { direction }), Is.EqualTo(expected));
            }
        }

        [Test]
        public void Bogyeong_RailBlockEvaluatesStraightCurveAndLock()
        {
            Type railBlockType = FindType("RailBlock");
            Type directionType = FindType("GridDir");
            GameObject railObject = new GameObject("RailBlock_Test");
            try
            {
                Component rail = railObject.AddComponent(railBlockType);
                MethodInfo applyShape = railBlockType.GetMethod("ApplyShape");
                MethodInfo evaluate = railBlockType.GetMethod("EvaluateLocal");
                object west = Enum.Parse(directionType, "West");
                object east = Enum.Parse(directionType, "East");
                object north = Enum.Parse(directionType, "North");

                applyShape.Invoke(rail, new[] { west, east });
                Assert.That(railBlockType.GetProperty("DerivedType").GetValue(rail).ToString(), Is.EqualTo("Straight"));
                Pose straightMidpoint = (Pose)evaluate.Invoke(rail, new object[] { west, east, 0.5f, 2f });
                Assert.That(straightMidpoint.position.sqrMagnitude, Is.LessThan(0.000001f));

                applyShape.Invoke(rail, new[] { west, north });
                Assert.That(railBlockType.GetProperty("DerivedType").GetValue(rail).ToString(), Is.EqualTo("Curve90"));
                Pose curveMidpoint = (Pose)evaluate.Invoke(rail, new object[] { west, north, 0.5f, 2f });
                Assert.That(curveMidpoint.position.x, Is.LessThan(0f));
                Assert.That(curveMidpoint.position.z, Is.GreaterThan(0f));

                railBlockType.GetMethod("Lock").Invoke(rail, null);
                Assert.That((bool)railBlockType.GetProperty("IsLocked").GetValue(rail), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(railObject);
            }
        }

        [Test]
        public void Seokmin_RuntimeStatsScaleDamageHealAndClamp()
        {
            Type dataType = FindType("RailGame.Enemy.Data.EnemyDataSO");
            Type statsType = FindType("RailGame.Enemy.Runtime.EnemyRuntimeStats");
            ScriptableObject data = ScriptableObject.CreateInstance(dataType);
            try
            {
                dataType.GetField("maxHealth").SetValue(data, 10f);
                dataType.GetField("attackPower").SetValue(data, 3f);
                dataType.GetField("moveSpeed").SetValue(data, 4f);
                object stats = Activator.CreateInstance(statsType, data, 2f);

                Assert.That(GetFloat(statsType, stats, "MaxHealth"), Is.EqualTo(20f));
                Assert.That(GetFloat(statsType, stats, "AttackPower"), Is.EqualTo(6f));
                statsType.GetMethod("TakeDamage").Invoke(stats, new object[] { 7f });
                statsType.GetMethod("Heal").Invoke(stats, new object[] { 2f });
                statsType.GetMethod("ModifyMoveSpeed").Invoke(stats, new object[] { 0.5f });
                statsType.GetMethod("TakeDamage").Invoke(stats, new object[] { 100f });

                Assert.That(GetFloat(statsType, stats, "CurrentHealth"), Is.Zero);
                Assert.That(GetFloat(statsType, stats, "MoveSpeed"), Is.EqualTo(2f));
                Assert.That((bool)statsType.GetProperty("IsDead").GetValue(stats), Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(data);
            }
        }

        [Test]
        public void Seokmin_StateContextMeasuresPlanarTargetDistance()
        {
            Type contextType = FindType("RailGame.Enemy.StateMachine.EnemyStateContext");
            GameObject self = new GameObject("EnemySelf_Test");
            GameObject target = new GameObject("EnemyTarget_Test");
            try
            {
                self.transform.position = new Vector3(1f, 10f, 2f);
                target.transform.position = new Vector3(4f, -5f, 6f);
                object context = Activator.CreateInstance(contextType);
                contextType.GetField("Self").SetValue(context, self.transform);
                contextType.GetField("Target").SetValue(context, target.transform);

                Assert.That(GetFloat(contextType, context, "DistanceToTarget"), Is.EqualTo(5f).Within(0.0001f));
                contextType.GetField("Target").SetValue(context, null);
                Assert.That(GetFloat(contextType, context, "DistanceToTarget"), Is.EqualTo(float.MaxValue));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(self);
                UnityEngine.Object.DestroyImmediate(target);
            }
        }

        [Test]
        public void EnemyObstructionDirectorSpawnsAtMarkerWithoutTrainDependency()
        {
            Type markerType = FindType("Railgame.Enemy.EnemySpawnMarker");
            Type directorType = FindType("Railgame.Enemy.RailgameEnemyObstructionDirector");
            GameObject root = new GameObject("ObstructionDirector_Test");
            GameObject markerObject = new GameObject("SpawnMarker_Test");
            GameObject spawnPoint = new GameObject("SpawnPoint_Test");
            GameObject prefab = new GameObject("EnemyPrefab_Test");
            GameObject spawned = null;
            root.SetActive(false);
            try
            {
                spawnPoint.transform.position = new Vector3(3f, 0f, 7f);
                Component marker = markerObject.AddComponent(markerType);
                markerType.GetMethod("Initialize").Invoke(marker,
                    new object[] { spawnPoint.transform, markerObject.transform, 0, true });

                Array markerArray = Array.CreateInstance(markerType, 1);
                markerArray.SetValue(marker, 0);
                Component director = root.AddComponent(directorType);
                directorType.GetMethod("Initialize").Invoke(director,
                    new object[] { new[] { prefab }, markerArray, null, false });

                spawned = (GameObject)directorType.GetMethod("SpawnOnce").Invoke(director, null);
                Assert.That(spawned, Is.Not.Null);
                Assert.That(spawned.transform.position, Is.EqualTo(spawnPoint.transform.position));
                Assert.That((int)directorType.GetProperty("AliveCount").GetValue(director), Is.EqualTo(1));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(spawned);
                UnityEngine.Object.DestroyImmediate(prefab);
                UnityEngine.Object.DestroyImmediate(spawnPoint);
                UnityEngine.Object.DestroyImmediate(markerObject);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void MainHudUpdatesIndependentModulesWithoutGameplayDependencies()
        {
            Type presenterType = FindType("Railgame.UI.RailgameHudPresenter");
            GameObject root = new GameObject("MainHud_Test");
            root.SetActive(false);
            Text route = CreateText("Route", root.transform);
            Text objective = CreateText("Objective", root.transform);
            Text bolts = CreateText("Bolts", root.transform);
            Text cargo = CreateText("Cargo", root.transform);
            Text interaction = CreateText("Interaction", root.transform);
            Text status = CreateText("Status", root.transform);
            GameObject statusRoot = new GameObject("StatusRoot");
            statusRoot.transform.SetParent(root.transform, false);
            try
            {
                Component presenter = root.AddComponent(presenterType);
                presenterType.GetMethod("Initialize").Invoke(presenter,
                    new object[] { route, objective, bolts, cargo, interaction, status, statusRoot, null });
                presenterType.GetMethod("SetRoute").Invoke(presenter, new object[] { "Pine Station", "River Bend" });
                presenterType.GetMethod("SetObjective").Invoke(presenter, new object[] { "Load two modules" });
                presenterType.GetMethod("SetBolts").Invoke(presenter, new object[] { 7 });
                presenterType.GetMethod("SetCargo").Invoke(presenter, new object[] { "Coolant", "E  Mount" });
                presenterType.GetMethod("SetInteraction").Invoke(presenter, new object[] { "E  Open workshop" });
                presenterType.GetMethod("SetStatus").Invoke(presenter, new object[] { "Route ready" });

                Assert.That(route.text, Does.Contain("PINE STATION").And.Contain("RIVER BEND"));
                Assert.That(objective.text, Does.Contain("LOAD TWO MODULES"));
                Assert.That(bolts.text, Is.EqualTo("BOLTS  07"));
                Assert.That(cargo.text, Does.Contain("COOLANT").And.Contain("E  MOUNT"));
                Assert.That(interaction.text, Is.EqualTo("E  OPEN WORKSHOP"));
                Assert.That(statusRoot.activeSelf, Is.True);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void MainHudRuntimeBridgeTracksRouteAndPhysicalShopState()
        {
            Type presenterType = FindType("Railgame.UI.RailgameHudPresenter");
            Type bridgeType = FindType("Railgame.UI.RailgameHudRuntimeBridge");
            Type sessionType = FindType("Railgame.Campaign.RailgameCampaignSession");
            GameObject root = new GameObject("MainHudBridge_Test");
            root.SetActive(false);
            Text route = CreateText("Route", root.transform);
            Text objective = CreateText("Objective", root.transform);
            Text bolts = CreateText("Bolts", root.transform);
            Text cargo = CreateText("Cargo", root.transform);
            Text interaction = CreateText("Interaction", root.transform);
            Text status = CreateText("Status", root.transform);
            GameObject statusRoot = new GameObject("StatusRoot");
            statusRoot.transform.SetParent(root.transform, false);
            ScriptableObject session = ScriptableObject.CreateInstance(sessionType);
            try
            {
                Component presenter = root.AddComponent(presenterType);
                presenterType.GetMethod("Initialize").Invoke(presenter,
                    new object[] { route, objective, bolts, cargo, interaction, status, statusRoot, null });
                Component bridge = root.AddComponent(bridgeType);
                bridgeType.GetMethod("Initialize").Invoke(bridge,
                    new object[] { presenter, session, null, null });

                sessionType.GetMethod("StartNewRun").Invoke(session, null);
                sessionType.GetMethod("MarkStageLoaded").Invoke(session, null);
                bridgeType.GetMethod("RefreshNow").Invoke(bridge, null);
                Assert.That(route.text, Does.Contain("SPRING LINE").And.Contain("PINE STATION"));
                Assert.That(objective.text, Does.Contain("REACH PINE STATION"));

                sessionType.GetMethod("CompleteStage").Invoke(session, null);
                bridgeType.GetMethod("RefreshNow").Invoke(bridge, null);
                Assert.That(route.text, Does.Contain("WORKSHOP").And.Contain("RIVER BEND"));
                Assert.That(objective.text, Does.Contain("MOUNT UPGRADES OR DEPART"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(session);
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static Text CreateText(string name, Transform parent)
        {
            GameObject item = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            item.transform.SetParent(parent, false);
            return item.GetComponent<Text>();
        }

        private static float GetFloat(Type type, object instance, string propertyName)
        {
            return (float)type.GetProperty(propertyName).GetValue(instance);
        }

        private static Type FindType(string fullName)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type type = assembly.GetType(fullName);
                if (type != null)
                    return type;
            }

            throw new AssertionException($"Type not found: {fullName}");
        }
    }
}
