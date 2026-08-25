using System;
using Railgame.Hansol.ShoulderView;
using Railgame.Shop;
using UnityEditor;
using UnityEngine;

namespace Railgame.Editor
{
    public static class RailgamePhysicalShopBuilder
    {
        public const string RootPrefabPath = "Assets/00.main/Shop/Prefabs/PF_PhysicalShopPrototype.prefab";
        public const string BoltPrefabPath = "Assets/00.main/Shop/Prefabs/PF_BoltPickup.prefab";
        private const string PrefabFolder = "Assets/00.main/Shop/Prefabs";

        [MenuItem("Railgame/Build Physical Shop Prototype")]
        public static void Build()
        {
            EnsureFolder("Assets/00.main/Shop");
            EnsureFolder(PrefabFolder);

            GameObject productionItem = CreateItemPrefab("ProductionUpgrade", SectionType.Production, 2,
                new Color(0.90f, 0.35f, 0.20f));
            GameObject waterItem = CreateItemPrefab("WaterUpgrade", SectionType.WaterTank, 2,
                new Color(0.20f, 0.60f, 0.90f));
            GameObject cargoItem = CreateItemPrefab("CargoUpgrade", SectionType.Cargo, 3,
                new Color(0.95f, 0.75f, 0.20f));
            CreateBoltPrefab();

            GameObject root = new("PhysicalShopPrototype");
            try
            {
                ShoulderShopEconomy economy = new GameObject("BankedBoltEconomy").AddComponent<ShoulderShopEconomy>();
                economy.transform.SetParent(root.transform, false);
                economy.Initialize(0);

                CreateDisplay(root.transform, productionItem, "Display_Production", new Vector3(-3f, 0f, 0f));
                CreateDisplay(root.transform, waterItem, "Display_Water", Vector3.zero);
                CreateDisplay(root.transform, cargoItem, "Display_Cargo", new Vector3(3f, 0f, 0f));

                RailgameShopSocket[] sockets =
                {
                    CreateSocket(root.transform, "Socket_Production", SectionType.Production, new Vector3(-2f, 0f, 4f)),
                    CreateSocket(root.transform, "Socket_WaterTank", SectionType.WaterTank, new Vector3(0f, 0f, 4f)),
                    CreateSocket(root.transform, "Socket_Cargo", SectionType.Cargo, new Vector3(2f, 0f, 4f))
                };

                GameObject departure = CreatePoint(root.transform, "DeparturePoint", new Vector3(0f, 0f, 7f));
                departure.AddComponent<RailgameShopCheckout>()
                    .Initialize(economy, sockets, Array.Empty<RailgameCarryHolder>());

                GameObject deposit = CreatePoint(root.transform, "BoltDepositPoint_ConnectToTrain", new Vector3(0f, 0f, -3f));
                deposit.AddComponent<RailgameBoltDeposit>().Initialize(economy);

                PrefabUtility.SaveAsPrefabAsset(root, RootPrefabPath);
                Debug.Log($"RAILGAME_PHYSICAL_SHOP_BUILD_OK prefab={RootPrefabPath}");
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static GameObject CreateItemPrefab(string itemName, SectionType sectionType, int price, Color color)
        {
            string path = $"{PrefabFolder}/PF_ShopItem_{itemName}.prefab";
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                root.name = $"ShopItem_{itemName}";
                root.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
                root.GetComponent<Renderer>().sharedMaterial = CreateMaterial(itemName, color);
                root.AddComponent<RailgamePhysicalShopItem>().Initialize(sectionType, price);
                PrefabUtility.SaveAsPrefabAsset(root, path);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
                throw new InvalidOperationException($"Failed to create shop item prefab: {path}");
            return prefab;
        }

        private static void CreateBoltPrefab()
        {
            GameObject root = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            try
            {
                root.name = "BoltPickup";
                root.transform.localScale = new Vector3(0.3f, 0.12f, 0.3f);
                root.GetComponent<Renderer>().sharedMaterial = CreateMaterial("Bolt", new Color(0.95f, 0.55f, 0.12f));
                root.AddComponent<RailgameBoltPickup>().Initialize(1);
                PrefabUtility.SaveAsPrefabAsset(root, BoltPrefabPath);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(root);
            }
        }

        private static void CreateDisplay(Transform parent, GameObject itemPrefab, string name, Vector3 position)
        {
            GameObject display = new(name);
            display.transform.SetParent(parent, false);
            display.transform.localPosition = position;
            GameObject item = (GameObject)PrefabUtility.InstantiatePrefab(itemPrefab, display.transform);
            item.name = itemPrefab.name;
            item.transform.localPosition = Vector3.up * 0.8f;
        }

        private static RailgameShopSocket CreateSocket(Transform parent, string name, SectionType type, Vector3 position)
        {
            GameObject socket = CreatePoint(parent, name, position);
            Transform anchor = new GameObject("MountAnchor").transform;
            anchor.SetParent(socket.transform, false);
            anchor.localPosition = Vector3.up * 0.5f;
            RailgameShopSocket component = socket.AddComponent<RailgameShopSocket>();
            component.Initialize(type, anchor);
            return component;
        }

        private static GameObject CreatePoint(Transform parent, string name, Vector3 position)
        {
            GameObject point = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            point.name = name;
            point.transform.SetParent(parent, false);
            point.transform.localPosition = position;
            point.transform.localScale = new Vector3(0.75f, 0.08f, 0.75f);
            point.GetComponent<Collider>().isTrigger = true;
            point.AddComponent<Rigidbody>().isKinematic = true;
            return point;
        }

        private static Material CreateMaterial(string itemName, Color color)
        {
            string path = $"{PrefabFolder}/M_ShopItem_{itemName}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                AssetDatabase.CreateAsset(material, path);
            }
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            string parent = System.IO.Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = System.IO.Path.GetFileName(path);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name) || !AssetDatabase.IsValidFolder(parent))
                throw new InvalidOperationException($"Invalid asset folder: {path}");
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
