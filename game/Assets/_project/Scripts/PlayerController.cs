using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Pulo & Chão")]
    [SerializeField] private float jumpForce = 9.5f;
    [SerializeField] private float gravityMultiplier = 1.6f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private float groundCheckDistance = 0.45f;
    [SerializeField] private LayerMask groundMask;

    [Header("Dive (Mergulho no Ar)")]
    [SerializeField] private float diveForwardForce = 10f;
    [SerializeField] private float diveUpwardBoost = 2.5f;
    [SerializeField] private float slideRecoveryTime = 0.8f;

    [Header("Wall Grab & Slide")]
    [SerializeField] private float wallSlideSpeed = -2.5f;
    [SerializeField] private float wallWalkHorizontalSpeed = 4f;
    [SerializeField] private LayerMask grabbableMask;
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float wallJumpVerticalForce = 8.5f;

    [Header("Estado")]
    [SerializeField] private bool canMove = true;

    [Header("Câmera")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private Collider playerCollider;
    private Vector2 moveInput;
    private bool isGrounded;
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    private float jumpCooldown = 0.1f;
    private float jumpCooldownTimer;

    // Dive states
    private bool isDiving;
    private bool hasAirDived;
    private bool hasJumped;
    private float slideRecoveryTimer;
    private Vector3 horizontalFacing = Vector3.forward;
    private bool isHoldingJump;
    private Vector3 groundNormal = Vector3.up;

    // Wall Grab states
    private bool isGrabbingWall;
    private float wallGrabGraceTimer;
    private float wallJumpCooldownTimer;
    private Vector3 wallNormal;
    private Vector3 lastWallJumpNormal;
    private GrabbableSurface currentGrabbableSurface;

    // External forces / Knockback
    private Vector3 externalForce;

    // Suporte a plataformas / superfícies móveis e giratórias
    private Rigidbody groundRigidbody;
    private Collider groundCollider;
    private Vector3 groundContactPoint;
    private bool collisionGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        playerCollider = GetComponent<Collider>();
        if (playerCollider != null)
        {
            PhysicsMaterial frictionless = new PhysicsMaterial("PlayerFrictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounciness = 0f,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
            playerCollider.material = frictionless;
        }

        Vector3 initFacing = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (initFacing.sqrMagnitude > 0.01f)
        {
            horizontalFacing = initFacing;
        }
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
        if (jumpCooldownTimer > 0f) jumpCooldownTimer -= Time.fixedDeltaTime;
        if (wallJumpCooldownTimer > 0f) wallJumpCooldownTimer -= Time.fixedDeltaTime;
        
        if (wallGrabGraceTimer > 0f)
        {
            wallGrabGraceTimer -= Time.fixedDeltaTime;
            if (wallGrabGraceTimer <= 0f) isGrabbingWall = false;
        }

        // O timer de recuperação só começa a contar DEPOIS que soltar o botão de pulo
        if (slideRecoveryTimer > 0f && !isHoldingJump)
        {
            slideRecoveryTimer -= Time.fixedDeltaTime;
        }

        if (isDiving && isGrounded && slideRecoveryTimer <= 0f)
        {
            isDiving = false;
        }

        if (externalForce.sqrMagnitude > 0.01f)
        {
            externalForce = Vector3.Lerp(externalForce, Vector3.zero, 5f * Time.fixedDeltaTime);
        }
        else
        {
            externalForce = Vector3.zero;
        }

        // Aplica gravidade extra para o pulo ficar menos "flutuante"
        if (!isGrounded && !isGrabbingWall)
        {
            rb.linearVelocity += Physics.gravity * (gravityMultiplier - 1f) * Time.fixedDeltaTime;
        }

        if (isGrabbingWall)
        {
            HandleWallGrab();
        }
        else
        {
            MovePlayer();
        }

        if (jumpBufferCounter > 0f && jumpCooldownTimer <= 0f && slideRecoveryTimer <= 0f)
        {
            if (isGrabbingWall)
            {
                WallJump();
            }
            else if (isGrounded)
            {
                Jump();
            }
            else if (!isGrounded && !hasAirDived && !isGrabbingWall && hasJumped)
            {
                PerformDive();
            }
        }

        if (jumpBufferCounter > 0f)
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }
    }

    private void CheckGround()
    {
        if (jumpCooldownTimer > 0f)
        {
            isGrounded = false;
            return;
        }

        if (collisionGrounded)
        {
            SetGroundedState(true);
            collisionGrounded = false;
            return;
        }

        Vector3 origin = (groundCheck != null) ? groundCheck.position + Vector3.up * 0.3f : transform.position;
        float checkRadius = groundCheckRadius;
        float checkDistance = groundCheckDistance;

        // Quando o personagem mergulha, o GroundCheck (que fica nos pés) rotaciona pra trás e pra cima
        // Isso faz ele perder a detecção de chão. Usamos o centro e aumentamos a distância!
        if (isDiving)
        {
            origin = transform.position;
            checkRadius = 0.5f;
            checkDistance = 0.6f;
        }

        RaycastHit hit;

        if (Physics.SphereCast(origin, checkRadius, Vector3.down, out hit, checkDistance, groundMask, QueryTriggerInteraction.Ignore) ||
            Physics.Raycast(origin, Vector3.down, out hit, checkDistance + checkRadius, groundMask, QueryTriggerInteraction.Ignore))
        {
            SetGroundedState(true);
            groundRigidbody = hit.rigidbody;
            groundCollider = hit.collider;
            groundContactPoint = hit.point;
            groundNormal = hit.normal;
        }
        else
        {
            SetGroundedState(false);
            groundRigidbody = null;
            groundCollider = null;
            groundNormal = Vector3.up;
        }
    }

    private void SetGroundedState(bool grounded)
    {
        bool wasGrounded = isGrounded;
        isGrounded = grounded;

        if (isGrounded)
        {
            hasAirDived = false;
            hasJumped = false;
            isGrabbingWall = false;
            wallGrabGraceTimer = 0f;
            lastWallJumpNormal = Vector3.zero; // Libera o pulo na parede

            // Aterrissagem do mergulho
            if (isDiving && !wasGrounded && slideRecoveryTimer <= 0f)
            {
                slideRecoveryTimer = slideRecoveryTime;
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (jumpCooldownTimer > 0f) return;

        bool touchedWallThisFrame = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            
            // Chão (tolerância para cilindro)
            if (contact.normal.y > 0.25f)
            {
                SetGroundedState(true);
                collisionGrounded = true;
                groundRigidbody = collision.rigidbody;
                groundCollider = contact.otherCollider;
                groundContactPoint = contact.point;
                groundNormal = contact.normal;
                return; // Ignora paredes se estiver pisando em chão
            }
            // Parede Vertical Agarrável
            else if (!isGrounded && contact.normal.y >= -0.1f && contact.normal.y <= 0.25f)
            {
                if ((grabbableMask.value & (1 << collision.gameObject.layer)) > 0)
                {
                    touchedWallThisFrame = true;
                    wallNormal = contact.normal;
                    horizontalFacing = -wallNormal;
                    collision.collider.TryGetComponent(out currentGrabbableSurface);
                }
            }
        }

        // Agarrar automático via colisão física
        if (touchedWallThisFrame && wallJumpCooldownTimer <= 0f && rb.linearVelocity.y <= 1.0f)
        {
            wallGrabGraceTimer = 0.15f;
            if (!isGrabbingWall)
            {
                isGrabbingWall = true;
                isDiving = false;
                hasAirDived = false;
                transform.rotation = Quaternion.LookRotation(horizontalFacing);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Se bater de frente em uma parede vertical enquanto mergulha sem se agarrar
        if (isDiving && !isGrounded)
        {
            for (int i = 0; i < collision.contactCount; i++)
            {
                ContactPoint contact = collision.GetContact(i);
                if (Mathf.Abs(contact.normal.y) < 0.3f)
                {
                    // Bonk na parede! Cancela a postura horizontal para não ficar preso deitado
                    isDiving = false;
                    transform.rotation = Quaternion.LookRotation(horizontalFacing);
                    break;
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (groundRigidbody == collision.rigidbody)
        {
            collisionGrounded = false;
            groundRigidbody = null;
        }
    }

    private void ReadInputs()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            jumpBufferCounter = 0f;
            isHoldingJump = false;
            return;
        }

        Vector2 input = Vector2.zero;
        isHoldingJump = false;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;

            if (Keyboard.current.spaceKey.isPressed) isHoldingJump = true;
            if (Keyboard.current.spaceKey.wasPressedThisFrame) jumpBufferCounter = jumpBufferTime;
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.04f) input = stick;

            if (Gamepad.current.buttonSouth.isPressed) isHoldingJump = true;
            if (Gamepad.current.buttonSouth.wasPressedThisFrame) jumpBufferCounter = jumpBufferTime;
        }

        moveInput = input.normalized;
    }

    private void MovePlayer()
    {
        Vector3 platformVelocity = Vector3.zero;
        if (isGrounded && groundRigidbody != null)
        {
            platformVelocity = groundRigidbody.GetPointVelocity(groundContactPoint);
            float platformYawSpeed = Vector3.Dot(groundRigidbody.angularVelocity, Vector3.up);
            if (Mathf.Abs(platformYawSpeed) > 0.001f)
            {
                Quaternion deltaYaw = Quaternion.Euler(0f, platformYawSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime, 0f);
                rb.MoveRotation(deltaYaw * rb.rotation);
            }
        }

        // --- ESTADO: MERGULHO / DIVE ---
        if (isDiving)
        {
            // Calcula o ângulo: 90 graus (deitado) até 0 graus (em pé)
            float pitchAngle = 90f;
            if (isGrounded && !isHoldingJump && slideRecoveryTimer > 0f)
            {
                float ratio = slideRecoveryTimer / slideRecoveryTime;
                
                // A divisão é: 80% do tempo inerte, 20% do tempo para erguer o corpo.
                // Como o ratio vai de 1.0 (início) a 0.0 (fim):
                // ratio > 0.20: ele fica totalmente imóvel deitado (InverseLerp retorna 1, pitch 90 graus).
                // ratio <= 0.20: reta final, ele se ergue até ficar de pé (InverseLerp de 1 para 0).
                float getUpRatio = Mathf.InverseLerp(0f, 0.20f, ratio);
                pitchAngle = Mathf.Lerp(0f, 90f, getUpRatio);
            }

            // Rotaciona o boneco na horizontal ou levantando gradualmente
            Quaternion baseRot = Quaternion.LookRotation(horizontalFacing);
            Quaternion diveRot = baseRot * Quaternion.Euler(pitchAngle, 0f, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, diveRot, 20f * Time.fixedDeltaTime);

            // Se o timer já ativou, quer dizer que ele tocou no chão!
            if (slideRecoveryTimer > 0f || isGrounded)
            {
                // Lê a fricção real do Physics Material do chão
                float groundFriction = 0.6f; // Fricção padrão da Unity
                if (groundCollider != null && groundCollider.sharedMaterial != null)
                {
                    groundFriction = groundCollider.sharedMaterial.dynamicFriction;
                }

                // Se o chão tiver fricção normal/alta (maior que 0.2), ele freia suavemente.
                // Se o level designer colocar um Physics Material liso (ex: 0.0), ele ignora o freio e escorrega!
                if (groundFriction > 0.2f)
                {
                    Vector3 currentVel = rb.linearVelocity;
                    // Desliza um pouco antes de parar completamente
                    currentVel.x = Mathf.Lerp(currentVel.x, 0f, 8f * Time.fixedDeltaTime);
                    currentVel.z = Mathf.Lerp(currentVel.z, 0f, 8f * Time.fixedDeltaTime);
                    rb.linearVelocity = currentVel + externalForce + platformVelocity;
                }
            }
            // No ar, a inércia do PerformDive/Gravidade age livremente
            return;
        }

        // --- ESTADO: MOVIMENTO NORMAL ---
        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            Vector3 moveDirection = (camForward * moveInput.y + camRight * moveInput.x).normalized;
            Vector3 targetVelocity = moveDirection * moveSpeed;

            rb.linearVelocity = new Vector3(
                targetVelocity.x + platformVelocity.x + externalForce.x,
                rb.linearVelocity.y,
                targetVelocity.z + platformVelocity.z + externalForce.z
            );

            horizontalFacing = moveDirection;
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = new Vector3(
                platformVelocity.x + externalForce.x,
                rb.linearVelocity.y,
                platformVelocity.z + externalForce.z
            );

            // Garante que o personagem fique de pé suavemente
            if (horizontalFacing.sqrMagnitude > 0.01f)
            {
                Quaternion standRot = Quaternion.LookRotation(horizontalFacing);
                transform.rotation = Quaternion.Slerp(transform.rotation, standRot, rotationSpeed * Time.fixedDeltaTime);
            }
        }
    }

    private void HandleWallGrab()
    {
        // 1. Velocidade Vertical (Deslize com controle Up/Down)
        float slideSpeed = currentGrabbableSurface != null ? currentGrabbableSurface.customSlideSpeed : wallSlideSpeed;
        if (moveInput.y > 0.1f) slideSpeed *= 0.2f;       // Cima: freia bastante (ex: -0.5f)
        else if (moveInput.y < -0.1f) slideSpeed *= 3.0f; // Baixo: escorrega muito rápido (ex: -7.5f)

        float newY = Mathf.MoveTowards(rb.linearVelocity.y, slideSpeed, 20f * Time.fixedDeltaTime);

        // 2. Movimento Horizontal pela parede (Esquerda/Direita)
        Vector3 wallRight = Vector3.ProjectOnPlane(cameraTransform.right, wallNormal).normalized;
        if (wallRight.sqrMagnitude < 0.01f) wallRight = Vector3.Cross(wallNormal, Vector3.up).normalized;

        Vector3 moveAlongWall = wallRight * moveInput.x * wallWalkHorizontalSpeed;

        // 3. Força contra a parede para manter a detecção física ativa
        Vector3 stickForce = -wallNormal * 2.5f;

        rb.linearVelocity = new Vector3(
            stickForce.x + moveAlongWall.x + externalForce.x, 
            newY, 
            stickForce.z + moveAlongWall.z + externalForce.z
        );

        // Mantém o personagem de pé e virado para a parede
        Quaternion targetRotation = Quaternion.LookRotation(-wallNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        jumpBufferCounter = 0f;
        jumpCooldownTimer = jumpCooldown;
        SetGroundedState(false);
        collisionGrounded = false;
        hasJumped = true;

        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
    }

    private void PerformDive()
    {
        jumpBufferCounter = 0f;
        hasAirDived = true;
        isDiving = true;
        isGrabbingWall = false;

        Vector3 diveDir = horizontalFacing;
        if (moveInput.magnitude >= 0.1f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0; camRight.y = 0;
            diveDir = (camForward.normalized * moveInput.y + camRight.normalized * moveInput.x).normalized;
        }

        horizontalFacing = diveDir;
        transform.rotation = Quaternion.LookRotation(diveDir);

        rb.linearVelocity = new Vector3(diveDir.x * diveForwardForce, diveUpwardBoost, diveDir.z * diveForwardForce);
    }

    private void WallJump()
    {
        if (currentGrabbableSurface != null && !currentGrabbableSurface.allowWallJump)
            return;

        jumpBufferCounter = 0f;
        isGrabbingWall = false;
        wallGrabGraceTimer = 0f;
        
        // Cooldown curtinho (0.15s) só pro motor de física não bugar nos primeiros frames
        wallJumpCooldownTimer = 0.15f; 

        hasAirDived = false;
        isDiving = false;
        hasJumped = true;

        // Registra a parede que usamos para pular, assim o jogador não pode grudar nela de novo!
        lastWallJumpNormal = wallNormal;

        Vector3 jumpDir = wallNormal;
        horizontalFacing = jumpDir;
        transform.rotation = Quaternion.LookRotation(jumpDir);
        
        // O segredo de um Wall Jump super fluído:
        // A força horizontal vira uma 'externalForce' que decai gradualmente.
        // O MovePlayer soma o seu WASD a essa externalForce. 
        // Assim você tem total controle no ar, mas uma força gigante te atira da parede inicialmente!
        externalForce = jumpDir * wallJumpHorizontalForce;

        // O pulo vertical vai direto no Rigidbody
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, wallJumpVerticalForce, rb.linearVelocity.z);
    }

    public void AddImpact(Vector3 force)
    {
        externalForce += force;
    }

    public void SetInputActive(bool active)
    {
        canMove = active;
        if (!active)
        {
            moveInput = Vector2.zero;
            jumpBufferCounter = 0f;
        }
    }

    /// <summary>
    /// Reseta completamente o estado físico e postural do jogador (usado ao morrer/respawnar).
    /// </summary>
    public void ResetState()
    {
        isDiving = false;
        hasAirDived = false;
        slideRecoveryTimer = 0f;
        isGrabbingWall = false;
        wallJumpCooldownTimer = 0f;
        jumpCooldownTimer = 0f;
        jumpBufferCounter = 0f;
        wallGrabGraceTimer = 0f;
        lastWallJumpNormal = Vector3.zero;
        externalForce = Vector3.zero;

        // Garante que o corpo volte à posição vertical imediatamente
        Vector3 forwardPlane = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        if (forwardPlane.sqrMagnitude < 0.01f) forwardPlane = Vector3.forward;
        horizontalFacing = forwardPlane;
        transform.rotation = Quaternion.LookRotation(forwardPlane);

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    /// <summary>
    /// Teleporta o jogador para a posição de respawn e restaura a postura vertical e física limpa.
    /// </summary>
    public void Respawn(Vector3 position)
    {
        transform.position = position;
        ResetState();
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = (groundCheck != null) ? groundCheck.position + Vector3.up * 0.3f : transform.position;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, groundCheckRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
