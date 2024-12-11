using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance; // Singleton pour un accès global

    private List<GameObject> items = new List<GameObject>(); // Liste logique des objets dans l'inventaire

    void Awake()
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

    /// <summary>
    /// Ajoute un objet à l'inventaire logique et visuel.
    /// </summary>
    /// <param name="item">L'objet à ajouter</param>
    public void AddToInventory(GameObject item)
    {
        // Ajout à la liste logique
        items.Add(item);

        // Ajout visuel dans l'UI d'inventaire
        InventoryUI.Instance.AddToInventory(item);

        // Désactive l'objet dans la scène
        item.SetActive(false);
    }

    /// <summary>
    /// Retourne la liste des objets dans l'inventaire.
    /// </summary>
    public List<GameObject> GetItems()
    {
        return items;
    }

    /// <summary>
    /// Retire un objet de l'inventaire et le réactive dans la scène.
    /// </summary>
    /// <param name="item">L'objet à retirer</param>
    public void RemoveFromInventory(GameObject item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            // Réactive l'objet dans la scène
            item.SetActive(true);
        }
    }
}
