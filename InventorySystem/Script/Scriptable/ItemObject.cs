using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;


//아이템의 종류 (어느 아이템이 어느 슬롯에 갈 수 있는지 없는지를 구분하기 위해)
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

//아이템의 등급
public enum RATING
{
    COMMON,
    UNCOMMON,
    RARE,
    UNIQUE,
    LEGEND
}

//아이템이 먹는 음식인지 블록을 설치하는 아이템인지 아무것도 아닌지 (아이템을 우클릭했을때 무슨 일이 일어날지 구분하기 위해)
public enum ITEM_
{
    NONE,
    INSTALL,
    EATING
}


//아이템은 스크립트블 오브젝트로 원본이 하나씩만 만들어지고 스크립트 상에서 복사가 될것임
//아이템은 등급에 따라 가지고있는 버프의 갯수가 달라질거임 (나중엔 등급에 따라 버프가 달려있는게 아니라 룬을 박는다던가 무기에는 공격스탯이 따로 달려있다던가) 
//이 스크립트블오브젝트는 ScriptableObjectMenu( [MenuItem("InventorySystem/Items/itemCreate")] )에서 만들 수 있도록 만들어져 있음
[System.Serializable]
public class ItemObject : ScriptableObject
{
    public Item data;
    //아이템의 이미지
    public Sprite sprite;
    //아이템의 실제 게임오브젝트
    public Transform model;
    //아이템에 붙을 수 있는 등급
    [Header("이 아이템을 생성했을때 보유 가능한 랭크 (없을경우 가장 낮은 등급)")]
    public RATING[] ratings;

    //새로운 아이템을 만들고 나서 리턴해줌
    public virtual ItemInterface CreateItem(int level)
    {
        Item newitem = new Item(this);
        return newitem;
    }
}


//새 아이템을 만들땐 반드시 CreateItem이것을 이용해야함
[System.Serializable]
public class Item : ItemInterface
{
    //기본정보
    [SerializeField]
    private string name;
    [SerializeField]
    public int id = -1;
    //한 슬롯에 여러개가 들어갈 수 있는지
    [SerializeField]
    protected int level;
    [SerializeField]
    private bool stackable;
    //단단한 정도
    [SerializeField]
    private Hardness hardness;

    //아이템이 장비 아이템인지 그런거
    [SerializeField] 
    private ITEM_TYPE type;

    [SerializeField]
    //이 아이템의 확정적으로 붙은 등급
    protected RATING rating;
    public RATING Rating { get { return rating; } }

    //설명
    [SerializeField]
    [TextArea(5, 10)]
    public string description;

    ////효과
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

    //원본을 만들때 씀 (ScriptableObject)
    public Item()
    {
        name = "";
        id = -1;
        hardness = new Hardness();
    }

    //이것은 원본을 통해 새로운 아이템을 만들때 쓰임
    //아이템을 획득 했을때 기본적으로 사용될 생성자
    public Item(ItemObject _itemObject)
    {
        //아이템의 기본정보 받는중
        name = _itemObject.data.name;
        id = _itemObject.data.id;
        hardness = new Hardness(_itemObject.data.hardness);
        stackable = _itemObject.data.stackable;
        type = _itemObject.data.type;
        description = _itemObject.data.description;

        ////아이템이 장비아이템이거나 룬일경우 추가적인 작업
        //if (type == ITEM_TYPE.RUNE)
        //{
        //    RuneAttributeDecision(_itemObject);
        //}
        //else
        //{
        //    EquipAttributeDecision();
        //    //아이템의 스탯 부여중
        //    StatDecision(_itemObject);
        //    //아이템의 효과 부여중
        //    ItemEffectDecision();
        //}

        //if (type == ITEM_TYPE.WEAPON)
        //{
        //    //ad나 ap true는 없음
        //    damageType = (DAMAGETYPE)Random.Range(0, 2);
        //    figure = Random.Range(level, level + level * 0.5f);
        //}
    }

    //이것은 아이템의 등급을 정해놓고 획득하도록 만든 생성자
    public Item(ItemObject _itemObject, RATING r)
    {
        //아이템의 기본정보 받는중
        name = _itemObject.data.name;
        id = _itemObject.data.id;
        hardness = new Hardness(_itemObject.data.hardness);
        stackable = _itemObject.data.stackable;
        type = _itemObject.data.type;
        description = _itemObject.data.description;

        //아이템의 등급 정하는중
        rating = r;

        ////아이템이 장비아이템이거나 룬일경우 추가적인 작업
        //if (type == ITEM_TYPE.RUNE)
        //{
        //    //룬인경우 룬 하나를 만들고 정보를 만듬
        //    RuneAttributeDecision(_itemObject);
        //}
        //else
        //{
        //    //장비인경우 빈 룬칸을 만들어야 할 수 있음 (등급에 따라 룬을 넣을 수 있기에)
        //    EquipAttributeDecision();
        //    //아이템의 스탯 부여중
        //    StatDecision(_itemObject);
        //    //아이템의 효과 부여중
        //    ItemEffectDecision();
        //}

        //if(type == ITEM_TYPE.WEAPON)
        //{
        //    //ad나 ap true는 없음
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
            //장비에 ratings.Length == 0 인건 실수이니까 그냥 오류가 나도록 함 (여기서 오류가 나면 ratings.Length == 0이라서 나는 거니까 수정해야함)
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
    //            //고유효과
    //            break;
    //        default:
    //            rune = new Rune[1];
    //            rune[0] = new Rune();
    //            //itemEffects = new ItemEffect[0];
    //            break;
    //    }
    //}

    ////수정필요 (존재하는 이펙트중에 하나가 아니라 종류별로 나눈 다음 하나를 리턴받아야 함)
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

////아이템으로 우클릭했을때 무슨 블록이 설치되는지
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
    NONE,   //아무것도 아님
    PICKAX, //곡괭이
    AXE,    //도끼
    SHOVELS,//삽
}