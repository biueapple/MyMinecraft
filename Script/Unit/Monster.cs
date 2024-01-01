using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DropItem
{
    public ItemObject item;
    public int token;
}
[System.Serializable]
public class DropTable
{
    [Header("어떤 아이템을 드롭할 가능성이 있는지")]
    public DropItem[] items;
    [Header("최소로 나올 아이템의 갯수")]
    public int minCount;
    [Header("최대로 나올 아이템의 갯수")]
    public int maxCount;
    [Header("같은 아이템이 나올 수 있는지")]
    public bool overlap;

    public void DrawLots(out int[] ids)
    {
        int count = Random.Range(minCount, maxCount);
        ids = new int[count];
        int allToken = items.Select(x => x.token).Sum();
        for (int i = 0; i < count; i++)
        {
            int token = Random.Range(0, allToken + 1);
            for(int j = 0; j < items.Length; j++)
            {
                if(token > items[j].token)
                    token -= items[j].token;
                else
                {
                    ids[i] = items[j].item.data.id;
                    break;
                }
            }
        }
    }
}

public class Monster : Unit
{
    //이 유닛이 죽을때 드랍할 아이템의 정보
    public DropTable dropItem;

    [Header("공격시 몇초후에 공격이 나가는가")]
    public float attackDelay;
    [Header("몬스터가 다가가는 최소거리")]
    public float minDistance;
    [Header("타점의 중심")]
    public Transform RBI;
    //공격 코루틴
    protected Coroutine attackCoroutine = null;

    //ui에 관련된 변수
    public World world;
    protected float fieldOfViewAngle = 60; // 카메라의 시야각을 설정합니다.
    protected float detectionRadius = 10f; // 카메라의 시야각 밖에서도 감지할 반경을 설정합니다.
    protected Camera cam;

    protected Canvas canvas;
    protected Text text_name;
    protected Text text_level;
    protected Image hpbar;
    protected Coroutine hpbarCoroutine = null;

    // Start is called before the first frame update
    new void Start()
    {
        base .Start();
        moveSystem.move_Mode += moveSystem.TargetMove;
        moveSystem.move_Mode += moveSystem.AutoJump;

        //
        cam = Camera.main;
        fieldOfViewAngle = cam.fieldOfView;

        //
        canvas = transform.GetChild(0).GetComponent<Canvas>();
        text_name = canvas.transform.GetChild(0).GetComponent<Text>();
        text_level = canvas.transform.GetChild(1).GetComponent<Text>();
        hpbar = canvas.transform.GetChild(3).GetComponent<Image>();

        //test
        stat.Barrier = new Barrier(8, 10, false);
    }

    // Update is called once per frame
    new void Update()
    {
        if(attackCoroutine == null)
            base.Update();
        //너무 가까우면 멈추도록
        TooClose();
        //공격 시도
        AttackAttempt();

        if(IsMonsterVisible())
        {
            if (hpbarCoroutine == null)
            {
                canvas.gameObject.SetActive(true);
                hpbarCoroutine = StartCoroutine(ViewInfomation());
            }
        }
        else
        {
            if(hpbarCoroutine != null)
            {
                canvas.gameObject.SetActive(false);
                StopCoroutine(hpbarCoroutine);
                hpbarCoroutine = null;
            }
        }
    }

    //공격의 사거리를 보여줌
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(RBI.transform.position, 1.5f);
    }

    public override void Hit(Stat perpetrator, float figure, ATTACKTYPE attack, DAMAGETYPE damage)
    {
        base.Hit(perpetrator, figure, attack, damage);
        if(stat.HP <= 0)
        {
            int[] ids;
            dropItem.DrawLots(out ids);
            for(int i = 0; i < ids.Length; i++)
            {
                DropItemManager.instance.DropItem(ids[i], transform.position + new Vector3(0, moveSystem.unitHeight * 0.5f, 0));
            }
            StopAllCoroutines();
            ObjectPooling.instance.DestroyObject(gameObject);
        }
    }

    //공격 시도
    public void AttackAttempt()
    {
        //이미 공격중이 아니면서
        if (attackCoroutine != null)
            return;
        //공격할 수 있는데
        if (stat.Attacktimer >= stat.AttackSpeed)
        {
            List<Collider> list = new List<Collider>();
            list.AddRange(Physics.OverlapSphere(RBI.transform.position, 1.5f));
            list.Remove(GetComponent<Collider>());
            //유닛이 사거리 안에 있다면
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetComponent<Unit>() != null)
                {
                    //공격시작
                    attackCoroutine = StartCoroutine(Attack(attackDelay));
                    //공격중엔 움직이지 못하도록
                    moveSystem.Is_Moving = false;
                    break;
                }
            }
        }
    }

    public void TooClose()
    {
        //너무 가까우면 멈추도록
        if(Vector3.Distance( moveSystem.Target.position, transform.position) <= minDistance)
            moveSystem.Is_Moving = false;
        //너무 가깝지 않으면서 공격중도 아니라면 움직이도록
        else if(attackCoroutine == null)
            moveSystem.Is_Moving = true;
    }

    private IEnumerator Attack(float timer)
    {
        //시간만큼 기다리고
        yield return new WaitForSeconds(timer);
        List<Collider> list = new List<Collider>();
        list.AddRange(Physics.OverlapSphere(RBI.transform.position, 1.5f));
        list.Remove(GetComponent<Collider>());
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].GetComponent<Unit>() != null)
            {
                //공격
                list[i].GetComponent<Unit>().Hit(stat, stat.AD, ATTACKTYPE.NOMAL, DAMAGETYPE.AD);
                break;
            }
        }
        //초기화
        attackCoroutine = null;
        stat.Attacktimer = 0;
        moveSystem.Is_Moving = true;
    }



    private bool IsMonsterVisible()
    {
        Vector3 directionToMonster = transform.position - cam.transform.position;
        float angle = Vector3.Angle(cam.transform.forward, directionToMonster);

        // 카메라의 시야각 내에 있는지 확인
        if (angle < (fieldOfViewAngle/* + detectionRadius*/))
        {
            RaycastHit hit;

            // 몬스터와 카메라 사이에 장애물이 있는지 확인
            if (Physics.Raycast(cam.transform.position, directionToMonster, out hit, 10))
            {
                if (hit.collider.gameObject.CompareTag("Monster"))
                {
                    // 몬스터와 카메라 사이에 다른 오브젝트가 없으면 true 반환
                    BlockLaycast ray = world.WorldRaycast(cam.transform.position, directionToMonster.normalized, hit.distance);
                    if (ray == null)
                        return true;
                    else
                        return false;
                }
            }
        }

        // 시야각 내에 없거나, 장애물이 있을 경우 false 반환
        return false;
    }

    private IEnumerator ViewInfomation()
    {
        text_name.text = name;
        text_level.text = level.ToString();

        while (true)
        {
            if (stat.MAXHP == 0)
                hpbar.fillAmount = 0;
            else
                hpbar.fillAmount = stat.HP / stat.MAXHP;

            canvas.transform.LookAt(cam.transform);
            canvas.transform.localEulerAngles += new Vector3(0, 180, 0);
            yield return null;
        }
    }
}
