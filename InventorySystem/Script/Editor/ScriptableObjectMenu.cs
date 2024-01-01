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

        //데이터베이스가 있으면 아이템을 데이터베이스에 넣기
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

        //데이터베이스가 있으면 아이템을 데이터베이스에 넣기
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

        //데이터베이스가 있으면 아이템을 데이터베이스에 넣기
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

        //데이터베이스가 있으면 아이템을 데이터베이스에 넣기
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
        //아이템을 만들고
        ItemObject item = ScriptableObject.CreateInstance<ItemObject>();
        AssetDatabase.CreateAsset(item, "Assets/InventorySystem/Item/NewItem.asset");
        AssetDatabase.SaveAssets();

        //데이터베이스가 있으면 아이템을 데이터베이스에 넣기
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
        //데이터베이스르 만들고 이미 있는 아이템들을 넣어주기 위해
        string[] ItemGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Item" });
        //데이터베이스를 만들고 이미 있는 컨테이너들에게 데이터베이스를 연결해주기 위해
        string[] Containers = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Container" });

        //0보다 크면 이미 데이터베이스가 있다는거 데이터베이스는 하나만 있어야 하기에 이미 있는걸 초기화 
        //이미 데이터베이스가 있는 상태에서 새로 만들어 초기화를 하는건 좋지 않음 이미 정해져 있는 id가 변할 수 있음
        //(이미 데이터베이스가 있다면 그냥 작동 안하도록 없앰)
        //id가 변하면 save load에서 문제가 일어날 수도 있음
        if (DatabaseGuids.Length == 0)
        {
            //아직 데이터베이스가 없음 새로 하나 만들고
            ItemDatabaseObject database = ScriptableObject.CreateInstance<ItemDatabaseObject>();
            AssetDatabase.CreateAsset(database, "Assets/InventorySystem/Database/NewDatabase.asset");
            AssetDatabase.SaveAssets();

            string assetPath;
            //기존에 있는 아이템들을 추가하고 id를 정해줌
            for (int i = 0; i < ItemGuids.Length; i++)
            {
                assetPath = AssetDatabase.GUIDToAssetPath(ItemGuids[i]);
                ItemObject item = (ItemObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ItemObject));
                database.ItemObjectAdd(item);
            }
            ////이미 만들어져있는 Container들에게 database 변수를 할당해줌
            //for (int i = 0; i < Containers.Length; i++)
            //{
            //    assetPath = AssetDatabase.GUIDToAssetPath(Containers[i]);
            //    ContainerObject container = (ContainerObject)AssetDatabase.LoadAssetAtPath(assetPath, typeof(ContainerObject));
            //}
        }
        //else
        //{
        //    //이미 데이터베이스가 하나 있음 데이터베이스는 하나만 있어야 하기에 이미 있는 것을 초기화
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
        //        Debug.Log("Assets/InventorySystem/Database 폴더에 다른 무엇인가가 있음 삭제필요");
        //        return;
        //    }
        //}

        Debug.Log("Database initialized");
    }

    //[MenuItem("InventorySystem/Container/ContainerCreate")]
    //static void CreateScriptableContainer()
    //{
    //    //컨테이너를 만들고
    //    ContainerObject container = ScriptableObject.CreateInstance<ContainerObject>();
    //    AssetDatabase.CreateAsset(container, "Assets/InventorySystem/Container/NewContainer.asset");
    //    AssetDatabase.SaveAssets();

    //    //데이터베이스가 있으면 컨테이너에 넣어주기
    //    string[] DatabaseGuids = AssetDatabase.FindAssets("", new string[] { "Assets/InventorySystem/Database" });
    //    if (DatabaseGuids.Length > 0)
    //    {
    //        string path = AssetDatabase.GUIDToAssetPath(DatabaseGuids[0]);
    //        ItemDatabaseObject database = (ItemDatabaseObject)AssetDatabase.LoadAssetAtPath(path, typeof(ItemDatabaseObject));
    //    }

    //    Debug.Log("Container initialized");
    //}
}
