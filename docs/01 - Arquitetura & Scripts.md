# 01 - Arquitetura Técnica & Scripts

Documentação técnica dos scripts centrais, padrões de física via PhysX e calibração de câmera do projeto.

---

## 1. Padrões de Projeto & Boas Práticas

* **Ciclo de Atualização:** Leituras de input tratadas exclusivamente no `Update()`; cálculos vetoriais e forças aplicadas no `FixedUpdate()`.
* **Encapsulamento:** Variáveis configuráveis utilizam `[SerializeField] private` com tooltips explicativos.
* **Desacoplamento:** Componentes modulares focados em responsabilidade única (`PlayerController`, `Rotator`, `KillZone`).

---

## 2. Scripts do Projeto

### 2.1 PlayerController.cs
Controla a movimentação 3D orientada pela visão da câmera, pulo com checagem estável de chão e herança tangencial de velocidade de plataformas/obstáculos móveis via `Rigidbody`.

#### Código C#
```csharp
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float rotationSpeed = 12f;

    [Header("Pulo & Chão")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private float groundCheckDistance = 0.35f;
    [SerializeField] private LayerMask groundMask;

    [Header("Estado")]
    [SerializeField] private bool canMove = true;

    [Header("Câmera")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private Vector2 moveInput;
    private bool isGrounded;
    private float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;
    private float jumpCooldown = 0.1f;
    private float jumpCooldownTimer;

    // Suporte a plataformas / superfícies móveis e giratórias
    private Rigidbody groundRigidbody;
    private Vector3 groundContactPoint;
    private bool collisionGrounded;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
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
        if (jumpCooldownTimer > 0f)
        {
            jumpCooldownTimer -= Time.fixedDeltaTime;
        }

        MovePlayer();

        if (jumpBufferCounter > 0f && isGrounded && jumpCooldownTimer <= 0f)
        {
            Jump();
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

        // Se OnCollisionStay detectou chão estável no FixedUpdate anterior
        if (collisionGrounded)
        {
            isGrounded = true;
            collisionGrounded = false; // Reset para o próximo ciclo de física
            return;
        }

        Vector3 origin;
        if (groundCheck != null)
        {
            // Começa 0.3m ACIMA do pé para nunca iniciar dentro do colisor (evita o bug de overlap inicial)
            origin = groundCheck.position + Vector3.up * 0.3f;
        }
        else
        {
            origin = transform.position;
        }

        RaycastHit hit;
        if (Physics.SphereCast(origin, groundCheckRadius, Vector3.down, out hit, groundCheckDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            isGrounded = true;
            groundRigidbody = hit.rigidbody;
            groundContactPoint = hit.point;
        }
        else if (Physics.Raycast(origin, Vector3.down, out hit, groundCheckDistance + groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore))
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

    private void OnCollisionStay(Collision collision)
    {
        if (jumpCooldownTimer > 0f) return;

        // Avalia os pontos de contato físicos
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint contact = collision.GetContact(i);
            // Normal apontando para cima (chão ou topo de obstáculo transitável)
            if (contact.normal.y > 0.45f)
            {
                isGrounded = true;
                collisionGrounded = true;
                groundRigidbody = collision.rigidbody;
                groundContactPoint = contact.point;
                break;
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
            return;
        }

        Vector2 input = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) input.y += 1f;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) input.y -= 1f;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) input.x -= 1f;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) input.x += 1f;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                jumpBufferCounter = jumpBufferTime;
            }
        }

        if (Gamepad.current != null)
        {
            Vector2 stick = Gamepad.current.leftStick.ReadValue();
            if (stick.sqrMagnitude > 0.04f)
            {
                input = stick;
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                jumpBufferCounter = jumpBufferTime;
            }
        }

        moveInput = input.normalized;
    }

    private void MovePlayer()
    {
        Vector3 platformVelocity = Vector3.zero;
        if (isGrounded && groundRigidbody != null)
        {
            platformVelocity = groundRigidbody.GetPointVelocity(groundContactPoint);

            // Herança de rotação angular da plataforma (para carrosséis / plataformas giratórias no eixo Y)
            float platformYawSpeed = Vector3.Dot(groundRigidbody.angularVelocity, Vector3.up);
            if (Mathf.Abs(platformYawSpeed) > 0.001f)
            {
                Quaternion deltaYaw = Quaternion.Euler(0f, platformYawSpeed * Mathf.Rad2Deg * Time.fixedDeltaTime, 0f);
                rb.MoveRotation(deltaYaw * rb.rotation);
            }
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

            // Soma a velocidade do jogador à velocidade tangencial da superfície
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
            // Transfere o impulso do chão mesmo sem comandos do jogador
            rb.linearVelocity = new Vector3(
                platformVelocity.x,
                rb.linearVelocity.y,
                platformVelocity.z
            );
        }
    }

    private void Jump()
    {
        jumpBufferCounter = 0f;
        jumpCooldownTimer = jumpCooldown;
        isGrounded = false;
        collisionGrounded = false;

        // Pulo consistente e responsivo (define a velocidade vertical mantendo o momento horizontal)
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce, rb.linearVelocity.z);
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

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = (groundCheck != null) ? groundCheck.position + Vector3.up * 0.3f : transform.position;
        Gizmos.color = isGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(origin, groundCheckRadius);
        Gizmos.DrawLine(origin, origin + Vector3.down * groundCheckDistance);
        Gizmos.DrawWireSphere(origin + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
```

### 2.2 Rotator.cs

Controla a rotação de obstáculos cinemáticos (como cilindros horizontais ou hélices). Aplica rotação via `Rigidbody.MoveRotation` dentro do ciclo físico para transmitir momento linear aos corpos que colidem com a superfície.

  

#### Código C#

```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rotator : MonoBehaviour
{
    [Header("Configuração de Rotação")]
    [Tooltip("Eixo de rotação local normalizado")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0);

    [Tooltip("Velocidade de rotação em graus por segundo")]
    [SerializeField] private float speed = 90f;

    [Tooltip("Se marcado, a rotação usa os eixos locais do objeto; se desmarcado, usa eixos globais.")]
    [SerializeField] private bool useLocalAxis = true;

    private Rigidbody rb;

    public float Speed => speed;
    public Vector3 RotationAxis => rotationAxis;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        Vector3 worldAxis;
        if (useLocalAxis)
        {
            Quaternion deltaRotation = Quaternion.Euler(rotationAxis.normalized * (speed * Time.fixedDeltaTime));
            rb.MoveRotation(rb.rotation * deltaRotation);
            worldAxis = transform.TransformDirection(rotationAxis.normalized);
        }
        else
        {
            worldAxis = rotationAxis.normalized;
            Quaternion deltaRotation = Quaternion.Euler(worldAxis * (speed * Time.fixedDeltaTime));
            rb.MoveRotation(deltaRotation * rb.rotation);
        }

        // Define a velocidade angular para transmissão de momento ao Player (GetPointVelocity)
        rb.angularVelocity = worldAxis * (speed * Mathf.Deg2Rad);
    }
}
```

### 2.3 KillZone.cs

Acoplado a um colisor com `Is Trigger = true`, posicionado na zona de queda (`Y: -15`). Detecta a queda do jogador, anula sua velocidade acumulada e o reposiciona no ponto de spawn ativo.

  

#### Código C#

```csharp
using UnityEngine;

public class KillZone : MonoBehaviour
{
    [Header("Referência de Respawn")]
    [Tooltip("Ponto ou coordenadas para onde o jogador deve retornar ao cair")]
    [SerializeField] private Vector3 respawnPosition = new Vector3(0, 1.5f, -40f);

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            other.transform.position = respawnPosition;
        }
    }

    // Permite atualização dinâmica por checkpoints futuros
    public void SetRespawnPosition(Vector3 newPosition)
    {
        respawnPosition = newPosition;
    }
}
```


### 2.4 FinishLine.cs

Gatilho posicionado na linha de chegada para marcar a conclusão do percurso. Detecta o jogador via tag, dispara eventos desacoplados via `UnityEvent` e opcionalmente desativa os comandos de movimentação do jogador.

#### Código C#

```csharp
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class FinishLine : MonoBehaviour
{
    [Header("Configurações do Gatilho")]
    [Tooltip("Tag usada para identificar o jogador")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Desativa os comandos do jogador ao cruzar a linha de chegada")]
    [SerializeField] private bool disablePlayerControlsOnFinish = true;

    [Header("Eventos de Conclusão")]
    [Tooltip("Ações disparadas quando o jogador alcança a linha de chegada")]
    [SerializeField] private UnityEvent onPlayerFinished;

    [Header("Debug Visual")]
    [SerializeField] private Color gizmoColor = new Color(0f, 1f, 0f, 0.4f);

    private bool isFinished = false;

    public bool IsFinished => isFinished;

    private void Awake()
    {
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            col.isTrigger = true;
            Debug.LogWarning($"[FinishLine] O Collider em '{gameObject.name}' foi alterado para 'Is Trigger = true' automaticamente.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isFinished) return;

        if (other.CompareTag(playerTag))
        {
            isFinished = true;
            Debug.Log("[FinishLine] 🏆 O jogador cruzou a linha de chegada!");

            if (disablePlayerControlsOnFinish)
            {
                if (other.TryGetComponent<PlayerController>(out var playerController))
                {
                    playerController.SetInputActive(false);
                }

                if (other.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                }
            }

            onPlayerFinished?.Invoke();
        }
    }

    public void ResetFinishLine()
    {
        isFinished = false;
    }

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col == null) return;

        Gizmos.color = gizmoColor;
        Gizmos.matrix = transform.localToWorldMatrix;

        if (col is BoxCollider box)
        {
            Gizmos.DrawCube(box.center, box.size);
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(box.center, box.size);
        }
    }
}
```

## 3. Calibração da Câmera Orbital (Cinemachine 3)

Configuração no `CinemachineCamera` para visão em terceira pessoa estável sem travamento de ângulo:

  

- **Binding Mode:** `World Space`
    
      
    
- **Rotation Control:** `Rotation Composer` com `Target Offset Y = 0.7`
    
      
    
- **Horizontal Axis:** Range `-180` a `180` com `Wrap: Ativado`
    
      
    
- **Vertical Axis:** Range `-25` a `60` com `Wrap: Desativado`
    
      
    
- **Three Ring Orbit:**
    
      
    - **Top:** Height `6.0` | Radius `6.5`
        
          
        
    - **Center:** Height `2.5` | Radius `6.0`
        
          
        
    - **Bottom:** Height `-2.0` | Radius `2.0` (aproximação automática nos calcanhares ao olhar rente ao chão)
        
          
        

## 4. Atrito & Materiais Físicos

Para eliminar escorregamentos involuntários nas quinas do cilindro giratório:

  

- **Physics Material:** `HighFrictionMat` aplicado no `Capsule Collider` do Player e no do obstáculo.
    
      
    
- **Valores:** `Dynamic Friction: 1.0`, `Static Friction: 1.0`, `Friction Combine: Maximum`.
    
      
    

## Links

- Voltar para [[docs/00 - GDD]]
    
      
    
- Ir para [[docs/02 - Devlog]]
### 2.5 Mec�nicas Avan�adas (Sess�o 04)
- **Air Dive**: Implementado em `PlayerController` como pulo duplo (`PerformDive`), aplicando impulso horizontal e pequeno *boost* vertical.
- **Wall Grab & Slide**: Detectado via `SphereCast` contra a `grabbableMask`. Congela a velocidade vertical em `wallSlideSpeed` ou baseia-se no script `GrabbableSurface.cs`, permitindo `WallJump`.
- **Knockback/Impacto**: For�as externas s�o agora tratadas com decaimento exponencial, permitindo que obst�culos como p�ndulos arremessem o jogador sem serem imediatamente anulados por `targetVelocity`.
