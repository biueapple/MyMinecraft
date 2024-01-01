using System.Collections.Generic;
using UnityEngine;

namespace WarriorSkills
{
    public class Friendly_Protection : Skill
    {
        //���ӽð�
        private float duration = 7;
        public Vector3 size;

        public override float coefficient { get { return 10 + level * 10 + player.STAT.MAXHP * 0.1f; } }
        public override string detail { get { return "10 + level * 10 + MAXHP * 0.1f"; } }
        public override string expaln 
        {
            get
            {
                string s = description.Replace("!", duration.ToString());
                return s.Replace("?", coefficient.ToString());
            } 
        }
        public override float CoolTime { get { return cooltime - level; } }

        public override void Init(Player player, OccupationUI @interface)
        {
            base.Init(player, @interface);
            type = SKILL_TYPE.PASSIVE;
        }

        public override void Updating()
        {
            base.Updating();
        }

        protected override void Install()
        {
            player.SkillList += InputKey;
        }

        protected override void Uninstall()
        {
            player.SkillList -= InputKey;
        }

        public void InputKey()
        {
            //Ű�� ��������
            if (Input.GetKeyDown(_keyCode))
            {
                if (cooltimer > 0)
                    return;
                //������ ������ �����ͼ� ���������� ���� �����
                //������ ������ �簢�������� �����ε� ���� Intersect.IsIsPointInCircleObject(��ġ, ������) �� ����ϸ� �� 

                List<Collider> colliders = new List<Collider>();
                colliders.AddRange(Physics.OverlapBox(player.transform.position, size));
                colliders.Remove(player.GetComponent<Collider>());
                for (int i = 0; i < colliders.Count; i++)
                {
                    if (colliders[i].GetComponent<Unit>() != null)
                    {
                        //���� ��ȣ���� �򵵷� �����ؾ���
                        colliders[i].GetComponent<Unit>().STAT.Barrier = new Barrier(coefficient, duration, true);
                    }
                }
                player.StartCoroutine(CooltimeCoroutine(CoolTime));
            }
        }
    }
}

