using UnityEngine;

namespace WarriorSkills
{
    public class FaintAttack : Skill
    {
        //���ӽð�
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
            //��ų�� �������µ�(level�� 0�̵Ǽ� ������) �̹� ȿ���� �ߵ����� �� ����
            //�׷���� �ߵ����� ȿ���� ���ֱ�
            if (player.STAT.AlreadyNomalBefore(Impact))
            {
                player.STAT.RemoveNomalAttackBefore(Impact);
            }
        }

        //��ų Ű�� ������ �ߵ��ϴ� ȿ����� ��
        private void InputKey()
        {
            //Ű�� ��������
            if (Input.GetKeyDown(_keyCode))
            {
                if (cooltimer > 0)
                    return;
                //�⺻������ ��ȭ�Ǵ� ��ų�ε� �̹� ��ȭ���϶� �� ��ȭ�ϸ� �ȵű⿡ �ߺ�üũ�� �ؾ���
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
        //��ų�� ����
        private float Impact(Stat per, Stat victim, float figure)
        {
            figure += coefficient;
            //��ų ȿ���� �ߵ������ϱ� ���ֱ�
            player.STAT.RemoveNomalAttackBefore(Impact);
            if(victim.GetComponent<MoveSystem>() != null)
            {
                victim.GetComponent<MoveSystem>().Faint(duration);
            }
            //��ų�� �ѹ� ��������� ���ӽð��� �������� ������ Ÿ�̸�
            StopCoroutine(coroutine);
            coroutine = null;
            return figure;
        }

        //Ÿ�̸� �������� ����� ȿ�� ���ֱ�
        private void RemoveSkill()
        {
            player.STAT.RemoveNomalAttackBefore(Impact);
        }
    }

}
