using System.Collections.Generic;
using UnityEngine;

namespace WarriorSkills
{
    public class Friendly_Protection : Skill
    {
        //지속시간
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
            //키를 눌렀을때
            if (Input.GetKeyDown(_keyCode))
            {
                if (cooltimer > 0)
                    return;
                //주위의 유닛을 가져와서 같은팀에게 전부 줘야함
                //지금은 범위가 사각형이지만 원으로도 가능 Intersect.IsIsPointInCircleObject(위치, 반지름) 을 사용하면 됨 

                List<Collider> colliders = new List<Collider>();
                colliders.AddRange(Physics.OverlapBox(player.transform.position, size));
                colliders.Remove(player.GetComponent<Collider>());
                for (int i = 0; i < colliders.Count; i++)
                {
                    if (colliders[i].GetComponent<Unit>() != null)
                    {
                        //팀만 보호막을 얻도록 수정해야함
                        colliders[i].GetComponent<Unit>().STAT.Barrier = new Barrier(coefficient, duration, true);
                    }
                }
                player.StartCoroutine(CooltimeCoroutine(CoolTime));
            }
        }
    }
}

