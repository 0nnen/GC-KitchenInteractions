using UnityEngine;
using UnityEngine.EventSystems;

public class SurfaceInteraction : MonoBehaviour, IPointerClickHandler
{
    [Header("Références")]
    [SerializeField] private RectTransform inventoryRectTransform;
    [SerializeField] private float smoothSpeed = 5f;

    private Vector2 targetPosition;
    private bool isMoving = false;

    private void Update()
    {
        if (isMoving)
        {
            inventoryRectTransform.anchoredPosition = Vector2.Lerp(
                inventoryRectTransform.anchoredPosition, targetPosition, Time.deltaTime * smoothSpeed);

            if (Vector2.Distance(inventoryRectTransform.anchoredPosition, targetPosition) < 0.1f)
            {
                inventoryRectTransform.anchoredPosition = targetPosition;
                isMoving = false;
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            MoveInventoryToCenter();
        }
    }

    private void MoveInventoryToCenter()
    {
        targetPosition = Vector2.zero; // Centre de l'écran
        isMoving = true;
    }
}
