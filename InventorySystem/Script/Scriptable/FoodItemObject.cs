using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FoodItemObject : ItemObject
{
    //먹는데 걸리는 시간
    public float eatingTimer = 1;
    //먹었을때 배부름이 차는 정도
    public float full = 1;
    [Header("SoundManager의 clips의 인덱스")]
    //먹을때 나는 소리
    public int soundIndex;

    public override ItemInterface CreateItem(int level)
    {
        FoodItem newItem = new FoodItem(this);
        return newItem;
    }
}

[System.Serializable]
public class FoodItem : Item
{
    public float eatingTimer = 1;
    public float full = 1;
    public int soundIndex;

    public FoodItem(FoodItemObject itemObject) : base(itemObject)
    {
        RatingDecision(itemObject);
        eatingTimer = itemObject.eatingTimer;
        full = itemObject.full;
        soundIndex = itemObject.soundIndex;
    }

    public override void RightMouseDown(Player player)
    {
        player.EatingStart(eatingTimer, full, soundIndex);
    }
    public override void RightMouseUp(Player player)
    {
        player.EatingStop();
    }
}
