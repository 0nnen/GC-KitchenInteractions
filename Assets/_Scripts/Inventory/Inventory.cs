using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance;

    private List<GameObject> items = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void AddToInventory(GameObject item)
    {
        if (!items.Contains(item)) // �vite les doublons
        {
            items.Add(item);
            InventoryUI.Instance.AddToInventory(item);
            item.SetActive(false);
        }
        else
        {
            Debug.LogWarning($"{item.name} est d�j� dans l'inventaire !");
        }
    }

    public void RemoveFromInventory(GameObject item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            InventoryUI.Instance.MoveObjectToScene(item);
            Debug.Log($"{item.name} retir� de l'inventaire.");
        }
        else
        {
            Debug.LogWarning($"{item.name} n'est pas dans l'inventaire !");
        }
    }

    public List<GameObject> GetInventoryItems()
    {
        return new List<GameObject>(items); // Retourne une copie pour �viter les modifications externes.
    }
}
