using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Linq;

public class FridgeUIManager : MonoBehaviour
{
    public static FridgeUIManager Instance;

    [Header("UI Références")]
    [SerializeField] private GameObject fridgeUI;           // Canvas de l'interface du frigo
    [SerializeField] private Transform fridgeGrid;          // Grille des slots du frigo
    [SerializeField] private GameObject fridgeSlotPrefab;   // Prefab pour un slot du frigo
    [SerializeField] private RectTransform fridgeArea;      // Zone du frigo
    public RectTransform FridgeArea => fridgeArea;

    [Header("Paramètres")]
    [SerializeField] private int maxItemsPerTab = 9;        // Nombre maximum d'objets par onglet

    [Header("Boutons d'Onglets")]
    [SerializeField] private Transform tabsButtonContainer; // Container pour les boutons d'onglets
    [SerializeField] private Button tabButtonPrefab;        // Prefab pour les boutons d'onglets
    [SerializeField] private Sprite normalSprite;           // Sprite par défaut du bouton
    [SerializeField] private Sprite activeSprite;           // Sprite pour le bouton actif

    [Header("UI Détails")]
    [SerializeField] private Image detailImage;             // Image affichée pour l'ingrédient
    [SerializeField] private TMP_Text titleText;            // Texte pour le titre
    [SerializeField] private TMP_Text descriptionText;      // Texte pour la description

    [Header("Error Message")]
    [SerializeField] private TMP_Text errorText;            // Texte d'erreur affiché
    [SerializeField] private CanvasGroup errorCanvasGroup;  // CanvasGroup pour gérer l'opacité
    [SerializeField] private float fadeDuration = 0.5f;     // Durée de l'animation (en secondes)
    [SerializeField] private float displayTime = 2f;        // Temps d'affichage du texte

    private List<List<GameObject>> tabs = new List<List<GameObject>>(); // Groupes d'objets par onglet
    private int activeTabIndex = 0;                         // Index de l'onglet actif
    private Dictionary<GameObject, GameObject> fridgeItemSlotMapping = new Dictionary<GameObject, GameObject>();
    private List<Button> tabButtons = new List<Button>();   // Liste des boutons pour gérer leur état
    private Vector2 basePosition;                           // Position de base du premier message
    private List<TMP_Text> activeErrorMessages = new List<TMP_Text>(); // Liste des messages actifs
    private bool isErrorMessageDisplayed = false;


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

        if (fridgeArea == null)
            Debug.LogError("La zone du frigo (fridgeArea) n'est pas assignée !");
        if (tabsButtonContainer == null)
            Debug.LogError("Le container des boutons d'onglets (tabsButtonContainer) n'est pas assigné !");

        CreateNewTab(); // Crée le premier onglet par défaut
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            TryHandleItemClick();
        }
    }

    public bool IsFridgeOpen()
    {
        return fridgeUI.activeSelf;
    }

    public void AddToFridge(GameObject item)
    {
        if (fridgeItemSlotMapping.ContainsKey(item))
        {
            Debug.LogWarning($"{item.name} est déjà dans le frigo !");
            return;
        }

        if (fridgeSlotPrefab == null || fridgeGrid == null)
        {
            Debug.LogError("FridgeUIManager : fridgeSlotPrefab ou fridgeGrid n'est pas assigné !");
            return;
        }

        // Récupérer la configuration de l'objet
        ObjectConfig config = DragAndDropManager.Instance.GetConfigForPrefab(item);
        if (config == null || config.ingredientData == null)
        {
            Debug.LogError($"AddToFridge : Aucun IngredientData trouvé pour {item.name} !");
            return;
        }

        // Rediriger automatiquement vers le prochain onglet disponible
        for (int i = activeTabIndex; i < tabs.Count; i++)
        {
            if (tabs[i].Count < maxItemsPerTab)
            {
                AddItemToTab(item, i);
                return;
            }
        }

        // Si aucun onglet n'a de place, créer un nouvel onglet
        if (tabs.Count < 3)
        {
            CreateNewTab();
            AddItemToTab(item, tabs.Count - 1);
            return;
        }

        // Si tous les onglets sont pleins, afficher une erreur
        ShowErrorMessage("Tous les onglets sont pleins.");
        Debug.LogWarning($"Impossible d'ajouter {item.name} : tous les onglets sont pleins.");
    }

    private void AddItemToTab(GameObject item, int tabIndex)
    {
        // Supprimer l'objet de l'inventaire
        Inventory.Instance.RemoveFromInventory(item);

        // Créer un slot pour l'objet
        GameObject slot = Instantiate(fridgeSlotPrefab);
        fridgeItemSlotMapping[item] = slot;

        // Configurer le slot avec les données du ScriptableObject
        ObjectConfig config = DragAndDropManager.Instance.GetConfigForPrefab(item);
        if (config != null && config.ingredientData != null)
        {
            SetupSlotWithIngredientData(slot, config.ingredientData);
            AddHoverHandlers(slot, config.ingredientData);
        }

        // Ajouter l'objet à l'onglet spécifié
        tabs[tabIndex].Add(slot);

        // Rafraîchir uniquement si l'onglet actif est mis à jour
        if (tabIndex == activeTabIndex)
        {
            RefreshTabDisplay();
        }

        Debug.Log($"{item.name} ajouté au frigo dans l'onglet {tabIndex + 1}.");
        item.SetActive(false); // Désactiver l'objet dans la scène
    }

    private void CreateNewTab()
    {
        UpdateTabButtonAppearance();
        if (tabs.Count >= 3)
        {
            Debug.LogWarning("Nombre maximum d'onglets atteint.");
            return;
        }

        // Créer un nouvel onglet vide
        List<GameObject> newTab = new List<GameObject>();
        tabs.Add(newTab);

        // Créer un bouton pour l'onglet
        int newTabIndex = tabs.Count - 1;
        Button newTabButton = Instantiate(tabButtonPrefab, tabsButtonContainer);
        newTabButton.GetComponentInChildren<TMP_Text>().text = $"Tab {newTabIndex + 1}";
        newTabButton.onClick.AddListener(() => SwitchTab(newTabIndex));
        tabButtons.Add(newTabButton);

        // Mettre à jour les boutons d'onglet
        UpdateTabButtonAppearance();
        Debug.Log($"Nouvel onglet {newTabIndex + 1} créé.");
    }

    private void SwitchTab(int tabIndex)
    {
        activeTabIndex = tabIndex;

        // Rafraîchir l'affichage des objets et les boutons
        RefreshTabDisplay();
        UpdateTabButtonAppearance();
    }

    private void RefreshTabDisplay()
    {
        // Masquer tous les slots
        foreach (var tab in tabs)
        {
            foreach (var slot in tab)
            {
                slot.SetActive(false);
            }
        }

        // Afficher les slots de l'onglet actif
        foreach (var slot in tabs[activeTabIndex])
        {
            slot.SetActive(true);
            slot.transform.SetParent(fridgeGrid); // Replacer les slots dans la grille active
        }
    }

    private void UpdateTabButtonAppearance()
    {
        for (int i = 0; i < tabButtons.Count; i++)
        {
            Button button = tabButtons[i];
            Image buttonImage = button.GetComponent<Image>();
            if (buttonImage != null)
            {
                buttonImage.sprite = (i == activeTabIndex) ? activeSprite : normalSprite;
            }
        }
    }

    public void RemoveFromFridge(GameObject item)
    {
        if (fridgeItemSlotMapping.TryGetValue(item, out GameObject slot))
        {
            if (slot != null)
            {
                // Supprimer le slot associé
                fridgeItemSlotMapping.Remove(item);

                // Trouver l'onglet contenant le slot
                for (int i = 0; i < tabs.Count; i++)
                {
                    if (tabs[i].Remove(slot))
                    {
                        slot.SetActive(false); // Supprimer visuellement le slot
                        break;
                    }
                }

                // Rééquilibrer les onglets après la suppression
                BalanceTabs();

                // Mettre à jour l'inventaire
                Inventory.Instance.AddToInventory(item);
                Debug.Log($"{item.name} retiré du frigo et ajouté à l'inventaire.");
            }
        }
        else
        {
            Debug.LogWarning($"{item.name} n'est pas dans le frigo !");
        }
        RefreshTabDisplay();
    }

    private void BalanceTabs()
    {
        // Réorganiser les objets entre les onglets
        for (int i = 0; i < tabs.Count - 1; i++)
        {
            while (tabs[i].Count < maxItemsPerTab && tabs[i + 1].Count > 0)
            {
                var itemToMove = tabs[i + 1][0];
                tabs[i + 1].RemoveAt(0);
                tabs[i].Add(itemToMove);
            }
        }

        // Supprimer les onglets vides
        for (int i = tabs.Count - 1; i >= 0; i--)
        {
            if (tabs[i].Count == 0 && i > 0) // Ne pas supprimer le premier onglet
            {
                Destroy(tabButtons[i].gameObject); // Supprimer le bouton de l'onglet
                tabButtons.RemoveAt(i);
                tabs.RemoveAt(i);
            }
        }

        // Mettre à jour les boutons d'onglet
        UpdateTabButtonAppearance();
    }

    private void SetupSlotWithIngredientData(GameObject slot, IngredientData data)
    {
        RawImage slotImage = slot.GetComponentInChildren<RawImage>();
        if (slotImage != null && data.ingredientSprite != null)
        {
            slotImage.texture = data.ingredientSprite.texture;
        }
    }

    private void AddHoverHandlers(GameObject slot, IngredientData data)
    {
        EventTrigger trigger = slot.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = slot.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerEnter
        };
        pointerEnter.callback.AddListener((_) => UpdateDetailsUI(data));
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry
        {
            eventID = EventTriggerType.PointerExit
        };
        pointerExit.callback.AddListener((_) => ClearDetailsUI());
        trigger.triggers.Add(pointerExit);
    }

    private void TryHandleItemClick()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            GameObject clickedObject = result.gameObject;

            // Vérifiez si l'objet cliqué est valide
            if (!IsValidGameObject(clickedObject))
            {
                Debug.LogWarning("TryHandleItemClick : Un objet cliqué est null ou détruit !");
                continue;
            }

            // Gestion pour l'inventaire
            if (InventoryUI.Instance != null && clickedObject.transform.IsChildOf(InventoryUI.Instance.InventoryArea))
            {
                GameObject item = InventoryUI.Instance.GetItemFromSlot(clickedObject);

                if (item != null)
                {
                    Inventory.Instance.RemoveFromInventory(item);
                    AddToFridge(item); // Transférer au frigo
                }
                return;
            }

            // Gestion pour le frigo
            if (clickedObject.transform.IsChildOf(fridgeGrid))
            {
                GameObject item = GetItemFromFridgeSlot(clickedObject);

                if (item != null)
                {
                    RemoveFromFridge(item); // Transférer à l'inventaire
                }
                return;
            }
        }
    }

    private GameObject GetItemFromFridgeSlot(GameObject slot)
    {
        foreach (var pair in fridgeItemSlotMapping)
        {
            if (pair.Value == slot)
            {
                return pair.Key;
            }
        }
        return null;
    }

    private bool IsValidGameObject(GameObject obj)
    {
        return obj != null && !obj.Equals(null);
    }



    private void UpdateDetailsUI(IngredientData data)
    {
        titleText.text = data.ingredientName;
        descriptionText.text = data.description;
        detailImage.sprite = data.ingredientSprite;
    }

    private void ClearDetailsUI()
    {
        titleText.text = string.Empty;
        descriptionText.text = string.Empty;
        detailImage.sprite = null;
    }

    public void ShowErrorMessage(string message)
    {
        if (!IsFridgeOpen()) return; // Ignore si le frigo est fermé
        if (isErrorMessageDisplayed) return; // Ignore si un message est déjà affiché

        isErrorMessageDisplayed = true; // Indique qu'un message est affiché
        errorText.text = message; // Met à jour le texte de l'erreur
        errorText.gameObject.SetActive(true);

        if (errorCanvasGroup != null)
        {
            StartCoroutine(FadeInAndOut());
        }
    }


    private IEnumerator FadeInAndOut()
    {
        // Apparition (Fade-In)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            errorCanvasGroup.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }

        // Temps d'affichage
        yield return new WaitForSeconds(displayTime);

        // Disparition (Fade-Out)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            errorCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        // Désactive le texte d'erreur et réinitialise le booléen
        errorText.gameObject.SetActive(false);
        isErrorMessageDisplayed = false;
    }

    private void RepositionErrorMessages()
    {
        for (int i = 0; i < activeErrorMessages.Count; i++)
        {
            TMP_Text errorText = activeErrorMessages[i];
            Vector2 newPosition = basePosition + new Vector2(0, -50 * (i + 1));
            errorText.rectTransform.anchoredPosition = newPosition;
        }
    }

    private void ResetErrorMessage()
    {
        StopAllCoroutines(); // Arrête toutes les animations en cours (FadeIn/FadeOut)
        if (errorCanvasGroup != null)
        {
            errorCanvasGroup.alpha = 0f; // Réinitialise l'alpha
        }
        if (errorText != null)
        {
            errorText.gameObject.SetActive(false); // Cache le texte
        }
        isErrorMessageDisplayed = false; // Réinitialise l'indicateur
    }


    private float EaseInOutExpo(float x)
    {
        if (x == 0)
            return 0;
        if (x == 1)
            return 1;
        if (x < 0.5f)
            return Mathf.Pow(2f, 20f * x - 10f) / 2f;
        return (2f - Mathf.Pow(2f, -20f * x + 10f)) / 2f;
    }

    private float EaseOutElastic(float x)
    {
        const float c4 = (2 * Mathf.PI) / 3;

        if (x == 0)
            return 0;
        if (x == 1)
            return 1;

        return Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;
    }
}
