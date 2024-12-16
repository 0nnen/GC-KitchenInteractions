using UnityEngine;
using TMPro;

public class InteractionCanvasHandler : MonoBehaviour
{
    [Header("Références")]
    [Tooltip("Canvas affichant le texte en World Space.")]
    [SerializeField] private GameObject interactionCanvas;

    [Tooltip("TextMeshPro pour le message.")]
    [SerializeField] private TMP_Text interactionText;

    [Tooltip("Caméra principale.")]
    [SerializeField] private Camera mainCamera;

    private Transform currentTarget;

    public void ShowMessage(string message, Transform target)
    {
        interactionText.text = message;
        interactionCanvas.SetActive(true);
        currentTarget = target;
    }

    public void HideMessage()
    {
        interactionCanvas.SetActive(false);
        currentTarget = null;
    }

    private void Update()
    {
        if (interactionCanvas.activeSelf && currentTarget != null)
        {
            // Positionner le canvas au-dessus de l'objet
            interactionCanvas.transform.position = currentTarget.position + Vector3.up * 1.5f;
            interactionCanvas.transform.LookAt(mainCamera.transform);
            interactionCanvas.transform.Rotate(0, 180, 0);
        }
    }
}
