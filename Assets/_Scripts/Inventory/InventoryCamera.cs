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
            Debug.LogError("Assignez une caméra et un objet à afficher !");
            return;
        }

        // Créer une Render Texture
        renderTexture = new RenderTexture(256, 256, 16);
        renderTexture.Create();

        // Configurer la caméra
        inventoryCamera.targetTexture = renderTexture;
        objectToRender.layer = LayerMask.NameToLayer("InventoryPreview");

        // Positionner la caméra
        inventoryCamera.transform.position = objectToRender.transform.position + new Vector3(0, 0, -2);
        inventoryCamera.transform.LookAt(objectToRender.transform);

        // Appliquer la Render Texture à un RawImage dans l'UI (ajustez si nécessaire)
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