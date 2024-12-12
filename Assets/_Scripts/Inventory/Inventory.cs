using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance; // Singleton pour un accès global

    private List<GameObject> items = new List<GameObject>(); // Liste logique des objets dans l'inventaire

    private void Awake()
    {
        // Configuration du Singleton
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
        items.Add(item);
        InventoryUI.Instance.AddToInventory(item);
        item.SetActive(false);
    }

    public void RemoveFromInventory(GameObject item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);
            item.SetActive(true);

            // Placer l'objet devant le joueur
            if (Camera.main != null)
            {
                item.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 2f;
            }
            else
            {
                Debug.LogWarning("Impossible de positionner l'objet, caméra non trouvée.");
            }
        }
    }

    public List<GameObject> GetItems()
    {
        return items;
    }
}