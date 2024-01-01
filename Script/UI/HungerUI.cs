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
        //�̹����� 10������ �׷��� ��� �̹����� �Ѿ��ϴ��� count�� ���
        float count = hungerSystem.Hunger * 10 / BlockInfo.maxHunger;

        for(int i = 0 ; i < hunger.Length; i++)
        {
            //count��ŭ �̹����� Ȱ��ȭ (setupUI�� ȣ�����ִ� hungerSystem.AddHunger�� int������ ������� ���ؾ� ȣ�����ִϱ� ������ (0 , 0.5f, 1) �� ���� fillAmount�� ��
            //������ �ִ� ������� ��ġ�� ���ϸ� �ٸ����� �� �� ����
            hunger[i].fillAmount = Mathf.Clamp(count, 0, 1);
            count--;
        }
    }
}
