using System.Collections;
using UnityEngine;

//���� ������ �ִ� ������
public class GroundItem : MonoBehaviour
{
    public ItemObject item;
    public int amount;
    public int level;
    //�߷����� �Ʒ��� �������°��� �����ϱ� ����
    private MoveSystem moveSystem;
    public Vector3Int Position { get { return new Vector3Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y), Mathf.RoundToInt(transform.position.z)); } }

    private void Start()
    {
       
    }

    public void Init(ItemObject item, World world)
    {
        this.item = item;
        
        moveSystem = GetComponent<MoveSystem>();
        moveSystem.world = world;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (item != null)
        {
            if (collision.transform.GetComponent<PlayerInventorySystem>() != null)
            {
                collision.transform.GetComponent<PlayerInventorySystem>().inventory.Acquired(item.CreateItem(level), amount);
                ObjectPooling.instance.RemoveItem(this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (item != null)
        {
            if (other.GetComponent<PlayerInventorySystem>() != null)
            {
                other.GetComponent<PlayerInventorySystem>().inventory.Acquired(item.CreateItem(level), amount);
                ObjectPooling.instance.RemoveItem(this);
            }
        }
    }
}
