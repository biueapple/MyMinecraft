using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPUI : MonoBehaviour
{
    public ATTRIBUTES attribute;
    public Image image;
    public Text now;
    public Text max;
    public Stat stat;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        ChangeFillAmount();
    }

    public void ChangeFillAmount()
    {
        switch (attribute)
        {
            case ATTRIBUTES.HP:
                if (stat.MAXHP == 0)
                    image.fillAmount = 0;
                else
                    image.fillAmount = stat.HP / stat.MAXHP;
                now.text = stat.HP.ToString("F0");
                max.text = stat.MAXHP.ToString("F0");
                break;
            case ATTRIBUTES.MP:
                if (stat.MAXMP == 0)
                    image.fillAmount = 0;
                else
                    image.fillAmount = stat.MP / stat.MAXMP;
                now.text = stat.MP.ToString("F0");
                max.text = stat.MAXMP.ToString("F0");
                break;
            case ATTRIBUTES.ATTACKSPEED:
                if (stat.AttackSpeed == 0)
                    image.fillAmount = 0;
                else
                    image.fillAmount = 1 - stat.Attacktimer / stat.AttackSpeed;
                break;
        }
    }
}
