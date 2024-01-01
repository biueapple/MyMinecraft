using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using UnityEngine;



//�κ��丮�� �����̳ʷ� ��ü �κ��丮�� �ƴ϶� ���â�̳� ���ڶ��� �̰��� ���⶧��
//ScriptableObject �� Serializable�� ���ٸ� �����ÿ� ������ �ʱ�ȭ�Ǵ� ������ ���� 
[System.Serializable]
public class ContainerObject
{
    //�������� ����� ���
    private string savePath;
    public string SavePath { set { savePath = value; } }
    public Container storage;

    public ContainerObject(int count, string savePath)
    {
        storage = new Container(count);
        this.savePath = savePath;
    }

    //�÷��̾ �������� ȹ���ϸ� ȣ���� �Լ�
    public void Acquired(ItemInterface _item, int _amount)
    {
        ContainerSlot slot;
        //�������� ��ĥ �� ����
        if (_item.Stackable)
        {
            //���� �������� �ִ��� Ȯ��
            slot = storage.GetSlot(_item);
            if(slot != null )
            {
                //������ ������ �߰�
                slot.AddAmount(_amount);
                return;
            }
            //���� �������� ���ٸ� ���� ä���ֱ�
            slot = storage.GetEmptySlot(_item.Type);
            if( slot != null )
            {
                slot.UpdateSlot(_item, _amount);
            }
        }
        //�������� ��ĥ �� ����
        else
        {
            //������� �ϳ��� �ֱ�
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

    //������ ����
    public void SwapContainerSlot(ContainerSlot _slot1, ContainerSlot _slot2)
    {
        if (_slot1.CanPlace(_slot2.GetItem) && _slot2.CanPlace(_slot1.GetItem))
        {
            ContainerSlot temp = new ContainerSlot(_slot2.GetItem, _slot2.Amount);
            _slot2.UpdateSlot(_slot1.GetItem, _slot1.Amount);
            _slot1.UpdateSlot(temp.GetItem, temp.Amount);
        }
    }



    //���� ��� : C:\Users\����ڸ�\AppData\LocalLow\DefaultCompany\My InventorySystem
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
                Debug.LogError($"���� �߻�: {ex.Message}");
            }
        }
    }

    //�ҷ����� ��� : C:\Users\����ڸ�\AppData\LocalLow\DefaultCompany\My InventorySystem
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

    //delegate�� ������� �����Ϳ��� ������ �ȳ�
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

//���� �������� �����ϴ� ����
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

    //���� �����߿� �Ű������� ���� �����۰� ���� �������� �ִٸ� ���� ���ٸ� null
    public ContainerSlot GetSlot(ItemInterface _item)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].GetItem.ID ==  _item.ID)
                return slots[i];
        }
        return null;
    }
    //����ִ� ������ ���� ����ִ°� ���ٸ� null
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

//�ϳ��� �������� �����ϴ� ����
[Serializable]
public class ContainerSlot
{
    //�� ���Կ� ���� �� �ִ� ����
    public ITEM_TYPE[] AllowedItems;
    //���� ������
    [SerializeField]
    private ItemInterface item;
    public ItemInterface GetItem { get { return item; } }
    //���� �������� ����
    [SerializeField]
    private int amount;
    public int Amount {  get { return amount; } }
    //�����ΰ� �ٲ�� ���� ȣ��
    [NonSerialized]
    public Action<ContainerSlot> afterCallback;
    //�����ΰ� �ٲ�� ���� ȣ��
    [NonSerialized]
    public Action<ContainerSlot> beforeCallback;

    //���� UpdateSlot�� �̿��ϴ°� ����
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

    //���ΰ�ħ���� ����
    public void UpdateSlot()
    {
        UpdateSlot(item, amount);   
    }

    public void AddAmount(int value)
    {
        UpdateSlot(item, amount + value);
    }

    //�� ���Կ� �� �������� ���� �� �ֳ�?
    public bool CanPlace(ItemInterface _item)
    {
        //���� ������ ���� �����̰ų� ��밡 �� �������̶�� ����
        if (AllowedItems.Length == 0 || _item.ID < 0)
        {
            return true;
        }

        if (amount > 0 && item.Stackable)
        {
            //���� ������ �ִµ� ��� �������� ���ǿ� �����ϸ� ����
            for (int i = 0; i < AllowedItems.Length; i++)
            {
                if (AllowedItems[i] == _item.Type)
                {
                    return true;
                }
            }
        }


        //���� ������ �ִµ� ��� �������� ���ǿ� �����ϸ� ����
        for (int i = 0; i < AllowedItems.Length; i++)
        {
            if (AllowedItems[i] == _item.Type)
            {
                return true;
            }
        }

        return false;
    }
    //�� ���Կ� �� �������� ���� �� �ֳ�?
    public bool CanPlace(ITEM_TYPE type)
    {
        //���� ������ ���� �����̰ų� ��밡 �� �������̶�� ����
        if (AllowedItems.Length == 0)
        {
            return true;
        }

        if (item.Stackable)
        {
            //���� ������ �ִµ� ��� �������� ���ǿ� �����ϸ� ����
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