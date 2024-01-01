using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//아이템을 담고있는 ContainerObject와 연동해서 ui로써 보여주는 클래스 이 클래스를 상속받는 StaticInterface,DynamicInterface를 이용해서 사용
//원래는 corsorSlot이라는게 없고 down한 슬롯과 up한 슬롯을 저장하고 swap하는게 끝이였지만
//아이템을 여러개로 나눈다거나 할때 필요해서 corsorSlot을 하나 만들고 downSlot과 moveSlot을 swap upSlot과 moveSlot을 swap하는 방법으로 바꿈
public abstract class ContainerInterface : MonoBehaviour
{
    protected string savePath;
    public ItemDatabaseObject database;
    //ui슬롯 프리팹
    public Slot_UI slot_UI_Prefab;
    //아이템을 담고있는 클래스
    protected ContainerObject containerObject;
    //슬롯과 ui를 엮어줌
    public Dictionary<ContainerSlot, Slot_UI> slotsInterface = new Dictionary<ContainerSlot, Slot_UI>();

    public Slot_UI corsorUI;
    public DescriptionItem descriptionItem;

    private Coroutine corsorCoroutine = null;


    public void Init(string save, ContainerObject container)
    {
        savePath = save;
        containerObject = container;
        //슬롯 UI 만들기
        CreateSlotsUI();
        //콜백 넣기
        for (int i = 0; i < containerObject.storage.slots.Length; i++)
        {
            containerObject.storage.slots[i].afterCallback += UpdateSlotUI;
            UpdateSlotUI(containerObject.storage.slots[i]);
        }
        //corsorUI.Slot같은 경우엔 모든 interface가 같이 쓰는거라서 null인 경우에만 새로 만드는것
        //ui는 인스펙터에서 넣어줘야 하고 ui안에 있는 slot을 새로 만드는 거임
        if (corsorUI.Slot == null)
        {
            corsorUI.Slot = new ContainerSlot();
            corsorUI.Slot.afterCallback += UpdateCorsorSlot;
        }
    }

    //이 클래스를 상속받은 클래스가 슬롯을 어떻게 만들지 만드는 함수임 (StaticInterface는 슬롯을 만들지 않고 인스펙터에서 받음)
    public abstract void CreateSlotsUI();

    //만들어야 하는것은 원하는 종류의 아이템만 보여주는 함수를 만들어야함 (소모품만 보여준다던가 장비만 보여준다던가)


    //CorsorSlot은 다른 슬롯들과는 다르게 자신의 interface가 없기에 따로 콜백을 넣어줘야 함
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

    //모든 ui와 연동된 슬롯이 콜백함수로 받는것 무언가 변화가 있을때 이미지와 text를 변화하는 함수
    //itemObjcet는 원본 아이템의 대한 정보를 갖고
    //container는 생성된 아이템의 정보를 갖는다
    public void UpdateSlotUI(ContainerSlot _slot)
    {
        // itemObject(sprite 소유) -> item(id 소유)
        // containerObject(database소유) -> container -> containerSlot(item소유) 
        if (_slot.Amount == 0 || _slot.GetItem.ID < 0)
        {
            //빈슬롯
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

    


    //슬롯이 아니라 슬롯ui에 input output이 있는 이유는 어차피 ui에 의해서 결정되는 사항이기때문
    //이 3개의 함수는 마우스로 아이템을 잡은 후 mouseDwon하면서 드래그하면 드래그한 칸에 일정하게 아이템을 배분하는 역할을 함
    //처음에 클릭한 칸에 몇개의 아이템이 있었는지
    int count;
    ItemInterface dragItem = null;
    //마우스 드래그한 칸들
    List<Slot_UI> dragSlots = new List<Slot_UI>();
    //마우스를 드래그할때 마우스가 벗어난 슬롯이 dragSlot에 들어가야 함

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
                //드래그한 오브젝트가 있으면서 && 드래그한게 slot이면서 && 그 칸이 비어있으면서 && 드래그 아이템이 들어갈 수 있으면서 && 이미 드래그한 칸이 아니라면 && 그리고 그 칸이 아이템을 넣을 수 있다면 && 선택한 칸이 가지고 있는 아이템보다 적다면
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
            // 1) 아이템을 잡은 상태로 아이템이 있는곳에 좌클릭은 서로 같은 아이템이면 합치기 같지 않다면 서로 교환
            // 2) 아이템을 잡은 상태로 아이템이 없는곳에 좌클릭은 아이템 놓기
            // 3) 아이템을 잡지 않은 상태로 아이템이 있는곳에 좌클릭은 아이템 전부 들기
            // 4) 아이템을 잡지 않은 상태로 아이템이 없는곳에 좌클릭은 아무일도 없음

            //이미 아이템을 잡은 상태임
            if (corsorUI.Slot.Amount > 0)
            {
                //클릭한 곳의 슬롯도 아이템이 있는곳임
                // 1)
                if (slot.Slot.Amount > 0)
                {
                    //같은 아이템이 있는곳에 아이템 놓기
                    if (corsorUI.Slot.GetItem.ID == slot.Slot.GetItem.ID && slot.input && slot.Slot.GetItem.Stackable)
                    {
                        slot.Slot.AddAmount(corsorUI.Slot.Amount);
                        corsorUI.Slot.AddAmount(-corsorUI.Slot.Amount);
                        StopMouse();
                    }
                    //아이템을 넣고 빼는게 가능함
                    else if (slot.input && slot.output && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                    }
                }
                //아이템이 없는곳임
                // 2)
                else
                {
                    //아이템을 넣는게 가능함
                    if (slot.input && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        //드래그를 할 수 있으니까
                        dragItem = corsorUI.Slot.GetItem;
                        count = corsorUI.Slot.Amount;
                        dragSlots.Add(slot);

                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                        StopMouse();
                    }
                }
            }
            //아이템을 잡지 않은 상태임
            else
            {
                //아이템이 있는 칸임
                // 3) 
                if (slot.Slot.Amount > 0)
                {
                    //클릭한 아이템 슬롯이 아이템을 뺄 수 있는 칸임
                    if (slot.output)
                    {
                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                        descriptionItem.ExitItem();
                        FollowMouse();
                    }
                }
                // 4) 는 없음
            }
        }



        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            // 1)아이템을 잡은 상태로 비어있지 않은 칸에 우클릭은 같은 아이템이라면 하나만 놓기 같지 않다면 좌클릭이랑 다르지 않게 교환
            // 2)아이템을 잡은 상태로 비어있는 칸에 우클릭은 아이템 하나만 놓기
            // 3)아이템을 잡지 않은 상태로 비어있지 않은 칸에 우클릭은 아이템 절반만 들기
            // 4)아이템을 잡지 않은 상태로 비어있는 칸에 우클릭은 아무일도 없음

            //이미 아이템을 잡은 상태임
            if (corsorUI.Slot.Amount > 0)
            {
                //클릭한 곳의 슬롯도 아이템이 있는곳임
                // 1)
                if (slot.Slot.Amount > 0)
                {
                    //같은 아이템이 있는곳에 아이템 놓기
                    if (corsorUI.Slot.GetItem.ID == slot.Slot.GetItem.ID && slot.input)
                    {
                        //아이템 하나만 놓기
                        slot.Slot.AddAmount(1);
                        corsorUI.Slot.AddAmount(-1);
                        //들고있는 아이템이 없다면 (다 썼다면 안보이고 마우스를 따라다니지 않게)
                        if (corsorUI.Slot.Amount <= 0)
                        {
                            StopMouse();
                        }
                    }
                    //같은 아이템은 아니지만 넣고 빼는게 가능한 경우
                    else if (slot.input && slot.output && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        //아이템 교환
                        containerObject.SwapContainerSlot(corsorUI.Slot, slot.Slot);
                    }
                }
                //아이템이 없는곳임
                // 2)
                else
                {
                    //아이템을 하나만 놓기
                    if (slot.input && slot.Slot.CanPlace(corsorUI.Slot.GetItem))
                    {
                        //아이템 하나만 놓기
                        slot.Slot.UpdateSlot(corsorUI.Slot.GetItem, 1);
                        corsorUI.Slot.AddAmount(-1);
                        //들고있는 아이템이 없다면 (다 썼다면 안보이고 마우스를 따라다니지 않게)
                        if (corsorUI.Slot.Amount <= 0)
                        {
                            StopMouse();
                        }
                    }
                }
            }
            //아이템을 잡지 않은 상태임
            else
            {
                //아이템이 있는 칸임
                // 3) 
                if (slot.Slot.Amount > 0)
                {
                    //클릭한 아이템 슬롯이 아이템을 뺄 수 있는 칸임
                    if (slot.output)
                    {
                        //아이템을 절반만 들기
                        int a = Mathf.CeilToInt(slot.Slot.Amount * 0.5f);
                        corsorUI.Slot.UpdateSlot(slot.Slot.GetItem, a);
                        slot.Slot.AddAmount(-a);
                        FollowMouse();
                    }
                }
                // 4) 는 없음
            }
        }
    }

    public void MouseUp(Slot_UI slot, PointerEventData eventData)
    {
        dragSlots.Clear();
        dragItem = null;
        count = 0;
    }

    //슬롯이 마우스를 따라가도록
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

    //corsorSlot에 아이템이 있다면 마우스를 따라 움직여야 하니까 while문에 조건을 줄수도 있지만 일단은 stopCoroutine을 쓰는걸로
    private IEnumerator CorsorMoving()
    {
        while(true)
        {
            corsorUI.transform.position = Input.mousePosition;
            yield return null;
        }
    }
}
