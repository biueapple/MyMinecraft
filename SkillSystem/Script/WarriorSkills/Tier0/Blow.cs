using System;
using System.Collections;
using UnityEngine;

namespace WarriorSkills
{
    [System.Serializable]
    public class Blow : Skill
    {
        public override float coefficient { get { return 10 + level * 3 + player.STAT.AD * 0.6f; } }
        public override string detail { get { return "10 + level * 3 + AD * 0.6"; } }
        public override string expaln { get { return description.Replace("?", coefficient.ToString()); } }
        public override float CoolTime { get { return cooltime - level; } }
        private float skilltimer = 3;

        public override void Init(Player player, OccupationUI @interface)
        {
            base.Init(player, @interface);
            type = SKILL_TYPE.ACTIVE;
        }

        public override void Updating()
        {
            base.Updating();
        }

        //스킬 적용시긴다는 뜻
        protected override void Install()
        {
            //SkillList 에 Impact를 넣는다는 매 호출하면서 키를 누를 경우에 발동한다는 뜻
            //중복체크를 하는 부분이였는데 없애기로 함
            //if (!player.IsActionAlreadyRegistered(InputKey))
            //{
                player.SkillList += InputKey;
            //}
        }
        //스킬 뺀다는 뜻
        protected override void Uninstall()
        {
            //중복체크를 하는 부분이였는데 없애기로 함
            //if (player.IsActionAlreadyRegistered(InputKey))
            //{
                player.SkillList -= InputKey;
                //스킬이 없어졌는데(level이 0이되서 없어짐) 이미 효과는 발동중일 수 있음
                //그런경우 발동중인 효과도 없애기
                if (player.STAT.AlreadyNomalBefore(Impact))
                {
                    player.STAT.RemoveNomalAttackBefore(Impact);
                }
            //}
        }
        //스킬 키를 누르면 발동하는 효과라는 뜻
        private void InputKey()
        {
            //키를 눌렀을때
            if (Input.GetKeyDown(_keyCode))
            {
                if (cooltimer > 0)
                    return;
                //기본공격이 강화되는 스킬인데 이미 강화중일때 또 강화하면 안돼기에 중복체크를 해야함
                if(!player.STAT.AlreadyNomalBefore(Impact))
                {
                    player.STAT.AddNomalAttackBefore(Impact);
                }
                if (coroutine != null)
                    player.StopCoroutine(coroutine);
                coroutine = player.StartCoroutine(skillTimer(skilltimer, RemoveSkill));
            }
        }
        //스킬의 내용
        private float Impact(Stat per, Stat victim, float figure)
        {
            figure += coefficient;
            //스킬 효과가 발동됬으니까 없애기
            player.STAT.RemoveNomalAttackBefore(Impact);
            //스킬을 한번 사용했을때 지속시간이 영원하진 않으니 타이머
            player.StopCoroutine(coroutine);
            coroutine = null;
            cooltimer = CoolTime;
            player.StartCoroutine(CooltimeCoroutine(CoolTime));
            return figure;
        }

        //타이머 지났으면 사용한 효과 없애기
        private void RemoveSkill()
        {
            player.STAT.RemoveNomalAttackBefore(Impact);
        }

        
    }
}
