using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : Unit
{
    //sound
    public SoundManager soundManager;
    protected AudioSource eatingAudio;

    //월드
    public Transform cam;
    public World world;
    //블록을 파괴했을때 아이템을 떨굴 매니저
    public DropItemManager dropItemManager;

    //플레이어
    public Transform highlightBlock;
    public Transform placeBlock;
    public Transform blokenBlock;
    //어느 간격으로 블록을 확인하는지
    public float checkIncrement = 0.1f;
    //최대거리
    public float reach = 8;

    //인벤토리
    private PlayerInventorySystem inventory;
    public PlayerInventorySystem InventorySystem { get { return inventory; } }
    //배고픔
    private HungerSystem hungerSystem;

    //위치
    //머리위치
    public Transform head;
    //손위치
    public Transform hand;
    //공격의 시작점
    public Transform RBI;

    public bool battleMode = false;

    //스킬
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
                //raycast를 대신함
                ray = world.WorldRaycast(cam.position, cam.forward, reach, checkIncrement);
                //내가 무슨 블록을 보고있는지 표시
                PlaceCursorBlocks();
                //블록을 캐는 함수
                MouseLeftClick();

                //들고있는 아이템에 따라 효과가 다름
                MouseLeftClickDown(); //(공격에 대한 내용도 들어가 있음)
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
            //무슨 값을 넣던 상관없음
            skillUI.Active = true;
        }

        if(Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }


    //플레이어가 무슨 블록을 바라보고 있는지 어디에 블록을 설치하게 되는지 보여주는 함수
    private void PlaceCursorBlocks()
    {
        if(ray != null)
        {
            //있다면 그 블록의 위치가 파괴될 블록의 위치고
            highlightBlock.position = ray.positionToInt;
            blokenBlock.position = ray.positionToInt;
            //그 전에 마지막으로 확인했던 블록의 위치가 블록이 설치될 위치
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

    //한번 누르면 블록을 파괴하는것이 아니라 누르고 있다면 일정 시간 후에 파괴되는걸로
    //들고있는 아이템의 강도 타입과 바라보는 블록의 강도타입이 같다면 적용 아니면 적용 하지 않음
    // 1이 파괴
    private  float breaking = 0;
    //내 손이 얼마나 단단한가
    private float hard = 1;
    //마지막 위치는 어디였는가 (지금 내가 부수는 블록이 달라진다면 {보고있는 블록이 달라진다면} 처음부터 다시 부셔야 하니까)
    private Vector3Int last;
    //공격의 범위
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
            //뭔가 손에 들고 있을때
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

    //블록을 캐는 함수
    private void MouseLeftClick()
    {
        if (Input.GetMouseButton(0))
        {
            //지금 내가 무슨 블록을 보고있다는 뜻
            if (ray != null)
            {
                //그 위치가 마지막 위치와 다르다면 내가 지금 다른 블록을 보고있다는 뜻
                if (last != ray.positionToInt)
                    breaking = 0;
                //데이터가 비어있지 않다면 (존재하는 블록이라면 {highlightBlock.gameObject.activeSelf는 없애도 괜찮을듯 함})
                if (ray.block != null)
                {
                    //내가 손에 들고있는 아이템이 존재한면서 타입도 같다면 (돌인데 곡괭이면 좋지만 도끼는 안좋음)
                    if (inventory.hotkey.storage.slots[inventory.Select].GetItem.ID > -1 && ray.block.hardness.type == inventory.hotkey.storage.slots[inventory.Select].GetItem.Hardness.type)
                    {
                        //들고있는 아이템의 단단함이 내 손의 단단함임
                        hard = inventory.hotkey.storage.slots[inventory.Select].GetItem.Hardness.hard;
                    }
                    else
                    {
                        //그렇지 않다면 기본적인 단단함
                        hard = 1;
                    }

                    breaking += hard / (float)ray.block.hardness.hard * Time.deltaTime;
                    //* 0.25 는 / 4 대신 한거 이미지가 4개로 나뉘어 있어서 
                    blokenBlock.GetComponent<Renderer>().material.SetFloat("_Id", (int)(breaking * 4));

                    //블록은 부셔짐
                    if (breaking >= 1)
                    {
                        //아이템 드랍하고
                        dropItemManager.DropItem(ray.block.itemID, ray.positionToInt);
                        //부셔진 곳은 air로 바꿔주고
                        world.InstallBlock(new BlockOrder(ray.positionToInt, _BLOCK.AIR));
                        //부셔졌으니 다시 0으로
                        breaking = 0;
                    }
                }
                last = ray.positionToInt;
            }
        }
    }

    //마우스를 때면 캐고있던 블록을 더이상 캐지 않도록 하는거임
    private void MouseLeftClickUp()
    {
        if (Input.GetMouseButtonUp(0))
        {
            //얼마나 캤는가를 0으로 하고
            breaking = 0;
            //부셔짐을 표현해주는 정도도 -1로 해줌
            blokenBlock.GetComponent<Renderer>().material.SetFloat("_Id", -1);
        }
    }

    //블록 설치와 음식먹기
    public Coroutine eatingCoroutine = null;
    private void MouseRightClick()
    {
        if (Input.GetMouseButtonDown(1))
        {
            //뭔가 손에 들고 있을때
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
            //뭔가 손에 들고 있을때
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
            //뭔가 손에 들고 있을때
            if (inventory.equipment.storage.slots[4].GetItem.ID >= 0)
            {
                inventory.equipment.storage.slots[4].GetItem.LeftMouseDown(this);
            }
        }
    }
    private void EquipRightClickDown()
    {

    }

    //음식먹는 코루틴
    protected IEnumerator Eating(float timer, float hunger, AudioClip sound)
    {
        //소리 한번 켜주고
        eatingAudio.PlayOneShot(sound);
        //시간 기다리고
        yield return new WaitForSeconds(timer);
        //배고픔 채우고
        hungerSystem.Hunger = hunger;
        //아이템 갯수 하나 까고
        inventory.hotkey.storage.slots[inventory.Select].AddAmount(-1);
        //소리도 멈추기
        eatingAudio.Stop();
        //먹는 코루틴 끝났다는 뜻
        eatingCoroutine = null;
    }

    public void EatingStart(float timer, float hunger, int soundIndex)
    {
        eatingCoroutine = StartCoroutine(Eating(timer, hunger, soundManager.clips[soundIndex]));
    }
    public void EatingStop()
    {
        //먹는 코루틴이 끝나지 않았음에도 우클릭을 up했다면 코루틴 멈춰주고 소리도 꺼주기
        if (eatingCoroutine != null)
        {
            StopCoroutine(eatingCoroutine);
            eatingAudio.Stop();
        }
    }

    
}
