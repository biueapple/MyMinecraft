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

        //��ų ����ñ�ٴ� ��
        protected override void Install()
        {
            //SkillList �� Impact�� �ִ´ٴ� �� ȣ���ϸ鼭 Ű�� ���� ��쿡 �ߵ��Ѵٴ� ��
            //�ߺ�üũ�� �ϴ� �κ��̿��µ� ���ֱ�� ��
            //if (!player.IsActionAlreadyRegistered(InputKey))
            //{
                player.SkillList += InputKey;
            //}
        }
        //��ų ���ٴ� ��
        protected override void Uninstall()
        {
            //�ߺ�üũ�� �ϴ� �κ��̿��µ� ���ֱ�� ��
            //if (player.IsActionAlreadyRegistered(InputKey))
            //{
                player.SkillList -= InputKey;
                //��ų�� �������µ�(level�� 0�̵Ǽ� ������) �̹� ȿ���� �ߵ����� �� ����
                //�׷���� �ߵ����� ȿ���� ���ֱ�
                if (player.STAT.AlreadyNomalBefore(Impact))
                {
                    player.STAT.RemoveNomalAttackBefore(Impact);
                }
            //}
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
                if(!player.STAT.AlreadyNomalBefore(Impact))
                {
                    player.STAT.AddNomalAttackBefore(Impact);
                }
                if (coroutine != null)
                    player.StopCoroutine(coroutine);
                coroutine = player.StartCoroutine(skillTimer(skilltimer, RemoveSkill));
            }
        }
        //��ų�� ����
        private float Impact(Stat per, Stat victim, float figure)
        {
            figure += coefficient;
            //��ų ȿ���� �ߵ������ϱ� ���ֱ�
            player.STAT.RemoveNomalAttackBefore(Impact);
            //��ų�� �ѹ� ��������� ���ӽð��� �������� ������ Ÿ�̸�
            player.StopCoroutine(coroutine);
            coroutine = null;
            cooltimer = CoolTime;
            player.StartCoroutine(CooltimeCoroutine(CoolTime));
            return figure;
        }

        //Ÿ�̸� �������� ����� ȿ�� ���ֱ�
        private void RemoveSkill()
        {
            player.STAT.RemoveNomalAttackBefore(Impact);
        }

        
    }
}
