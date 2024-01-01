using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponItemObject : EquipItemObject
{
    public int minDamage;
    public int maxDamage;
    [Header("레벨당 증가할 대미지")]
    public int levelDamage;
    public Vector3 size;
    public override ItemInterface CreateItem(int level)
    {
        WeaponItem newItem = new WeaponItem(this, level);
        return newItem;
    }
}

[System.Serializable]
public class WeaponItem : EquipItem
{
    public DAMAGETYPE type;
    public int damage;
    public float x;
    public float y;
    public float z;
    public Vector3 Size { get { return new Vector3(x, y, z); } }
    public WeaponItem(WeaponItemObject itemObject, int level) : base(itemObject, level)
    {
        type = (DAMAGETYPE)Random.Range(0, 2);
        damage = Random.Range(itemObject.minDamage + level * itemObject.levelDamage, itemObject.maxDamage + level * itemObject.levelDamage);
        x = itemObject.size.x;
        y = itemObject.size.y;
        z = itemObject.size.z;
    }
    public override void LeftMouseDown(Player player)
    {
        List<Collider> list = new List<Collider>();
        //RBI.position + cam.forward * range.z * 0.5f
        list.AddRange(Physics.OverlapBox(player.RBI.position + player.cam.transform.forward * z, Size * 0.5f, Quaternion.Euler(player.transform.eulerAngles), LayerMask.GetMask("Unit")));
        list.Remove(player.GetComponent<Collider>());
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].GetComponent<Unit>() != null)
            {
                list[i].GetComponent<Unit>().Hit(player.STAT, player.STAT.AD, ATTACKTYPE.NOMAL, DAMAGETYPE.AD);
                player.STAT.Attacktimer = 0;
                WeaponDamage(player.STAT, list[i].GetComponent<Unit>().STAT, damage);
                return;
            }
        }
    }
    public override void Equip(Stat stat)
    {
        base.Equip(stat);
        //stat.AddNomalAttackAfter(WeaponDamage);
    }
    public override void Release(Stat stat)
    {
        base.Release(stat);
        //stat.RemoveNomalAttackAfter(WeaponDamage);
    }
    public void WeaponDamage(Stat perpetrator, Stat victim, float figure)
    {
        victim.Be_Attacked(perpetrator, figure, ATTACKTYPE.NONE, type);
    }
}
