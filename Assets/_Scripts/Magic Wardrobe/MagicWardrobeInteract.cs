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
                interactionText.text = "Appuyez sur E pour ouvrir l'armoire magique";
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

            // Si le joueur s'éloigne, fermer l'UI de l'armoire
            if (isWardrobeOpen)
            {
                CloseWardrobeUI();
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
        if (isAnimating) return;

        isAnimating = true;
        isWardrobeOpen = !isWardrobeOpen;

        if (isWardrobeOpen)
        {
            interactionCanvas.gameObject.SetActive(false);
            wardrobeUI.SetActive(true);

            GenerateRandomItems();

            StartCoroutine(AnimateWardrobeUI(wardrobeClosedPosition, wardrobeOpenPosition, () =>
            {
                isAnimating = false;
            }));
        }
        else
        {
            CloseWardrobeUI();
        }
    }

    private void CloseWardrobeUI()
    {
        if (isAnimating || !isWardrobeOpen) return;

        isAnimating = true;
        isWardrobeOpen = false;

        StartCoroutine(AnimateWardrobeUI(wardrobeOpenPosition, wardrobeClosedPosition, () =>
        {
            wardrobeUI.SetActive(false);
            ClearWardrobeUI();
            isAnimating = false;
        }));
    }

    private void GenerateRandomItems()
    {
        ClearWardrobeUI();

        for (int i = 0; i < maxSlots; i++)
        {
            if (possibleItems.Count == 0) break;

            IngredientData randomItem = possibleItems[Random.Range(0, possibleItems.Count)];
            GameObject slot = Instantiate(slotPrefab, wardrobeGrid);

            GameObject imageObject = new GameObject("ItemImage");
            imageObject.transform.SetParent(slot.transform, false);
            imageObject.AddComponent<RectTransform>().sizeDelta = new Vector2(100, 100);
            Image slotImage = imageObject.AddComponent<Image>();
            slotImage.sprite = randomItem.ingredientSprite;

            AddHoverHandlers(slot, randomItem);

            currentItems.Add(slot);
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
        instance.name = data.Prefab.name; // Optionnel : pour maintenir la lisibilité
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
