using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;


//�������� ���� (��� �������� ��� ���Կ� �� �� �ִ��� �������� �����ϱ� ����)
public enum ITEM_TYPE
{
    DEFAULT,
    CONSUM,
    HELMET,
    TOP,
    BOTTOM,
    BOOTS,
    WEAPON,
    SHIELD,
    RUNE,

}

//�������� ���
public enum RATING
{
    COMMON,
    UNCOMMON,
    RARE,
    UNIQUE,
    LEGEND
}

//�������� �Դ� �������� ����� ��ġ�ϴ� ���������� �ƹ��͵� �ƴ��� (�������� ��Ŭ�������� ���� ���� �Ͼ�� �����ϱ� ����)
public enum ITEM_
{
    NONE,
    INSTALL,
    EATING
}


//�������� ��ũ��Ʈ�� ������Ʈ�� ������ �ϳ����� ��������� ��ũ��Ʈ �󿡼� ���簡 �ɰ���
//�������� ��޿� ���� �������ִ� ������ ������ �޶������� (���߿� ��޿� ���� ������ �޷��ִ°� �ƴ϶� ���� �ڴ´ٴ��� ���⿡�� ���ݽ����� ���� �޷��ִٴ���) 
//�� ��ũ��Ʈ�������Ʈ�� ScriptableObjectMenu( [MenuItem("InventorySystem/Items/itemCreate")] )���� ���� �� �ֵ��� ������� ����
[System.Serializable]
public class ItemObject : ScriptableObject
{
    public Item data;
    //�������� �̹���
    public Sprite sprite;
    //�������� ���� ���ӿ�����Ʈ
    public Transform model;
    //�����ۿ� ���� �� �ִ� ���
    [Header("�� �������� ���������� ���� ������ ��ũ (������� ���� ���� ���)")]
    public RATING[] ratings;

    //���ο� �������� ����� ���� ��������
    public virtual ItemInterface CreateItem(int level)
    {
        Item newitem = new Item(this);
        return newitem;
    }
}


//�� �������� ���鶩 �ݵ�� CreateItem�̰��� �̿��ؾ���
[System.Serializable]
public class Item : ItemInterface
{
    //�⺻����
    [SerializeField]
    private string name;
    [SerializeField]
    public int id = -1;
    //�� ���Կ� �������� �� �� �ִ���
    [SerializeField]
    protected int level;
    [SerializeField]
    private bool stackable;
    //�ܴ��� ����
    [SerializeField]
    private Hardness hardness;

    //�������� ��� ���������� �׷���
    [SerializeField] 
    private ITEM_TYPE type;

    [SerializeField]
    //�� �������� Ȯ�������� ���� ���
    protected RATING rating;
    public RATING Rating { get { return rating; } }

    //����
    [SerializeField]
    [TextArea(5, 10)]
    public string description;

    ////ȿ��
    //protected ItemEffect[] itemEffects;
    //public ItemEffect[] ItemEffects { get { return itemEffects; } }



    //interface
    int ItemInterface.ID => id;

    string ItemInterface.Name => name;

    int ItemInterface.Level => level;

    ITEM_TYPE ItemInterface.Type => type;

    bool ItemInterface.Stackable => stackable;

    Hardness ItemInterface.Hardness => hardness;

    RATING ItemInterface.Rating => rating;

    string ItemInterface.Description => description;

    public virtual void RightMouseDown(Player player) { }
    public virtual void RightMouseUp(Player player) { }
    public virtual void LeftMouseDown(Player player)
    {
        List<Collider> colliders = new List<Collider>();
        colliders.AddRange(Physics.OverlapBox(player.RBI.position + player.cam.forward * 0.1f, new Vector3(0.5f,0.5f,0.5f), Quaternion.Euler(player.transform.eulerAngles), LayerMask.GetMask("Unit")));
        colliders.Remove(player.GetComponent<Collider>());
        for (int i = 0; i < colliders.Count; i++)
        {
            if (colliders[i].GetComponent<Unit>() != null)
            {
                colliders[i].GetComponent<Unit>().Hit(player.STAT, player.STAT.AD, ATTACKTYPE.NOMAL, DAMAGETYPE.AD);
                player.STAT.Attacktimer = 0;
                return;
            }
        }
    }
    public virtual void LeftMouseUp(Player player) { }
    public virtual void Equip(Stat stat) { }
    public virtual void Release(Stat stat) { }
    //

    //������ ���鶧 �� (ScriptableObject)
    public Item()
    {
        name = "";
        id = -1;
        hardness = new Hardness();
    }

    //�̰��� ������ ���� ���ο� �������� ���鶧 ����
    //�������� ȹ�� ������ �⺻������ ���� ������
    public Item(ItemObject _itemObject)
    {
        //�������� �⺻���� �޴���
        name = _itemObject.data.name;
        id = _itemObject.data.id;
        hardness = new Hardness(_itemObject.data.hardness);
        stackable = _itemObject.data.stackable;
        type = _itemObject.data.type;
        description = _itemObject.data.description;

        ////�������� ���������̰ų� ���ϰ�� �߰����� �۾�
        //if (type == ITEM_TYPE.RUNE)
        //{
        //    RuneAttributeDecision(_itemObject);
        //}
        //else
        //{
        //    EquipAttributeDecision();
        //    //�������� ���� �ο���
        //    StatDecision(_itemObject);
        //    //�������� ȿ�� �ο���
        //    ItemEffectDecision();
        //}

        //if (type == ITEM_TYPE.WEAPON)
        //{
        //    //ad�� ap true�� ����
        //    damageType = (DAMAGETYPE)Random.Range(0, 2);
        //    figure = Random.Range(level, level + level * 0.5f);
        //}
    }

    //�̰��� �������� ����� ���س��� ȹ���ϵ��� ���� ������
    public Item(ItemObject _itemObject, RATING r)
    {
        //�������� �⺻���� �޴���
        name = _itemObject.data.name;
        id = _itemObject.data.id;
        hardness = new Hardness(_itemObject.data.hardness);
        stackable = _itemObject.data.stackable;
        type = _itemObject.data.type;
        description = _itemObject.data.description;

        //�������� ��� ���ϴ���
        rating = r;

        ////�������� ���������̰ų� ���ϰ�� �߰����� �۾�
        //if (type == ITEM_TYPE.RUNE)
        //{
        //    //���ΰ�� �� �ϳ��� ����� ������ ����
        //    RuneAttributeDecision(_itemObject);
        //}
        //else
        //{
        //    //����ΰ�� �� ��ĭ�� ������ �� �� ���� (��޿� ���� ���� ���� �� �ֱ⿡)
        //    EquipAttributeDecision();
        //    //�������� ���� �ο���
        //    StatDecision(_itemObject);
        //    //�������� ȿ�� �ο���
        //    ItemEffectDecision();
        //}

        //if(type == ITEM_TYPE.WEAPON)
        //{
        //    //ad�� ap true�� ����
        //    damageType = (DAMAGETYPE)Random.Range(0, 2);
        //    figure = Random.Range(level, level + level * 0.5f);
        //}
    }


    public void RatingDecision(ItemObject _itemObject)
    {
        if (_itemObject.ratings.Length == 0)
            rating = RATING.COMMON;
        else 
        //if (type == ITEM_TYPE.HELMET || type == ITEM_TYPE.TOP || type == ITEM_TYPE.BOTTOM ||
        //    type == ITEM_TYPE.BOOTS || type == ITEM_TYPE.WEAPON || type == ITEM_TYPE.SHIELD ||
        //    type == ITEM_TYPE.RUNE)
        {
            //��� ratings.Length == 0 �ΰ� �Ǽ��̴ϱ� �׳� ������ ������ �� (���⼭ ������ ���� ratings.Length == 0�̶� ���� �Ŵϱ� �����ؾ���)
            //if (_itemObject.ratings.Length > 0)
            //{
            rating = _itemObject.ratings[Random.Range(0, _itemObject.ratings.Length)];
            //}
        }
    }

    //public void RuneAttributeDecision(EquipItemObject _itemObject)
    //{
    //    switch (rating)
    //    {
    //        case RATING.COMMON:
    //            rune = new Rune[1];
    //            rune[0] = new Rune(_itemObject, 2, rating);
    //            //itemEffects = new ItemEffect[0];

    //            break;
    //        case RATING.UNCOMMON:
    //            rune = new Rune[1];
    //            rune[0] = new Rune(_itemObject, 3, rating);
    //            //itemEffects = new ItemEffect[0];

    //            break;
    //        case RATING.RARE:
    //            rune = new Rune[1];
    //            rune[0] = new Rune(_itemObject, 2, rating);
    //            //itemEffects = new ItemEffect[0];

    //            break;
    //        case RATING.UNIQUE:
    //            rune = new Rune[1];
    //            rune[0] = new Rune(_itemObject, 1, rating);
    //            //itemEffects = new ItemEffect[0];
    //            break;
    //        case RATING.LEGEND:
    //            rune = new Rune[1];
    //            rune[0] = new Rune(_itemObject, 4, rating);
    //            //����ȿ��
    //            break;
    //        default:
    //            rune = new Rune[1];
    //            rune[0] = new Rune();
    //            //itemEffects = new ItemEffect[0];
    //            break;
    //    }
    //}

    ////�����ʿ� (�����ϴ� ����Ʈ�߿� �ϳ��� �ƴ϶� �������� ���� ���� �ϳ��� ���Ϲ޾ƾ� ��)
    //public void ItemEffectDecision()
    //{
    //    for (int i = 0; i < itemEffects.Length; i++)
    //    {
    //        itemEffects[i] = ItemImpactManager.GetEffect();
    //    }
    //}

    //public bool InsertRune(Item _item)
    //{
    //    for(int i = 0; i <  rune.Length; i++)
    //    {
    //        if (rune[i].AttributeValues.Length == 0)
    //        {
    //            return rune[i].InsertRune(_item);
    //        }
    //    }
    //    return false;
    //}
}


public interface ItemInterface
{
    public int ID { get; }
    public string Name { get; }
    public int Level { get; }
    public ITEM_TYPE Type { get;  }
    public bool Stackable { get; }
    public Hardness Hardness { get; }
    public RATING Rating { get; }
    public string Description { get; }

    public void RightMouseDown(Player player);
    public void RightMouseUp(Player player);
    public void LeftMouseDown(Player player);
    public void LeftMouseUp(Player player);
    public void Equip(Stat stat);
    public void Release(Stat stat);
    public T GetComponent<T>() where T : Item
    {
        try 
        {
            return (T)this;
        } 
        catch 
        {
            return null;
        }
    }
}




//[System.Serializable]
//public class ItemRightClick
//{
//    public ItemRightClick()
//    {
//        install = null;
//        late = 0;
//        hunger = 0;
//    }
//    public ItemRightClick(ItemRightClick click)
//    {
//        install = click.install;
//        late = click.late;
//        hunger = click.hunger;
//    }
//    public InstallBlock install;
//    public float late;
//    public float hunger;
//}

////���������� ��Ŭ�������� ���� ����� ��ġ�Ǵ���
//[System.Serializable]
//public class InstallBlock
//{
//    public _BLOCK blockIndex;
//}

[System.Serializable]
public class Hardness
{
    public HardnessType type;
    public int hard;

    public Hardness()
    {
        type = HardnessType.NONE;
        hard = 1;
    }
    public Hardness(Hardness h)
    {
        type = h.type;
        hard = h.hard;
    }
}

public enum HardnessType
{
    NONE,   //�ƹ��͵� �ƴ�
    PICKAX, //���
    AXE,    //����
    SHOVELS,//��
}