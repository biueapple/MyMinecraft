using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageUI : MonoBehaviour
{
    public Camera cam;
    public List<Stat> statList;
    public Text text;
    Color ad = new Color(1, 0.4980392f, 0); //orange
    Color ap = Color.blue;
    Color tr = Color.white;
    Color he = Color.green;
    Color ba = Color.cyan;
    //같은 유닛에게 띄우는 대미지가 겹치지 않도록
    List<Stat> stats = new List<Stat>();


    ////color값
    //private Color red = new Color(0.5660378f, 0, 0);
    //private Color orange = new Color(0.7647059f, 0.4909484f, 0.2f);
    //private Color green = new Color(0, 0.5647059f, 0);
    //private Color blue = new Color(0, 0.2810156f, 1);

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < statList.Count; i++)
        {
            statList[i].DamageTextingUI(ViewDamage);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ViewDamage(Stat victim, float figure, DAMAGETYPE damageType, bool barrier)
    {
        //월드위치를 cam의 스크린 위치로 변환 후에 text 표시
        if (figure > 0)
        {
            Text t = ObjectPooling.instance.CreateObject(text.gameObject, transform).GetComponent<Text>();
            switch (damageType)
            {
                case DAMAGETYPE.AD:
                    t.color = ad;
                    break;
                case DAMAGETYPE.AP:
                    t.color = ap;
                    break;
                case DAMAGETYPE.TRUE:
                    t.color = tr;
                    break;
                case DAMAGETYPE.HEAL:
                    t.color = he;
                    break;
            }

            Outline[] line = t.GetComponents<Outline>();
            if (barrier)
            {
                for (int i = 0; i < line.Length; i++)
                    line[i].effectColor = ba;
            }
            else
            {
                for (int i = 0; i < line.Length; i++)
                    line[i].effectColor = Color.black;
            }
            Vector3 worldPosition = new Vector3(0, victim.GetComponent<MoveSystem>().unitHeight * 0.2f, 0);
            Vector3 screenPosition = new Vector3();
            for(int i=0; i <stats.Count; i++)
            {
                if (stats[i] == victim)
                {
                    screenPosition += new Vector3(0,25f,0);
                }
            }
            t.transform.position = cam.WorldToScreenPoint(victim.transform.position + new Vector3(0, victim.GetComponent<MoveSystem>().unitHeight, 0) + worldPosition) + screenPosition;
            stats.Add(victim);
            StartCoroutine(TextingDelete(1, t.gameObject, victim, worldPosition, screenPosition));
            t.text = figure.ToString();
        }
    }

    private IEnumerator TextingDelete(float timer, GameObject text, Stat stat, Vector3 worldPosition, Vector3 screenPosition)
    {
        float t = 0;
        while(t < timer)
        {
            t += Time.deltaTime;
            text.transform.position = cam.WorldToScreenPoint(stat.transform.position + new Vector3(0, stat.GetComponent<MoveSystem>().unitHeight, 0) + worldPosition) + screenPosition;
            yield return null;
        }
        ObjectPooling.instance.DestroyObject(text);
        stats.Remove(stat);
    }
}
