using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipItemObject : ItemObject
{
    [Header("�� �������� ���������� ���� ������ ����")]
    //�����ۿ� ���� �� �ִ� ������ ����
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
    //�� �������� Ȯ�������� ���� ����
    [SerializeField]
    protected AttributeValue[] attributeValues;
    public AttributeValue[] AttributeValues { get { return attributeValues; } }
    //��
    [SerializeField]
    protected Rune[] rune;
    public Rune[] GetRune { get { return rune; } }

    public EquipItem(EquipItemObject itemObject, int level) : base(itemObject)
    {
        this.level = Mathf.Max(Random.Range(level - 3, level + 3), 0);
        
        //�������� ��� ���ϴ���
        RatingDecision(itemObject);
        AttributeCountDecision();
        //�������� ���� �ο���
        AttributeValueDecision(itemObject);
        ////�������� ȿ�� �ο���
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
        //���Կ� �鵵 �پ��ٸ� �鰹����ŭ �׸��� ��ȿ� �ִ� ������ ������ŭ ���������
        for (int i = 0; i < rune.Length; i++)
        {
            for (int j = 0; j < rune[i].AttributeValues.Length; j++)
            {
                stat.AddStat(rune[i].AttributeValues[j].Attribute, rune[i].AttributeValues[j].Value);
            }
        }

        ////�������� ȿ���� �ִµ� �����ۿ��� ȿ���� ���� ȿ���� ���� ������ �ֱ⶧���� ������ ȿ���� �����ͼ� ����
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
        //���Կ� �鵵 �پ��ٸ� �鰹����ŭ �׸��� ��ȿ� �ִ� ������ ������ŭ ���������
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
        //attributeValues�� ���� ���� ����
        //itemEffects rune�� ��� ����
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
                //����ȿ��
                //itemEffects = new ItemEffect[0];
                rune = new Rune[0];
                break;
            default:
                attributeValues = new AttributeValue[0];
                //itemEffects = new ItemEffect[0];
                rune = new Rune[0];
                break;
        }
        //�������� �� ���� ������ (���߿� ���� �ִ� ����̶� ������ �����س���)
        for (int i = 0; i < rune.Length; i++)
        {
            rune[i] = new Rune();
        }
    }

    public void AttributeValueDecision(EquipItemObject _itemObject)
    {
        //���⼭ ������ ���°��� buffs�� �������� �ʾұ� ������ �����ؾ��� (���ų� ���̸鼭 ������ �� �ִ� ������ ������ ���⶧���� ������ ���°�)
        int index;
        for (int i = 0; i < attributeValues.Length; i++)
        {
            index = Random.Range(0, _itemObject.buffs.Length);
            attributeValues[i] = new AttributeValue(_itemObject.buffs[index].Attribute, _itemObject.buffs[index].min, _itemObject.buffs[index].max, level);
        }
    }
}

///         ����  ȿ��  ��
///COMMON    2     1    x
///UNCOMMON  3     1    x 
///RARE      2     0    o     
///UNIQUE    1     2    o   
///LEGEND    4     1    x 
///
///���� 3~5���� �������� ���� ����
///�鵵 ����� �ֳ�
///���� ���ȸ� �÷��ִ� �����ΰ�
///�ܾ �ϼ��ϸ� �߰� ����
///�����غ��� �鵵 �������ε�

[System.Serializable]
//��ް� ���ڴ� �������� �ް� ������ ����� ������ �޴´�
//�鿡�� ȿ���� �־�����ϴ��� �����
public class Rune
{
    [SerializeField]
    protected AttributeValue[] attributeValues;
    public AttributeValue[] AttributeValues { get { return attributeValues; } }
    [SerializeField]
    private char word;
    public char Word { get { return word; } }

    //��� �������� �ڽ��� ������� ���鶧
    public Rune()
    {
        attributeValues = new AttributeValue[0];
    }
    //�� �������� �ڽ��� ���� ���� �����Ҷ�
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

    //���� ��� �������� ������ 
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
/// �������� ������ �ִ� ����
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
    [Header("������ �ö� �ɷ�ġ")]
    public int levelFigure;

    public AttributeValue(ATTRIBUTES _attribute, int _min, int _max, int level)
    {
        attribute = _attribute;
        min = _min + level * levelFigure;
        max = _max + level * levelFigure;
        SetValue();
    }
    //�������� �÷��ִ� ������ ��ġ
    public void SetValue()
    {
        value = Random.Range(min, max);
    }
}