using UnityEngine;

public class ObjectInteract : MonoBehaviour
{
    [Header("R�f�rences")]
    [Tooltip("Gestionnaire pour l'UI d'interaction.")]
    [SerializeField] private InteractionCanvasHandler interactionCanvas;

    [Tooltip("Cam�ra principale.")]
    [SerializeField] private Camera mainCamera;

    [Tooltip("Port�e maximale d'interaction.")]
    [SerializeField] private float interactionRange = 3f;

    private bool isPlayerInRange = false;

    private void Update()
    {
        DetectInteraction();
    }

    private void DetectInteraction()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                float distance = Vector3.Distance(Camera.main.transform.position, transform.position);

                if (distance <= interactionRange)
                {
                    if (!isPlayerInRange)
                    {
                        isPlayerInRange = true;
                        interactionCanvas.ShowMessage("Maintenez clic gauche pour d�placer", transform);
                    }
                    return;
                }
            }
        }

        if (isPlayerInRange)
        {
            isPlayerInRange = false;
            interactionCanvas.HideMessage();
        }
    }
}
