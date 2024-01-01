using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SKILLKIND
{
    WARRIOR,

}

public class SkillKind : MonoBehaviour
{
    public OccupationUI[] SkillInterfaces = null;

    public bool Active
    {
        get
        {
            if(gameObject.activeSelf)
            {
                return true;
            }
            else
            {
                for(int i = 0; i < SkillInterfaces.Length; i++)
                    if (SkillInterfaces[i].gameObject.activeSelf)
                        return true;
            }
            return false;
        }
        set
        {
            for(int i = 0; i < SkillInterfaces.Length;i++)
            {
                if (SkillInterfaces[i].gameObject.activeSelf)
                {
                    SkillInterfaces[i].gameObject.SetActive(false);
                    return;
                }
            }
            if(gameObject.activeSelf)
            {
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnWarrior_Interface()
    {
        for (int i = 0; i < SkillInterfaces.Length; i++)
        {
            if (SkillInterfaces[i].kind == SKILLKIND.WARRIOR)
            {
                SkillInterfaces[i].Init();
                SkillInterfaces[i].gameObject.SetActive(true);
                gameObject.SetActive(false);
                break;
            }
        }
    }
}
