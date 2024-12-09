using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Paramètres de mouvement")]
    [SerializeField, Tooltip("Vitesse de marche du joueur.")]
    private float walkSpeed = 2f;
    [SerializeField, Tooltip("Vitesse de course du joueur.")]
    private float runSpeed = 4f;
    [SerializeField, Tooltip("Force de gravité appliquée au joueur.")]
    private float gravity = -9.81f;
    [SerializeField, Tooltip("Hauteur maximale de saut.")]
    private float jumpHeight = 1.5f;
    [SerializeField, Tooltip("Sensibilité de la souris.")]
    private float mouseSensitivity = 100f;

    [Header("Références")]
    [SerializeField] private Animator animator; // Référence à l'Animator
    [SerializeField] private Transform cameraTransform; // Transform de la caméra (enfant)

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private InputSystem_Actions inputActions;

    private float cameraPitch = 0f; // Rotation verticale de la caméra

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
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

    private void Update()
    {
        HandleMovement();
        HandleLook();
    }

    private void HandleMovement()
    {
        // Vérification si le joueur est au sol
        isGrounded = characterController.isGrounded;

        // Réinitialisation de la vitesse verticale si au sol
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Empêche une accumulation négative de gravité
        }

        // Lecture des entrées de mouvement
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);

        // Convertir les entrées utilisateur en direction locale (selon la caméra)
        move = cameraTransform.forward * move.z + cameraTransform.right * move.x;
        move.y = 0; // Ignore tout mouvement vertical pour ce calcul

        // Gestion de la vitesse (marche ou course)
        bool isRunning = inputActions.Player.Sprint.ReadValue<float>() > 0.5f;
        float speed = isRunning ? runSpeed : walkSpeed;

        // Appliquer le mouvement horizontal
        characterController.Move(move.normalized * speed * Time.deltaTime);

        // Gestion des animations
        animator.SetFloat("Speed", move.magnitude * speed);
        animator.SetBool("IsJumping", !isGrounded && velocity.y > 0);
        animator.SetBool("IsFalling", !isGrounded && velocity.y < 0);

        // Gestion du saut
        if (inputActions.Player.Jump.triggered && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            animator.SetBool("IsJumping", true);
        }

        // Appliquer la gravité
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // Réinitialiser les animations si au sol
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
    }

    private void HandleLook()
    {
        // Lecture des entrées de la souris
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // Rotation horizontale (rotation du joueur)
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // Rotation verticale (rotation de la caméra)
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); // Limite l'angle vertical
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
