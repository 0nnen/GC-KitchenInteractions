using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Param�tres de mouvement")]
    [SerializeField] private float walkSpeed = 2f;

    [SerializeField] private float runSpeed = 4f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Param�tres de rotation")]
    [SerializeField] private float mouseSensitivity = 10f;

    [SerializeField] private float lateralSensitivity = 5f; // Sensibilit� pour les mouvements lat�raux
    [SerializeField] private float tiltAmount = 5f; // Inclinaison maximale pendant le mouvement
    [SerializeField] private float tiltDamping = 5f; // Lissage de l'inclinaison

    [Header("R�f�rences")]
    [SerializeField] private Transform cameraTransform;

    [SerializeField] private Animator animator;

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private InputSystem_Actions inputActions;
    private float cameraPitch = 0f;
    private float currentTilt = 0f;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        inputActions = new InputSystem_Actions();

        Cursor.lockState = CursorLockMode.Confined;
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    private void Update()
    {
        isGrounded = CheckGrounded();
        HandleMovement();
        ApplyGravity();
        // RotatePlayerWithMouse();
        UpdateAnimator();
        ApplyDynamicTilt();
    }

    /// <summary>
    /// V�rifie si le joueur est au sol.
    /// </summary>
    private bool CheckGrounded()
    {
        return characterController.isGrounded;
    }

    /// <summary>
    /// G�re les d�placements du joueur.
    /// </summary>
    private void HandleMovement()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = Vector3.zero;

        // Calcul des d�placements avant/arri�re et lat�raux
        move += transform.forward * input.y; // Avant/arri�re
        move += transform.right * input.x;  // Lat�raux

        // Rotation pour les d�placements lat�raux
        if (Mathf.Abs(input.x) > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(transform.right * input.x);
            float sensitivity = Mathf.Lerp(mouseSensitivity, lateralSensitivity, Mathf.Abs(input.x));
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, sensitivity * Time.deltaTime);
        }

        // Gestion de la vitesse (marche ou course)
        bool isRunning = inputActions.Player.Sprint.ReadValue<float>() > 0.5f;
        float speed = isRunning ? runSpeed : walkSpeed;

        // Applique le mouvement
        characterController.Move(move.normalized * speed * Time.deltaTime);
    }

    /// <summary>
    /// Applique la gravit� et g�re le saut.
    /// </summary>
    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (inputActions.Player.Jump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        if (!isGrounded)
        {
            velocity.y += gravity * Time.deltaTime; // Appliquer la gravit� si le joueur n'est pas au sol
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// G�re la rotation du joueur et de la cam�ra � l'aide de la souris.
    /// </summary>
    private void RotatePlayerWithMouse()
    {
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f);
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    /// <summary>
    /// Met � jour les param�tres de l'Animator en fonction de l'�tat du joueur.
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator == null) return;

        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        float speed = input.magnitude;

        animator.SetFloat("Speed", speed);
        animator.SetBool("IsJumping", !isGrounded && velocity.y > 0);
        animator.SetBool("IsFalling", !isGrounded && velocity.y < 0);
    }

    /// <summary>
    /// Applique une inclinaison dynamique � la cam�ra en fonction du mouvement lat�ral.
    /// </summary>
    private void ApplyDynamicTilt()
    {
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        float targetTilt = -input.x * tiltAmount;

        // Lissage de l'inclinaison
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltDamping * Time.deltaTime);

        // Appliquer l'inclinaison � la cam�ra
        Quaternion tiltRotation = Quaternion.Euler(cameraTransform.localEulerAngles.x, cameraTransform.localEulerAngles.y, currentTilt);
        cameraTransform.localRotation = tiltRotation;
    }
}