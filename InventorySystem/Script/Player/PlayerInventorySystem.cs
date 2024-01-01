using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class PlayerInventorySystem : MonoBehaviour
{
    private Player player;
    //�κ��丮 ���� ��ġ
    public string inventorySavePath;
    //�κ��丮 �������
    public ContainerObject inventory;
    //�κ��丮�� ǥ���� �������̽�
    public ContainerInterface inventoryInterface;
    //ui�� Ȱ��ȭ �������� �˾Ƴ��� ����
    public GameObject inventoryGameobject;

    //equip
    public string equipmentSavePath;
    //������ ��� �������
    public ContainerObject equipment;
    public ContainerInterface equipmentInterface;
    //ui�� Ȱ��ȭ �������� �˾Ƴ��� ����
    public GameObject equipmentGameobject;

    //crafting
    //public ContainerInterface craftingInterface;
    public GameObject craftingGameobject;

    //���� interface

    //���� �� ���������� �˾Ƴ��� ���� (ui Ȱ��ȭ������ �������)
    public string hotkeySavePath;
    public ContainerObject hotkey;
    public ContainerInterface hotkeyInterface;
    //��� �����ϸ� �� ĳ������ ������ �ö�
    public Unit character;

    //���� ������ ���������� (hoykey��)
    private int select;
    public int Select { get { return select; } }
    //���� ���� ������ ���������� ������ �� �� �ֵ��� ǥ�����ִ� ������Ʈ
    public Transform highlight;

    //�տ� ����ִ� �������� ��
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
            //â�� ������
            if (value == false)
            {
                //�տ� ����ִ� �������� �ִٸ�
                if(hotkeyInterface.corsorUI.Slot.Amount > 0)
                {
                    //��������
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
        //���� interface�� init�ؾ���

        //equipment�� ���â�̶� �������� ���� ���ö����� ������ �߰��ϰų� ������ �ϱ⿡
        if (equipment != null)
        {
            for (int i = 0; i < equipment.storage.slots.Length; i++)
            {
                equipment.storage.slots[i].afterCallback += EquipmentSlotAfter;
                //ó�� �����Ҷ� ��� �԰��ִٸ� ������ �÷�����ϴϱ�
                //EquipmentSlotBefore�� �ֱ� ���� �ϴ� ������ start�ϱ����� �̹� Container�� �������� �ִ� ���¶�
                //�̹� �ִ¸�ŭ ���� �ٽ� �־��ִ� ���̱⿡ EquipmentSlotBefore�� �ֱ� ���� �ϴ°�
                equipment.storage.slots[i].UpdateSlot();
                equipment.storage.slots[i].beforeCallback += EquipmentSlotBefore;
            }
        }
        for (int i = 0; i < inventory.storage.slots.Length; i++)
        {
            inventory.storage.slots[i].UpdateSlot();
        }
        select = 0;
        //�������� �������� �����ִ� �Լ�

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
        //���콺 ��ũ���� �������ٸ� scroll�� ���� 0�� �ƴ�
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

                //�ִ밪���� Ŀ���� 0����
                if (select > hotkeyInterface.slotsInterface.Count - 1)
                    select = 0;
                //�ּҰ����� �۾����� �ִ밪����
                else if (select < 0)
                    select = hotkeyInterface.slotsInterface.Count - 1;

                //�������� ���� ���� (�տ� ����ִ°� ���̵���) �����ֵ���
                ViewModelItem(hotkey.storage.slots[select]);
                //+=
                hotkey.storage.slots[select].afterCallback += ViewModelItem;
                //���� ĭ�� ���������� �����ֱ� ���� ��ġ����
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


    //�÷��̾ �������� ������(select���� �����۽���) �� �÷��̾��� �տ� ������ �ϸ�
    //�÷��̾ ������ �ٲٸ� �÷��̾��� �տ��� ������� �ϰ�
    //�÷��̾��� �տ� �������� �������� ������ٸ� �տ��� ������� �Ѵ�
    //�Լ��� �÷��̾ �������� �����۽��Կ� action�� �ְ�
    //�ٸ��ɷ� �ٲܶ����� -= �� ���� ������ ���Կ� +=�� �̿��ؼ� 
    //���Կ� update�� ���涧���� �÷��̾��� �տ� ���� �׸��� �������� �ؾ�
    //ui���� �÷��̾ �������� �������� ��� �ٸ����� �Ű�����
    //����� �÷��̾� �տ��� �������� ������ �ʰ� �ɰ���
    //�̰��� InventoryHoykey���Ը� �ش��ϴ� ����
    //after�� �־�� ��
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
        //���� �ø���
        if (_slot.GetItem.ID < 0)
            return;

        //���� ��ĭ�� �پ��ִ� ������ �縸ŭ ������
        _slot.GetItem.Equip(character.STAT);
    }

    public void EquipmentSlotBefore(ContainerSlot _slot)
    {
        //���� ������
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
