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
        if (!items.Contains(item))
        {
            items.Add(item);
            InventoryUI.Instance.AddToInventory(item);
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
        }
        else
        {
            Debug.LogWarning($"{item.name} n'est pas dans l'inventaire !");
        }
    }
}
