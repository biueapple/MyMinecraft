using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OccupationUI : MonoBehaviour
{
    public UIController controller;
    public Player player;
    public SKILLKIND kind;
    //스킬들
    public Canvas canvas;
    public Image linePre;
    private List<Image> lineList = new List<Image>();
    public Skill[] skills;
    public Text description;
    private Coroutine follow = null;
    //스킬을 tree로 구현하지 못하는 이유는 독립된 스킬이 있을 수 있기 때문에
    //스킬은 전단계의 스킬의 레벨이 필요한 경우가 있다
    //스킬을 찍을때마다 전체 스킬의 조건을 확인해야함


    public void Init()
    {
        for (int i = 0; i < skills.Length; i++)
        {
            skills[i].Init(player, this);
            skills[i].TriggerUpdateAdd = Updating;
        }
    }

    public void Updating()
    {
        for(int i = 0; i < skills.Length; i++)
        {
            skills[i].Updating();
        }
    }

    public void Description(Skill s)
    {
        //스킬의 이름과 설명
        description.text = s.S_Name + '\n';
        description.text += s.expaln;

        //설명이 마우스를 따라다니도록 (나중에 바꿔야 할듯함)
        if (follow == null)
            follow = StartCoroutine(FollowMouse());
        //설명 오브젝트가 보이도록
        description.gameObject.SetActive(true);

        //이 밑은 조건의 대한 스크립트기에 조건이 없는 스킬은 여기서 끝
        if (s.trigger.triggers.Length == 0)
            return;

        //조건에 대한 설명
        description.text += "\n\n";
        description.text += "필요 조건 : "; 
        switch(s.trigger.type)
        {
            case TRIGGER_TYPE.ALL:
                description.text += "아래 스킬들이 전부 " + s.trigger.needLevel + " 이상 필요";
                break;
            case TRIGGER_TYPE.ONE:
                description.text += "아래 스킬들이 하나라도 " + s.trigger.needLevel + " 이상 필요";
                break;
            case TRIGGER_TYPE.SUM:
                description.text += "아래 스킬들의 레벨의 합이 " + s.trigger.needLevel + " 이상 필요";
                break;
        }

        //조건의 이름과 위치를 알려줄 라인 (오브젝트풀링 적용)
        description.text += "\n";
        for (int i = 0; i < s.trigger.triggers.Length; i++)
        {
            //조건의 이름
            description.text += s.trigger.triggers[i].S_Name + '\n';
            //라인 생성과 부모객채
            lineList.Add(ObjectPooling.instance.CreateObject(linePre.gameObject, canvas.transform,
                //라인의 위치
                s.transform.position + (s.trigger.triggers[i].transform.position - s.transform.position) / 2,
                //라인의 각도
                Quaternion.Euler(0, 0, AngleCalculate.Angle_Calculation_Z(s.transform, s.trigger.triggers[i].transform) + 90)).GetComponent<Image>());
            //라인 길이
            lineList[lineList.Count - 1].GetComponent<RectTransform>().sizeDelta = new Vector2(10, Vector2.Distance(s.transform.position, s.trigger.triggers[i].transform.position)) ;
            //하이라키의 인덱스 변경 (라인이 interface의 스프라이트에 가려지는데 반쯤 투명해서 괜찮아서 놔둠 안보이면 interface의 스프라이트가 불투명해서일 수 있음)
            lineList[lineList.Count - 1].transform.SetSiblingIndex(0);
        }
    }

    private IEnumerator FollowMouse()
    {
        while (true)
        {
            description.transform.position = Input.mousePosition;
            yield return null;
        }
    }

    public void EndDescription()
    {
        if(follow != null)
            StopCoroutine(follow);
        follow = null;
        description.gameObject.SetActive(false);
        for(int i = 0; i < lineList.Count; i++)
        {
            ObjectPooling.instance.DestroyObject(lineList[i].gameObject);
        }
        lineList.Clear();
    }


    public SkillUI corsor;
    private Coroutine coroutine = null;
    public void SkillSelect(Skill skill)
    {
        if(skill.Level > 0)
        {
            corsor.skill = skill;
            corsor.gameObject.SetActive(true);
            corsor.Setting();
            coroutine = StartCoroutine(SkillSelecting());
        }
        
    }

    public void SkillSelectEnd(SkillUI ui)
    {
        if(coroutine != null)
        {
            StopCoroutine(coroutine);
            corsor.gameObject.SetActive(false);
        }
    }
    private IEnumerator SkillSelecting()
    {
        while (true)
        {
            if(Input.GetMouseButtonDown(0))
            {
                SkillUI ui = controller.GetGraphicRay<SkillUI>();
                if(ui != null)
                {
                    ui.Interlock(corsor.skill);
                }
                corsor.gameObject.SetActive(false);
                corsor.skill = null;
                corsor.Setting();
                break;
            }
            corsor.transform.position = Input.mousePosition;
            yield return null;
        }
    }
}
