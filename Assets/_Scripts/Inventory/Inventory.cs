using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public static Inventory Instance; // Singleton pour un acc�s global

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
    /// Ajoute un objet � l'inventaire logique et visuel.
    /// </summary>
    /// <param name="item">L'objet � ajouter</param>
    public void AddToInventory(GameObject item)
    {
        // Ajout � la liste logique
        items.Add(item);

        // Ajout visuel dans l'UI d'inventaire
        InventoryUI.Instance.AddToInventory(item);

        // D�sactive l'objet dans la sc�ne
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
    /// Retire un objet de l'inventaire et le r�active dans la sc�ne.
    /// </summary>
    /// <param name="item">L'objet � retirer</param>
    public void RemoveFromInventory(GameObject item)
    {
        if (items.Contains(item))
        {
            items.Remove(item);

            // R�active l'objet dans la sc�ne
            item.SetActive(true);
        }
    }
}
