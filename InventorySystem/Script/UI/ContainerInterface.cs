using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//�������� ����ִ� ContainerObject�� �����ؼ� ui�ν� �����ִ� Ŭ���� �� Ŭ������ ��ӹ޴� StaticInterface,DynamicInterface�� �̿��ؼ� ���
//������ corsorSlot�̶�°� ���� down�� ���԰� up�� ������ �����ϰ� swap�ϴ°� ���̿�����
//�������� �������� �����ٰų� �Ҷ� �ʿ��ؼ� corsorSlot�� �ϳ� ����� downSlot�� moveSlot�� swap upSlot�� moveSlot�� swap�ϴ� ������� �ٲ�
public abstract class ContainerInterface : MonoBehaviour
{
    protected string savePath;
    public ItemDatabaseObject database;
    //ui���� ������
    public Slot_UI slot_UI_Prefab;
    //�������� ����ִ� Ŭ����
    protected ContainerObject containerObject;
    //���԰� ui�� ������
    public Dictionary<ContainerSlot, Slot_UI> slotsInterface = new Dictionary<ContainerSlot, Slot_UI>();

    public Slot_UI corsorUI;
    public DescriptionItem descriptionItem;

    private Coroutine corsorCoroutine = null;


    public void Init(string save, ContainerObject container)
    {
        savePath = save;
        containerObject = container;
        //���� UI �����
        CreateSlotsUI();
        //�ݹ� �ֱ�
        for (int i = 0; i < containerObject.storage.slots.Length; i++)
        {
            containerObject.storage.slots[i].afterCallback += UpdateSlotUI;
            UpdateSlotUI(containerObject.storage.slots[i]);
        }
        //corsorUI.Slot���� ��쿣 ��� interface�� ���� ���°Ŷ� null�� ��쿡�� ���� ����°�
        //ui�� �ν����Ϳ��� �־���� �ϰ� ui�ȿ� �ִ� slot�� ���� ����� ����
        if (corsorUI.Slot == null)
        {
            corsorUI.Slot = new ContainerSlot();
            corsorUI.Slot.afterCallback += UpdateCorsorSlot;
        }
    }

    //�� Ŭ������ ��ӹ��� Ŭ������ ������ ��� ������ ����� �Լ��� (StaticInterface�� ������ ������ �ʰ� �ν����Ϳ��� ����)
    public abstract void CreateSlotsUI();

    //������ �ϴ°��� ���ϴ� ������ �����۸� �����ִ� �Լ��� �������� (�Ҹ�ǰ�� �����شٴ��� ��� �����شٴ���)


    //CorsorSlot�� �ٸ� ���Ե���� �ٸ��� �ڽ��� interface�� ���⿡ ���� �ݹ��� �־���� ��
    public void UpdateCorsorSlot(ContainerSlot _slot)
    {
        if (_slot.Amount == 0 || _slot.GetItem.ID < 0)
        {
            corsorUI.Setting(null, "", new Color(1, 1, 1, 0));
        }
        else
        {
            corsorUI.Setting(database.GetItemObjectWithId(_slot.GetItem.ID).sprite, _slot.Amount == 1 ? "" : _slot.Amount.ToString(), Color.white);
        }
    }

    //��� ui�� ������ ������ �ݹ��Լ��� �޴°� ���� ��ȭ�� ������ �̹����� text�� ��ȭ�ϴ� �Լ�
    //itemObjcet�� ���� �������� ���� ������ ����
    //container�� ������ �������� ������ ���´�
    public void UpdateSlotUI(ContainerSlot _slot)
    {
        // itemObject(sprite ����) -> item(id ����)
        // containerObject(database����) -> container -> containerSlot(item����) 
        if (_slot.Amount == 0 || _slot.GetItem.ID < 0)
        {
            //�󽽷�
            if (slotsInterface[_slot] != null)
                slotsInterface[_slot].Setting(null, "", new Color(1,1,1,0));
        }
        else
        {
            if (slotsInterface[_slot] != null)
            {
                slotsInterface[_slot].Setting(database.GetItemObjectWithId(_slot.GetItem.ID).sprite, _slot.Amount == 1 ? "" : _slot.Amount.ToString(), Color.white);
            }
        }
    }

    


    //������ �ƴ϶� ����ui�� input output�� �ִ� ������ ������ ui�� ���ؼ� �����Ǵ� �����̱⶧��
    //�� 3���� �Լ��� ���콺�� �������� ���� �� mouseDwon�ϸ鼭 �巡���ϸ� �巡���� ĭ�� �����ϰ� �������� ����ϴ� ������ ��
    //ó���� Ŭ���� ĭ�� ��� �������� �־�����
    int count;
    ItemInterface dragItem = null;
    //���콺 �巡���� ĭ��
    List<Slot_UI> dragSlots = new List<Slot_UI>();
    //���콺�� �巡���Ҷ� ���콺�� ��� ������ dragSlot�� ���� ��

    public void MouseEnter(Slot_UI slot, PointerEventData eventData)
    {
        if(dragItem == null)
        {
            descriptionItem.ViewItem(slot);
        }
    }
    public void MouseExit(Slot_UI slot, PointerEventData eventData)
    {
        if (dragItem == null)
        {
            descriptionItem.ExitItem();
        }
    }

    public void MouseDrag(Slot_UI slot, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (dragSlots.Count > 0)
            {
                //�巡���� ������Ʈ�� �����鼭 && �巡���Ѱ� slot�̸鼭 && �� ĭ�� ��������鼭 && �巡�� �������� �� �� �����鼭 && �̹� �巡���� ĭ�� �ƴ϶�� && �׸��� �� ĭ�� �������� ���� �� �ִٸ� && ������ ĭ�� ������ �ִ� �����ۺ��� ���ٸ�
                if (eventData.pointerEnter != null && eventData.pointerEnter.transform.GetComponent<Slot_UI>() != null &&
                    eventData.pointerEnter.transform.GetComponent<Slot_UI>().Slot.Amount == 0 && eventData.pointerEnter.transform.GetComponent<Slot_UI>().Slot.CanPlace(dragItem) &&
                    !dragSlots.Contains(eventData.pointerEnter.transform.GetComponent<Slot_UI>()) &&
                    eventData.pointerEnter.transform.GetComponent<Slot_UI>().input && dragSlots.Count < count)
                {
                    dragSlots.Add(eventData.pointerEnter.transform.GetComponent<Slot_UI>());
                    for (int i = 0; i < dragSlots.Count; i++)
                    {
                        dragSlots[i].Slot.UpdateSlot(dragItem, count / dragSlots.Count);
                    }
                    for (int i = 0; i < count % dragSlots.Count; i++)
                    {
                        dragSlots[i].Slot.AddAmount(1);
                    }
                }
            }
        }
    }

    public void MouseDown(Slot_UI slot, PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            // 1) �������� ���� ���·� �������� �ִ°��� ��Ŭ���� ���� ���� �������̸� ��ġ�� ���� �ʴٸ� ���� ��ȯ
            // 2) �������� ���� ���·� �������� ���°��� ��Ŭ���� ������ ����
            // 3) �������� ���� ���� ���·� �������� �ִ°��� ��Ŭ���� ������ ���� ���
            // 4) �������� ���� ���� ���·� �������� ���°��� ��Ŭ���� �ƹ��ϵ� ����

            //�̹� �������� ���� ������
            if (corsorUI.Slot.Amount > 0)
            {
                //Ŭ���� ���� ���Ե� �������� �ִ°���
                // 1)
                if (slot.Slot.Amount > 0)
                {
                    //���� �������� �ִ°��� ������ ����
                    if (corsorUI.Slot.GetItem.ID == slot.Slot.GetItem.ID && slot.input && slot.Slot.GetItem.Stackable)
                    {
                        slot.Slot.AddAmount(corsorUI.Slot.Amount);
                        corsorUI.Slot.AddAmount(-corsorUI.Slot.Amount);
                        StopMouse();
                    }
                    //�������� �ְ� ���°� ������
                    else if (slot.input && slot.output && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                    }
                }
                //�������� ���°���
                // 2)
                else
                {
                    //�������� �ִ°� ������
                    if (slot.input && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        //�巡�׸� �� �� �����ϱ�
                        dragItem = corsorUI.Slot.GetItem;
                        count = corsorUI.Slot.Amount;
                        dragSlots.Add(slot);

                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                        StopMouse();
                    }
                }
            }
            //�������� ���� ���� ������
            else
            {
                //�������� �ִ� ĭ��
                // 3) 
                if (slot.Slot.Amount > 0)
                {
                    //Ŭ���� ������ ������ �������� �� �� �ִ� ĭ��
                    if (slot.output)
                    {
                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                        descriptionItem.ExitItem();
                        FollowMouse();
                    }
                }
                // 4) �� ����
            }
        }



        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 1)�������� ���� ���·� ������� ���� ĭ�� ��Ŭ���� ���� �������̶�� �ϳ��� ���� ���� �ʴٸ� ��Ŭ���̶� �ٸ��� �ʰ� ��ȯ
            // 2)�������� ���� ���·� ����ִ� ĭ�� ��Ŭ���� ������ �ϳ��� ����
            // 3)�������� ���� ���� ���·� ������� ���� ĭ�� ��Ŭ���� ������ ���ݸ� ���
            // 4)�������� ���� ���� ���·� ����ִ� ĭ�� ��Ŭ���� �ƹ��ϵ� ����

            //�̹� �������� ���� ������
            if (corsorUI.Slot.Amount > 0)
            {
                //Ŭ���� ���� ���Ե� �������� �ִ°���
                // 1)
                if (slot.Slot.Amount > 0)
                {
                    //���� �������� �ִ°��� ������ ����
                    if (corsorUI.Slot.GetItem.ID == slot.Slot.GetItem.ID && slot.input)
                    {
                        //������ �ϳ��� ����
                        slot.Slot.AddAmount(1);
                        corsorUI.Slot.AddAmount(-1);
                        //����ִ� �������� ���ٸ� (�� ��ٸ� �Ⱥ��̰� ���콺�� ����ٴ��� �ʰ�)
                        if (corsorUI.Slot.Amount <= 0)
                        {
                            StopMouse();
                        }
                    }
                    //���� �������� �ƴ����� �ְ� ���°� ������ ���
                    else if (slot.input && slot.output && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        //������ ��ȯ
                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                    }
                }
                //�������� ���°���
                // 2)
                else
                {
                    //�������� �ϳ��� ����
                    if (slot.input && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        //������ �ϳ��� ����
                        slot.Slot.UpdateSlot(corsorUI.Slot.GetItem, 1);
                        corsorUI.Slot.AddAmount(-1);
                        //����ִ� �������� ���ٸ� (�� ��ٸ� �Ⱥ��̰� ���콺�� ����ٴ��� �ʰ�)
                        if (corsorUI.Slot.Amount <= 0)
                        {
                            StopMouse();
                        }
                    }
                }
            }
            //�������� ���� ���� ������
            else
            {
                //�������� �ִ� ĭ��
                // 3) 
                if (slot.Slot.Amount > 0)
                {
                    //Ŭ���� ������ ������ �������� �� �� �ִ� ĭ��
                    if (slot.output)
                    {
                        //�������� ���ݸ� ���
                        int a = Mathf.CeilToInt(slot.Slot.Amount * 0.5f);
                        corsorUI.Slot.UpdateSlot(slot.Slot.GetItem, a);
                        slot.Slot.AddAmount(-a);
                        FollowMouse();
                    }
                }
                // 4) �� ����
            }
        }
    }

    public void MouseUp(Slot_UI slot, PointerEventData eventData)
    {
        dragSlots.Clear();
        dragItem = null;
        count = 0;
    }

    //������ ���콺�� ���󰡵���
    public void FollowMouse()
    {
        corsorUI.gameObject.SetActive(true);
        if (corsorCoroutine == null)
        {
            corsorCoroutine = StartCoroutine(CorsorMoving());
        }
        else
        {
            StopCoroutine(corsorCoroutine);
            corsorCoroutine = StartCoroutine(CorsorMoving());
        }
    }

    public void StopMouse()
    {
        corsorUI.gameObject.SetActive(false);
        if (corsorCoroutine != null)
        {
            StopCoroutine(corsorCoroutine);
            corsorCoroutine = null;
        }
    }

    //corsorSlot�� �������� �ִٸ� ���콺�� ���� �������� �ϴϱ� while���� ������ �ټ��� ������ �ϴ��� stopCoroutine�� ���°ɷ�
    private IEnumerator CorsorMoving()
    {
        while(true)
        {
            corsorUI.transform.position = Input.mousePosition;
            yield return null;
        }
    }
}
