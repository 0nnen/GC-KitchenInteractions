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
        ObjectConfig config = DragAndDropManager.Instance.GetConfigForPrefab(item);

        if (config != null && !config.isMovable)
        {
            Debug.LogWarning($"{item.name} ne peut pas être ajouté à l'inventaire car il n'est pas déplaçable.");
            return;
        }

        // Vérifiez si l'objet est déjà dans InventoryUI
        if (InventoryUI.Instance.IsItemInUI(item))
        {
            Debug.LogWarning($"{item.name} est déjà dans l'inventaire (via l'UI).");
            return;
        }

        // Ajoutez l'objet dans la liste et l'UI
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
            InventoryUI.Instance.MoveObjectToScene(item);
            Debug.Log($"{item.name} retiré de l'inventaire.");
        }
        else
        {
            Debug.LogWarning($"{item.name} n'est pas dans l'inventaire !");
        }
    }

    public bool IsInInventory(GameObject item)
    {
        return items.Contains(item);
    }
}
