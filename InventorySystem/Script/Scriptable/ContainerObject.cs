using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;



//인벤토리를 컨테이너로 대체 인벤토리만 아니라 장비창이나 상자또한 이것을 쓰기때문
//ScriptableObject 는 Serializable가 없다면 수정시에 내용이 초기화되는 문제가 생김 
[System.Serializable]
public class ContainerObject
{
    //아이템이 저장될 경로
    private string savePath;
    public string SavePath { set { savePath = value; } }
    public Container storage;

    public ContainerObject(int count, string savePath)
    {
        storage = new Container(count);
        this.savePath = savePath;
    }

    //플레이어가 아이템을 획득하면 호출할 함수
    public void Acquired(ItemInterface _item, int _amount)
    {
        ContainerSlot slot;
        //아이템이 겹칠 수 있음
        if (_item.Stackable)
        {
            //같은 아이템이 있는지 확인
            slot = storage.GetSlot(_item);
            if(slot != null )
            {
                //있으면 갯수만 추가
                slot.AddAmount(_amount);
                return;
            }
            //같은 아이템이 없다면 새로 채워넣기
            slot = storage.GetEmptySlot(_item.Type);
            if( slot != null )
            {
                slot.UpdateSlot(_item, _amount);
            }
        }
        //아이템이 겹칠 수 없음
        else
        {
            //빈공간에 하나씩 넣기
            for(int i = 0; i < _amount; i++)
            {
                slot = storage.GetEmptySlot(_item.Type);
                if( slot != null )
                {
                    slot.UpdateSlot(_item, 1);
                    return;
                }
            }
        }
    }

    //아이템 스왑
    public void SwapContainerSlot(ContainerSlot _slot1, ContainerSlot _slot2)
    {
        if (_slot1.CanPlace(_slot2.GetItem) && _slot2.CanPlace(_slot1.GetItem))
        {
            ContainerSlot temp = new ContainerSlot(_slot2.GetItem, _slot2.Amount);
            _slot2.UpdateSlot(_slot1.GetItem, _slot1.Amount);
            _slot1.UpdateSlot(temp.GetItem, temp.Amount);
        }
    }



    //저장 경로 : C:\Users\사용자명\AppData\LocalLow\DefaultCompany\My InventorySystem
    //[ContextMenu("Save")]
    public void Save()
    {
        if(savePath != null)
        {
            try
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Create, FileAccess.Write);
                formatter.Serialize(stream, storage);
                stream.Close();
            }
            catch (Exception ex)
            {
                Debug.LogError($"예외 발생: {ex.Message}");
            }
        }
    }

    //불러오기 경로 : C:\Users\사용자명\AppData\LocalLow\DefaultCompany\My InventorySystem
    //[ContextMenu("Load")]
    public void Load()
    {
        if(savePath != null)
        {
            if (File.Exists(string.Concat(Application.persistentDataPath, savePath)))
            {
                IFormatter formatter = new BinaryFormatter();
                Stream stream = new FileStream(string.Concat(Application.persistentDataPath, savePath), FileMode.Open, FileAccess.Read);
                Container newContainer = (Container)formatter.Deserialize(stream);
                for (int i = 0; i < storage.slots.Length; i++)
                {
                    storage.slots[i].UpdateSlot(newContainer.slots[i].GetItem, newContainer.slots[i].Amount);
                }
                stream.Close();
            }
        }
    }

    //delegate를 없애줘야 에디터에서 오류가 안남
    //[ContextMenu("Clear")]
    public void Clear()
    {
        for (int i = 0; i < storage.slots.Length; i++)
        {
            storage.slots[i].afterCallback = null;
            storage.slots[i].beforeCallback = null;
            storage.slots[i].UpdateSlot(new Item(), 0);
        }
    }
}

//여러 아이템을 저장하는 공간
[Serializable]
public class Container
{
    public ContainerSlot[] slots;

    public Container(int count)
    {
        slots = new ContainerSlot[count];
        for(int i = 0; i < count; i++)
        {
            slots[i] = new ContainerSlot();
        }
    }

    //현재 슬롯중에 매개변수로 들어온 아이템과 같은 아이템이 있다면 리턴 없다면 null
    public ContainerSlot GetSlot(ItemInterface _item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetItem.ID ==  _item.ID)
                return slots[i];
        }
        return null;
    }
    //비어있는 슬롯을 리턴 비어있는게 없다면 null
    public ContainerSlot GetEmptySlot(ITEM_TYPE type)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if ((slots[i].Amount == 0 || slots[i].GetItem.ID == -1) && slots[i].CanPlace(type))
            {
                return slots[i];
            }
        }
        return null;
    }
}

//하나의 아이템을 저장하는 공간
[Serializable]
public class ContainerSlot
{
    //이 슬롯에 들어올 수 있는 종류
    public ITEM_TYPE[] AllowedItems;
    //현재 아이템
    [SerializeField]
    private ItemInterface item;
    public ItemInterface GetItem { get { return item; } }
    //현재 아이템의 갯수
    [SerializeField]
    private int amount;
    public int Amount {  get { return amount; } }
    //무엇인가 바뀌고 나서 호출
    [NonSerialized]
    public Action<ContainerSlot> afterCallback;
    //무엇인가 바뀌기 전에 호출
    [NonSerialized]
    public Action<ContainerSlot> beforeCallback;

    //모든걸 UpdateSlot을 이용하는게 편함
    public ContainerSlot()
    {
        UpdateSlot(new Item(), 0);
        AllowedItems = new ITEM_TYPE[0];
    }

    public ContainerSlot(ItemInterface _item, int _amount)
    {
        UpdateSlot(_item, _amount);
        AllowedItems = new ITEM_TYPE[0];
    }

    public void UpdateSlot(ItemInterface _item, int _amount)
    {
        if (beforeCallback != null)
            beforeCallback(this);

        amount = _amount;
        if (amount > 0)
            item = _item;
        else
            item = new Item();

        if (afterCallback != null)
            afterCallback(this);
    }

    //새로고침같은 느낌
    public void UpdateSlot()
    {
        UpdateSlot(item, amount);   
    }

    public void AddAmount(int value)
    {
        UpdateSlot(item, amount + value);
    }

    //이 슬롯에 이 아이템이 들어올 수 있나?
    public bool CanPlace(ItemInterface _item)
    {
        //내가 조건이 없는 슬롯이거나 상대가 빈 아이템이라면 가능
        if (AllowedItems.Length == 0 || _item.ID < 0)
        {
            return true;
        }

        if (amount > 0 && item.Stackable)
        {
            //내가 조건이 있는데 상대 아이템이 조건에 부합하면 가능
            for (int i = 0; i < AllowedItems.Length; i++)
            {
                if (AllowedItems[i] == _item.Type)
                {
                    return true;
                }
            }
        }


        //내가 조건이 있는데 상대 아이템이 조건에 부합하면 가능
        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if (AllowedItems[i] == _item.Type)
            {
                return true;
            }
        }

        return false;
    }
    //이 슬롯에 이 아이템이 들어올 수 있나?
    public bool CanPlace(ITEM_TYPE type)
    {
        //내가 조건이 없는 슬롯이거나 상대가 빈 아이템이라면 가능
        if (AllowedItems.Length == 0)
        {
            return true;
        }

        if (item.Stackable)
        {
            //내가 조건이 있는데 상대 아이템이 조건에 부합하면 가능
            for (int i = 0; i < AllowedItems.Length; i++)
            {
                if (AllowedItems[i] == type)
                {
                    return true;
                }
            }
        }

        return false;
    }
}