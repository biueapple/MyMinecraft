using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling instance;

    public List<GameObject> Objects = new List<GameObject>();
    //groundItem전용
    public List<GroundItem> Item = new List<GroundItem>();
    public ItemDatabaseObject data;
    public GroundItem groundItem;
    GameObject parent;

    private void Awake()
    {
        instance = this;

        Init();
    }


    public void Init()
    {
        if (parent == null)
            parent = new GameObject("오브젝트풀링");

        for (int i = 0; i < 10; i++)
        {
            Item.Add(Instantiate(groundItem, parent.transform));
            Item[Item.Count - 1].gameObject.SetActive(false);    
        }
    }

    //CreateObject 이 함수는 프리팹화된 obj를 받고 그것으로 만든 오브젝트가 있는지
    //비활성화 리스트에서 찾으면 그것을 활성화 후 비활성화 리스트에서 리무브 활성화 리스트에 추가 후 리턴
    //없다면 하나 만들고 활성화 리스트에 추가

    public GameObject CreateObject(GameObject obj)
    {
        //일단 부모객체를 만들어서 그안에 넣겠다는건데 없어도 괜찮음
        if (parent == null)
            parent = new GameObject("오브젝트풀링");
        //같은 오브젝트가 있는지 확인 GetInstanceID() 는 같은 프리팹으로 만들어 진것인지 확인 실행마다 값이 달라지기에 플레이중에만 비교가능 GetInstanceID()이게 같은 프리팹으로 만들어도 값이 다르네
        GameObject g = Objects.FirstOrDefault(otherObj => otherObj.name == obj.name && !otherObj.activeSelf);
        if (g == null)
        {
            //새 오브젝트 만들기
            GameObject go = GameObject.Instantiate(obj, parent.transform);
            go.name = obj.name;
            Objects.Add(go);
            go.gameObject.SetActive(true);
            return go;
        }
        else
        {
            //이미 있는 오브젝트 리턴
            g.transform.SetParent(parent.transform);
            g.SetActive(true);
            return g;
        }
    }

    public GameObject CreateObject(GameObject obj, Transform parent)
    {
        GameObject g = Objects.FirstOrDefault(otherObj => otherObj.name == obj.name && !otherObj.activeSelf);
        if (g == null)
        {
            GameObject go = GameObject.Instantiate(obj, parent);
            go.name = obj.name;
            Objects.Add(go);
            go.gameObject.SetActive(true);
            return go;
        }
        else
        {
            g.transform.parent.SetParent(parent);
            g.SetActive(true);
            return g;
        }
    }

    public GameObject CreateObject(GameObject obj, Transform parent, Vector3 position, Quaternion quaternion)
    {
        GameObject g = Objects.FirstOrDefault(otherObj => otherObj.name == obj.name && !otherObj.activeSelf);
        if (g == null)
        {
            GameObject go = GameObject.Instantiate(obj, position, quaternion, parent);
            go.name = obj.name;
            Objects.Add(go);
            go.gameObject.SetActive(true);
            return go;
        }
        else
        {
            g.transform.parent.SetParent(parent);
            g.transform.position = position;
            g.transform.rotation = quaternion;
            g.SetActive(true) ;
            return g;
        }
    }

    public void DestroyObject(GameObject obj)
    {
        if (!Objects.Contains(obj))
        {
            Objects.Add(obj);
        }
        obj.gameObject.SetActive(false);
    }

    //groundItem전용
    public GroundItem CreateItem()
    {
        for (int i = 0; i < Item.Count; i++)
        {
            if (!Item[i].gameObject.activeSelf)
            {
                Item[i].gameObject.SetActive(true);
                return Item[i];
            }
        }

        if (parent == null)
            parent = new GameObject("오브젝트풀링");

        Item.Add(Instantiate(groundItem, parent.transform));
        return Item[Item.Count - 1];    
    }
    public void RemoveItem(GroundItem groundItem)
    {
        groundItem.gameObject.SetActive(false);
        if (parent == null)
            parent = new GameObject("오브젝트풀링");
        groundItem.transform.parent = parent.transform;
        if(!Item.Contains(groundItem))
            Item.Add(groundItem);
    }
}
