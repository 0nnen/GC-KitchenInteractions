using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("Surlignage")]
    [SerializeField] private HighlightEffect highlightEffect; // Effet visuel (facultatif)

    [Header("Interaction avanc�e")]
    [SerializeField] private Cinemachine.CinemachineVirtualCamera associatedCamera;
    [SerializeField] private GameObject cookingModeUI; // UI sp�cifique pour le mode de cuisson
    [SerializeField] private ParticleSystem fireParticles; // Particules sous l'objet
    [SerializeField] private CookingManager cookingManager; // R�f�rence au CookingManager
    [SerializeField] private Transform foodPlacementPoint; // Point o� l'aliment est plac� pour la cuisson

    [Header("Propri�t�s")]
    [SerializeField] private bool canReceiveChildren = false;

    public bool CanReceiveChildren => canReceiveChildren;

    private GameObject currentFoodItem; // Aliment actuellement en cuisson
    private bool isCooking = false; // Indique si le mode de cuisson est actif

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Start()
    {
        if (cookingModeUI != null) cookingModeUI.SetActive(false);
        if (fireParticles != null) fireParticles.Stop();
    }

    public void OnFocused()
    {
        if (highlightEffect != null) highlightEffect.EnableHighlight(true);
    }

    public void OnFocusLost()
    {
        if (highlightEffect != null) highlightEffect.EnableHighlight(false);
    }

    public void Interact()
    {
        if (currentFoodItem == null)
        {
            GameObject selectedFood = Inventory.Instance.GetItems().Find(item => item.CompareTag("Food"));

            if (selectedFood != null)
            {
                selectedFood.transform.position = foodPlacementPoint.position;
                selectedFood.transform.rotation = Quaternion.identity;
                selectedFood.SetActive(true);

                currentFoodItem = selectedFood;

                if (cookingManager != null)
                {
                    cookingManager.SetFoodItem(currentFoodItem.transform);
                }

                EnterCookingMode();
            }
            else
            {
                Debug.Log("Aucun aliment s�lectionn� dans l'inventaire !");
            }
        }
        else
        {
            Debug.Log("Un aliment est d�j� en cuisson !");
        }
    }

    private void EnterCookingMode()
    {
        if (associatedCamera != null)
        {
            associatedCamera.Priority = 10;
        }

        if (fireParticles != null) fireParticles.Play();
        if (cookingModeUI != null) cookingModeUI.SetActive(true);

        isCooking = true;
    }

    private void ExitCookingMode()
    {
        if (associatedCamera != null)
        {
            associatedCamera.Priority = 0;
        }

        if (fireParticles != null) fireParticles.Stop();
        if (cookingModeUI != null) cookingModeUI.SetActive(false);

        currentFoodItem = null;
        isCooking = false;
    }

    private void Update()
    {
        if (isCooking)
        {
            if (inputActions.Player.Interact.triggered || inputActions.UI.Cancel.triggered)
            {
                ExitCookingMode();
            }
        }
    }
}
