using UnityEngine;
using TMPro;

public class InteractionMessageHandler : MonoBehaviour
{
    public static InteractionMessageHandler Instance;

    [Header("Références")]
    [SerializeField] private GameObject interactionCanvasPrefab; // Canvas en World Space
    [SerializeField] private Camera mainCamera;

    private GameObject currentCanvas;
    private TMP_Text interactionText;
    private Transform target;

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

    public void ShowMessage(string message, Transform target)
    {
        if (currentCanvas == null)
        {
            currentCanvas = Instantiate(interactionCanvasPrefab, transform);
            interactionText = currentCanvas.GetComponentInChildren<TMP_Text>();
        }

        this.target = target;
        interactionText.text = message;
        currentCanvas.SetActive(true);
    }

    public void HideMessage()
    {
        if (currentCanvas != null)
        {
            currentCanvas.SetActive(false);
            target = null;
        }
    }

    private void Update()
    {
        if (currentCanvas != null && currentCanvas.activeSelf && target != null)
        {
            // Positionner le canvas au-dessus de l'objet
            currentCanvas.transform.position = target.position + Vector3.up * 1.5f;
            currentCanvas.transform.LookAt(mainCamera.transform);
            currentCanvas.transform.Rotate(0, 180, 0);
        }
    }
}
