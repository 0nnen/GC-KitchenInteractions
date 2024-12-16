using UnityEngine;
using TMPro;

public class FridgeInteract : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Canvas en World Space pour afficher le texte.")]
    [SerializeField] private GameObject interactCanvas;

    [Tooltip("TextMeshPro pour afficher le message.")]
    [SerializeField] private TMP_Text interactText;

    [Tooltip("UI principale du frigo.")]
    [SerializeField] private GameObject fridgeUI;

    [Tooltip("Transform du joueur.")]
    [SerializeField] private Transform playerTransform;

    [Header("Paramètres")]
    [Tooltip("Distance maximale pour interagir.")]
    [SerializeField] private float interactionRange = 3f;

    private bool isPlayerInRange = false;
    private bool isFridgeOpen = false;

    private void Start()
    {
        interactCanvas.SetActive(false);
        fridgeUI.SetActive(false);
    }

    private void Update()
    {
        float distanceToPlayer = Vector3.Distance(playerTransform.position, transform.position);

        if (distanceToPlayer <= interactionRange)
        {
            if (!isPlayerInRange)
            {
                isPlayerInRange = true;
                ShowInteractionMessage("Appuyez sur E pour ouvrir le frigo");
            }

            if (Input.GetKeyDown(KeyCode.E))
            {
                ToggleFridgeUI();
            }
        }
        else
        {
            if (isPlayerInRange)
            {
                isPlayerInRange = false;
                HideInteractionMessage();
            }
        }
    }

    private void ShowInteractionMessage(string message)
    {
        interactText.text = message;
        interactCanvas.SetActive(true);
    }

    private void HideInteractionMessage()
    {
        interactCanvas.SetActive(false);
    }

    private void ToggleFridgeUI()
    {
        isFridgeOpen = !isFridgeOpen;
        fridgeUI.SetActive(isFridgeOpen);
        interactCanvas.SetActive(!isFridgeOpen);

        Debug.Log(isFridgeOpen ? "Frigo ouvert" : "Frigo fermé");
    }
}
