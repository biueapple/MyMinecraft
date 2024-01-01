using System.Collections.Generic;
using UnityEngine;

//추상클래스로 만들려 했지만 인스펙터에 안보이는 문제가 생김
//장비아이템에 대한 효과를 넣은것임
/// <summary>
/// 아이템의 중복되는 구현점을 몰아서 넣음
/// </summary>
[System.Serializable]
public class ItemEffect
{
    //이름 지금은 이름을 가지고 비교하기에 같은 이름을 가진 효과는 있으면 안됌
    [SerializeField]
    protected string effectName;
    public string Name { get { return effectName; } }

    //효과에 대한 설명
    [SerializeField]
    protected string description;
    public string Description { get { return description; } }

    //수치
    protected float figure;
    public float Figure { get { return figure; } }

    public virtual void Installation(Stat stat)
    {

    }
    public virtual void Uninstallation(Stat stat)
    {

    }
}

/// <summary>
/// 기본 공격 전에 발동되는 효과를 가진 효과는 이것을 상속받아서 쓰자
/// </summary>
[System.Serializable]
public abstract class NomalBeforeEffect : ItemEffect
{
    protected abstract float Impact(Stat perpetrator, Stat victim, float figure);
    public override void Installation(Stat stat)
    {
        stat.AddNomalAttackBefore(Impact);
    }
    public override void Uninstallation(Stat stat)
    {
        stat.RemoveNomalAttackBefore(Impact);
    }
}

/// <summary>
/// 기본 공격 후에 발동되는 효과를 가진 효과는 이것을 상속받아서 쓰자
/// </summary>
[System.Serializable]
public abstract class NomalAfterEffect : ItemEffect
{
    protected abstract void Impact(Stat perpetrator, Stat victim, float figure);
    public override void Installation(Stat stat)
    {
        stat.AddNomalAttackAfter(Impact);
    }
    public override void Uninstallation(Stat stat)
    {
        stat.RemoveNomalAttackAfter(Impact);
    }
}


//클래스를 상속받아서 생성자와 Impact만 override해주면 끝

[System.Serializable]
/// <summary>
/// 매 기본 공격마다 10의 대미지를 추가하는 효과
/// </summary>
public class AdditionalDamage : NomalBeforeEffect
{
    public AdditionalDamage()
    {
        effectName = "추가대미지";
        figure = 10;
        description = "기본공격시 대미지를 " + figure + "만큼 올린다";
    }

    protected override float Impact(Stat perpetrator, Stat victim, float figure)
    {
        return figure + this.figure;
    }
}

[System.Serializable]
/// <summary>
/// 매 기본 공격마다 10의 물리피해를 주는 효과
/// </summary>
public class AdditionalAttacks : NomalAfterEffect
{
    public AdditionalAttacks()
    {
        effectName = "추가공격";
        figure = 10;
        description = "기본공격시" + figure + "의 추가 물리피해를 입힌다";
    }
    protected override void Impact(Stat perpetrator, Stat victim, float figure)
    {
        victim.Be_Attacked(perpetrator, this.figure, ATTACKTYPE.NONE, DAMAGETYPE.AD);
    }
}


//아이템 효과들을 종류별로 나눠서 리턴해야함
//아이템 효과를 등급으로 분류할지 종류로 분류할지 뭐로하지
//공격 효과가 있고 수비 효과가 있고 효과에도 등급이 있을텐데
//효과 등급은 확률로 하고 종류로 분류하는게
/// <summary>
/// 모든 아이템 효과를 가지고 리턴해주는 클래스
/// </summary>
public static class ItemImpactManager
{
    static List<ItemEffect> effects;
    public static void Init()
    {
        if(effects == null)
        {
            effects = new List<ItemEffect>
            {
                new AdditionalAttacks(),
                new AdditionalDamage(),
            };
        }
    }

    public static ItemEffect GetEffect()
    {
        Init();
        return effects[Random.Range(0, effects.Count)];
    }

    public static ItemEffect GetEffect(ItemEffect e)
    {
        Init();
        for (int i = 0; i < effects.Count; i++)
        {
            if (effects[i].Name.Equals(e.Name))
                return effects[i];
        }
        return null;
    }
}

//진짜 많은 문제가 있었다
//ScriptableObject에 저장된 클래스는 ItemEffect i = new AdditionalAttacks();
//이런 식으로 해도 다음 플레이할 때는 AdditionalAttacks가 아니라 ItemEffect로서 존재해
//AdditionalAttacks나 NomalAfterEffect에서 override 한 함수가 아니라 ItemEffect 안에 있는 virtual 함수를 호출함
//문제를 찾는 것도 오래 걸렸고 해결하는 방법도 많이 헤맸다.

//스킬도 ScriptableObject로 만들어서 아이템이 가질 수 있는 종류의 효과들을 변수처럼 넣어서 사용할지 (value처럼)
//그냥 랜덤으로 해야할지
//ScriptableObject로 만들면 item처럼 id를 만들어서 string으로 비교하는 일이 없어질지도
//근데 아이템 하나하나 들어갈 수 있는 효과들을 넣어주자니 너무 많을거같은데
//근데 하나하나 들어갈 효과를 컨트롤 할 수 있는건 괜찮은거 같긴 한데 