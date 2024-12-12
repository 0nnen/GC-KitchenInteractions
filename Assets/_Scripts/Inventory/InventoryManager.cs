using UnityEngine;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance;

    [Header("UI")]
    [SerializeField] private Transform inventoryGrid;
    [SerializeField] private GameObject inventorySlotPrefab;
    [SerializeField] private Camera inventoryCamera;
    [SerializeField] private DragAndDropHandler dragAndDropHandler;

    [Header("Logique")]
    [SerializeField] private int maxInventorySize = 9;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddToInventory(GameObject item)
    {
        if (inventoryGrid.childCount >= maxInventorySize)
        {
            Debug.LogWarning("Inventaire plein !");
            return;
        }

        // Créer un slot
        GameObject slot = Instantiate(inventorySlotPrefab, inventoryGrid);
        RawImage slotImage = slot.GetComponentInChildren<RawImage>();

        // Capture l'aperçu de l'objet
        slotImage.texture = CaptureItemPreview(item);

        // Configurer le bouton pour le drag
        Button button = slot.GetComponent<Button>();
        button.onClick.AddListener(() => dragAndDropHandler.BeginDrag(item));

        // Désactiver l'objet après capture
        item.SetActive(false);
        Debug.Log($"{item.name} ajouté à l'inventaire.");
    }

    private Texture CaptureItemPreview(GameObject item)
    {
        RenderTexture renderTexture = new RenderTexture(256, 256, 16);
        inventoryCamera.targetTexture = renderTexture;

        Bounds bounds = item.GetComponent<Renderer>().bounds;
        inventoryCamera.transform.position = bounds.center + Vector3.back * bounds.size.z * 2;
        inventoryCamera.transform.LookAt(bounds.center);

        inventoryCamera.Render();
        inventoryCamera.targetTexture = null;

        return renderTexture;
    }
}
