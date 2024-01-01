using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CraftingBox : MonoBehaviour
{
    private ContainerInterface containerInterface;
    private ContainerObject containerObject;
    public Crafting craftingManager;
    public ItemDatabaseObject itemDatabaseObject;
    // Start is called before the first frame update
    void Start()
    {
        containerObject = new ContainerObject(10, null);
        containerInterface = GetComponent<ContainerInterface>();
        containerInterface.Init(null, containerObject);
        for (int i = 0; i < 9; i++)
        {
            containerObject.storage.slots[i].afterCallback += CraftingCheck;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CraftingCheck(ContainerSlot slot)
    { 
        Table matrix = new Table3X3();
        int[,] ints = new int[3, 3]
        {
            { containerObject.storage.slots[0].GetItem.ID ,containerObject.storage.slots[1].GetItem.ID,containerObject.storage.slots[2].GetItem.ID },
            { containerObject.storage.slots[3].GetItem.ID,containerObject.storage.slots[4].GetItem.ID,containerObject.storage.slots[5].GetItem.ID },
            { containerObject.storage.slots[6].GetItem.ID,containerObject.storage.slots[7].GetItem.ID,containerObject.storage.slots[8].GetItem.ID }
        };
        matrix.codes = ints;
        matrix.Slice(matrix, out matrix);
        int id;
        int count;
        craftingManager.Combination(matrix, out id, out count);

        if(itemDatabaseObject.GetItemObjectWithId(id) != null)
        {
            containerObject.storage.slots[9].UpdateSlot(itemDatabaseObject.GetItemObjectWithId(id).data, count);
            containerObject.storage.slots[9].beforeCallback += ResultOutput;
        }
        else if(itemDatabaseObject.GetItemObjectWithId(id) == null && containerObject.storage.slots[9].GetItem.ID != -1)
        {
            containerObject.storage.slots[9].beforeCallback -= ResultOutput;
            containerObject.storage.slots[9].UpdateSlot(null, 0);
        }
    }

    public void ResultOutput(ContainerSlot slot)
    {
        for (int i = 0; i < 9; i++)
        {
            if(containerObject.storage.slots[i].GetItem.ID != -1)
                containerObject.storage.slots[i].AddAmount(-1);
        }
    }
}
