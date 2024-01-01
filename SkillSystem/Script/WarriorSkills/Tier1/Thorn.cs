
namespace WarriorSkills
{
    public class Thorn : Skill
    {
        public override float coefficient { get { return 10 + level * 5 + player.STAT.AP * 0.6f; } }
        public override string detail { get { return "10 + level * 5 + AP * 0.6f"; } }
        public override string expaln { get { return description.Replace("?", coefficient.ToString()); } }
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
            player.STAT.AddHitNomalAfter(Impact);
        }
        protected override void Uninstall()
        {
            player.STAT.RemoveHitNomalAfter(Impact);
        }

        //스킬의 내용
        private void Impact(Stat per, Stat victim, float figure)
        {
            per.Be_Attacked(player.STAT, coefficient, ATTACKTYPE.NONE, DAMAGETYPE.AD);
        }
    }
}

