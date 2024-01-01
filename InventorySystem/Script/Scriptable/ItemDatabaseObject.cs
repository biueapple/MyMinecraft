using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//이 스크립트블오브젝트는 ScriptableObjectMenu( [MenuItem("InventorySystem/Database/DatabaseCreate")] )에서 만들 수 있도록 만들어져 있음
//이 데이터베이스는 만들어져있는 원본 아이템들을 가지고 있는 오브젝트임
//데이터베이스는 하나만 존재해야함
public class ItemDatabaseObject : ScriptableObject
{
    //현재 존재하는 모든 아이템원본에 대한 데이터
    [SerializeField]
    private List<ItemObject> ItemObjects = new List<ItemObject>();
    public List<ItemObject> Data { get { return ItemObjects; } }

    //새로운 아이템이 만들어 졌을때 호출 ScriptableObjectMenu에서 사용함
    public void ItemObjectAdd(ItemObject _itemObject)
    {
        if(ItemObjects == null)
            ItemObjects = new List<ItemObject>();
        ItemObjects.Add(_itemObject);
        _itemObject.data.id = ItemObjects.Count - 1;
    }

    //아이템에 대한 정보를 id를 줬을때 리턴해줌
    public ItemObject GetItemObjectWithId(int _id)
    {
        if(_id > ItemObjects.Count || _id < 0)
            return null;

        if (ItemObjects[_id].data.id == _id)
            return ItemObjects[_id];
        for (int i = 0; i < ItemObjects.Count; i++)
        {
            if(ItemObjects[i].data.id == _id)
                return ItemObjects[i];
        }
        return null;
    }
}
