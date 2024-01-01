using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerInventorySystem : MonoBehaviour
{
    private Player player;
    //인벤토리 저장 위치
    public string inventorySavePath;
    //인벤토리 저장공간
    public ContainerObject inventory;
    //인벤토리를 표시할 인터페이스
    public ContainerInterface inventoryInterface;
    //ui가 활성화 상태인지 알아내기 위해
    public GameObject inventoryGameobject;

    //equip
    public string equipmentSavePath;
    //장착한 장비 저장공간
    public ContainerObject equipment;
    public ContainerInterface equipmentInterface;
    //ui가 활성화 상태인지 알아내기 위해
    public GameObject equipmentGameobject;

    //crafting
    //public ContainerInterface craftingInterface;
    public GameObject craftingGameobject;

    //예비 interface

    //지금 뭘 선택중인지 알아내기 위해 (ui 활성화인지는 상관없음)
    public string hotkeySavePath;
    public ContainerObject hotkey;
    public ContainerInterface hotkeyInterface;
    //장비를 장착하면 이 캐릭터의 스탯이 올라감
    public Unit character;

    //무슨 슬롯을 선택중인지 (hoykey의)
    private int select;
    public int Select { get { return select; } }
    //내가 무슨 슬롯을 선택중인지 유저가 알 수 있도록 표시해주는 오브젝트
    public Transform highlight;

    //손에 들고있는 아이템의 모델
    private Transform model;

    public bool Active 
    { 
        get
        {
            if(inventoryGameobject.activeSelf || equipmentGameobject.activeSelf || craftingGameobject.activeSelf)
                return true;
            return false;
        }
        set
        {
            inventoryGameobject.SetActive(value);
            equipmentGameobject.SetActive(value);
            craftingGameobject.SetActive(value);
            //창을 닫을때
            if (value == false)
            {
                //손에 들고있는 아이템이 있다면
                if(hotkeyInterface.corsorUI.Slot.Amount > 0)
                {
                    //던지도록
                    player.dropItemManager.Thorw(hotkeyInterface.corsorUI.Slot.GetItem.ID, transform.position + new Vector3(0,1.8f,0) + transform.forward, hotkeyInterface.corsorUI.Slot.Amount);
                    hotkeyInterface.corsorUI.Slot.AddAmount(-hotkeyInterface.corsorUI.Slot.Amount);
                    hotkeyInterface.StopMouse();
                }
                hotkeyInterface.descriptionItem.ExitItem();
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init()
    {
        player = GetComponent<Player>();
        inventory = new ContainerObject(9, inventorySavePath);
        equipment.SavePath = equipmentSavePath;
        hotkey = new ContainerObject(9, hotkeySavePath);
        inventoryInterface.Init(inventorySavePath, inventory);
        equipmentInterface.Init(equipmentSavePath, equipment);
        hotkeyInterface.Init(hotkeySavePath, hotkey);
        //예비 interface도 init해야함

        //equipment는 장비창이라 아이템이 들어가고 나올때마다 스탯을 추가하거나 내려야 하기에
        if (equipment != null)
        {
            for (int i = 0; i < equipment.storage.slots.Length; i++)
            {
                equipment.storage.slots[i].afterCallback += EquipmentSlotAfter;
                //처음 시작할때 장비를 입고있다면 스탯을 올려줘야하니까
                //EquipmentSlotBefore를 넣기 전에 하는 이유는 start하기전에 이미 Container에 아이템이 있는 상태라
                //이미 있는만큼 빼고 다시 넣어주는 꼴이기에 EquipmentSlotBefore를 넣기 전에 하는것
                equipment.storage.slots[i].UpdateSlot();
                equipment.storage.slots[i].beforeCallback += EquipmentSlotBefore;
            }
        }
        for (int i = 0; i < inventory.storage.slots.Length; i++)
        {
            inventory.storage.slots[i].UpdateSlot();
        }
        select = 0;
        //아이템의 실제모델을 보여주는 함수

        hotkey.storage.slots[select].afterCallback += ViewModelItem;

        if (inventory != null)
            inventory.Load();
        if (equipment != null)
            equipment.Load();
        //if (hotkey != null)
        //    hotkey.Load();
        ViewModelItem(hotkey.storage.slots[select]);
    }

    // Update is called once per frame
    void Update()
    {
        //마우스 스크롤을 움직였다면 scroll의 값이 0이 아님
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
        {
            if(!player.battleMode)
            {
                // -=
                hotkey.storage.slots[select].afterCallback -= ViewModelItem;
                if (scroll > 0)
                    select--;
                else
                    select++;

                //최대값보다 커지면 0으로
                if (select > hotkeyInterface.slotsInterface.Count - 1)
                    select = 0;
                //최소값보다 작아지면 최대값으로
                else if (select < 0)
                    select = hotkeyInterface.slotsInterface.Count - 1;

                //아이템의 실제 모델을 (손에 들고있는걸 보이도록) 보여주도록
                ViewModelItem(hotkey.storage.slots[select]);
                //+=
                hotkey.storage.slots[select].afterCallback += ViewModelItem;
                //무슨 칸을 선택중인지 보여주기 위해 위치조정
                highlight.position = hotkeyInterface.slotsInterface[hotkey.storage.slots[select]].transform.position;
            }
        }
    }

    public void BattileModeOn()
    {
        hotkey.storage.slots[select].afterCallback -= ViewModelItem;
        equipment.storage.slots[4].afterCallback += ViewModelItem;
        ViewModelItem(equipment.storage.slots[4]);

        hotkeyInterface.gameObject.SetActive(false);
    }
    public void BattileModeOff()
    {
        hotkey.storage.slots[select].afterCallback += ViewModelItem;
        equipment.storage.slots[4].afterCallback -= ViewModelItem;
        ViewModelItem(hotkey.storage.slots[select]);

        hotkeyInterface.gameObject.SetActive(true);
    }


    //플레이어가 선택중인 아이템(select중인 아이템슬롯) 은 플레이어의 손에 보여야 하며
    //플레이어가 선택을 바꾸면 플레이어의 손에서 사라져야 하고
    //플레이어의 손에 선택중인 아이템이 사라진다면 손에서 사라져야 한다
    //함수를 플레이어가 선택중인 아이템슬롯에 action을 넣고
    //다른걸로 바꿀때마다 -= 과 새로 선택한 슬롯에 +=를 이용해서 
    //슬롯에 update가 생길때마다 플레이어의 손에 모델을 그리는 형식으로 해야
    //ui에서 플레이어가 선택중인 아이템을 들고 다른곳에 옮겼을때
    //제대로 플레이어 손에서 아이템이 보이지 않게 될것임
    //이것은 InventoryHoykey에게만 해당하는 사항
    //after에 넣어야 함
    public void ViewModelItem(ContainerSlot _slot)
    {
        Transform hand = player.hand;
        if (model != null)
        {
            ObjectPooling.instance.DestroyObject(model.gameObject);
        }
            

        if(_slot.GetItem.ID >= 0 && ObjectPooling.instance.data.Data[_slot.GetItem.ID].model != null)
        {
            model = ObjectPooling.instance.CreateObject(ObjectPooling.instance.data.Data[_slot.GetItem.ID].model.gameObject, hand).transform;
        }
    }

    public void EquipmentSlotAfter(ContainerSlot _slot)
    {
        //스텟 올리기
        if (_slot.GetItem.ID < 0)
            return;

        //슬롯 한칸에 붙어있는 스탯의 양만큼 더해줌
        _slot.GetItem.Equip(character.STAT);
    }

    public void EquipmentSlotBefore(ContainerSlot _slot)
    {
        //스텟 내리기
        if (_slot.GetItem.ID < 0)
            return;
        _slot.GetItem.Release(character.STAT);
    }


    private void OnTriggerEnter(Collider other)
    {

    }

    private void OnCollisionEnter(Collision collision)
    {

    }

    private void OnApplicationQuit()
    {
        if (inventory != null)
            inventory.Save();
        if (equipment != null)
            equipment.Save();
        //if (hotkey != null)
        //    hotkey.Save();
    }
}
