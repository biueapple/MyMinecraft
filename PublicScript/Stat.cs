using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum DAMAGETYPE
{
    AD,
    AP,
    TRUE,
    HEAL,

}

public enum ATTACKTYPE
{
    NONE,
    NOMAL,
    SPECIAL
}

public enum ATTRIBUTES
{
    HP,
    MP,
    DEFENCE,
    RESISTANCE,
    AD,
    AP,
    ATTACKSPEED,
    CRITICAL,
    CRITICALMULTIPLIER,
    SPEED,
}

//shield
//Barrier
//protective

[System.Serializable]
public class Stat : MonoBehaviour
{
    //체력
    [SerializeField]
    private float hp;
    public float HP { get { return  hp; } }
    [SerializeField]
    private float maxHp;
    public float MAXHP { get { return maxHp; } set { maxHp += value; hp += value; } }
    //체력 자연 회복량
    private float naturalHP;

    //배리어는 방마저영향을 받지 않음
    private List<Barrier> barrier;
    public float barrierFigure 
    {
        get
        {
            float f = 0;
            if (barrier == null)
                return f;
            for (int i = 0; i < barrier.Count; i++)
                f += barrier[i].figure;
            return f;
        }
    }
    public Barrier Barrier 
    { 
        set
        {
            if (barrier == null)
                barrier = new List<Barrier>();
            barrier.Add(value);
            if (barrierCoroutine == null && value.continuing == false)
                barrierCoroutine = StartCoroutine(BarrierCoroutine());
        }
    }
    private Coroutine barrierCoroutine = null;

    //마나
    [SerializeField]
    private float mp;
    public float MP { get { return mp; } }
    [SerializeField]
    private float maxMp;
    public float MAXMP { get { return maxMp; }}
    private float naturalMP;

    //방어력
    [SerializeField]
    private float defence;
    public float DEFENCE { get { return defence; }}
    
    //저항력
    [SerializeField]
    private float resistance;
    public float RESISTANCE { get { return resistance; }}

    //공격력
    [SerializeField]
    private float ad;
    public float AD { get { return ad; }}

    //주문력
    [SerializeField]
    private float ap;
    public float AP { get { return ap; }}

    //공격속도
    [SerializeField]
    private float attackSpeed;
    public float AttackSpeed { get { return attackSpeed; }}

    [SerializeField]
    private float attacktimer;
    public float Attacktimer { get {  return attacktimer; } set { attacktimer = value; } }

    //치명타 확률
    [SerializeField]
    private float critical;
    public float Critical { get { return critical; }}

    //치명타 배율
    [SerializeField]
    private float criticalMultiplier;
    public float CriticalMultiplier { get { return criticalMultiplier; }}

    //이동속도
    [SerializeField]
    private float speed;
    public float SPEED { get { return speed; }}


    //Natural_Recovery_HP
    private Coroutine nrhp = null;
    //Natural_Recovery_MP
    private Coroutine nrmp = null;
    //Dot_Poison
    private Coroutine dot_poison = null;
    private Coroutine dot_burn = null;
    private Coroutine dot_shock = null;
    private Coroutine dot_bleeding = null;

    //투사체와 즉발 공격 둘다 통일성을 위해 맞았을때 attack hit 둘다 호출하는 걸로
    //공격전에 발동하는 델리게이트는 return이나 out으로 대미지의 변경을 하는 방법을 생각했는데
    //out 키워드는 불가능하고 return은 마지막으로 들어간 함수의 리턴만 받기때문에
    //list로 하나씩 호출하기로 함

    //Func의 경우 가해자 피해자 수치 리턴 순
    //Action의 경우 가해자 피해자 수치 순

    //기본 공격 시 전에 발동
    private List<Func<Stat, Stat, float, float>> nomalAttackBefore = null;

    //기본 공격 시 후에 발동
    private Action<Stat, Stat, float> nomalAttackAfter = null;

    //그 외의 공격 전에 발동
    private List<Func<Stat, Stat, float, float>> specialAttackBefore = null;
    // 후에 발동
    private Action<Stat, Stat, float> specialAttackAfter = null;

    //기본공격 받기 전에 발동
    private List<Func<Stat, Stat, float, float>> hitNomalBefore = null;
    // 후에 발동
    private Action<Stat, Stat, float> hitNomalAfter = null;

    //그 외의 공격을 받기 전에 발동
    private List<Func<Stat, Stat, float, float>> hitSpecialBefore = null;
    // 후에 발동
    private Action<Stat, Stat, float> hitSpecialAfter = null;

    //내가 누군갈 회복 시키기 전에 (hp) 자신 포함
    private List<Func<Stat, Stat, float, float>> healOtherBefore = null;
    // 후에 발동
    private Action<Stat, Stat, float> healOtherAfter = null;

    //모든 순서가 끝난 후 콜하는 Action (피해자, 수치, 타입)
    private Action<Stat, float, DAMAGETYPE, bool> textingUI = null;


    //상태이상을 받는지 안받는지
    public bool poison = true;
    public bool burn = true;
    public bool shock = true;
    public bool bleeding = true;


    //도트뎀이나 자연 회복이 몇초에 한번 일어날지
    public float dotTime = 2;
    public float naturalTime = 2;

    public string SetText()
    {
        string str = "체력 = " + Mathf.Floor(hp * 100f) / 100f + " / " + Mathf.Floor(maxHp * 100f) / 100f + "\n" +
                    "마나 = " + Mathf.Floor(mp * 100f) / 100f + " / " + Mathf.Floor(maxMp * 100f) / 100f + "\n" +
                    "체력재생 = " + naturalHP + "\n" +
                    "마나재생 = " + naturalMP + "\n" +
                    "방어력 = " + defence + "\n" +
                    "저항력 = " + resistance + "\n" +
                    "공격력 = " + ad + "\n" +
                    "주문력 = " + ap + "\n" +
                    "속도 = " + speed;
        return str;
    }

    public void Init(float _hp, float _mp,float _nh,float _nm, float _de, float _re, float _ad,float _ap, float _attackSpeed, float _critical, float _critacalMultiple, float _speed)
    {
        hp = _hp;
        maxHp = _hp;
        mp = _mp;
        maxMp = _mp;
        naturalHP = _nh;
        naturalMP = _nm;
        defence = _de;
        resistance = _re;
        ad = _ad;
        ap = _ap;
        attackSpeed = _attackSpeed;
        attacktimer = attackSpeed;
        critical = _critical;
        criticalMultiplier = _critacalMultiple;
        speed = _speed;
    }


    public void AddStat(ATTRIBUTES attribute, float value)
    {
        switch (attribute)
        {
            case ATTRIBUTES.HP:
                maxHp += value;
                hp += value;
                break;
            case ATTRIBUTES.MP:
                maxMp += value;
                mp += value;
                break;
            case ATTRIBUTES.DEFENCE:
                defence += value;
                break;
            case ATTRIBUTES.RESISTANCE:
                resistance += value;
                break;
            case ATTRIBUTES.AD:
                ad += value;
                break;
            case ATTRIBUTES.AP:
                ap += value;
                break;
            case ATTRIBUTES.ATTACKSPEED:
                attackSpeed += value;
                break;
            case ATTRIBUTES.CRITICAL:
                critical += value;
                break;
            case ATTRIBUTES.CRITICALMULTIPLIER:
                criticalMultiplier += value;
                break;
            case ATTRIBUTES.SPEED:
                speed += value;
                break;
        }
    }
    public void TakeStat(ATTRIBUTES attribute, float value)
    {
        switch (attribute)
        {
            case ATTRIBUTES.HP:
                maxHp -= value;
                if(hp > maxHp)
                    hp = maxHp;
                break;
            case ATTRIBUTES.MP:
                maxMp -= value;
                if(mp > maxMp)
                    mp = maxMp;
                break;
            case ATTRIBUTES.DEFENCE:
                defence -= value;
                break;
            case ATTRIBUTES.RESISTANCE:
                resistance -= value;
                break;
            case ATTRIBUTES.AD:
                ad -= value;
                break;
            case ATTRIBUTES.AP:
                ap -= value;
                break;
            case ATTRIBUTES.ATTACKSPEED:
                attackSpeed -= value;
                break;
            case ATTRIBUTES.CRITICAL:
                critical -= value;
                break;
            case ATTRIBUTES.CRITICALMULTIPLIER:
                criticalMultiplier -= value;
                break;
            case ATTRIBUTES.SPEED:
                speed -= value;
                break;
        }
    }

    public void MinusHp(float figure)
    {
        hp -= figure;
        if(nrhp == null )
        {
            nrhp = StartCoroutine(Natural_Recovery_HP());
        }
        Debug.Log($"{name} 남은 체력 {hp}");
    }

    private IEnumerator BarrierCoroutine()
    {
        while(barrier.Count != 0)
        {
            for(int i = 0; i < barrier.Count; i++)
            {
                if (barrier[i].continuing)
                {
                    barrier[i].timer -= Time.deltaTime;
                    if (barrier[i].timer <= 0)
                    {
                        barrier.RemoveAt(i);
                        i--;
                    }
                }
            }
            yield return null;
        }
        barrierCoroutine = null;
    }

    public void MinusMp(float figure) 
    { 
        mp -= figure;
        if (mp < 0)
            mp = 0;
        if (nrmp == null)
        {
            nrmp = StartCoroutine(Natural_Recovery_MP());
        }
    }

    public void RecoveryHP(Stat perpetrator, float figure)
    {
        figure = CallHealBefore(perpetrator, this, figure);

        if (hp + figure > maxHp)
            figure = maxHp - hp;
        hp += figure;

        CallHealAfter(perpetrator, this, figure);
    }

    public void RecoveryMP(Stat perpetrator, float figure)
    {
        if (mp + figure > maxMp)
            figure = maxMp - mp;
        mp += figure;
    }

    public float BarrierAbsorption(float f)
    {
        if (barrier == null)
            return f;
        for(int i = 0; i < barrier.Count; i++)
        {
            if (barrier[i].figure <= f)
            {
                f -= barrier[i].figure;
                barrier.RemoveAt(i);
                i--;
            }
            else
            {
                barrier[i].figure -= f;
                f = 0;
                break;
            }
            if (f <= 0)
                return 0;
        }

        return f;
    }

    /// <summary>
    /// ad 대미지를 방어력에 맞게 조정해줌
    /// </summary>
    /// <param name="figure">대미지</param>
    /// <param name="penetration">방어구 관통력 (고정) </param>
    /// <param name="per">방어구 관통력 (퍼센트)</param>
    /// <returns>방어력과 관통력을 거쳐간 대미지</returns>
    public float Halved_AD(float figure, float penetration, float per)
    {
        float defence = this.defence - penetration;
        defence -= defence * per * 0.01f;
        return (100 / (100 + defence) * figure);
    }
    /// <summary>
    /// ap 대미지를 방어력에 맞게 조정해줌
    /// </summary>
    /// <param name="figure">대미지</param>
    /// <param name="penetration">저항력 관통력 (고정) </param>
    /// <param name="per">저항력 관통력 (퍼센트)</param>
    /// <returns>저항력과 관통력을 거쳐간 대미지</returns>
    public float Halved_AP(float figure, float penetration, float per)
    {
        float resistance = this.resistance - penetration;
        resistance -= resistance * per * 0.01f;
        return (100 / (100 + resistance) * figure);
    }

    public float Be_Attacked(Stat perpetrator, float figure, ATTACKTYPE attack, DAMAGETYPE damage)
    {
        //배리어는 방마저영향을 받지 않음
        float f = BarrierAbsorption(figure);
        Debug.Log("보호막으로 막은 양" + (figure - f));
        CallTextingUI(this, figure - f, damage, true);
        figure = f;

        switch (attack)
        {
            case ATTACKTYPE.NONE:
                {
                    switch (damage)
                    {
                        case DAMAGETYPE.AD:
                            Be_Attacked_AD(figure, perpetrator);
                            break;
                        case DAMAGETYPE.AP:
                            Be_Attacked_AP(figure, perpetrator);
                            break;
                        case DAMAGETYPE.TRUE:
                            Be_Attacked_TRUE(figure, perpetrator);
                            break;
                        case DAMAGETYPE.HEAL:
                            RecoveryHP(perpetrator, figure);
                            break;
                    }
                }
                break;
            case ATTACKTYPE.NOMAL:
                {
                    figure = perpetrator.CallNomalAttackBefore(perpetrator, this, figure);
                    figure = CallHitNomalBefore(perpetrator, this, figure);
                    switch (damage)
                    {
                        case DAMAGETYPE.AD:
                            Be_Attacked_AD(figure, perpetrator);
                            break;
                        case DAMAGETYPE.AP:
                            Be_Attacked_AP(figure, perpetrator);
                            break;
                        case DAMAGETYPE.TRUE:
                            Be_Attacked_TRUE(figure, perpetrator);
                            break;
                        case DAMAGETYPE.HEAL:
                            RecoveryHP(perpetrator, figure);
                            break;
                    }
                    CallHitNomalAfter(perpetrator, this, figure);
                    perpetrator.CallNomalAttackAfter(perpetrator, this, figure);
                }
                break;
            case ATTACKTYPE.SPECIAL:
                {
                    figure = perpetrator.CallSpecialAttackBefore(perpetrator, this, figure);
                    figure = CallHitSpecialBefore(perpetrator, this, figure);
                    switch (damage)
                    {
                        case DAMAGETYPE.AD:
                            Be_Attacked_AD(figure, perpetrator);
                            break;
                        case DAMAGETYPE.AP:
                            Be_Attacked_AP(figure, perpetrator);
                            break;
                        case DAMAGETYPE.TRUE:
                            Be_Attacked_TRUE(figure, perpetrator);
                            break;
                        case DAMAGETYPE.HEAL:
                            RecoveryHP(perpetrator, figure);
                            break;
                    }
                    CallHitSpecialAfter(perpetrator, this, figure);
                    perpetrator.CallSpecialAttackAfter(perpetrator, this, figure);
                }
                break;
        }

        CallTextingUI(this, figure, damage, false);
        return figure;
    }

    /// <summary>
    /// 누군가에게 공격 받았을 때
    /// </summary>
    /// <param name="figure">대매지</param>
    /// <param name="penetration">관통력</param>
    /// <param name="per">관통력 (퍼센트)</param>
    /// <param name="perpetrator">누가 공격한건지</param>
    private float Be_Attacked_AD(float figure, Stat perpetrator)
    {
        float damage = Halved_AD(figure, 0, 0);
        MinusHp(damage);

        return damage;
    }
    /// <summary>
    /// 누군가에게 공격 받았을 때
    /// </summary>
    /// <param name="figure">대매지</param>
    /// <param name="penetration">관통력</param>
    /// <param name="per">관통력 (퍼센트)</param>
    /// <param name="perpetrator">누가 공격한건지</param>
    private float Be_Attacked_AP(float figure, Stat perpetrator)
    {
        float damage = Halved_AP(figure, 0, 0);
        MinusHp(damage);

        return damage;
    }
    /// <summary>
    /// 누군가에게 공격 받았을 때
    /// </summary>
    /// <param name="figure">대매지</param>
    /// <param name="perpetrator">누가 공격한건지</param>
    private float Be_Attacked_TRUE(float figure, Stat perpetrator)
    {
        MinusHp(figure);

        return figure;
    }

    //자연 회복, max보다 많아지면 끝남
    private IEnumerator Natural_Recovery_HP()
    {
        float t = 0;
        while(hp < maxHp)
        {
            t += Time.deltaTime;

            if(t > naturalTime)
            {
                hp += naturalHP;
                t = 0;
            }

            yield return null;
        }
    }
    private IEnumerator Natural_Recovery_MP()
    {
        float t = 0;
        while (mp < maxMp)
        {
            t += Time.deltaTime;

            if (t > naturalTime)
            {
                mp += naturalMP;
                t = 0;
            }

            yield return null;
        }
    }

    //도트대미지는 받을때 이미 발동중인 도트대미지를 없앤 후 발동함
    //  독 : 방어력이나 저항력 판정을 받지 않음 attackDelegate 판정을 받음
    /// <summary>
    /// 독 도트뎀 1초마다 대미지가 들어감
    /// </summary>
    /// <param name="duration">발동 횟수</param>
    /// <param name="figure">1회당 대미지</param>
    /// <param name="attack">공격시 발동하는 delegate</param>
    /// <param name="perpetrator">누가 했는지</param>
    public void Be_Attacked_Poison(int duration, float figure, Action<Stat, float> attack, Stat perpetrator)
    {
        if(poison)
        {
            if (dot_poison != null)
            {
                StopCoroutine(dot_poison);
            }
            dot_poison = StartCoroutine(Dot_Poison(duration, figure, attack));
        }
    }
    private IEnumerator Dot_Poison(int duration, float figure, Action<Stat, float> attack)
    {
        int du = 0;
        float t = 0;
        while (du <= duration)
        {
            yield return null;

            t += Time.deltaTime;

            if(t >= dotTime)
            {
                MinusHp(figure);
                attack(this, figure);
                du++;
                t = 0;
            }
        }
        dot_poison = null;
    }
    //화상 : 방어력이나 저항력 판정을 받지 않음 hitDelegate판정을 받음
    /// <summary>
    /// 화상 도트뎀 1초마다 대미지가 들어감
    /// </summary>
    /// <param name="duration">발동 횟수</param>
    /// <param name="figure">1화당 대미지</param>
    /// <param name="perpetrator">누가 했는지</param>
    public void Be_Attacked_Burn(int duration, float figure, Stat perpetrator)
    {
        if(burn)
        {
            if (dot_burn != null)
            {
                StopCoroutine(dot_burn);
            }
            dot_burn = StartCoroutine(Dot_Burn(duration, figure, perpetrator));
        }
    }
    private IEnumerator Dot_Burn(int duration, float figure, Stat perpetrator)
    {
        int du = 0;
        float t = 0;
        while (du <= duration)
        {
            yield return null;

            t += Time.deltaTime;

            if (t >= dotTime)
            {
                Be_Attacked_TRUE(figure, perpetrator);
                du++;
                t = 0;
            }
        }
        dot_burn = null;
    }
    //감전 : ap딜 저항력 판정을 받음 hitDelegate판정을 받음
    /// <summary>
    /// 감전 도트뎀 1초마다 대미지가 들어감
    /// </summary>
    /// <param name="duration">발동 횟수</param>
    /// <param name="figure">1회당 대미지</param>
    /// <param name="penetration">방어구 관통력(+)</param>
    /// <param name="per">방어구 관통력(%)</param>
    /// <param name="perpetrator">누가 했는지</param>
    public void Be_Attacked_Shock(int duration, float figure, float penetration, float per, Stat perpetrator)
    {
        if(shock)
        {
            if (dot_shock != null)
            {
                StopCoroutine(dot_shock);
            }
            dot_shock = StartCoroutine(Dot_Shock(duration, figure, penetration, per, perpetrator));
        }
    }
    private IEnumerator Dot_Shock(int duration, float figure, float penetration, float per, Stat perpetrator)
    {
        int du = 0;
        float t = 0;    
        while (du <= duration)
        {
            yield return null;

            t += Time.deltaTime;

            if(t >= dotTime)
            {
                Be_Attacked_AP(figure, perpetrator);
                du++;
                t = 0;
            }
        }
        dot_shock = null;
    }
    //출혈 : ad딜 방어력 판정을 받음 attackDelegate 판정을 받음
    /// <summary>
    /// 출혈 도트뎀 1초마다 대미지가 들어감
    /// </summary>
    /// <param name="duration">발동 횟수</param>
    /// <param name="figure">1회당 대미지</param>
    /// <param name="penetration">방어구 관통력(+)</param>
    /// <param name="per">방어구 관통력(%)</param>
    /// <param name="attack">공격시 발동하는 delegate</param>
    /// <param name="perpetrator">누가 했는지</param>
    public void Be_Attacked_Bleeding(int duration, float figure, float penetration, float per, Action<Stat, float> attack, Stat perpetrator)
    {
        if(bleeding)
        {
            if (dot_bleeding != null)
            {
                StopCoroutine(dot_bleeding);
            }
            dot_bleeding = StartCoroutine(Dot_Bleeding(duration, figure, penetration, per, attack));
        }
    }
    private IEnumerator Dot_Bleeding(int duration, float figure, float penetration, float per, Action<Stat, float> attack)
    {
        int du = 0;
        float t = 0;
        float damage;
        while (du <= duration)
        {
            yield return null;

            t += Time.deltaTime;

            if(t >= dotTime)
            {
                damage = Halved_AD(figure, penetration, per);
                MinusHp(damage);
                attack(this, figure);
                du++;
                t = 0;
            }
        }
        dot_bleeding = null;
    }


    //테스트 해본 결과, 추가는 중복방지를 해야하지만 빼는것은 상관없음 null일때 빼도 에러 안나고
    //중복도 알아서 하나만 빼짐
    //하지만 호출할때는 null이면 오류남

    //기본 공격 라인
    //기본 공격 전 호출 추가
    //중복방지는 하지 않는걸로 
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void AddNomalAttackBefore(Func<Stat, Stat, float, float> action)
    {
        if (nomalAttackBefore == null)
            nomalAttackBefore = new List<Func<Stat, Stat, float, float>>();
        //중복방지
        //if (!nomalAttackBefore.Contains(action))
            nomalAttackBefore.Add(action);
    }
    //기본 공격 후 호출 추가
    public void AddNomalAttackAfter(Action<Stat, Stat, float> action)
    {
        if(nomalAttackAfter == null)
            nomalAttackAfter += action;
        //중복방지
        else //if (!nomalAttackAfter.GetInvocationList().Contains(action))
            nomalAttackAfter += action;
    }
    //기본 공격 전 호출 빼기
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void RemoveNomalAttackBefore(Func<Stat, Stat, float, float> action)
    {
        if (nomalAttackBefore == null)
            return;
        nomalAttackBefore.Remove(action);
    }
    //기본 공격 후 호출 빼기
    public void RemoveNomalAttackAfter(Action<Stat, Stat, float> action)
    {
        nomalAttackAfter -= action;
    }

    //특수 공격 라인
    //특수 공격 전 호출 추가
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void AddSpecialAttackBefore(Func<Stat, Stat, float, float> action)
    {
        if (specialAttackBefore == null)
            specialAttackBefore = new List<Func<Stat, Stat, float, float>>();
        //if (!specialAttackBefore.Contains(action))
            specialAttackBefore.Add(action);
    }
    //특수 공격 후 호출 추가
    public void AddSecialAttackAfter(Action<Stat, Stat, float> action)
    {
        if(specialAttackAfter == null)
            specialAttackAfter += action;
        //중복방지
        else// if (!specialAttackAfter.GetInvocationList().Contains(action))
            specialAttackAfter += action;
    }
    //특수 공격 전 호출 빼기
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void RemoveSpecialAttackBefore(Func<Stat, Stat, float, float> action)
    {
        if (specialAttackBefore == null)
            return;
        specialAttackBefore.Remove(action);
    }
    //특수 공격 후 호출 빼기
    public void RemoveSecialAttackAfter(Action<Stat, Stat, float> action)
    {
        specialAttackAfter -= action;
    }

    //기본 공격 라인
    //기본 공격 받기 전에 호출 추가
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void AddHitNomalBefore(Func<Stat, Stat, float, float> action)
    {
        if (hitNomalBefore == null)
            hitNomalBefore = new List<Func<Stat, Stat, float, float>>();
        //if (!hitNomalBefore.Contains(action))
            hitNomalBefore.Add(action);
    }
    //기본 공격 받은 후 호출 추가
    public void AddHitNomalAfter(Action<Stat, Stat, float> action)
    {
        if (hitNomalAfter == null)
            hitNomalAfter += action;
        //중복방지
        else //if (!hitNomalAfter.GetInvocationList().Contains(action))
            hitNomalAfter += action;
    }
    //기본 공격 받기 전 호출 빼기
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void RemoveHitNomalBefore(Func<Stat, Stat, float, float> action)
    {
        if (hitNomalBefore == null)
            return;
        hitNomalBefore.Remove(action);
    }
    //기본 공격 받을 시 호출 빼기
    public void RemoveHitNomalAfter(Action<Stat, Stat, float> action)
    {
        hitNomalAfter -= action;
    }

    //특수 공격 라인
    //특수 공격 받기 전 호출 추가
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void AddHitSpecialBefore(Func<Stat, Stat, float, float> action)
    {
        if(hitSpecialBefore == null)
            hitSpecialBefore = new List<Func<Stat, Stat, float, float>> ();
        //if (!hitSpecialBefore.Contains(action))
            hitSpecialBefore.Add(action);
    }
    //특수 공격 받은 후 호출 추가
    public void AddHitSpecialAfter(Action<Stat, Stat, float> action)
    {
        if(hitSpecialAfter == null)
            hitSpecialAfter += action;
        //중복방지
        else// if (!hitSpecialAfter.GetInvocationList().Contains(action))
            hitSpecialAfter += action;
    }
    //특수 공격 받기 전 호출 추가
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void RemoveHitSpecialBefore(Func<Stat, Stat, float, float> action)
    {
        if(hitSpecialBefore == null)
            return;
        hitSpecialBefore.Remove(action);
    }
    //특수 공격 받은 후 호출 빼기
    public void RemoveHitSecial(Action<Stat, Stat, float> action)
    {
        hitSpecialAfter -= action;
    }

    //힐 라인
    //힐 받기 전 호출 추가
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void AddHealBefore(Func<Stat, Stat, float, float> action)
    {
        if (healOtherBefore == null)
            healOtherBefore = new List<Func<Stat, Stat, float, float>>();
        //if (!healOtherBefore.Contains(action))
            healOtherBefore.Add(action);
    }
    //힐 받은 후 호출 추가
    public void AddHealAfter(Action<Stat, Stat, float> action)
    {
        if(healOtherAfter == null)
            healOtherAfter += action;
        //중복방지
        else //if (!healOtherAfter.GetInvocationList().Contains(action))
            healOtherAfter += action;
    }
    //힐 받기 전 호출 빼기
    /// <summary>
    /// 가해자 피해자 수치 리턴값 순
    /// </summary>
    /// <param name="action">가해자 피해자 수치 리턴값 순</param>
    public void RemoveHealBefore(Func<Stat, Stat, float, float> action)
    {
        if(healOtherBefore == null)
            return;
        healOtherBefore.Remove(action);
    }
    //힐 받은 후 호출 빼기
    public void RemoveHealAfter(Action<Stat, Stat, float> action)
    {
        healOtherAfter -= action;
    }


    //델리게이트 호출
    //기본 공격 전 호출
    private float CallNomalAttackBefore(Stat perpetrator, Stat victim, float figure)
    {
        if (nomalAttackBefore == null)
            return figure;
       
        for(int i = 0; i < nomalAttackBefore.Count; i++)
        {
            figure = nomalAttackBefore[i](perpetrator, victim, figure);
        }
        return figure;
    }
    //기본 공격 후 호출
    private void CallNomalAttackAfter(Stat perpetrator, Stat victim, float figure)
    {
        if (nomalAttackAfter != null) nomalAttackAfter(perpetrator, victim, figure);
    }

    //특수 공격 전 호출
    private float CallSpecialAttackBefore(Stat perpetrator, Stat victim, float figure)
    {
        if (specialAttackBefore == null)
            return figure;
        for (int i = 0; i < specialAttackBefore.Count; i++)
        {
            figure = specialAttackBefore[i](perpetrator, victim, figure);
        }
        return figure;
    }
    //특수 공격 후 호출
    private void CallSpecialAttackAfter(Stat perpetrator, Stat victim, float figure)
    {
        if (specialAttackAfter != null) specialAttackAfter(perpetrator, victim, figure);
    }

    //기본 공격 받기 전 호출
    private float CallHitNomalBefore(Stat perpetrator, Stat victim, float figure)
    {
        if (hitNomalBefore == null)
            return figure;
        for (int i = 0; i < hitNomalBefore.Count; i++)
        {
            figure = hitNomalBefore[i](perpetrator, victim, figure);
        }
        return figure;
    }
    //기본 공격 받은 후 호출
    private void CallHitNomalAfter(Stat perpetrator, Stat victim, float figure)
    {
        if (hitNomalAfter != null) hitNomalAfter(perpetrator, victim, figure);
    }

    //특수 공격 받기 전 호출
    private float CallHitSpecialBefore(Stat perpetrator, Stat victim, float figure)
    {
        if (hitSpecialBefore == null)
            return figure;
        for (int i = 0; i < hitSpecialBefore.Count; i++)
        {
            figure = hitSpecialBefore[i](perpetrator, victim, figure);
        }
        return figure;
    }
    //특수 공격 받은 후 호출
    private void CallHitSpecialAfter(Stat perpetrator, Stat victim, float figure)
    {
        if (hitSpecialAfter != null) specialAttackAfter(perpetrator, victim, figure);
    }
    //힐 받기 전 호출
    private float CallHealBefore(Stat perpetrator, Stat victim, float figure)
    {
        if (healOtherBefore == null)
            return figure;
        for (int i = 0; i < healOtherBefore.Count; i++)
        {
            figure = healOtherBefore[i](perpetrator, victim, figure);
        }
        return figure;
    }
    //힐 받은 후 호출
    private void CallHealAfter(Stat perpetrator, Stat victim, float figure)
    {
        if (healOtherAfter != null) healOtherAfter(perpetrator, victim, figure);
    }


    //
    public void DamageTextingUI(Action<Stat, float, DAMAGETYPE, bool> action)
    {
        textingUI = action;
    }
    public void RemoveTextingUI(Action<Stat, float, DAMAGETYPE, bool> action)
    {
        textingUI = null;
    }
    public void CallTextingUI(Stat victim, float figure, DAMAGETYPE type, bool barrier)
    {
        if (textingUI != null)
            textingUI(victim, figure, type, barrier);
    }
    //

    //중복처리
    public bool AlreadyNomalBefore(Func<Stat, Stat, float, float> action)
    {
        if(nomalAttackBefore == null)
            return false;
        if (nomalAttackBefore.Contains(action))
            return true;
        return false;
    }
    public bool AlreadyNomalAfter(Action<Stat, Stat, float> action)
    {
        if (nomalAttackAfter.GetInvocationList().Contains(action))
            return true;
        return false;
    }
    public bool AlreadySpecialBefore(Func<Stat, Stat, float, float> action)
    {
        if (specialAttackBefore == null)
            return false;
        if (specialAttackBefore.Contains(action))
            return true;
        return false;
    }
    public bool AlreadySpecialAfter(Action<Stat, Stat, float> action)
    {
        if (specialAttackAfter.GetInvocationList().Contains(action))
            return true;
        return false;
    }
    public bool AlreadyHitNomalBefore(Func<Stat, Stat, float, float> action)
    {
        if (hitNomalBefore == null)
            return false;
        if (hitNomalBefore.Contains(action))
            return true;
        return false;
    }
    public bool AlreadyHitNomalAfter(Action<Stat, Stat, float> action)
    {
        if (hitNomalAfter.GetInvocationList().Contains(action))
            return true;
        return false;
    }
    public bool AlreadyHitSpecialBefore(Func<Stat, Stat, float, float> action)
    {
        if (hitSpecialBefore == null)
            return false;
        if (hitSpecialBefore.Contains(action))
            return true;
        return false;
    }
    public bool AlreadyHitSpecialAfter(Action<Stat, Stat, float> action)
    {
        if (hitSpecialAfter.GetInvocationList().Contains(action))
            return true;
        return false;
    }
    public bool AlreadyHealBefore(Func<Stat, Stat, float, float> action)
    {
        if (healOtherBefore == null)
            return false;
        if (healOtherBefore.Contains(action))
            return true;
        return false;
    }
    public bool AlreadyHealAfter(Action<Stat, Stat, float> action)
    {
        if (healOtherAfter.GetInvocationList().Contains(action))
            return true;
        return false;
    }
}

//배리어는 방마저영향을 받지 않음
public class Barrier
{
    public float figure;
    public float timer;
    //timer의 영향을 받는가
    public bool continuing;
    /// <summary>
    /// 새 보호막
    /// </summary>
    /// <param name="figure">보호막 양</param>
    /// <param name="timer">보호막 유지 시간</param>
    /// <param name="continuing">보호막이 시간이 지나면 사라지는지 (true가 사라지는거)</param>
    public Barrier(float figure, float timer, bool continuing)
    {
        this.figure = figure;
        this.timer = timer;
        this.continuing = continuing;
    }
}