using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//��Ʈ�������� ����� (sprint���¸鼭 ������ٸ� ���� ���´� ����� ������ {����Ļ��°� �޸��� ���º��� ���ڰ� ũ�⿡ �켱��})
public enum MOVE_STATE
{
    //���´� 2�辿 �ö󰡾���
    WALK = 0,
    SPRINT = 1,
    HUNGER = 2,
    STUN = 4,

    
}

public class MoveSystem : MonoBehaviour
{
    //������ �����̸� ���� ����ҵ� �ٷ� ���� ����� ������ ����Ű�� ������ ������ ������ �����ϱ� �ѹ� �������� ������ ������ �ٽ� ������ �����ϵ���
    public Transform cam;
    public World world;

    //���� ����
    [SerializeField]
    protected MOVE_STATE state;
    public MOVE_STATE State { get { return state; } }
    public int SetState { set { state += value; } }

    //�ȱ� �ӵ�
    protected Stat stat;
    protected float walkSpeed = 3;
    //�޸��� �ӵ�
    //protected float sprintSpeed = 6;
    //������ �Ŀ�
    public float jumpForce = 5;

    //������ ũ��
    public float unitWidth = 0.15f;
    public float unitHeight;

    //���������� ������ ���ִ��� isJump�� ���� ��������� ����
    
    public bool isJump;
    public bool isGrounded;

    //�����϶� �׻� ���̴� ������ 
    //InputMove�� ���ٸ� ���� �� �Է¿� ���� ���ϰ�
    //TargetMove�� ���ٸ� �ؿ� ������� �Բ� Ÿ���� ���� ����
    protected Vector3 velocity;

    protected Vector3 velocityMomemtum;

    private float mouseHorizontal;
    private float mouseVertical;
    //���콺 ������ ����
    public float maxVertical = 70;
    public float minVertical = -70;
    //������ ������ ���İ��� �����ϴ°� �ƴ϶� �ڿ������� �ö󰡷��� ���� y���� ���� ������ �߷°� ������ ���� ���� ������ �ʿ���
    public float jumpMomemtum = 0;

    //TargetMove�� ���ٸ� ����� ������
    [SerializeField]
    protected Transform target;
    public Transform Target { get { return target; } }
    private float horizontal;
    private float vertical;

    //���� ����ϴ� �����̴� ���
    public Action move_Mode;

    //�߷��� �������� ���� ����
    public bool usingGravity = true;

    //�����̵��� ���� ����
    private bool cc = false;
    public bool CC { get { return cc; } }   
    private Action crowd_Control = null;
    private float distance = 0;
    private float s = 0;
    private Vector3 direction;

    //cc�⳪ �׷��� ���� �׳� �ȿ������� �ȴٴ� ������ �ȿ����̰� �ϴ� ���� (�������̶���� target�� �ʹ� �����ٴ���)
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
        //moveMode�� ���� ��쿣 �� �����Ӹ��� velocity�� �ٽ� ���� �����ִϱ� �������� 
        //moveMode�� ���ٸ� ������ velocity�� ���� �����ִ� ���·� ���� (�߷���) �������� �����ȵŰ� ���� �������� ������ ����
        //�׷��� ������ �׻� velocity�� ���� ���� �� �ؾ� ������ ����
        velocity = Vector3.zero;
        //�����ӿ����� �ϴ� �Լ��� �ְ� cc�⿡ �ɸ��� ���� ���¸� && �ȿ������� �Ǵ� ��Ȳ
        if (move_Mode != null && !cc && is_moving)
            move_Mode();
        MoveState();
        //�̰��� �����̻��̳� �¾����� �����ӿ� ���� ��ȭ�� ���ִ� �Լ��� ���� �ҵ���
        if (crowd_Control != null && cc)
            crowd_Control();
        //�ܺ��� ���� �� ��
        ExternalForce();
        CalculateVelocity();
        Move();
    }


    public void ApplyExternalForce(Vector3 externalForce)
    {
        velocityMomemtum += externalForce;
    }

    private float forceAttenuation = 0.999f; // ���� ���� ��� (0.0f���� 1.0f ������ ��) �������� ���� ����
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

            // ���� ���踦 ����
            velocityMomemtum *= (1 - forceAttenuation * Time.deltaTime);

            // ���� ����� �۾����� ��, ���� ���ͷ� �ʱ�ȭ
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
    /// ���� �Ÿ���ŭ ���� �̵��ϴ� �Լ�
    /// </summary>
    /// <param name="dir">����</param>
    /// <param name="dis">�Ÿ�</param>
    /// <param name="speed">�ӵ�</param>
    public void ForceMove_Distance(Vector3 dir, float dis, float speed)
    {
        //�ӵ� �����ְ�
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

    //���� ���� �������� �������� (���� ���°� �켱�� {������鼭 ���ϻ��¶�� ���� ���´� ���ϻ�����})
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

    //�� �������� üũ
    public bool GetState(MOVE_STATE state)
    {
        MOVE_STATE s = this.state & state;
        if(s == state)
            return true;
        return false;
    }

    protected void Move()
    {
        //�� ȸ��
        transform.Rotate(Vector3.up * mouseHorizontal);
        //ī�޶� ȸ��
        if (cam != null)
        {
            cam.Rotate(Vector3.right * mouseVertical);
            //NormalizeAngle�� ���� ������ 0������ �� Ŀ���� maxVertical�� �̵��ϴ� ���� ����
            cam.eulerAngles = new Vector3(Mathf.Clamp(NormalizeAngle(cam.eulerAngles.x), minVertical, maxVertical), cam.eulerAngles.y, cam.eulerAngles.z);
        }

        //�� �̵�
        transform.Translate(velocity, Space.World);
    }

    protected void MoveState()
    {
        //���� ���¿� ���� �Է°��� ��ȭ�� ����
        if (NowState() == MOVE_STATE.STUN)
        {
            //���ϻ��¸� �������� ���ϰ� ������ �ٸ� ������� ��ȭ�� �ʿ���
            velocity.x *= 0;
            velocity.z *= 0;
        }
        else if (NowState() == MOVE_STATE.HUNGER)
        {
            //������� �ȴ¼ӵ�
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
            //�۶��� �ٴ� �ӵ�
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
            //�������� �ȴ� �ӵ�
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
        //�߷�
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


        //���� �����µ� ����               �ڷ� �����µ� ����
        if ((velocity.z > 0 && front) || (velocity.z < 0 && back))
            velocity.z = 0;
        // �������� ����                  ������ ����
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
        //���� ���°� �ƴϸ鼭
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

            //�տ� ��ĭ�� ����� �ִµ�
            if (world.Vector3WorldBlock(transform.position + new Vector3(velocity.x,0, velocity.z) + width))
            {
                bool j = true;
                //�� ��ĭ�� ������ ũ�⸸ŭ ����� ���ٸ� �ڵ�����
                for (int i = 1; i < Mathf.CeilToInt(unitHeight); i++)
                {
                    //�� ��ĭ�� ����� �ִٸ� �������� �ʵ���
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

    //�Է¹޾� �����̵���
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

    //Ÿ���� �Ѿư�����
    //mouseVertical�� ���� ������ �Ѿ�� ������ ����
    public void TargetMove()
    {
        //������ ������� ������ �˾Ƴ���
        Vector3 dir = target.transform.position - transform.position;
        //�������� �����ӵ��� �̵�
        velocity = dir.normalized * Time.deltaTime;
        //x z �� y�� ������ �˾Ƴ���
        horizontal = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        //y z �� x�� ������ �˾Ƴ���
        vertical = Mathf.Atan2(dir.y, dir.z) * Mathf.Rad2Deg;

        //y �� ������ �÷��̾ ȸ����Ű�µ� lerp�� ��ŭ �������� �ϴ����� �˾Ƴ��� �ϴ°Ŷ� ���� ������ �� ������ ��ŭ�� ���
        mouseHorizontal = Mathf.LerpAngle(transform.eulerAngles.y, horizontal, Time.deltaTime * 3) - transform.eulerAngles.y;
        //x �� ������ ī�޶� ȸ����Ű�µ� ��ŭ �������� �ϴ��� �˾Ƴ��� �ؼ� ���� ������ ���µ� ī�޶�� ȸ������ ������ �ݴ�� (���ΰ����� -) -vertiacl �� �־���
        //ī�޶�� ������ ����
        if (cam != null)
        {
            //-vertical �� �ϴ� ������ ī�޶��� ���� ���ض��� ���� ������ ������ -�� �Ǵµ� ���� vertical�� +�� �Ǳ� ������ �ݴ�� ����
            //ī�޶󿡼� ������ Ÿ���� ������ �ѹ��� ���� ���� �ذ����� �𸣰���
            mouseVertical = Mathf.LerpAngle(cam.eulerAngles.x, -vertical, Time.deltaTime) - cam.eulerAngles.x;
        }
    }



    //�Ʒ��� ����� �ִ���
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

    //���� ����� �ִ���
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


    //���� �������� ������ �� �ִ��� ���
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

    //270���� -90���� ���������� ���ϱ� ����⿡
    //������ �׻� -180 ~ 180�� �̳��� ����� ���� ���ϱ⿡ ���ϰ� ����a
    public static float NormalizeAngle(float angle)
    {
        angle %= 360; // ������ 360�� ���� ���� ����

        if (angle < -180)
        {
            angle += 360; // -180�� �̸��� ���, 180���� ���� 180�� ���� ���� �̵�
        }
        else if (angle > 180)
        {
            angle -= 360; // 180�� �ʰ��� ���, 180���� �� 180�� ���� ���� �̵�
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
