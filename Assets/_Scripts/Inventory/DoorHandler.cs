using UnityEngine;

public class DoorHandler : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private bool rotateOnX = false; // Rotation sur l'axe X
    [SerializeField] private bool rotateOnY = true; // Rotation sur l'axe Y
    [SerializeField] private float rotationLimit = 120f; // Angle maximal d'ouverture
    [SerializeField] private float rotationSpeed = 5f;  // Vitesse de rotation

    private float currentRotation = 0f;

    /// <summary>
    /// Fait pivoter la porte en fonction du mouvement de la souris.
    /// </summary>
    /// <param name="delta">Delta de rotation (input souris).</param>
    public void RotateDoor(float delta)
    {
        // Calcul de la nouvelle rotation en respectant les limites
        float newRotation = Mathf.Clamp(currentRotation + delta * rotationSpeed, 0f, rotationLimit);

        // Calcul de la rotation à appliquer
        float rotationStep = newRotation - currentRotation;

        // Appliquer la rotation sur le pivot de l'objet
        if (rotateOnX)
        {
            transform.Rotate(Vector3.right, rotationStep, Space.Self);
        }
        else if (rotateOnY)
        {
            transform.Rotate(Vector3.up, rotationStep, Space.Self);
        }

        // Mettre à jour l'état actuel
        currentRotation = newRotation;

        Debug.Log($"Door rotated to {currentRotation}° on {(rotateOnX ? "X" : "Y")} axis");
    }
}
