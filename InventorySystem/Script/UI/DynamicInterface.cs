using UnityEngine;

//시작위치와 한줄에 몇개가 들어갈지 칸들의 떨어져 있는 거리 등을 입력하면 그만큼의 슬롯을 만들어주고 사용하는 클래스
//play를 하면 슬롯을 만들고 그것을 쓰겠다는것
public class DynamicInterface : ContainerInterface
{
    //DynamicInterface 는 슬롯들을 자동으로 만들어줌
    //ui의 시작지점
    public int X_START;
    public int Y_START;
    //ui간의 얼마만큼 떨어져 있는지 한줄에 몇개가 들어가는지
    public int X_SPACE_BETWWEN_ITEM;
    public int NUMBER_OF_COLUMN;
    public int Y_SPACE_BETWWEN_ITEMS;

    public bool input;
    public bool output;

    //ui만들기
    public override void CreateSlotsUI()
    {
        for (int i = 0; i < containerObject.storage.slots.Length; i++)
        {
            containerObject.storage.slots[i] = new ContainerSlot();
            //ui만들고
            Slot_UI slot_ui = Instantiate(slot_UI_Prefab, Vector3.zero, Quaternion.identity, transform);
            slot_ui.ContainerInterface = this;
            slot_ui.name = "slot_" + i;
            slot_ui.Slot = containerObject.storage.slots[i];
            //위치정하고
            slot_ui.GetComponent<RectTransform>().localPosition = GetPosition(i);
            slot_ui.input = input;
            slot_ui.output = output;

            //저장
            slotsInterface.Add(containerObject.storage.slots[i], slot_ui);
        }
    }

    //ui포지션을 계산해줌
    private Vector3 GetPosition(int i)
    {
        return new Vector3(X_START + (X_SPACE_BETWWEN_ITEM * (i % NUMBER_OF_COLUMN)), Y_START + (-Y_SPACE_BETWWEN_ITEMS * (i / NUMBER_OF_COLUMN)), 0);
    }
}
