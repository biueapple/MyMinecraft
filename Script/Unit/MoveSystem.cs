using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//비트연산으로 사용함 (sprint상태면서 배고프다면 현재 상태는 배고픈 상태임 {배고픔상태가 달리는 상태보다 숫자가 크기에 우선됨})
public enum MOVE_STATE
{
    //상태는 2배씩 올라가야함
    WALK = 0,
    SPRINT = 1,
    HUNGER = 2,
    STUN = 4,

    
}

public class MoveSystem : MonoBehaviour
{
    //점프에 딜레이를 조금 줘야할듯 바로 위에 블록이 있을때 점프키를 누르면 여러번 점프가 나가니까 한번 누르고나서 조금은 지나야 다시 점프가 가능하도록
    public Transform cam;
    public World world;

    //현재 상태
    [SerializeField]
    protected MOVE_STATE state;
    public MOVE_STATE State { get { return state; } }
    public int SetState { set { state += value; } }

    //걷기 속도
    protected Stat stat;
    protected float walkSpeed = 3;
    //달리기 속도
    //protected float sprintSpeed = 6;
    //점프의 파워
    public float jumpForce = 5;

    //유닛의 크기
    public float unitWidth = 0.15f;
    public float unitHeight;

    //점프중인지 땅위에 서있는지 isJump는 현재 사용중이지 않음
    
    public bool isJump;
    public bool isGrounded;

    //움직일때 항상 쓰이는 변수들 
    //InputMove을 쓴다면 내가 한 입력에 따라 변하고
    //TargetMove을 쓴다면 밑에 변수들과 함께 타겟을 따라 변함
    protected Vector3 velocity;

    protected Vector3 velocityMomemtum;

    private float mouseHorizontal;
    private float mouseVertical;
    //마우스 가동의 범위
    public float maxVertical = 70;
    public float minVertical = -70;
    //점프를 했을때 순식간에 점프하는게 아니라 자연스럽게 올라가려면 현재 y축의 힘을 가지고 중력과 점프의 힘을 넣을 변수가 필요함
    public float jumpMomemtum = 0;

    //TargetMove을 쓴다면 사용할 변수들
    [SerializeField]
    protected Transform target;
    public Transform Target { get { return target; } }
    private float horizontal;
    private float vertical;

    //지금 사용하는 움직이는 방식
    public Action move_Mode;

    //중력을 받을지에 대한 변수
    public bool usingGravity = true;

    //강제이동에 대한 변수
    private bool cc = false;
    public bool CC { get { return cc; } }   
    private Action crowd_Control = null;
    private float distance = 0;
    private float s = 0;
    private Vector3 direction;

    //cc기나 그런거 말고 그냥 안움직여도 된다는 느낌의 안움직이게 하는 변수 (공격중이라던가 target과 너무 가깝다던가)
    protected bool is_moving = true;
    public bool Is_Moving { get { return is_moving; } set { is_moving = value; } }

    // Start is called before the first frame update
    protected void Start()
    {
        stat = GetComponent<Stat>();
        //move_Mode += AutoJump;
        //move_Mode += InputMove;
    }

    // Update is called once per frame
    protected void Update()
    {
        //moveMode가 있을 경우엔 매 프레임마다 velocity를 다시 값을 정해주니까 괜찮은데 
        //moveMode가 없다면 기존의 velocity의 값이 남아있는 상태로 값이 (중력이) 더해져서 말도안돼게 빨리 떨어지는 현상이 나옴
        //그렇기 때문에 항상 velocity의 값을 없앤 후 해야 문제가 없음
        velocity = Vector3.zero;
        //움직임역할을 하는 함수가 있고 cc기에 걸리지 않은 상태면 && 안움직여도 되는 상황
        if (move_Mode != null && !cc && is_moving)
            move_Mode();
        MoveState();
        //이곳에 상태이상이나 맞았을때 움직임에 대한 변화를 해주는 함수가 들어가야 할듯함
        if (crowd_Control != null && cc)
            crowd_Control();
        //외부의 힘이 들어갈 곳
        ExternalForce();
        CalculateVelocity();
        Move();
    }


    public void ApplyExternalForce(Vector3 externalForce)
    {
        velocityMomemtum += externalForce;
    }

    private float forceAttenuation = 0.999f; // 힘의 감쇠 계수 (0.0f에서 1.0f 사이의 값) 높을수록 많이 감쇠
    private void ExternalForce()
    {
        if (velocityMomemtum != Vector3.zero)
        {
            //Vector3 force = velocityMomemtum * 1.2f * Time.deltaTime;
            //velocity += force;
            //velocityMomemtum -= force;
            //if (velocityMomemtum.magnitude <= force.magnitude)
            //    velocityMomemtum = Vector3.zero;

            Vector3 force = velocityMomemtum * Time.deltaTime;
            velocity += force;

            // 힘의 감쇠를 적용
            velocityMomemtum *= (1 - forceAttenuation * Time.deltaTime);

            // 힘이 충분히 작아졌을 때, 제로 벡터로 초기화
            if (velocityMomemtum.magnitude <= 1f)
                velocityMomemtum = Vector3.zero;
        }
    }
    
    public void Force_Invalidation()
    {
        cc = false;
        if(crowd_Control != null)
        {
            crowd_Control = null;
        }
    }


    float faintTimer = 0;
    public void Faint(float timer)
    {
        faintTimer = timer;
        crowd_Control += FaintAction;
        cc = true;
        if (!GetState(MOVE_STATE.STUN))
            state += (int)MOVE_STATE.STUN;
    }
    private void FaintAction()
    {
        faintTimer -= Time.deltaTime;
        if(faintTimer <= 0)
        {
            crowd_Control -= FaintAction;
            if(GetState(MOVE_STATE.STUN))
                state -= MOVE_STATE.STUN;
            cc = false;
        }
    }
    /// <summary>
    /// 일정 거리만큼 강제 이동하는 함수
    /// </summary>
    /// <param name="dir">방향</param>
    /// <param name="dis">거리</param>
    /// <param name="speed">속도</param>
    public void ForceMove_Distance(Vector3 dir, float dis, float speed)
    {
        //속도 정해주고
        if (speed == 0)
        {
            if (stat == null)
                s = walkSpeed;
            else
                s = stat.SPEED;
        }

        direction = dir.normalized;
        distance = dis;
        crowd_Control += ForceMoveDistance;
        cc = true;
    }
    private void ForceMoveDistance()
    {
        velocity = direction * s * Time.deltaTime;
        distance -= Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(transform.position.x + velocity.x, transform.position.z + velocity.z));
        if(distance <= 0)
        {
            crowd_Control -= ForceMoveDistance;
            distance = 0;
            s = 0;
            direction = Vector3.zero;
            cc = false;
        }
    }
    public void ForceMove_Time(Vector3 dir, float timer)
    {

    }

    //현재 무슨 상태인지 리턴해줌 (높은 상태가 우선됨 {배고프면서 스턴상태라면 지금 상태는 스턴상태임})
    public MOVE_STATE NowState()
    {
        if(state >= MOVE_STATE.STUN)
            return MOVE_STATE.STUN;
        else if(state >= MOVE_STATE.HUNGER)
            return MOVE_STATE.HUNGER;
        else if(state >= MOVE_STATE.SPRINT) 
            return MOVE_STATE.SPRINT;

        return MOVE_STATE.WALK;
    }

    //이 상태인지 체크
    public bool GetState(MOVE_STATE state)
    {
        MOVE_STATE s = this.state & state;
        if(s == state)
            return true;
        return false;
    }

    protected void Move()
    {
        //몸 회전
        transform.Rotate(Vector3.up * mouseHorizontal);
        //카메라 회전
        if (cam != null)
        {
            cam.Rotate(Vector3.right * mouseVertical);
            //NormalizeAngle를 하지 않으면 0도에서 더 커질시 maxVertical로 이동하는 일이 생김
            cam.eulerAngles = new Vector3(Mathf.Clamp(NormalizeAngle(cam.eulerAngles.x), minVertical, maxVertical), cam.eulerAngles.y, cam.eulerAngles.z);
        }

        //몸 이동
        transform.Translate(velocity, Space.World);
    }

    protected void MoveState()
    {
        //현재 상태에 따라 입력값에 변화가 있음
        if (NowState() == MOVE_STATE.STUN)
        {
            //스턴상태면 움직이지 못하게 했지만 다른 방식으로 변화가 필요함
            velocity.x *= 0;
            velocity.z *= 0;
        }
        else if (NowState() == MOVE_STATE.HUNGER)
        {
            //배고프면 걷는속도
            if (stat != null)
            {
                velocity.x *= stat.SPEED;
                velocity.z *= stat.SPEED;
            }
            else
            {
                velocity.x *= walkSpeed;
                velocity.z *= walkSpeed;
            }
        }
        else if (NowState() == MOVE_STATE.SPRINT)
        {
            //뛸때는 뛰는 속도
            if (stat != null)
            {
                velocity.x *= stat.SPEED * 2;
                velocity.z *= stat.SPEED * 2;
            }
            else
            {
                velocity.x *= walkSpeed * 2;
                velocity.z *= walkSpeed * 2;
            }
        }
        else if (NowState() == MOVE_STATE.WALK)
        {
            //걸을때는 걷는 속도
            if (stat != null)
            {
                velocity.x *= stat.SPEED;
                velocity.z *= stat.SPEED;
            }
            else
            {
                velocity.x *= walkSpeed;
                velocity.z *= walkSpeed;
            }
        }
    }

    protected virtual void CalculateVelocity()
    {
        //중력
        if(usingGravity)
        {
            jumpMomemtum += Time.deltaTime * BlockInfo.gravity;
        }
        else
        {
            if(jumpMomemtum > 0)
                jumpMomemtum += Time.deltaTime * BlockInfo.gravity;
            else if(jumpMomemtum < 0)
                jumpMomemtum -= Time.deltaTime * BlockInfo.gravity;
            if (jumpMomemtum < 0.01 && jumpMomemtum > -0.01)
                jumpMomemtum = 0;
        }
        velocity.y += jumpMomemtum * Time.deltaTime;


        //앞을 가려는데 막힘               뒤로 가려는데 막힘
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        // 오른쪽이 막힘                  왼쪽이 막힘
        if ((velocity.x > 0 && right) || (velocity.x < 0 && left))
            velocity.x = 0;


        if (velocity.y < 0)
        {
            velocity.y = checkDownSpeed(velocity.y);
        }
        else if (velocity.y > 0)
        {
            velocity.y = checkUpSpeed(velocity.y);
        }

        if (velocity.y == 0)
            jumpMomemtum = 0;
    }

    protected void Jump()
    {
        if (isGrounded)
        {
            jumpMomemtum = jumpForce;
            isJump = true;
            isGrounded = false;
        }
    }

    public void AutoJump()
    {
        //점프 상태가 아니면서
        if (!isJump)
        {
            Vector3 width = new Vector3 (unitWidth, 0, unitWidth);
            if(velocity.x < 0)
                width.x = -unitWidth;
            else if(velocity.x == 0)
                width.x = 0;

            if(velocity.z < 0)
                width.z = -unitWidth;
            else if(velocity.z == 0)
                width.z = 0;

            //앞에 한칸엔 블록이 있는데
            if (world.Vector3WorldBlock(transform.position + new Vector3(velocity.x,0, velocity.z) + width))
            {
                bool j = true;
                //그 위칸이 유닛의 크기만큼 블록이 없다면 자동점프
                for (int i = 1; i < Mathf.CeilToInt(unitHeight); i++)
                {
                    //그 위칸에 블록이 있다면 점프하지 않도록
                    if (world.Vector3WorldBlock(transform.position + new Vector3(velocity.x, 0, velocity.z) + new Vector3(width.x, i, width.z)))
                    {
                        j = false;
                        break;
                    }
                }
                if(j)
                {
                    Jump();
                }
            }
        }
    }

    public void InputFly()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            jumpMomemtum = jumpForce;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            jumpMomemtum = -jumpForce;
        }
        velocity = ((transform.forward * Input.GetAxisRaw("Vertical")) + (transform.right * Input.GetAxisRaw("Horizontal"))).normalized * Time.deltaTime;
        mouseHorizontal = Input.GetAxisRaw("Mouse X");
        mouseVertical = -Input.GetAxisRaw("Mouse Y");
    }

    //입력받아 움직이도록
    public void InputMove()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !GetState(MOVE_STATE.SPRINT))
            state += (int)MOVE_STATE.SPRINT;
        else if(Input.GetKeyUp(KeyCode.LeftShift) && GetState(MOVE_STATE.SPRINT))
            state -= (int)MOVE_STATE.SPRINT;

        if (Input.GetKey(KeyCode.Space))
            Jump();

        velocity = ((transform.forward * Input.GetAxisRaw("Vertical")) + (transform.right * Input.GetAxisRaw("Horizontal"))).normalized * Time.deltaTime;
        mouseHorizontal = Input.GetAxisRaw("Mouse X");
        mouseVertical = -Input.GetAxisRaw("Mouse Y");
    }

    //타겟을 쫓아가도록
    //mouseVertical에 일정 각도를 넘어가면 문제가 생김
    public void TargetMove()
    {
        //각도를 재기위해 방향을 알아내고
        Vector3 dir = target.transform.position - transform.position;
        //방향으로 일정속도로 이동
        velocity = dir.normalized * Time.deltaTime;
        //x z 로 y축 각도를 알아내고
        horizontal = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        //y z 로 x축 각도를 알아내서
        vertical = Mathf.Atan2(dir.y, dir.z) * Mathf.Rad2Deg;

        //y 축 각도로 플레이어를 회전시키는데 lerp로 얼만큼 움직여야 하는지를 알아내야 하는거라서 현재 각도를 빼 움직일 만큼만 얻고
        mouseHorizontal = Mathf.LerpAngle(transform.eulerAngles.y, horizontal, Time.deltaTime * 3) - transform.eulerAngles.y;
        //x 축 각도로 카메라를 회전시키는데 얼만큼 움직여야 하는지 알아내야 해서 현재 각도를 빼는데 카메라는 회전축의 기준이 반대라서 (위로갈수록 -) -vertiacl 를 넣어줌
        //카메라는 문제가 있음
        if (cam != null)
        {
            //-vertical 를 하는 이유는 카메라의 각도 기준때문 위를 볼수록 각도가 -가 되는데 위에 vertical은 +가 되기 때문에 반대로 해줌
            //카메라에서 문제는 타겟이 유닛을 한바퀴 돌면 생김 해결방법을 모르겠음
            mouseVertical = Mathf.LerpAngle(cam.eulerAngles.x, -vertical, Time.deltaTime) - cam.eulerAngles.x;
        }
    }



    //아래에 블록이 있는지
    private float checkDownSpeed(float downSpeed)
    {
        if (
            world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y + downSpeed, transform.position.z - (unitWidth - 0.1f))) ||
            world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y + downSpeed, transform.position.z - (unitWidth - 0.1f))) ||
            world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y + downSpeed, transform.position.z + (unitWidth - 0.1f))) ||
            world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y + downSpeed, transform.position.z + (unitWidth - 0.1f)))
          )
        {
            isGrounded = true;
            isJump = false;
            return 0;
        }
        else
        {
            isGrounded = false;
            isJump = true;
            return downSpeed;
        }
    }

    //위에 블록이 있는지
    private float checkUpSpeed(float upSpeed)
    {
        if (
            world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y + unitHeight + upSpeed, transform.position.z - (unitWidth - 0.1f))) ||
            world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y + unitHeight + upSpeed, transform.position.z - (unitWidth - 0.1f))) ||
            world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y + unitHeight + upSpeed, transform.position.z + (unitWidth - 0.1f))) ||
            world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y + unitHeight + upSpeed, transform.position.z + (unitWidth - 0.1f)))
          )
        {
            return 0;
        }
        else
        {
            return upSpeed;
        }
    }


    //그쪽 방향으로 움직일 수 있는지 계산
    public bool front
    {
        get
        {
            if (
                world.Vector3WorldBlock(new Vector3(transform.position.x, transform.position.y, transform.position.z + unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y, transform.position.z + unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y, transform.position.z + unitWidth)) ||

                world.Vector3WorldBlock(new Vector3(transform.position.x, transform.position.y + unitHeight, transform.position.z + unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y + unitHeight, transform.position.z + unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y + unitHeight, transform.position.z + unitWidth))
              )
                return true;
            else
                return false;
        }
    }

    public bool back
    {
        get
        {
            if (
                world.Vector3WorldBlock(new Vector3(transform.position.x, transform.position.y, transform.position.z - unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y, transform.position.z - unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y, transform.position.z - unitWidth)) ||

                world.Vector3WorldBlock(new Vector3(transform.position.x, transform.position.y + unitHeight, transform.position.z - unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - (unitWidth - 0.1f), transform.position.y + unitHeight, transform.position.z - unitWidth)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + (unitWidth - 0.1f), transform.position.y + unitHeight, transform.position.z - unitWidth))
              )
                return true;
            else
                return false;
        }
    }

    public bool left
    {
        get
        {
            if (
                world.Vector3WorldBlock(new Vector3(transform.position.x - unitWidth, transform.position.y, transform.position.z)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - unitWidth, transform.position.y, transform.position.z + (unitWidth - 0.1f))) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - unitWidth, transform.position.y, transform.position.z - (unitWidth - 0.1f))) ||

                world.Vector3WorldBlock(new Vector3(transform.position.x - unitWidth, transform.position.y + unitHeight, transform.position.z)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - unitWidth, transform.position.y + unitHeight, transform.position.z + (unitWidth - 0.1f))) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x - unitWidth, transform.position.y + unitHeight, transform.position.z - (unitWidth - 0.1f)))
              )
                return true;
            else
                return false;
        }
    }

    public bool right
    {
        get
        {
            if (
                world.Vector3WorldBlock(new Vector3(transform.position.x + unitWidth, transform.position.y, transform.position.z)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + unitWidth, transform.position.y, transform.position.z + (unitWidth - 0.1f))) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + unitWidth, transform.position.y, transform.position.z - (unitWidth - 0.1f))) ||

                world.Vector3WorldBlock(new Vector3(transform.position.x + unitWidth, transform.position.y + unitHeight, transform.position.z)) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + unitWidth, transform.position.y + unitHeight, transform.position.z + (unitWidth - 0.1f))) ||
                world.Vector3WorldBlock(new Vector3(transform.position.x + unitWidth, transform.position.y + unitHeight, transform.position.z - (unitWidth - 0.1f)))
              )
                return true;
            else
                return false;
        }
    }

    //270도와 -90도는 같은거지만 비교하기 힘들기에
    //각도를 항상 -180 ~ 180도 이내로 만들어 서로 비교하기에 편하게 만듬a
    public static float NormalizeAngle(float angle)
    {
        angle %= 360; // 각도를 360도 범위 내로 줄임

        if (angle < -180)
        {
            angle += 360; // -180도 미만일 경우, 180도를 더해 180도 범위 내로 이동
        }
        else if (angle > 180)
        {
            angle -= 360; // 180도 초과일 경우, 180도를 빼 180도 범위 내로 이동
        }

        return angle;
    }
}


//    [SerializeField]
//    private int skillPoint;
//    public int SkillPoint { get { return skillPoint;} set { skillPoint += value; } }

//    public Action SkillList = null;

//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    // Update is called once per frame
//    void Update()
//    {
//        InputMovement();
//        Non_ForcedMovement(velocity);
//        if (SkillList != null)
//        {
//            SkillList();
//        }

//    }



//    public void Test()
//    {
//        enemy.stat.Be_Attacked(stat, 10, ATTACKTYPE.NOMAL, DAMAGETYPE.AD);
//    }

//}
