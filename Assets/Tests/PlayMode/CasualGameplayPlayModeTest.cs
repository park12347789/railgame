using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Railgame.Tests
{
    public sealed class CasualGameplayPlayModeTest : InputTestFixture
    {
        [UnityTest]
        public IEnumerator LobbyStartAlwaysLoadsSpringFoundation()
        {
            Time.timeScale = 1f;
            InputSystem.AddDevice<Keyboard>();
            SceneManager.LoadScene("Railgame_Lobby");
            yield return null;
            Click("StartButton");
            while (SceneManager.GetActiveScene().name != "Map_Procedural_Spring") yield return null;
            yield return null;

            Assert.That(Find("Railgame.Player.RailgamePlayerController"), Is.Not.Null);
            Assert.That(Find("Railgame.Player.WaterSlowVolume"), Is.Not.Null);
            Assert.That(Find("Railgame.UI.RailgameGameMenuController"), Is.Not.Null);
            Component generator = Find("Railgame.Map.ProceduralMapGenerator");
            int selectedIndex = StaticProperty<int>(generator.GetType(), "SelectedVariantIndex");
            Assert.That(selectedIndex, Is.InRange(0, 4));
            AssertCuratedVariant(generator, selectedIndex);
        }

        [UnityTest]
        public IEnumerator SpringStationCheckoutLoadsSavedSummerVariant()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Railgame_Lobby");
            yield return null;
            Click("StartButton");
            while (SceneManager.GetActiveScene().name != "Map_Procedural_Spring") yield return null;
            yield return null;

            Component flow = Find("Railgame.Campaign.RailgameStageFlowController");
            Invoke(flow, "CompleteAtStation");
            Component checkout = Find("Railgame.Shop.RailgameShopCheckout");
            Assert.That(checkout, Is.Not.Null);
            Assert.That(checkout.gameObject.activeInHierarchy, Is.True);
            Assert.That(Property<bool>(checkout, "CanDepart"), Is.True);
            Assert.That((bool)Invoke(checkout, "TryCheckout"), Is.True);

            while (SceneManager.GetActiveScene().name != "Map_Procedural_Summer") yield return null;
            yield return null;
            Component generator = Find("Railgame.Map.ProceduralMapGenerator");
            Object session = Resources.Load("RailgameCampaignSession");
            int summerIndex = Property<int>(session, "SummerVariantIndex");
            Assert.That(StaticProperty<int>(generator.GetType(), "SelectedVariantIndex"), Is.EqualTo(summerIndex));
            AssertCuratedVariant(generator, summerIndex);
        }

        [UnityTest]
        public IEnumerator LobbyToSpringCoreLoopWorks()
        {
            Time.timeScale = 1f;
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            SceneManager.LoadScene("Railgame_Lobby");
            yield return null;

            Assert.That(Find("Railgame.UI.RailgameLobbyController"), Is.Not.Null);
            Click("SettingsButton");
            Component settings = Find("Railgame.UI.RailgameSettingsPanel");
            Assert.That(settings.gameObject.activeSelf, Is.True);
            Invoke(settings, "CancelAndClose");
            Assert.That(settings.gameObject.activeSelf, Is.False);

            Click("StartButton");
            while (SceneManager.GetActiveScene().name != "Map_Procedural_Spring")
                yield return null;
            yield return null;

            Component player = Find("Railgame.Player.RailgamePlayerController");
            Assert.That(player, Is.Not.Null);

            Vector3 playerStart = player.transform.position;
            for (int frame = 0; frame < 20; frame++)
            {
                Invoke(player, "SimulateInput", Vector2.up, false, 0.02f);
                yield return null;
            }
            Assert.That(player.transform.position.z - playerStart.z, Is.GreaterThan(0.5f));

            Component generator = Find("Railgame.Map.ProceduralMapGenerator");
            foreach (Component slot in FindAll("Railgame.Map.ResourceSpawnSlot"))
                slot.gameObject.SetActive(false);
            Physics.SyncTransforms();
            CharacterController controller = player.GetComponent<CharacterController>();
            bool climbed = false;
            Vector2Int[] directions = { Vector2Int.left, Vector2Int.right, Vector2Int.down, Vector2Int.up };
            foreach (Component dirtBlock in FindAll("Railgame.Map.DirtBlock"))
            {
                Vector2Int cell = Property<Vector2Int>(dirtBlock, "Cell");
                foreach (Vector2Int direction in directions)
                {
                    Vector3 target = generator.transform.TransformPoint(new Vector3(cell.x + 0.5f, 2f, cell.y + 0.5f));
                    Vector3 rayStart = generator.transform.TransformPoint(new Vector3(cell.x + 0.5f + direction.x, 3f,
                        cell.y + 0.5f + direction.y));
                    if (!Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, 3f) ||
                        hit.collider.GetComponentInParent(FindType("Railgame.Map.DirtBlock")) != null ||
                        Mathf.Abs(hit.point.y - 1f) > 0.12f)
                        continue;

                    Teleport(controller, hit.point + Vector3.up * 0.04f);
                    Field(player, "verticalVelocity", 0f);
                    for (int frame = 0; frame < 8; frame++) yield return null;
                    Invoke(player, "SimulateInput", Vector2.zero, false, 0.02f);
                    Assert.That(Property<bool>(player, "IsGrounded"), Is.True, "Player did not settle before jump.");
                    Behaviour playerBehaviour = (Behaviour)player;
                    playerBehaviour.enabled = false;
                    float maximumY = player.transform.position.y;
                    for (int frame = 0; frame < 100; frame++)
                    {
                        Vector3 delta = target - player.transform.position;
                        Vector2 input = new(delta.x, delta.z);
                        if (input.magnitude > 0.15f)
                            input = input.normalized * 0.35f;
                        else
                            input = Vector2.zero;
                        Invoke(player, "SimulateInput", input, frame == 0, 0.02f);
                        yield return null;
                        maximumY = Mathf.Max(maximumY, player.transform.position.y);
                    }
                    playerBehaviour.enabled = true;
                    float settleDeadline = Time.time + 0.75f;
                    while (Time.time < settleDeadline) yield return null;

                    float planarDistance = Vector2.Distance(new Vector2(player.transform.position.x, player.transform.position.z),
                        new Vector2(target.x, target.z));
                    climbed = Mathf.Abs(player.transform.position.y - 2f) <= 0.12f && planarDistance <= 0.5f;
                    if (!climbed)
                        Debug.Log($"DIRT_CLIMB_DIAGNOSTIC cell={cell} maxY={maximumY:F3} finalY={player.transform.position.y:F3} distance={planarDistance:F3}");
                    if (climbed) break;
                }
                if (climbed) break;
            }
            Assert.That(climbed, Is.True, "Player could not jump onto a generated 1m dirt block.");

            Component water = Find("Railgame.Player.WaterSlowVolume");
            Teleport(controller, water.GetComponent<Collider>().bounds.center + Vector3.up * 3f);
            float deadline = Time.realtimeSinceStartup + 8f;
            while ((!Property<bool>(player, "IsInWater") || player.transform.position.y > 0.15f) &&
                   Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(Property<bool>(player, "IsInWater"), Is.True);
            Assert.That(player.transform.position.y, Is.GreaterThanOrEqualTo(-0.05f));
            Assert.That(Property<float>(player, "EffectiveMoveSpeed"), Is.LessThan(Property<float>(player, "MoveSpeed")));

            Component navigation = Find("Railgame.Map.RuntimeNavigationController");
            int updateCount = Property<int>(navigation, "CompletedUpdateCount");
            bool mined = false;
            foreach (Component dirt in FindAll("Railgame.Map.DirtBlock"))
            {
                if (!(bool)Invoke(dirt, "Mine")) continue;
                mined = true;
                break;
            }
            Assert.That(mined, Is.True, "No generated dirt block was mineable.");
            deadline = Time.realtimeSinceStartup + 12f;
            while (Property<int>(navigation, "CompletedUpdateCount") == updateCount && Time.realtimeSinceStartup < deadline)
                yield return null;
            Assert.That(Property<int>(navigation, "CompletedUpdateCount"), Is.GreaterThan(updateCount));
            Component menu = Find("Railgame.UI.RailgameGameMenuController");
            Press(keyboard.escapeKey);
            InputSystem.Update();
            Assert.That(keyboard.escapeKey.wasPressedThisFrame, Is.True, "Escape test input was not delivered.");
            Invoke(menu, "Update");
            Assert.That(Time.timeScale, Is.Zero, "Escape did not pause gameplay.");
            Release(keyboard.escapeKey);
            InputSystem.Update();
            yield return null;

            Invoke(menu, "Resume");
            Assert.That(Time.timeScale, Is.EqualTo(1f));
        }

        [UnityTest]
        public IEnumerator EveryCuratedSeasonVariantGeneratesExpectedMap()
        {
            Time.timeScale = 1f;
            Type generatorType = FindType("Railgame.Map.ProceduralMapGenerator");
            foreach (string sceneName in new[] { "Map_Procedural_Spring", "Map_Procedural_Summer" })
            for (int index = 0; index < 5; index++)
            {
                PrepareCampaign(sceneName.EndsWith("Spring", StringComparison.Ordinal));
                InvokeStatic(generatorType, "SelectVariant", index);
                SceneManager.LoadScene(sceneName);
                yield return null;

                Component generator = Find("Railgame.Map.ProceduralMapGenerator");
                AssertCuratedVariant(generator, index);
                Assert.That(Property<int>(generator, "GeneratedTreeCount"), Is.EqualTo(96));
                Assert.That(Property<int>(generator, "GeneratedIronCount"), Is.EqualTo(96));
                Assert.That(Property<int>(generator, "GeneratedResourceClusterCount"), Is.EqualTo(24));
                Assert.That(Property<int>(generator, "GeneratedEnemySpawnMarkerCount"), Is.EqualTo(8));
                Assert.That((bool)Invoke(generator, "HasCompleteMovementPath"), Is.True);
                Assert.That((bool)Invoke(generator, "HasRailPathAfterMining"), Is.True);
            }
            InvokeStatic(generatorType, "SelectVariant", 0);
        }

        [UnityTest]
        public IEnumerator RestartKeepsCuratedVariant()
        {
            Time.timeScale = 1f;
            Type generatorType = FindType("Railgame.Map.ProceduralMapGenerator");
            Object session = PrepareCampaign(true);
            int selectedIndex = Property<int>(session, "SpringVariantIndex");
            InvokeStatic(generatorType, "SelectVariant", selectedIndex);
            SceneManager.LoadScene("Map_Procedural_Spring");
            yield return null;

            Component before = Find("Railgame.Map.ProceduralMapGenerator");
            int seed = Property<int>(before, "WorldSeed");
            string hash = Property<string>(before, "LastLayoutHash");
            Component menu = Find("Railgame.UI.RailgameGameMenuController");
            Invoke(menu, "Restart");
            yield return null;
            yield return null;

            Component after = Find("Railgame.Map.ProceduralMapGenerator");
            Assert.That(StaticProperty<int>(generatorType, "SelectedVariantIndex"), Is.EqualTo(selectedIndex));
            Assert.That(Property<int>(after, "WorldSeed"), Is.EqualTo(seed));
            Assert.That(Property<string>(after, "LastLayoutHash"), Is.EqualTo(hash));
            InvokeStatic(generatorType, "SelectVariant", 0);
        }

        private static void AssertCuratedVariant(Component generator, int index)
        {
            Object profile = Property<Object>(generator, "Profile");
            object variant = profile.GetType().GetMethod("GetCuratedVariant")?.Invoke(profile, new object[] { index });
            Assert.That(variant, Is.Not.Null);
            int expectedSeed = (int)variant.GetType().GetProperty("Seed")?.GetValue(variant);
            string expectedHash = (string)variant.GetType().GetProperty("ExpectedLayoutHash")?.GetValue(variant);
            Assert.That(Property<int>(generator, "WorldSeed"), Is.EqualTo(expectedSeed));
            Assert.That(Property<string>(generator, "LastLayoutHash"), Is.EqualTo(expectedHash));
        }

        private static Object PrepareCampaign(bool spring)
        {
            Object session = Resources.Load("RailgameCampaignSession");
            Assert.That(session, Is.Not.Null, "Campaign session resource missing.");
            Invoke(session, "ResetToLobby");
            Invoke(session, "StartNewRun");
            if (spring)
                return session;
            Invoke(session, "MarkStageLoaded");
            Invoke(session, "CompleteStage");
            Invoke(session, "ContinueFromShop");
            return session;
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetType(name) is Type type)
                    return type;
            throw new InvalidOperationException($"Type not found: {name}");
        }

        private static Component Find(string typeName)
        {
            Type type = FindType(typeName);
            foreach (Object item in Resources.FindObjectsOfTypeAll(type))
                if (item is Component component && component.gameObject.scene.IsValid())
                    return component;
            return null;
        }

        private static Component[] FindAll(string typeName)
        {
            Type type = FindType(typeName);
            var results = new System.Collections.Generic.List<Component>();
            foreach (Object item in Resources.FindObjectsOfTypeAll(type))
                if (item is Component component && component.gameObject.scene.IsValid())
                    results.Add(component);
            return results.ToArray();
        }

        private static object Invoke(Component target, string method, params object[] args) =>
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.Invoke(target, args);

        private static object Invoke(Object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.Invoke(target, args);

        private static T Property<T>(Object target, string name) =>
            (T)target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?.GetValue(target);

        private static T StaticProperty<T>(Type type, string name) =>
            (T)type.GetProperty(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.GetValue(null);

        private static object InvokeStatic(Type type, string method, params object[] args) =>
            type.GetMethod(method, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)?.Invoke(null, args);

        private static void Field(Object target, string name, object value) =>
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.SetValue(target, value);

        private static void Click(string name)
        {
            GameObject button = GameObject.Find(name);
            Assert.That(button, Is.Not.Null, $"Button missing: {name}");
            Component component = button.GetComponent(FindType("UnityEngine.UI.Button"));
            object onClick = component.GetType().GetProperty("onClick")?.GetValue(component);
            onClick?.GetType().GetMethod("Invoke")?.Invoke(onClick, null);
        }

        private static void Teleport(CharacterController controller, Vector3 position)
        {
            controller.enabled = false;
            controller.transform.position = position;
            controller.enabled = true;
            Physics.SyncTransforms();
        }
    }
}
