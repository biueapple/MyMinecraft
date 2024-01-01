using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HungerSystem : MonoBehaviour
{
    //배고프면 달릴 수 없도록 만들기 위해
    MoveSystem moveSystem;
    //마지막으로 있었던 위치 (이동거리에 따라 배고픔이 줄어들기 때문에)
    Vector3 last;
    //마지막 위치에서부터의 거리
    float distance;
    //현재 배고픔
    [SerializeField]
    private float hunger = 20;
    public float Hunger { get { return hunger; } set { hunger += value; } }
    
    //배고픔이 줄어들때 (int단위로) 호출해줌 (기본적으로 ui를 변경할때 와 달릴 수 있을때와 없을때를 구분할때 사용함)
    private Action hungerAction = null;
    //음식을 먹거나 했을때 쓰려고
    public Action AddHunger { set { hungerAction += value; } }
    //일단 만들었지만 사용중이지는 않음
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
        //스턴중에는 배고픔이 닳지 않음 (자신이 움직이는것이 아니니까)
        if (moveSystem.State >= MOVE_STATE.STUN)
        {
            
        }
        //달리는 중에는 배고픔이 닳
        else if (moveSystem.State >= MOVE_STATE.SPRINT)
        {
            //y좌표가 올라가는건 거리에 추가
            if (transform.position.y > last.y)
                distance = Vector3.Distance(transform.position, last);
            //y좌표가 내려갈때는 이동거리에 추가하지 않음
            else
                distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(last.x, last.z));
            hunger -= distance * Time.deltaTime;
        }
        //걷는중에도 마찬가지
        else if (moveSystem.State >= MOVE_STATE.WALK)
        {
            if (transform.position.y > last.y)
                distance = Vector3.Distance(transform.position, last);
            else
                distance = Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(last.x, last.z));
            hunger -= distance * Time.deltaTime;
        }
        //마지막에 있던 위치 넣어주기
        last = transform.position;

        //callback 마지막 배고픔과 비교해서 다르다면 callback을 호출
        if ((int)hunger != lastHunger)
        {
            hungerAction();
            lastHunger = (int)hunger;
        }
    }


    //배고픔이 BlockInfo.minHunger이하면 달리지 못함
    public void ChangeHunger()
    {
        //배고픔 체크
        //배고프지 않음
        if(hunger >= BlockInfo.minHunger)
        {
            //현재 상태가 배고픔 상태인지
            int re = (int)moveSystem.State & (int)MOVE_STATE.HUNGER;
            //이미 배고픈 상태였다면 더이상 배고픈 상태가 아니도록
            if (re == (int)MOVE_STATE.HUNGER)
            {
                moveSystem.SetState = -(int)MOVE_STATE.HUNGER;
            }
        }
        //배고픔 (달릴 수 없음)
        else
        {
            //현재 상태가 배고픔 상태인지
            int re = (int)moveSystem.State & (int)MOVE_STATE.HUNGER;
            //배고픈 상태가 아니라면 배고픈 상태로 만듦
            if (re != (int)MOVE_STATE.HUNGER)
            {
                moveSystem.SetState = (int)MOVE_STATE.HUNGER;
            }
        }
    }
}
