using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    //sound
    public SoundManager soundManager;
    protected AudioSource eatingAudio;

    //����
    public Transform cam;
    public World world;
    //����� �ı������� �������� ���� �Ŵ���
    public DropItemManager dropItemManager;

    //�÷��̾�
    public Transform highlightBlock;
    public Transform placeBlock;
    public Transform blokenBlock;
    //��� �������� ����� Ȯ���ϴ���
    public float checkIncrement = 0.1f;
    //�ִ�Ÿ�
    public float reach = 8;

    //�κ��丮
    private PlayerInventorySystem inventory;
    public PlayerInventorySystem InventorySystem { get { return inventory; } }
    //�����
    private HungerSystem hungerSystem;

    //��ġ
    //�Ӹ���ġ
    public Transform head;
    //����ġ
    public Transform hand;
    //������ ������
    public Transform RBI;

    public bool battleMode = false;

    //��ų
    [SerializeField]
    private int skillPoint;
    public int SkillPoint { get { return skillPoint; } set { skillPoint += value; } }

    public Action SkillList = null;
    public SkillKind skillUI;
    public GameObject skillHoykey;

    //

    BlockLaycast ray = null;
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        inventory = GetComponent<PlayerInventorySystem>();
        inventory.Init();
        hungerSystem = GetComponent<HungerSystem>();
        eatingAudio = head.GetComponent<AudioSource>();
        moveSystem.move_Mode += moveSystem.InputMove;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();
        if(!inventory.Active && !skillUI.Active)
        {
            if(battleMode)
            {
                EquipLeftClickDown();
                EquipRightClickDown();
                if (SkillList != null)
                    SkillList();
            }
            else
            {
                //raycast�� �����
                ray = world.WorldRaycast(cam.position, cam.forward, reach, checkIncrement);
                //���� ���� ����� �����ִ��� ǥ��
                PlaceCursorBlocks();
                //����� ĳ�� �Լ�
                MouseLeftClick();

                //����ִ� �����ۿ� ���� ȿ���� �ٸ�
                MouseLeftClickDown(); //(���ݿ� ���� ���뵵 �� ����)
                MouseLeftClickUp();
                MouseRightClick();
                MouseRightClickUp();
            }
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            inventory.Active = !inventory.Active;
        }

        if(Input.GetKeyDown(KeyCode.B))
        {
            battleMode = !battleMode;
            if(battleMode)
            {
                inventory.BattileModeOn();
                highlightBlock.gameObject.SetActive(false);
                placeBlock.gameObject.SetActive(false);
                blokenBlock.gameObject.SetActive(false);

                skillHoykey.gameObject.SetActive(true);
            }
            else
            {
                inventory.BattileModeOff();

                skillHoykey.gameObject.SetActive(false);
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            //���� ���� �ִ� �������
            skillUI.Active = true;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }


    //�÷��̾ ���� ����� �ٶ󺸰� �ִ��� ��� ����� ��ġ�ϰ� �Ǵ��� �����ִ� �Լ�
    private void PlaceCursorBlocks()
    {
        if(ray != null)
        {
            //�ִٸ� �� ����� ��ġ�� �ı��� ����� ��ġ��
            highlightBlock.position = ray.positionToInt;
            blokenBlock.position = ray.positionToInt;
            //�� ���� ���������� Ȯ���ߴ� ����� ��ġ�� ����� ��ġ�� ��ġ
            placeBlock.position = ray.lastPositionToInt;

            highlightBlock.gameObject.SetActive(true);
            blokenBlock.gameObject.SetActive(true);
            placeBlock.gameObject.SetActive(true);
        }
        else
        {
            highlightBlock.gameObject.SetActive(false);
            blokenBlock.gameObject.SetActive(false);
            placeBlock.gameObject.SetActive(false);
        }
    }

    //�ѹ� ������ ����� �ı��ϴ°��� �ƴ϶� ������ �ִٸ� ���� �ð� �Ŀ� �ı��Ǵ°ɷ�
    //����ִ� �������� ���� Ÿ�԰� �ٶ󺸴� ����� ����Ÿ���� ���ٸ� ���� �ƴϸ� ���� ���� ����
    // 1�� �ı�
    private  float breaking = 0;
    //�� ���� �󸶳� �ܴ��Ѱ�
    private float hard = 1;
    //������ ��ġ�� ��𿴴°� (���� ���� �μ��� ����� �޶����ٸ� {�����ִ� ����� �޶����ٸ�} ó������ �ٽ� �μž� �ϴϱ�)
    private Vector3Int last;
    //������ ����
    private Vector3 range = new Vector3(1, 1, 2);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(RBI.position + cam.forward * range.z * 0.5f , range);
    }

    private void MouseLeftClickDown()
    {
        if(Input.GetMouseButtonDown(0))
        {
            //���� �տ� ��� ������
            if (inventory.hotkey.storage.slots[inventory.Select].GetItem.ID >= 0)
            {
                inventory.hotkey.storage.slots[inventory.Select].GetItem.LeftMouseDown(this);
            }
            else if (stat.Attacktimer >= stat.AttackSpeed)
            {
                List<Collider> colliders = new List<Collider> ();
                colliders.AddRange(Physics.OverlapBox(RBI.position + cam.forward * range.z * 0.5f, range * 0.5f, Quaternion.Euler(transform.eulerAngles), LayerMask.GetMask("Unit")));
                colliders.Remove(GetComponent<Collider>());
                for(int i = 0; i < colliders.Count; i++)
                {
                    if (colliders[i].GetComponent<Unit>()!=null)
                    {
                        colliders[i].GetComponent<Unit>().Hit(stat, stat.AD, ATTACKTYPE.NOMAL, DAMAGETYPE.AD);
                        stat.Attacktimer = 0;
                        return;
                    }
                }
            }
        }
    }

    //����� ĳ�� �Լ�
    private void MouseLeftClick()
    {
        if (Input.GetMouseButton(0))
        {
            //���� ���� ���� ����� �����ִٴ� ��
            if (ray != null)
            {
                //�� ��ġ�� ������ ��ġ�� �ٸ��ٸ� ���� ���� �ٸ� ����� �����ִٴ� ��
                if (last != ray.positionToInt)
                    breaking = 0;
                //�����Ͱ� ������� �ʴٸ� (�����ϴ� ����̶�� {highlightBlock.gameObject.activeSelf�� ���ֵ� �������� ��})
                if (ray.block != null)
                {
                    //���� �տ� ����ִ� �������� �����Ѹ鼭 Ÿ�Ե� ���ٸ� (���ε� ��̸� ������ ������ ������)
                    if (inventory.hotkey.storage.slots[inventory.Select].GetItem.ID > -1 && ray.block.hardness.type == inventory.hotkey.storage.slots[inventory.Select].GetItem.Hardness.type)
                    {
                        //����ִ� �������� �ܴ����� �� ���� �ܴ�����
                        hard = inventory.hotkey.storage.slots[inventory.Select].GetItem.Hardness.hard;
                    }
                    else
                    {
                        //�׷��� �ʴٸ� �⺻���� �ܴ���
                        hard = 1;
                    }

                    breaking += hard / (float)ray.block.hardness.hard * Time.deltaTime;
                    //* 0.25 �� / 4 ��� �Ѱ� �̹����� 4���� ������ �־ 
                    blokenBlock.GetComponent<Renderer>().material.SetFloat("_Id", (int)(breaking * 4));

                    //����� �μ���
                    if (breaking >= 1)
                    {
                        //������ ����ϰ�
                        dropItemManager.DropItem(ray.block.itemID, ray.positionToInt);
                        //�μ��� ���� air�� �ٲ��ְ�
                        world.InstallBlock(new BlockOrder(ray.positionToInt, _BLOCK.AIR));
                        //�μ������� �ٽ� 0����
                        breaking = 0;
                    }
                }
                last = ray.positionToInt;
            }
        }
    }

    //���콺�� ���� ĳ���ִ� ����� ���̻� ĳ�� �ʵ��� �ϴ°���
    private void MouseLeftClickUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //�󸶳� ĺ�°��� 0���� �ϰ�
            breaking = 0;
            //�μ����� ǥ�����ִ� ������ -1�� ����
            blokenBlock.GetComponent<Renderer>().material.SetFloat("_Id", -1);
        }
    }

    //��� ��ġ�� ���ĸԱ�
    public Coroutine eatingCoroutine = null;
    private void MouseRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //���� �տ� ��� ������
            if (inventory.hotkey.storage.slots[inventory.Select].GetItem.ID >= 0)
            {
                inventory.hotkey.storage.slots[inventory.Select].GetItem.RightMouseDown(this);
            }
        }
    }

    private void MouseRightClickUp()
    {
        if (Input.GetMouseButtonUp(1))
        {
            //���� �տ� ��� ������
            if (inventory.hotkey.storage.slots[inventory.Select].GetItem.ID >= 0)
            {
                inventory.hotkey.storage.slots[inventory.Select].GetItem.RightMouseUp(this);
            }
        }
    }

    private void EquipLeftClickDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //���� �տ� ��� ������
            if (inventory.equipment.storage.slots[4].GetItem.ID >= 0)
            {
                inventory.equipment.storage.slots[4].GetItem.LeftMouseDown(this);
            }
        }
    }
    private void EquipRightClickDown()
    {

    }

    //���ĸԴ� �ڷ�ƾ
    protected IEnumerator Eating(float timer, float hunger, AudioClip sound)
    {
        //�Ҹ� �ѹ� ���ְ�
        eatingAudio.PlayOneShot(sound);
        //�ð� ��ٸ���
        yield return new WaitForSeconds(timer);
        //����� ä���
        hungerSystem.Hunger = hunger;
        //������ ���� �ϳ� ���
        inventory.hotkey.storage.slots[inventory.Select].AddAmount(-1);
        //�Ҹ��� ���߱�
        eatingAudio.Stop();
        //�Դ� �ڷ�ƾ �����ٴ� ��
        eatingCoroutine = null;
    }

    public void EatingStart(float timer, float hunger, int soundIndex)
    {
        eatingCoroutine = StartCoroutine(Eating(timer, hunger, soundManager.clips[soundIndex]));
    }
    public void EatingStop()
    {
        //�Դ� �ڷ�ƾ�� ������ �ʾ������� ��Ŭ���� up�ߴٸ� �ڷ�ƾ �����ְ� �Ҹ��� ���ֱ�
        if (eatingCoroutine != null)
        {
            StopCoroutine(eatingCoroutine);
            eatingAudio.Stop();
        }
    }

    
}
