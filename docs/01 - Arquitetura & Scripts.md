````
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

    // Herança de velocidade para superfícies e obstáculos móveis
    private Rigidbody groundRigidbody;
    private Vector3 groundContactPoint;

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

        // Leitura via Teclado
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

        // Leitura via Gamepad
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
        Vector3 platformVelocity = Vector3.zero;
        if (isGrounded && groundRigidbody != null)
        {
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

            // Combina o vetor do jogador à velocidade tangencial da superfície
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
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}
````

### 2.2 Rotator.cs

Controla a rotação de obstáculos cinemáticos (como cilindros horizontais ou hélices). Aplica rotação via `Rigidbody.MoveRotation` dentro do ciclo físico para transmitir momento linear aos corpos que colidem com a superfície.

  

#### Código C#

C#

```
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rotator : MonoBehaviour
{
    [Header("Configuração de Rotação")]
    [Tooltip("Eixo de rotação local ou global normalizado")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0);

    [Tooltip("Velocidade de rotação em graus por segundo")]
    [SerializeField] private float speed = 90f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
    }

    private void FixedUpdate()
    {
        Quaternion deltaRotation = Quaternion.Euler(rotationAxis.normalized * (speed * Time.fixedDeltaTime));
        rb.MoveRotation(rb.rotation * deltaRotation);
    }
}
```

### 2.3 KillZone.cs

Acoplado a um colisor com `Is Trigger = true`, posicionado na zona de queda (`Y: -15`). Detecta a queda do jogador, anula sua velocidade acumulada e o reposiciona no ponto de spawn ativo.

  

#### Código C#

C#

```
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