using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Railgame.Tests
{
    public sealed class RailgameCampaignSessionTests
    {
        private ScriptableObject session;
        private Type sessionType;

        [SetUp]
        public void SetUp()
        {
            sessionType = FindType("Railgame.Campaign.RailgameCampaignSession");
            session = ScriptableObject.CreateInstance(sessionType);
        }

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(session);
        }

        [Test]
        public void NewRunSelectsBothVariantsOnceAndCompletesTwoStages()
        {
            Invoke("StartNewRun");
            int spring = Property<int>("SpringVariantIndex");
            int summer = Property<int>("SummerVariantIndex");
            int variantCount = (int)sessionType.GetField("VariantCount")?.GetRawConstantValue();

            Assert.That(spring, Is.InRange(0, variantCount - 1));
            Assert.That(summer, Is.InRange(0, variantCount - 1));
            AssertState("LoadingSpring");
            Assert.That(Property<object>("CurrentSeason").ToString(), Is.EqualTo("Spring"));
            Assert.That(Property<int>("CurrentVariantIndex"), Is.EqualTo(spring));

            Invoke("MarkStageLoaded");
            Invoke("CompleteStage");
            AssertState("StationShop");
            AssertSelections(spring, summer);

            Invoke("ContinueFromShop");
            AssertState("LoadingSummer");
            Assert.That(Property<object>("CurrentSeason").ToString(), Is.EqualTo("Summer"));
            Assert.That(Property<int>("CurrentVariantIndex"), Is.EqualTo(summer));

            Invoke("MarkStageLoaded");
            Invoke("CompleteStage");
            AssertState("Results");
            AssertSelections(spring, summer);
        }

        [Test]
        public void RetryKeepsSpringAndSummerSelections()
        {
            Invoke("StartNewRun");
            int spring = Property<int>("SpringVariantIndex");
            int summer = Property<int>("SummerVariantIndex");

            Invoke("MarkStageLoaded");
            Invoke("FailStage");
            Invoke("RetryStage");
            AssertState("LoadingSpring");
            AssertSelections(spring, summer);

            Invoke("MarkStageLoaded");
            Invoke("CompleteStage");
            Invoke("ContinueFromShop");
            Invoke("MarkStageLoaded");
            Invoke("FailStage");
            Invoke("RetryStage");
            AssertState("LoadingSummer");
            AssertSelections(spring, summer);
        }

        [Test]
        public void InvalidTransitionLogsAndThrows()
        {
            LogAssert.Expect(LogType.Error,
                "RAILGAME_CAMPAIGN_INVALID_TRANSITION operation=ContinueFromShop state=Lobby");
            TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => Invoke("ContinueFromShop"));
            Assert.That(error?.InnerException, Is.TypeOf<InvalidOperationException>());
            AssertState("Lobby");
        }

        [Test]
        public void ResetClearsRunAndReturnsToLobby()
        {
            Invoke("StartNewRun");
            Invoke("ResetToLobby");

            AssertState("Lobby");
            Assert.That(Property<int>("SpringVariantIndex"), Is.Zero);
            Assert.That(Property<int>("SummerVariantIndex"), Is.Zero);
            TargetInvocationException error = Assert.Throws<TargetInvocationException>(() => Property<object>("CurrentSeason"));
            Assert.That(error?.InnerException, Is.TypeOf<InvalidOperationException>());
        }

        private void AssertSelections(int spring, int summer)
        {
            Assert.That(Property<int>("SpringVariantIndex"), Is.EqualTo(spring));
            Assert.That(Property<int>("SummerVariantIndex"), Is.EqualTo(summer));
        }

        private void AssertState(string expected)
        {
            Assert.That(Property<object>("State").ToString(), Is.EqualTo(expected));
        }

        private void Invoke(string method)
        {
            sessionType.GetMethod(method, BindingFlags.Instance | BindingFlags.Public)?.Invoke(session, null);
        }

        private T Property<T>(string name)
        {
            return (T)sessionType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public)?.GetValue(session);
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetType(name) is Type type)
                    return type;
            throw new InvalidOperationException($"Type not found: {name}");
        }
    }
}
