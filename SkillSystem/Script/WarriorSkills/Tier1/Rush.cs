using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WarriorSkills
{
    public class Rush : Skill
    {
        public override float coefficient { get { return 0; } }

        public override string detail { get { return ""; } }

        public override string expaln { get { return description; } }

        public override float CoolTime { get { return cooltime - level; } }

        public override void Init(Player player, OccupationUI @interface)
        {
            base.Init(player, @interface);
            type = SKILL_TYPE.ACTIVE;
        }

        protected override void Install()
        {
            player.SkillList += InputKey;
        }

        protected override void Uninstall()
        {
            player.SkillList -= InputKey;
        }

        //스킬 키를 누르면 발동하는 효과라는 뜻
        private void InputKey()
        {
            //키를 눌렀을때
            if (Input.GetKeyDown(_keyCode))
            {
                Impact();
            }
        }
        //스킬의 내용
        private void Impact()
        {
            if (cooltimer > 0)
                return;
            player.GetComponent<MoveSystem>().ForceMove_Distance(player.transform.forward, 5, 8 + level * 0.1f);
            player.StartCoroutine(CooltimeCoroutine(CoolTime));
        }
    }

}
