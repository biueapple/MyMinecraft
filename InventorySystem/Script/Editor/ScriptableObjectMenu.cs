using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScriptableObjectMenu : MonoBehaviour
{
    [MenuItem("InventorySystem/Items/WeaponitemCreate")]
    static void CreateScriptbleWeaponItem()
    {
        WeaponItemObject item = ScriptableObject.CreateInstance<WeaponItemObject>();
        AssetDatabase.CreateAsset(item, "Assets/InventorySystem/Item/NewWeaponItem.asset");
        AssetDatabase.SaveAssets();

        //�����ͺ��̽��� ������ �������� �����ͺ��̽��� �ֱ�
        string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
        if (DatabaseGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
            ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));
            database.ItemObjectAdd(item);
        }

        Debug.Log("WeaponItem initialized");
    }

    [MenuItem("InventorySystem/Items/EquipitemCreate")]
    static void CreateScriptbleEquipItem()
    {
        EquipItemObject item = ScriptableObject.CreateInstance<EquipItemObject>();
        AssetDatabase.CreateAsset(item, "Assets/InventorySystem/Item/NewEquipItem.asset");
        AssetDatabase.SaveAssets();

        //�����ͺ��̽��� ������ �������� �����ͺ��̽��� �ֱ�
        string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
        if (DatabaseGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
            ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));
            database.ItemObjectAdd(item);
        }

        Debug.Log("EquipItem initialized");
    }

    [MenuItem("InventorySystem/Items/FooditemCreate")]
    static void CreateScriptbleFoodItem()
    {
        FoodItemObject item = ScriptableObject.CreateInstance<FoodItemObject>();
        AssetDatabase.CreateAsset(item, "Assets/InventorySystem/Item/NewFoodItem.asset");
        AssetDatabase.SaveAssets();

        //�����ͺ��̽��� ������ �������� �����ͺ��̽��� �ֱ�
        string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
        if (DatabaseGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
            ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));
            database.ItemObjectAdd(item);
        }

        Debug.Log("FoodItem initialized");
    }

    [MenuItem("InventorySystem/Items/InstallitemCreate")]
    static void CreateScriptbleInstallItem()
    {
        InstallItemOjbect item = ScriptableObject.CreateInstance<InstallItemOjbect>();
        AssetDatabase.CreateAsset(item, "Assets/InventorySystem/Item/NewInstallItem.asset");
        AssetDatabase.SaveAssets();

        //�����ͺ��̽��� ������ �������� �����ͺ��̽��� �ֱ�
        string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
        if (DatabaseGuids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
            ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));
            database.ItemObjectAdd(item);
        }

        Debug.Log("InstallItem initialized");
    }

    [MenuItem("InventorySystem/Items/itemCreate")]
    static void CreateScriptableItem()
    {
        //�������� �����
        ItemObject item = ScriptableObject.CreateInstance<ItemObject>();
        AssetDatabase.CreateAsset(item, "Assets/InventorySystem/Item/NewItem.asset");
        AssetDatabase.SaveAssets();

        //�����ͺ��̽��� ������ �������� �����ͺ��̽��� �ֱ�
        string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
        if (DatabaseGuids.Length > 0 )
        {
            string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
            ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));
            database.ItemObjectAdd(item);
        }

        Debug.Log("Item initialized");
    }

    [MenuItem("InventorySystem/Database/DatabaseCreate")]
    static void CreateScritableDatabase()
    {
        string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
        //�����ͺ��̽��� ����� �̹� �ִ� �����۵��� �־��ֱ� ����
        string[] ItemGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Item" });
        //�����ͺ��̽��� ����� �̹� �ִ� �����̳ʵ鿡�� �����ͺ��̽��� �������ֱ� ����
        string[] Containers = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Container" });

        //0���� ũ�� �̹� �����ͺ��̽��� �ִٴ°� �����ͺ��̽��� �ϳ��� �־�� �ϱ⿡ �̹� �ִ°� �ʱ�ȭ 
        //�̹� �����ͺ��̽��� �ִ� ���¿��� ���� ����� �ʱ�ȭ�� �ϴ°� ���� ���� �̹� ������ �ִ� id�� ���� �� ����
        //(�̹� �����ͺ��̽��� �ִٸ� �׳� �۵� ���ϵ��� ����)
        //id�� ���ϸ� save load���� ������ �Ͼ ���� ����
        if (DatabaseGuids.Length == 0)
        {
            //���� �����ͺ��̽��� ���� ���� �ϳ� �����
            ItemDatabaseObject database = ScriptableObject.CreateInstance<ItemDatabaseObject>();
            AssetDatabase.CreateAsset(database, "Assets/InventorySystem/Database/NewDatabase.asset");
            AssetDatabase.SaveAssets();

            string assetPath;
            //������ �ִ� �����۵��� �߰��ϰ� id�� ������
            for (int i = 0; i < ItemGuids.Length; i++)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(ItemGuids[i]);
                ItemObject item = (ItemObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ItemObject));
                database.ItemObjectAdd(item);
            }
            ////�̹� ��������ִ� Container�鿡�� database ������ �Ҵ�����
            //for (int i = 0; i < Containers.Length; i++)
            //{
            //    assetPath = AssetDatabase.GUIDToAssetPath(Containers[i]);
            //    ContainerObject container = (ContainerObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ContainerObject));
            //}
        }
        //else
        //{
        //    //�̹� �����ͺ��̽��� �ϳ� ���� �����ͺ��̽��� �ϳ��� �־�� �ϱ⿡ �̹� �ִ� ���� �ʱ�ȭ
        //    string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
        //    ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));

        //    if(database != null)
        //    {
        //        database.ItemObjects.Clear();
        //        string assetPath;
        //        for (int i = 0; i < ItemGuids.Length; i++)
        //        {
        //            assetPath = AssetDatabase.GUIDToAssetPath(ItemGuids[i]);
        //            ItemObject item = (ItemObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ItemObject));
        //            database.ItemObjectAdd(item);
        //        }
        //        for (int i = 0; i < Containers.Length; i++)
        //        {
        //            assetPath = AssetDatabase.GUIDToAssetPath(Containers[i]);
        //            ContainerObject container = (ContainerObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ContainerObject));
        //            container.database = database;
        //        }
        //    }
        //    else
        //    {
        //        Debug.Log("Assets/InventorySystem/Database ������ �ٸ� �����ΰ��� ���� �����ʿ�");
        //        return;
        //    }
        //}

        Debug.Log("Database initialized");
    }

    //[MenuItem("InventorySystem/Container/ContainerCreate")]
    //static void CreateScriptableContainer()
    //{
    //    //�����̳ʸ� �����
    //    ContainerObject container = ScriptableObject.CreateInstance<ContainerObject>();
    //    AssetDatabase.CreateAsset(container, "Assets/InventorySystem/Container/NewContainer.asset");
    //    AssetDatabase.SaveAssets();

    //    //�����ͺ��̽��� ������ �����̳ʿ� �־��ֱ�
    //    string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
    //    if (DatabaseGuids.Length > 0)
    //    {
    //        string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
    //        ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));
    //    }

    //    Debug.Log("Container initialized");
    //}
}
