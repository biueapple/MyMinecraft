using UnityEngine;

namespace WarriorSkills
{
    public class FaintAttack : Skill
    {
        //지속시간
        private float duration = 2;
        private float skilltimer = 3;

        public override float coefficient { get { return 10 + level * 10 + player.STAT.MAXHP * 0.1f + player.STAT.AD * 0.3f; } }
        public override string detail { get { return "10 + level * 10 + MAXHP * 0.1f + AD * 0.3"; } }
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
            type = SKILL_TYPE.ACTIVE;
        }

        protected override void Install()
        {
            player.SkillList += InputKey;
        }

        protected override void Uninstall()
        {
            player.SkillList -= InputKey;
            //스킬이 없어졌는데(level이 0이되서 없어짐) 이미 효과는 발동중일 수 있음
            //그런경우 발동중인 효과도 없애기
            if (player.STAT.AlreadyNomalBefore(Impact))
            {
                player.STAT.RemoveNomalAttackBefore(Impact);
            }
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
                if (!player.STAT.AlreadyNomalBefore(Impact))
                {
                    player.STAT.AddNomalAttackBefore(Impact);
                }
                if (coroutine != null)
                    StopCoroutine(coroutine);
                coroutine = StartCoroutine(skillTimer(skilltimer, RemoveSkill));
                StartCoroutine(CooltimeCoroutine(CoolTime));
            }
        }
        //스킬의 내용
        private float Impact(Stat per, Stat victim, float figure)
        {
            figure += coefficient;
            //스킬 효과가 발동됬으니까 없애기
            player.STAT.RemoveNomalAttackBefore(Impact);
            if(victim.GetComponent<MoveSystem>() != null)
            {
                victim.GetComponent<MoveSystem>().Faint(duration);
            }
            //스킬을 한번 사용했을때 지속시간이 영원하진 않으니 타이머
            StopCoroutine(coroutine);
            coroutine = null;
            return figure;
        }

        //타이머 지났으면 사용한 효과 없애기
        private void RemoveSkill()
        {
            player.STAT.RemoveNomalAttackBefore(Impact);
        }
    }

}
