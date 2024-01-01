using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DescriptionItem : MonoBehaviour
{
    public UIController controller;

    public Image baseInfo;
    public Text m_name;
    public Text m_level;
    public Text m_type;
    public Text m_rating;
    public Text m_Amount;

    public Image damageInfo;
    public Text m_damage;

    public Image attributeInfoPre;
    public List<Image> m_attributeInfo;
    public Text m_attribute;

    public Image descriptionInfo;
    public Text m_description;

    //����
    private int height;

    private Coroutine mouseFallow = null;
    //10����, ���ٴ� 25, ������ - �ڽ��� ũ��
    public void ViewItem(Slot_UI slot)
    {
        //m_attributeInfo.Count == 0�� if���� �ִ� ������ ���콺�� ������ �������� ���� �����ٴϸ�
        //InvalidOperationException: The operation is not possible when moved past all properties (Next returned false)
        //���� ������ gpt���� ����� SerializedProperty�� �ݺ� ���߿� ������ �̵��ϸ鼭 �� �̻��� �Ӽ��� ���� �� �߻��մϴ�.
        //��� �ؼ� list�� ó���� ���� ������ �����ϵ��� �ٲ�
        if (slot.Slot.GetItem.ID >= 0 && m_attributeInfo.Count == 0)
        {
            if (mouseFallow != null)
                StopCoroutine(mouseFallow);
            mouseFallow = StartCoroutine(controller.MouseFallow(gameObject));

            height = 0;
            baseInfo.gameObject.SetActive(true);
            m_name.text = "�̸� : " + slot.Slot.GetItem.Name;
            m_level.text = "���� : " + slot.Slot.GetItem.Level;
            m_type.text = slot.Slot.GetItem.Type.ToString();
            m_rating.text = slot.Slot.GetItem.Rating.ToString();
            m_Amount.text = slot.Slot.Amount.ToString() + " ��";
            height = -30;

            if(slot.Slot.GetItem.GetComponent<WeaponItem>() != null)
            {
                damageInfo.gameObject.SetActive(true );
                m_damage.text = (slot.Slot.GetItem.GetComponent<WeaponItem>().type == DAMAGETYPE.AD ? "���������" : "���������") + " : " + slot.Slot.GetItem.GetComponent<WeaponItem>().damage;
                height -= 30;
            }

            if (slot.Slot.GetItem.GetComponent<EquipItem>() != null)
            {
                for (int i = 0; i < slot.Slot.GetItem.GetComponent<EquipItem>().AttributeValues.Length; i++)
                {
                    m_attributeInfo.Add(ObjectPooling.instance.CreateObject(attributeInfoPre.gameObject, transform).GetComponent<Image>());
                    m_attribute = m_attributeInfo[i].transform.GetChild(0).GetComponent<Text>();
                    m_attribute.text = slot.Slot.GetItem.GetComponent<EquipItem>().AttributeValues[i].Attribute + " : " + slot.Slot.GetItem.GetComponent<EquipItem>().AttributeValues[i].Value;
                    m_attributeInfo[i].transform.position = baseInfo.transform.position;
                    m_attributeInfo[i].transform.position += new Vector3(0, height, 0);
                    height -= 30;
                }
            }
            descriptionInfo.gameObject.SetActive(true);
            m_description.text = slot.Slot.GetItem.Description;
            descriptionInfo.transform.position = baseInfo.transform.position;
            descriptionInfo.transform.position += new Vector3(0, height - 20, 0);
        }
    }
    public void ExitItem()
    {
        if (mouseFallow != null)
            StopCoroutine(mouseFallow);
        baseInfo.gameObject.SetActive(false);
        damageInfo.gameObject.SetActive(false);
        for(int i = 0; i < m_attributeInfo.Count; i++)
        {
            ObjectPooling.instance.DestroyObject(m_attributeInfo[i].gameObject);
        }
        m_attributeInfo.Clear();
        descriptionInfo.gameObject.SetActive(false);
    }

}
