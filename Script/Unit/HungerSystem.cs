using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HungerSystem : MonoBehaviour
{
    //������� �޸� �� ������ ����� ����
    MoveSystem moveSystem;
    //���������� �־��� ��ġ (�̵��Ÿ��� ���� ������� �پ��� ������)
    Vector3 last;
    //������ ��ġ���������� �Ÿ�
    float distance;
    //���� �����
    [SerializeField]
    private float hunger = 20;
    public float Hunger { get { return hunger; } set { hunger += value; } }
    
    //������� �پ�鶧 (int������) ȣ������ (�⺻������ ui�� �����Ҷ� �� �޸� �� �������� �������� �����Ҷ� �����)
    private Action hungerAction = null;
    //������ �԰ų� ������ ������
    public Action AddHunger { set { hungerAction += value; } }
    //�ϴ� ��������� ����������� ����
    public Action RemoveHunger { set { hungerAction -= value; } }

    private int lastHunger;
    // Start is called before the first frame update
    void Start()
    {
        moveSystem = GetComponent<MoveSystem>();
        last = transform.position;
        lastHunger = (int)hunger;
        hungerAction += ChangeHunger;
    }

    // Update is called once per frame
    void Update()
    {
        //�����߿��� ������� ���� ���� (�ڽ��� �����̴°��� �ƴϴϱ�)
        if (moveSystem.State >= MOVE_STATE.STUN)
        {
            
        }
        //�޸��� �߿��� ������� ��
        else if (moveSystem.State >= MOVE_STATE.SPRINT)
        {
            //y��ǥ�� �ö󰡴°� �Ÿ��� �߰�
            if (transform.position.y > last.y)
                distance = Vector3.Distance(transform.position, last);
            //y��ǥ�� ���������� �̵��Ÿ��� �߰����� ����
            else
                distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(last.x, last.z));
            hunger -= distance * Time.deltaTime;
        }
        //�ȴ��߿��� ��������
        else if (moveSystem.State >= MOVE_STATE.WALK)
        {
            if (transform.position.y > last.y)
                distance = Vector3.Distance(transform.position, last);
            else
                distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(last.x, last.z));
            hunger -= distance * Time.deltaTime;
        }
        //�������� �ִ� ��ġ �־��ֱ�
        last = transform.position;

        //callback ������ ����İ� ���ؼ� �ٸ��ٸ� callback�� ȣ��
        if ((int)hunger != lastHunger)
        {
            hungerAction();
            lastHunger = (int)hunger;
        }
    }


    //������� BlockInfo.minHunger���ϸ� �޸��� ����
    public void ChangeHunger()
    {
        //����� üũ
        //������� ����
        if(hunger >= BlockInfo.minHunger)
        {
            //���� ���°� ����� ��������
            int re = (int)moveSystem.State & (int)MOVE_STATE.HUNGER;
            //�̹� ����� ���¿��ٸ� ���̻� ����� ���°� �ƴϵ���
            if (re == (int)MOVE_STATE.HUNGER)
            {
                moveSystem.SetState = -(int)MOVE_STATE.HUNGER;
            }
        }
        //����� (�޸� �� ����)
        else
        {
            //���� ���°� ����� ��������
            int re = (int)moveSystem.State & (int)MOVE_STATE.HUNGER;
            //����� ���°� �ƴ϶�� ����� ���·� ����
            if (re != (int)MOVE_STATE.HUNGER)
            {
                moveSystem.SetState = (int)MOVE_STATE.HUNGER;
            }
        }
    }
}
