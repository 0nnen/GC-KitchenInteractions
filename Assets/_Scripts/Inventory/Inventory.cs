using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    private List<GameObject> items = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void AddToInventory(GameObject item)
    {
        // Rechercher le ObjectConfig correspondant à l'item dans la liste
        ObjectConfig config = DragAndDropManager.Instance.GetConfigForPrefab(item);

        if (config != null && !config.isMovable)
        {
            Debug.LogWarning($"{item.name} ne peut pas être ajouté à l'inventaire car il n'est pas déplaçable.");
            return;
        }

        if (!items.Contains(item))
        {
            items.Add(item);
            InventoryUI.Instance.AddToInventory(item);
            Debug.Log($"{item.name} ajouté à l'inventaire.");
        }
        else
        {
            Debug.LogWarning($"{item.name} est déjà dans l'inventaire !");
        }
    }



public void RemoveFromInventory(GameObject item)
{
    if (items.Contains(item))
    {
        items.Remove(item);
        InventoryUI.Instance.MoveObjectToScene(item); // Centralisez la logique ici
        Debug.Log($"{item.name} retiré de l'inventaire.");
    }
    else
    {
        Debug.LogWarning($"{item.name} n'est pas dans l'inventaire !");
    }
}

}
