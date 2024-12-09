using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Param�tres de mouvement")]
    [SerializeField, Tooltip("Vitesse de marche du joueur.")]
    private float walkSpeed = 2f;
    [SerializeField, Tooltip("Vitesse de course du joueur.")]
    private float runSpeed = 4f;
    [SerializeField, Tooltip("Force de gravit� appliqu�e au joueur.")]
    private float gravity = -9.81f;
    [SerializeField, Tooltip("Hauteur maximale de saut.")]
    private float jumpHeight = 1.5f;
    [SerializeField, Tooltip("Sensibilit� de la souris.")]
    private float mouseSensitivity = 100f;

    [Header("R�f�rences")]
    [SerializeField] private Animator animator; // R�f�rence � l'Animator
    [SerializeField] private Transform cameraTransform; // Transform de la cam�ra (enfant)

    private CharacterController characterController;
    private Vector3 velocity;
    private bool isGrounded;
    private InputSystem_Actions inputActions;

    private float cameraPitch = 0f; // Rotation verticale de la cam�ra

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
        // V�rification si le joueur est au sol
        isGrounded = characterController.isGrounded;

        // R�initialisation de la vitesse verticale si au sol
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // Emp�che une accumulation n�gative de gravit�
        }

        // Lecture des entr�es de mouvement
        Vector2 input = inputActions.Player.Move.ReadValue<Vector2>();
        Vector3 move = new Vector3(input.x, 0, input.y);

        // Convertir les entr�es utilisateur en direction locale (selon la cam�ra)
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

        // Appliquer la gravit�
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // R�initialiser les animations si au sol
        if (isGrounded)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsFalling", false);
        }
    }

    private void HandleLook()
    {
        // Lecture des entr�es de la souris
        Vector2 lookInput = inputActions.Player.Look.ReadValue<Vector2>();

        // Rotation horizontale (rotation du joueur)
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime;
        transform.Rotate(Vector3.up * mouseX);

        // Rotation verticale (rotation de la cam�ra)
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime;
        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, -90f, 90f); // Limite l'angle vertical
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }
}
