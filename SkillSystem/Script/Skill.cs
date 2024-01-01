using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum SKILL_TYPE
{
    PASSIVE,
    ACTIVE,
}

public enum TRIGGER_TYPE
{
    ALL,    //둘다 level만큼 있어야 한다
    ONE,    //하나만 level만큼 있으면 된다
    SUM     //합쳐서 level만큼 있으면 된다
}


//시작할때 체크해야 할것
//배울 수 있는 스킬인가 (배울 수 있다면 deactivation_Image = false)
//스킬레벨이 0인가 (0이면 minus false max면 plus false)
//키를 눌러야 발동하는 액티브 스킬의 경우 player.SkillList에 추가해야하고
//상시 발동중인 패시브 스킬의 경우 player.stat 에 바로 넣어줘야함
//중복체크는 추가할때와 뺄때 해야하는데 
//액티브 스킬의 중복체크는 player.IsActionAlreadyRegistered에서 false가 나오면 추가해도 된다는것
//패시브 스킬의 중복체크는 player.stat.Already 가 붙은 함수를 이용하면 됨
//근데 스킬의 중복체크를 해야하나 싶네 
//스킬의 중복체크는 안하는걸로
[System.Serializable]
public abstract class Skill : MonoBehaviour , IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    protected Player player;
    protected OccupationUI skillInterface;
    [SerializeField]
    protected string s_name;
    public string S_Name { get { return s_name; } }
    //설명
    [TextArea(5, 10)]
    public string description;
    /// <summary>
    /// 스킬 값 (대미지같은거)
    /// </summary>
    public abstract float coefficient { get; }
    /// <summary>
    /// 스킬 값이 어떻게 정해지는지 string으로 리턴
    /// </summary>
    public abstract string detail { get; }
    /// <summary>
    /// 모든 설명을 리턴
    /// </summary>
    public abstract string expaln { get; }

    [SerializeField]
    protected Sprite sprite;
    public Sprite S_Sprite { get { return sprite;} }
    [SerializeField]
    protected int level;
    public int Level { get { return level; } }
    [SerializeField]
    protected int maxLevel;
    //오리지널 쿨타임이 몇초인지
    [SerializeField] 
    protected float cooltime;
    //쿨타임이 몇초 남았는지
    protected float cooltimer;
    //스킬 레벨에 따른 쿨타임이 몇초인지
    public abstract float CoolTime { get; }
    public float CoolTimer { get {  return cooltimer; } }
    [SerializeField]
    protected SKILL_TYPE type;
    public SKILL_TYPE TYPE { get { return type; } }

    [SerializeField]
    protected Text levelText;
    [SerializeField]
    protected Button minusButton;
    [SerializeField]
    protected Button plusButton;
    [SerializeField]
    protected Image deactivation_Image;
    [SerializeField]
    protected KeyCode _keyCode;
    public KeyCode Code { get { return _keyCode; } set { _keyCode = value; } }

    public SkillTrigger trigger;

    //스킬이 유지되는 타이머를 저장하는 코루틴
    protected Coroutine coroutine = null;

    //스킬을 찍으면 스킬을 적용하는 함수
    //install은 skill의 level이 0에서 올라갈때 한번 호출함
    protected abstract void Install();
    //스킬을 취소하면 스킬을 뺀다는 함수
    //install은 skill의 level이 0될때 한번 호출함
    protected abstract void Uninstall();

    protected Action triggerUpdate = null;
    public Action TriggerUpdateAdd { set { triggerUpdate += value; } }
    public Action TriggerUpdateRemove { set { triggerUpdate -= value; } }
    protected Action skillUse = null;
    public Action SkillUseAdd { set { skillUse += value; } }
    public Action SkillUseRemove { set { skillUse -= value; } }

    public virtual void Init(Player player, OccupationUI @interface)
    {
        GetComponent<Image>().sprite = sprite;
        this.player = player;
        skillInterface = @interface;
        Updating();
        minusButton.onClick.RemoveAllListeners();
        minusButton.onClick.AddListener(LevelDown);
        plusButton.onClick.RemoveAllListeners();
        plusButton.onClick.AddListener(LevelUp);
    }

    public void LevelUp()
    {
        if (player.SkillPoint > 0 && level < maxLevel)
        {
            if(level == 0)
            {
                Install();
            }

            player.SkillPoint = -1;
            level++;

            if (triggerUpdate != null)
                triggerUpdate();
        }
    }

    public void LevelDown()
    {
        if(level > 0)
        {
            player.SkillPoint = 1;
            level--;

            if(triggerUpdate != null)
                triggerUpdate();

            if (level == 0)
            {
                Uninstall();
            }
        }
    }

    public virtual void Updating()
    {
        //이 스킬을 찍을 수 있는 상태
        if(trigger.TriggerCheck())
        {
            deactivation_Image.enabled = false;
            levelText.text = level == 0 ? "" : level.ToString();
            if (level <= 0)
            {
                minusButton.gameObject.SetActive(false);
                plusButton.gameObject.SetActive(true);
            }
            else if (level >= maxLevel)
            {
                minusButton.gameObject.SetActive(true);
                plusButton.gameObject.SetActive(false);
            }
            else
            {
                minusButton.gameObject.SetActive(true);
                plusButton.gameObject.SetActive(true);
            }
        }
        //이 스킬을 찍을 수 없는 상태
        else
        {
            deactivation_Image.enabled = true;
            levelText.text = "";
            minusButton.gameObject.SetActive(false);
            plusButton.gameObject.SetActive(false);
            player.SkillPoint = level;
            level = 0;
        }
    }

    //타이머
    protected IEnumerator skillTimer(float f, Action action)
    {
        float timer = f;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        if (action != null)
        {
            action();
        }
        coroutine = null;
    }

    protected IEnumerator CooltimeCoroutine(float t)
    {
        if (skillUse != null)
        {
            skillUse();
        }
            
        cooltimer = t;
        while(cooltimer >= 0)
        {
            cooltimer -= Time.deltaTime;
            yield return null;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        skillInterface.Description(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        skillInterface.EndDescription();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        skillInterface.SkillSelect(this);
    }
}

//이 스킬이 이정도 레벨이여야만 한다
[System.Serializable]
public class SkillTrigger
{
    //조건 스킬들
    public Skill[] triggers;
    //조건의 타입
    public TRIGGER_TYPE type;
    //조건 레벨
    public int needLevel;

    public bool TriggerCheck()
    {
        switch(type)
        {
            case TRIGGER_TYPE.ALL:
                return TriggerAll();
            case TRIGGER_TYPE.ONE:
                return TriggerOne();
            case TRIGGER_TYPE.SUM:
                return TriggerSum();
            default:
                return true;
        }
    }

    public bool TriggerAll()
    {
        for(int i = 0; i < triggers.Length; i++)
        {
            if(triggers[i].Level < needLevel)
                return false;
        }
        return true;
    }

    public bool TriggerOne()
    {
        for (int i = 0; i < triggers.Length; i++)
        {
            if (triggers[i].Level >= needLevel)
                return true;
        }
        return false;
    }

    public bool TriggerSum()
    {
        int l = 0;
        for (int i = 0; i < triggers.Length; i++)
        {
            l += triggers[i].Level;
            if (l >= needLevel)
                return true;
        }
        return false;
    }
}