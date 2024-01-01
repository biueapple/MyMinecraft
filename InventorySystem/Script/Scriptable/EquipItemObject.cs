using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItemObject : ItemObject
{
    [Header("이 아이템을 생성했을때 보유 가능한 스탯")]
    //아이템에 붙을 수 있는 스탯의 종류
    public AttributeValue[] buffs;
    public override ItemInterface CreateItem(int level)
    {
        EquipItem newItem = new EquipItem(this, level);
        return newItem;
    }
}

[System.Serializable]
public class EquipItem : Item
{
    //이 아이템의 확정적으로 붙은 스탯
    [SerializeField]
    protected AttributeValue[] attributeValues;
    public AttributeValue[] AttributeValues { get { return attributeValues; } }
    //룬
    [SerializeField]
    protected Rune[] rune;
    public Rune[] GetRune { get { return rune; } }

    public EquipItem(EquipItemObject itemObject, int level) : base(itemObject)
    {
        this.level = Mathf.Max(Random.Range(level - 3, level + 3), 0);
        
        //아이템의 등급 정하는중
        RatingDecision(itemObject);
        AttributeCountDecision();
        //아이템의 스탯 부여중
        AttributeValueDecision(itemObject);
        ////아이템의 효과 부여중
        //ItemEffectDecision();
    }

    public override void RightMouseDown(Player player)
    {
        
    }

    public override void RightMouseUp(Player player)
    {

    }

    public override void Equip(Stat stat)
    {
        for (int i = 0; i < attributeValues.Length; i++)
        {
            stat.AddStat(attributeValues[i].Attribute, attributeValues[i].Value);
        }
        //슬롯에 룬도 붙었다면 룬갯수만큼 그리고 룬안에 있는 스탯의 갯수만큼 더해줘야함
        for (int i = 0; i < rune.Length; i++)
        {
            for (int j = 0; j < rune[i].AttributeValues.Length; j++)
            {
                stat.AddStat(rune[i].AttributeValues[j].Attribute, rune[i].AttributeValues[j].Value);
            }
        }

        ////아이템의 효과를 넣는데 아이템에게 효과는 없고 효과에 대한 정보만 있기때문에 정보로 효과를 가져와서 넣음
        //for (int i = 0; i < _slot.GetItem.ItemEffects.Length; i++)
        //{
        //    _slot.GetItem.ItemEffects[i] = ItemImpactManager.GetEffect(_slot.GetItem.ItemEffects[i]);
        //    _slot.GetItem.ItemEffects[i].Installation(character.STAT);
        //}
    }

    public override void Release(Stat stat)
    {
        for (int i = 0; i < attributeValues.Length; i++)
        {
            stat.TakeStat(attributeValues[i].Attribute, attributeValues[i].Value);
        }
        //슬롯에 룬도 붙었다면 룬갯수만큼 그리고 룬안에 있는 스탯의 갯수만큼 더해줘야함
        for (int i = 0; i < rune.Length; i++)
        {
            for (int j = 0; j < rune[i].AttributeValues.Length; j++)
            {
                stat.TakeStat(rune[i].AttributeValues[j].Attribute, rune[i].AttributeValues[j].Value);
            }
        }
    }

    public void AttributeCountDecision()
    {
        //attributeValues는 장비와 룬이 소유
        //itemEffects rune은 장비만 소유
        switch (rating)
        {
            case RATING.COMMON:
                attributeValues = new AttributeValue[2];
                //itemEffects = new ItemEffect[1];
                rune = new Rune[0];
                break;
            case RATING.UNCOMMON:
                attributeValues = new AttributeValue[3];
                //itemEffects = new ItemEffect[1];
                rune = new Rune[0];
                break;
            case RATING.RARE:
                attributeValues = new AttributeValue[2];
                //itemEffects = new ItemEffect[0];
                rune = new Rune[Random.Range(3, 6)];
                break;
            case RATING.UNIQUE:
                attributeValues = new AttributeValue[1];
                //itemEffects = new ItemEffect[2];
                rune = new Rune[Random.Range(3, 6)];
                break;
            case RATING.LEGEND:
                attributeValues = new AttributeValue[4];
                //고유효과
                //itemEffects = new ItemEffect[0];
                rune = new Rune[0];
                break;
            default:
                attributeValues = new AttributeValue[0];
                //itemEffects = new ItemEffect[0];
                rune = new Rune[0];
                break;
        }
        //아이템의 룬 공간 마련중 (나중에 껴서 넣는 방식이라 공간만 마련해놓자)
        for (int i = 0; i < rune.Length; i++)
        {
            rune[i] = new Rune();
        }
    }

    public void AttributeValueDecision(EquipItemObject _itemObject)
    {
        //여기서 오류가 나는것은 buffs를 정해주지 않았기 때문에 수정해야함 (장비거나 룬이면서 소유할 수 있는 스탯의 종류가 없기때문에 오류가 나는것)
        int index;
        for (int i = 0; i < attributeValues.Length; i++)
        {
            index = Random.Range(0, _itemObject.buffs.Length);
            attributeValues[i] = new AttributeValue(_itemObject.buffs[index].Attribute, _itemObject.buffs[index].min, _itemObject.buffs[index].max, level);
        }
    }
}

///         스탯  효과  룬
///COMMON    2     1    x
///UNCOMMON  3     1    x 
///RARE      2     0    o     
///UNIQUE    1     2    o   
///LEGEND    4     1    x 
///
///룬은 3~5까지 랜덤으로 보유 가능
///룬도 등급이 있네
///룬은 스탯만 올려주는 느낌인가
///단어를 완성하면 추가 스탯
///생각해보니 룬도 아이템인데

[System.Serializable]
//등급과 문자는 랜덤으로 받고 스탯은 등급의 영향을 받는다
//룬에도 효과를 넣어줘야하는지 고민중
public class Rune
{
    [SerializeField]
    protected AttributeValue[] attributeValues;
    public AttributeValue[] AttributeValues { get { return attributeValues; } }
    [SerializeField]
    private char word;
    public char Word { get { return word; } }

    //장비 아이템이 자신의 룬공간을 만들때
    public Rune()
    {
        attributeValues = new AttributeValue[0];
    }
    //룬 아이템이 자신이 가진 룬을 설정할때
    public Rune(EquipItemObject _itemObject, int count, RATING r)
    {
        attributeValues = new AttributeValue[count];
        //int index;
        //for (int i = 0; i < count; i++)
        //{
        //    index = Random.Range(0, _itemObject.buffs.Length);
        //    attributeValues[i] = new AttributeValue(_itemObject.buffs[index].attribute, _itemObject.buffs[index].min + (int)r, _itemObject.buffs[index].max + (int)r);
        //}
        SetRandomCharacter();
    }

    //아직 장비 아이템이 없으니 
    //public bool InsertRune(Item data)
    //{
    //    if(data.type == ITEM_TYPE.RUNE)
    //    {
    //        attributeValues = new AttributeValue[data.AttributeValues.Length];
    //        for (int i = 0; i < attributeValues.Length; i++)
    //        {
    //            attributeValues[i] = new AttributeValue(data.AttributeValues[i].attribute, data.AttributeValues[i].Value, data.AttributeValues[i].Value);
    //        }
    //        return true;
    //    }
    //    return false;
    //}

    public void SetRandomCharacter()
    {
        word = (char)Random.Range('A', 'Z' + 1);
    }
}


/// <summary>
/// 아이템이 가지고 있는 스텟
/// </summary>
[System.Serializable]
public class AttributeValue
{
    [SerializeField]
    private ATTRIBUTES attribute;
    public ATTRIBUTES Attribute { get { return attribute; } }
    [SerializeField]
    private int value;
    public int Value { get { return value; } }
    public int min;
    public int max;
    [Header("레벨당 올라갈 능력치")]
    public int levelFigure;

    public AttributeValue(ATTRIBUTES _attribute, int _min, int _max, int level)
    {
        attribute = _attribute;
        min = _min + level * levelFigure;
        max = _max + level * levelFigure;
        SetValue();
    }
    //아이템이 올려주는 스텟의 수치
    public void SetValue()
    {
        value = Random.Range(min, max);
    }
}