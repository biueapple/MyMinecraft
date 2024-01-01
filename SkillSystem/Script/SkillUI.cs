using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillUI : MonoBehaviour , IPointerClickHandler
{
    public Skill skill;
    private Image fillImage;
    private Text level;

    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void Interlock(Skill skill)
    {
        this.skill = skill;
        Setting();
        skill.TriggerUpdateAdd = Setting;
        skill.SkillUseAdd = FillAmount;
    }

    public void Setting()
    {
        if (fillImage == null)
            fillImage = transform.GetChild(0).GetComponent<Image>();
        if (level == null)
            level = transform.GetChild(1).GetComponent<Text>();

        if (skill != null)
        {
            GetComponent<Image>().sprite = skill.S_Sprite;
            level.text = skill.Level.ToString();
        }
        else
        {
            GetComponent<Image>().sprite = null;
            level.text = "";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            skill.TriggerUpdateRemove = Setting;
            skill = null;
            Setting();
        }
    }

    private void FillAmount()
    {
        StartCoroutine(ColltimeFillAmount());
    }

    private IEnumerator ColltimeFillAmount()
    {
        while(skill.CoolTimer > 0)
        {
            fillImage.fillAmount = skill.CoolTimer / skill.CoolTime;
            yield return null;
        }
    }
}
