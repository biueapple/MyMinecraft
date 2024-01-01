using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HungerUI : MonoBehaviour
{
    public HungerSystem hungerSystem;
    public Image[] hunger;
    // Start is called before the first frame update
    void Start()
    {
        hungerSystem.AddHunger = SetupUI;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetupUI()
    {
        //이미지는 10개있음 그래서 몇개의 이미지를 켜야하는지 count로 계산
        float count = hungerSystem.Hunger * 10 / BlockInfo.maxHunger;

        for(int i = 0 ; i < hunger.Length; i++)
        {
            //count만큼 이미지를 활성화 (setupUI를 호출해주는 hungerSystem.AddHunger는 int단위로 배고픔이 변해야 호출해주니까 보통은 (0 , 0.5f, 1) 의 값만 fillAmount에 들어감
            //하지만 최대 배고픔의 수치가 변하면 다른값도 들어갈 수 있음
            hunger[i].fillAmount = Mathf.Clamp(count, 0, 1);
            count--;
        }
    }
}
