using System.Collections;
using UnityEngine;

//땅에 떨어져 있는 아이템
public class GroundItem : MonoBehaviour
{
    public ItemObject item;
    public int amount;
    public int level;
    //중력으로 아래로 떨어지는것을 구현하기 위해
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
