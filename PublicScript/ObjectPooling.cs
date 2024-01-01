using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    public static ObjectPooling instance;

    public List<GameObject> Objects = new List<GameObject>();
    //groundItem����
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
            parent = new GameObject("������ƮǮ��");

        for (int i = 0; i < 10; i++)
        {
            Item.Add(Instantiate(groundItem, parent.transform));
            Item[Item.Count - 1].gameObject.SetActive(false);    
        }
    }

    //CreateObject �� �Լ��� ������ȭ�� obj�� �ް� �װ����� ���� ������Ʈ�� �ִ���
    //��Ȱ��ȭ ����Ʈ���� ã���� �װ��� Ȱ��ȭ �� ��Ȱ��ȭ ����Ʈ���� ������ Ȱ��ȭ ����Ʈ�� �߰� �� ����
    //���ٸ� �ϳ� ����� Ȱ��ȭ ����Ʈ�� �߰�

    public GameObject CreateObject(GameObject obj)
    {
        //�ϴ� �θ�ü�� ���� �׾ȿ� �ְڴٴ°ǵ� ��� ������
        if (parent == null)
            parent = new GameObject("������ƮǮ��");
        //���� ������Ʈ�� �ִ��� Ȯ�� GetInstanceID() �� ���� ���������� ����� �������� Ȯ�� ���ึ�� ���� �޶����⿡ �÷����߿��� �񱳰��� GetInstanceID()�̰� ���� ���������� ���� ���� �ٸ���
        GameObject g = Objects.FirstOrDefault(otherObj => otherObj.name == obj.name && !otherObj.activeSelf);
        if (g == null)
        {
            //�� ������Ʈ �����
            GameObject go = GameObject.Instantiate(obj, parent.transform);
            go.name = obj.name;
            Objects.Add(go);
            go.gameObject.SetActive(true);
            return go;
        }
        else
        {
            //�̹� �ִ� ������Ʈ ����
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

    //groundItem����
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
            parent = new GameObject("������ƮǮ��");

        Item.Add(Instantiate(groundItem, parent.transform));
        return Item[Item.Count - 1];    
    }
    public void RemoveItem(GroundItem groundItem)
    {
        groundItem.gameObject.SetActive(false);
        if (parent == null)
            parent = new GameObject("������ƮǮ��");
        groundItem.transform.parent = parent.transform;
        if(!Item.Contains(groundItem))
            Item.Add(groundItem);
    }
}
