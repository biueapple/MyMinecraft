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
    [Header("� �������� ����� ���ɼ��� �ִ���")]
    public DropItem[] items;
    [Header("�ּҷ� ���� �������� ����")]
    public int minCount;
    [Header("�ִ�� ���� �������� ����")]
    public int maxCount;
    [Header("���� �������� ���� �� �ִ���")]
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
    //�� ������ ������ ����� �������� ����
    public DropTable dropItem;

    [Header("���ݽ� �����Ŀ� ������ �����°�")]
    public float attackDelay;
    [Header("���Ͱ� �ٰ����� �ּҰŸ�")]
    public float minDistance;
    [Header("Ÿ���� �߽�")]
    public Transform RBI;
    //���� �ڷ�ƾ
    protected Coroutine attackCoroutine = null;

    //ui�� ���õ� ����
    public World world;
    protected float fieldOfViewAngle = 60; // ī�޶��� �þ߰��� �����մϴ�.
    protected float detectionRadius = 10f; // ī�޶��� �þ߰� �ۿ����� ������ �ݰ��� �����մϴ�.
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
        //�ʹ� ������ ���ߵ���
        TooClose();
        //���� �õ�
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

    //������ ��Ÿ��� ������
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

    //���� �õ�
    public void AttackAttempt()
    {
        //�̹� �������� �ƴϸ鼭
        if (attackCoroutine != null)
            return;
        //������ �� �ִµ�
        if (stat.Attacktimer >= stat.AttackSpeed)
        {
            List<Collider> list = new List<Collider>();
            list.AddRange(Physics.OverlapSphere(RBI.transform.position, 1.5f));
            list.Remove(GetComponent<Collider>());
            //������ ��Ÿ� �ȿ� �ִٸ�
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].GetComponent<Unit>() != null)
                {
                    //���ݽ���
                    attackCoroutine = StartCoroutine(Attack(attackDelay));
                    //�����߿� �������� ���ϵ���
                    moveSystem.Is_Moving = false;
                    break;
                }
            }
        }
    }

    public void TooClose()
    {
        //�ʹ� ������ ���ߵ���
        if(Vector3.Distance( moveSystem.Target.position, transform.position) <= minDistance)
            moveSystem.Is_Moving = false;
        //�ʹ� ������ �����鼭 �����ߵ� �ƴ϶�� �����̵���
        else if(attackCoroutine == null)
            moveSystem.Is_Moving = true;
    }

    private IEnumerator Attack(float timer)
    {
        //�ð���ŭ ��ٸ���
        yield return new WaitForSeconds(timer);
        List<Collider> list = new List<Collider>();
        list.AddRange(Physics.OverlapSphere(RBI.transform.position, 1.5f));
        list.Remove(GetComponent<Collider>());
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].GetComponent<Unit>() != null)
            {
                //����
                list[i].GetComponent<Unit>().Hit(stat, stat.AD, ATTACKTYPE.NOMAL, DAMAGETYPE.AD);
                break;
            }
        }
        //�ʱ�ȭ
        attackCoroutine = null;
        stat.Attacktimer = 0;
        moveSystem.Is_Moving = true;
    }



    private bool IsMonsterVisible()
    {
        Vector3 directionToMonster = transform.position - cam.transform.position;
        float angle = Vector3.Angle(cam.transform.forward, directionToMonster);

        // ī�޶��� �þ߰� ���� �ִ��� Ȯ��
        if (angle < (fieldOfViewAngle/* + detectionRadius*/))
        {
            RaycastHit hit;

            // ���Ϳ� ī�޶� ���̿� ��ֹ��� �ִ��� Ȯ��
            if (Physics.Raycast(cam.transform.position, directionToMonster, out hit, 10))
            {
                if (hit.collider.gameObject.CompareTag("Monster"))
                {
                    // ���Ϳ� ī�޶� ���̿� �ٸ� ������Ʈ�� ������ true ��ȯ
                    BlockLaycast ray = world.WorldRaycast(cam.transform.position, directionToMonster.normalized, hit.distance);
                    if (ray == null)
                        return true;
                    else
                        return false;
                }
            }
        }

        // �þ߰� ���� ���ų�, ��ֹ��� ���� ��� false ��ȯ
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
