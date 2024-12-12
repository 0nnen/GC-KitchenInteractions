using UnityEngine;
using UnityEngine.UI;

public class CookingManager : MonoBehaviour
{
    private Transform foodItem; // Aliment actuellement en cuisson
    [SerializeField] private Slider fireSlider; // Slider de puissance
    [SerializeField] private ParticleSystem fireParticles; // Particules de feu

    private float rotationSpeed = 100f; // Vitesse de rotation

    private void Update()
    {
        // Faire tourner l'aliment en cuisson
        if (foodItem != null)
        {
            foodItem.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Ajuster les particules en fonction du Slider
        if (fireParticles != null)
        {
            var emission = fireParticles.emission;
            emission.rateOverTime = fireSlider.value * 10; // Ajuste le débit des particules
        }
    }

    /// <summary>
    /// Définit l'aliment en cours de cuisson.
    /// </summary>
    /// <param name="newFoodItem">L'aliment à cuire.</param>
    public void SetFoodItem(Transform newFoodItem)
    {
        foodItem = newFoodItem;
        Debug.Log($"Nouvel aliment assigné : {newFoodItem.name}");
    }
}
