using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems;


public class MagicWardrobeInteract : MonoBehaviour
{
    [Header("UI Interaction")]
    [SerializeField] private Canvas interactionCanvas;
    [SerializeField] private TMP_Text interactionText;
    [SerializeField] private Transform canvasAnchor;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Vector3 canvasOffset = new Vector3(0, 1f, 0);
    [SerializeField] private bool dynamicOrientation = true;

    [Header("UI de l'armoire magique")]
    [SerializeField] private GameObject wardrobeUI;
    [SerializeField] private Transform wardrobeGrid;
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private RectTransform wardrobeUIRect;

    [Header("UI Détails de l'item")]
    [SerializeField] private Image detailImage;
    [SerializeField] private TMP_Text detailTitleText;
    [SerializeField] private TMP_Text detailDescriptionText;

    [Header("Paramètres")]
    [SerializeField] private float interactionRange = 3.0f;
    [SerializeField] private float animationDuration = 1f;
    [SerializeField] private int maxSlots = 3;
    [SerializeField] private List<IngredientData> possibleItems;

    private bool isPlayerInRange = false;
    private bool isWardrobeOpen = false;
    private bool isAnimating = false;
    private List<GameObject> currentItems = new List<GameObject>();
    private Vector2 wardrobeClosedPosition = new Vector2(0, -500);
    private Vector2 wardrobeOpenPosition = new Vector2(0, 0);

    private void Start()
    {
        if (interactionCanvas != null)
            interactionCanvas.gameObject.SetActive(false);

        wardrobeUI.SetActive(false);

        if (wardrobeUIRect != null)
        {
            wardrobeUIRect.anchoredPosition = wardrobeClosedPosition;
        }

        if (possibleItems == null || possibleItems.Count == 0)
        {
            Debug.LogError("Aucun objet assigné à la liste possibleItems !");
        }
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);
        HandleCanvasVisibility(distanceToPlayer);

        if (distanceToPlayer <= interactionRange)
        {
            if (!isPlayerInRange)
            {
                isPlayerInRange = true;
                interactionCanvas.gameObject.SetActive(true);
                interactionText.text = "Press E";
            }

            if (Input.GetKeyDown(KeyCode.E) && !isAnimating)
            {
                ToggleWardrobeUI();
            }
        }
        else
        {
            if (isPlayerInRange)
            {
                isPlayerInRange = false;
                interactionCanvas.gameObject.SetActive(false);
            }

            if (isWardrobeOpen)
            {
                CloseWardrobeUI(forceImmediate: true);
            }
        }
    }

    private void HandleCanvasVisibility(float distanceToPlayer)
    {
        if (interactionCanvas == null || canvasAnchor == null || playerTransform == null) return;

        if (distanceToPlayer <= interactionRange)
        {
            if (!interactionCanvas.gameObject.activeSelf)
            {
                interactionCanvas.gameObject.SetActive(true);
            }

            interactionCanvas.transform.position = canvasAnchor.position + canvasOffset;

            if (dynamicOrientation)
            {
                Vector3 directionToPlayer = playerTransform.position - interactionCanvas.transform.position;
                Quaternion lookRotation = Quaternion.LookRotation(-directionToPlayer.normalized);
                interactionCanvas.transform.rotation = lookRotation;
            }
        }
        else
        {
            if (interactionCanvas.gameObject.activeSelf)
            {
                interactionCanvas.gameObject.SetActive(false);
            }
        }
    }

    private void ToggleWardrobeUI()
    {
        if (isAnimating)
        {
            CloseWardrobeUI(forceImmediate: true);
            return;
        }

        if (isWardrobeOpen)
        {
            CloseWardrobeUI();
        }
        else
        {
            OpenWardrobeUI();
        }
    }

    private void OpenWardrobeUI()
    {
        isAnimating = true;
        isWardrobeOpen = true;

        interactionCanvas.gameObject.SetActive(false);
        wardrobeUI.SetActive(true);

        GenerateRandomItems();

        StartCoroutine(AnimateWardrobeUI(wardrobeClosedPosition, wardrobeOpenPosition, () =>
        {
            isAnimating = false;
        }));
    }

    private void CloseWardrobeUI(bool forceImmediate = false)
    {
        if (!isWardrobeOpen) return;
        if (isAnimating && !forceImmediate) return;

        isAnimating = true;
        isWardrobeOpen = false;

        if (forceImmediate)
        {
            wardrobeUIRect.anchoredPosition = wardrobeClosedPosition;
            wardrobeUI.SetActive(false);
            ClearWardrobeUI();
            isAnimating = false;
        }
        else
        {
            StartCoroutine(AnimateWardrobeUI(wardrobeOpenPosition, wardrobeClosedPosition, () =>
            {
                wardrobeUI.SetActive(false);
                ClearWardrobeUI();
                isAnimating = false;
            }));
        }
    }

    private void ClearWardrobeUI()
    {
        foreach (GameObject item in currentItems)
        {
            Destroy(item);
        }
        currentItems.Clear();
    }

    private void GenerateRandomItems()
    {
        ClearWardrobeUI();

        for (int i = 0; i < maxSlots; i++)
        {
            if (possibleItems.Count == 0) break;

            // Sélection d'un IngredientData aléatoire
            IngredientData randomItem = possibleItems[Random.Range(0, possibleItems.Count)];

            // Création d'un slot dans l'UI
            GameObject slot = Instantiate(slotPrefab, wardrobeGrid);

            GameObject imageObject = new GameObject("ItemImage");
            imageObject.transform.SetParent(slot.transform, false);
            imageObject.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            Image slotImage = imageObject.AddComponent<Image>();
            slotImage.sprite = randomItem.ingredientSprite;

            // Instanciation du prefab
            GameObject instance = Instantiate(randomItem.Prefab);
            instance.name = randomItem.ingredientName;

            // Ajouter l'IngredientData au prefab instancié
            IngredientComponent ingredientComponent = instance.AddComponent<IngredientComponent>();
            ingredientComponent.SetIngredientData(randomItem);

            // Ajouter à ObjectConfig
            AddToObjectConfig(instance, randomItem);

            // Ajout des handlers pour l'UI
            AddHoverHandlers(slot, randomItem);

            // Ajout à la liste des items actuels
            currentItems.Add(slot);
        }
    }

    private void AddToObjectConfig(GameObject instance, IngredientData ingredientData)
    {
        DragAndDropManager dragManager = DragAndDropManager.Instance;

        if (dragManager == null)
        {
            Debug.LogError("DragAndDropManager introuvable !");
            return;
        }

        // Vérifier si l'objet est déjà dans ObjectConfig
        ObjectConfig existingConfig = dragManager.ObjectConfigs.Find(config => config.prefab == instance);

        if (existingConfig == null)
        {
            // Ajouter une nouvelle configuration
            dragManager.ObjectConfigs.Add(new ObjectConfig
            {
                category = "Ingredients", // Exemple : vous pouvez ajuster selon vos besoins
                prefab = instance,
                ingredientData = ingredientData,
                isMovable = true
            });

            Debug.Log($"Ajouté à ObjectConfig : {instance.name} avec {ingredientData.ingredientName}");
        }
        else
        {
            Debug.LogWarning($"L'objet {instance.name} est déjà présent dans ObjectConfig !");
        }
    }

    private void AddHoverHandlers(GameObject slot, IngredientData data)
    {
        Button slotButton = slot.AddComponent<Button>();
        slotButton.onClick.AddListener(() => TransferItemToInventory(data));

        EventTrigger trigger = slot.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((_) => UpdateDetailsUI(data));
        trigger.triggers.Add(pointerEnter);

        EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((_) => ClearDetailsUI());
        trigger.triggers.Add(pointerExit);
    }

    private void UpdateDetailsUI(IngredientData data)
    {
        if (detailImage != null) detailImage.sprite = data.ingredientSprite;
        if (detailTitleText != null) detailTitleText.text = data.ingredientName;
        if (detailDescriptionText != null) detailDescriptionText.text = data.description;
    }

    private void ClearDetailsUI()
    {
        if (detailImage != null) detailImage.sprite = null;
        if (detailTitleText != null) detailTitleText.text = string.Empty;
        if (detailDescriptionText != null) detailDescriptionText.text = string.Empty;
    }

    private void TransferItemToInventory(IngredientData data)
    {
        GameObject instance = Instantiate(data.Prefab);
        instance.name = data.ingredientName;

        IngredientComponent ingredientComponent = instance.AddComponent<IngredientComponent>();
        ingredientComponent.SetIngredientData(data);

        Inventory.Instance.AddToInventory(instance);
        Debug.Log($"{data.ingredientName} ajouté à l'inventaire.");
    }

    private IEnumerator AnimateWardrobeUI(Vector2 startPos, Vector2 endPos, System.Action onAnimationComplete)
    {
        float timer = 0f;

        while (timer < animationDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / animationDuration);
            float easedT = EaseOutElastic(t);

            wardrobeUIRect.anchoredPosition = Vector2.Lerp(startPos, endPos, easedT);
            yield return null;
        }

        wardrobeUIRect.anchoredPosition = endPos;

        onAnimationComplete?.Invoke();
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
