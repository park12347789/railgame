using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Railgame.Tests
{
    public sealed class RailgamePhysicalShopTests
    {
        private readonly List<GameObject> created = new();

        [TearDown]
        public void TearDown()
        {
            foreach (GameObject item in created)
                if (item != null)
                    Object.DestroyImmediate(item);
            created.Clear();
        }

        [Test]
        public void PickupMountDetachAndCheckoutChargesOnce()
        {
            Component interactor = Add("Player", "Railgame.Hansol.ShoulderView.ShoulderInteractor");
            Component holder = interactor.gameObject.AddComponent(FindType("Railgame.Shop.RailgameCarryHolder"));
            Transform hand = Child(interactor.transform, "Hand");
            Invoke(holder, "Initialize", hand);

            object engine = Enum.Parse(FindType("SectionType"), "Engine");
            Component item = Add("EngineItem", "Railgame.Shop.RailgamePhysicalShopItem");
            Invoke(item, "Initialize", engine, 2, false);
            Component socket = Add("EngineSocket", "Railgame.Shop.RailgameShopSocket");
            Transform mount = Child(socket.transform, "Mount");
            Invoke(socket, "Initialize", engine, mount, null);

            Invoke(item, "Interact", interactor);
            Assert.That(Property<Component>(holder, "HeldItem"), Is.SameAs(item));
            Assert.That((bool)Invoke(socket, "TryMountOrDetach", holder), Is.True);
            Assert.That(Property<Component>(socket, "MountedItem"), Is.SameAs(item));
            Assert.That(Property<Component>(holder, "HeldItem"), Is.Null);

            Assert.That((bool)Invoke(socket, "TryMountOrDetach", holder), Is.True);
            Assert.That(Property<Component>(holder, "HeldItem"), Is.SameAs(item));
            Assert.That((bool)Invoke(socket, "TryMountOrDetach", holder), Is.True);

            Component economy = Add("Economy", "Railgame.Hansol.ShoulderView.ShoulderShopEconomy");
            Invoke(economy, "Initialize", 5);
            Component checkout = Add("Checkout", "Railgame.Shop.RailgameShopCheckout");
            Invoke(checkout, "Initialize", economy, TypedArray(socket), TypedArray(holder));

            Assert.That(Property<int>(checkout, "PendingTotal"), Is.EqualTo(2));
            Assert.That(Property<bool>(checkout, "CanDepart"), Is.True);
            Assert.That((bool)Invoke(checkout, "TryCheckout"), Is.True);
            Assert.That(Property<int>(economy, "Bolts"), Is.EqualTo(3));
            Assert.That(Property<bool>(item, "Owned"), Is.True);

            LogAssert.Expect(LogType.Error, "RAILGAME_SHOP_CHECKOUT_ALREADY_COMPLETED");
            Assert.That((bool)Invoke(checkout, "TryCheckout"), Is.False);
            Assert.That(Property<int>(economy, "Bolts"), Is.EqualTo(3));
        }

        [Test]
        public void HeldUnpaidAndInsufficientBoltsLeaveStateUnchanged()
        {
            Component interactor = Add("Player", "Railgame.Hansol.ShoulderView.ShoulderInteractor");
            Component holder = interactor.gameObject.AddComponent(FindType("Railgame.Shop.RailgameCarryHolder"));
            Invoke(holder, "Initialize", Child(interactor.transform, "Hand"));

            object cargo = Enum.Parse(FindType("SectionType"), "Cargo");
            Component item = Add("CargoItem", "Railgame.Shop.RailgamePhysicalShopItem");
            Invoke(item, "Initialize", cargo, 5, false);
            Invoke(item, "Interact", interactor);

            Component socket = Add("CargoSocket", "Railgame.Shop.RailgameShopSocket");
            Invoke(socket, "Initialize", cargo, Child(socket.transform, "Mount"), null);
            Component economy = Add("Economy", "Railgame.Hansol.ShoulderView.ShoulderShopEconomy");
            Invoke(economy, "Initialize", 4);
            Component checkout = Add("Checkout", "Railgame.Shop.RailgameShopCheckout");
            Invoke(checkout, "Initialize", economy, TypedArray(socket), TypedArray(holder));

            LogAssert.Expect(LogType.Error, "RAILGAME_SHOP_UNPAID_ITEM_HELD item=CargoItem");
            Assert.That((bool)Invoke(checkout, "TryCheckout"), Is.False);
            Assert.That(Property<int>(economy, "Bolts"), Is.EqualTo(4));
            Assert.That(Property<bool>(item, "Owned"), Is.False);

            Assert.That((bool)Invoke(socket, "TryMountOrDetach", holder), Is.True);
            LogAssert.Expect(LogType.Error, "RAILGAME_SHOP_BOLTS_INSUFFICIENT have=4 need=5");
            Assert.That((bool)Invoke(checkout, "TryCheckout"), Is.False);
            Assert.That(Property<int>(economy, "Bolts"), Is.EqualTo(4));
            Assert.That(Property<bool>(item, "Owned"), Is.False);
        }

        [Test]
        public void BoltOnlyIncreasesBankAfterTrainDeposit()
        {
            Component interactor = Add("Player", "Railgame.Hansol.ShoulderView.ShoulderInteractor");
            Component holder = interactor.gameObject.AddComponent(FindType("Railgame.Shop.RailgameCarryHolder"));
            Invoke(holder, "Initialize", Child(interactor.transform, "Hand"));
            Component economy = Add("Economy", "Railgame.Hansol.ShoulderView.ShoulderShopEconomy");
            Invoke(economy, "Initialize", 0);
            Component bolt = Add("Bolt", "Railgame.Shop.RailgameBoltPickup");
            Invoke(bolt, "Initialize", 1);
            Component deposit = Add("TrainDeposit", "Railgame.Shop.RailgameBoltDeposit");
            Invoke(deposit, "Initialize", economy);

            Invoke(bolt, "Interact", interactor);
            Assert.That(Property<int>(economy, "Bolts"), Is.Zero);
            Assert.That(Property<Component>(holder, "HeldItem"), Is.SameAs(bolt));

            Assert.That((bool)Invoke(deposit, "TryDeposit", holder), Is.True);
            Assert.That(Property<int>(economy, "Bolts"), Is.EqualTo(1));
            Assert.That(Property<bool>(bolt, "IsBanked"), Is.True);
            Assert.That(Property<Component>(holder, "HeldItem"), Is.Null);
        }

        private Component Add(string name, string typeName)
        {
            GameObject item = new(name);
            created.Add(item);
            return item.AddComponent(FindType(typeName));
        }

        private static Transform Child(Transform parent, string name)
        {
            Transform child = new GameObject(name).transform;
            child.SetParent(parent, false);
            return child;
        }

        private static Array TypedArray(Component item)
        {
            Array values = Array.CreateInstance(item.GetType(), 1);
            values.SetValue(item, 0);
            return values;
        }

        private static Type FindType(string name)
        {
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                if (assembly.GetType(name) is Type type)
                    return type;
            throw new InvalidOperationException($"Type not found: {name}");
        }

        private static object Invoke(Component target, string method, params object[] args)
        {
            MethodInfo info = target.GetType().GetMethod(method,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, $"Method missing: {target.GetType().FullName}.{method}");
            return info.Invoke(target, args);
        }

        private static T Property<T>(Object target, string name)
        {
            PropertyInfo info = target.GetType().GetProperty(name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, $"Property missing: {target.GetType().FullName}.{name}");
            return (T)info.GetValue(target);
        }
    }
}
