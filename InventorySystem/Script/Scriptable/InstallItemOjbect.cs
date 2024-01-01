using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class InstallItemOjbect : ItemObject
{
    [Header("��ġ�Ǵ� ���")]
    public _BLOCK blockIndex;

    public override ItemInterface CreateItem(int level)
    {
        InstallItem newItem = new InstallItem(this);
        return newItem;
    }
}

[System.Serializable]
public class InstallItem : Item
{
    //��Ŭ���� ȿ�� (���� ����� ��ġ����)
    public _BLOCK blockIndex;

    public InstallItem(InstallItemOjbect itemObject) : base(itemObject)
    {
        RatingDecision(itemObject);
        blockIndex = itemObject.blockIndex;
    }

    public override void RightMouseDown(Player player)
    {
        if (player.placeBlock.gameObject.activeSelf)
        {
            //�� ��ġ�� ����� ��ġ
            Vector3Int position = new Vector3Int(Mathf.RoundToInt(player.placeBlock.position.x), Mathf.RoundToInt(player.placeBlock.position.y), Mathf.RoundToInt(player.placeBlock.position.z));
            player.world.InstallBlock(new BlockOrder(position, blockIndex));
            //�������� ������ 1�� ���ְ�
            player.InventorySystem.hotkey.storage.slots[player.InventorySystem.Select].AddAmount(-1);
        }
    }
}
