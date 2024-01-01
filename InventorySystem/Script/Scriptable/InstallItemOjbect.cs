using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class InstallItemOjbect : ItemObject
{
    [Header("설치되는 블록")]
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
    //우클릭의 효과 (무슨 블록을 설치할지)
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
            //그 위치에 블록을 설치
            Vector3Int position = new Vector3Int(Mathf.RoundToInt(player.placeBlock.position.x), Mathf.RoundToInt(player.placeBlock.position.y), Mathf.RoundToInt(player.placeBlock.position.z));
            player.world.InstallBlock(new BlockOrder(position, blockIndex));
            //아이템의 갯수도 1개 빼주고
            player.InventorySystem.hotkey.storage.slots[player.InventorySystem.Select].AddAmount(-1);
        }
    }
}
