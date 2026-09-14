using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Pulo & Chão")]
    [SerializeField] private float jumpForce = 6f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.25f;
    [SerializeField] private LayerMask groundMask;

    [Header("Câmera")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private bool jumpRequested;

    // Suporte a plataformas / superfícies móveis e giratórias
    private Rigidbody groundRigidbody;
    private Vector3 groundContactPoint;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    private void Update()
    {
        ReadInputs();
        CheckGround();
    }

    private void FixedUpdate()
    {
        MovePlayer();

        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
    }

    private void CheckGround()
    {
        if (groundCheck == null) return;

        // Faz um raio/esfera para baixo para checar contato e identificar se o chão tem Rigidbody
        RaycastHit hit;
        if (Physics.SphereCast(groundCheck.position + Vector3.up * 0.1f, groundDistance, Vector3.down, out hit, groundDistance * 2f, groundMask))
        {
            isGrounded = true;
            groundRigidbody = hit.rigidbody;
            groundContactPoint = hit.point;
        }
        else
        {
            isGrounded = false;
            groundRigidbody = null;
        }
    }

    private void ReadInputs()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
            {
                jumpRequested = true;
            }
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.04f)
            {
                input = stick;
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame && isGrounded)
            {
                jumpRequested = true;
            }
        }

        moveInput = input.normalized;
    }

    private void MovePlayer()
    {
        // Calcula a velocidade transmitida pela superfície móvel/giratória
        Vector3 platformVelocity = Vector3.zero;
        if (isGrounded && groundRigidbody != null)
        {
            // Pega a velocidade tangencial exata do ponto onde o jogador está encostando
            platformVelocity = groundRigidbody.GetPointVelocity(groundContactPoint);
        }

        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            Vector3 targetVelocity = moveDirection * moveSpeed;

            // Soma a velocidade de locomoção com a velocidade do chão
            rb.linearVelocity = new Vector3(
                targetVelocity.x + platformVelocity.x,
                rb.linearVelocity.y,
                targetVelocity.z + platformVelocity.z
            );

            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Se não houver comando de andar, o boneco herda a velocidade da superfície
            rb.linearVelocity = new Vector3(
                platformVelocity.x,
                rb.linearVelocity.y,
                platformVelocity.z
            );
        }
    }

    private void Jump()
    {
        // Preserva o impulso horizontal adquirido do chão e aplica o pulo vertical
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}