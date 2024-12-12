using UnityEngine;
using UnityEngine.UI;

public class InventoryCameraTest : MonoBehaviour
{
    [SerializeField] private Camera inventoryCamera;
    [SerializeField] private GameObject objectToRender;

    private RenderTexture renderTexture;

    private void Start()
    {
        if (inventoryCamera == null || objectToRender == null)
        {
            Debug.LogError("Assignez une cam�ra et un objet � afficher !");
            return;
        }

        // Cr�er une Render Texture
        renderTexture = new RenderTexture(256, 256, 16);
        renderTexture.Create();

        // Configurer la cam�ra
        inventoryCamera.targetTexture = renderTexture;
        objectToRender.layer = LayerMask.NameToLayer("InventoryPreview");

        // Positionner la cam�ra
        inventoryCamera.transform.position = objectToRender.transform.position + new Vector3(0, 0, -2);
        inventoryCamera.transform.LookAt(objectToRender.transform);

        // Appliquer la Render Texture � un RawImage dans l'UI (ajustez si n�cessaire)
        RawImage rawImage = FindObjectOfType<RawImage>();
        if (rawImage != null)
        {
            rawImage.texture = renderTexture;
        }
    }

    private void OnDestroy()
    {
        if (renderTexture != null)
        {
            renderTexture.Release();
            Destroy(renderTexture);
        }
    }
}