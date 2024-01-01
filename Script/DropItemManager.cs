using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItemManager : MonoBehaviour
{
    public static DropItemManager instance;
    public World world;
    public GroundItem itemObject;
    public ItemDatabaseObject itemDatabase;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GroundItem DropItem(int id, Vector3 position)
    {
        //-0.45는 아이템의 크기
        GroundItem item = ObjectPooling.instance.CreateItem();
        //Instantiate(itemObject, position + new Vector3(0, -0.45f,0), Quaternion.identity);
        item.transform.position = position + new Vector3(0, -0.45f, 0);
        item.Init(itemDatabase.Data[id], world);
        item.amount = 1;
        return item;
    }

    public void Thorw(int id, Vector3 position, int amount)
    {
        GroundItem item = DropItem(id, position);
        item.amount = amount;
        item.GetComponent<MoveSystem>().ForceMove_Distance(Vector3.forward, 3, 0);
        item.GetComponent<MoveSystem>().isGrounded = false;
        StartCoroutine(Falling(item));
    }

    //아이템을 스폰하면 아래로 떨어져야 하니까
    private IEnumerator Falling(GroundItem item)
    {
        float timer = 0;
        item.GetComponent<Collider>().enabled = false;
        while(true)
        {
            timer += Time.deltaTime;
            yield return null;
            if(timer > 1)
            {
                item.GetComponent<Collider>().enabled = true;
            }
            if(item.GetComponent<MoveSystem>().isGrounded)
            {
                item.GetComponent<MoveSystem>().Force_Invalidation();
                if(!item.GetComponent<Collider>().enabled)
                    item.GetComponent<Collider>().enabled = true;
                break;
            }
        }
    }

    //둥둥 떠있는 느낌
    private IEnumerator Floating()
    {
        while (gameObject.activeSelf)
        {


            yield return null;
        }
    }
}
