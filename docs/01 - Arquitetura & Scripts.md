
## 1. PlayerController.cs
Controla a movimentação tridimensional do personagem orientada pela visão da câmera, pulo com checagem de chão e física via Rigidbody.

### Código Base
```csharp
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimentação")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Pulo & Chão")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.2f;
    [SerializeField] private LayerMask groundMask;

    [Header("Câmera")]
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private Vector2 inputVector;
    private bool isGrounded;
    private bool jumpRequested;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Evita que colisões físicas tombem o personagem
        rb.freezeRotation = true;
    }

    private void Update()
    {
        // 1. Leitura de inputs (sempre no Update)
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        inputVector = new Vector2(horizontal, vertical).normalized;

        // 2. Verificação de chão via esfera
        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        }

        // 3. Captura do pulo
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            jumpRequested = true;
        }
    }

    private void FixedUpdate()
    {
        // Movimentação e física (sempre no FixedUpdate)
        MovePlayer();

        if (jumpRequested)
        {
            Jump();
            jumpRequested = false;
        }
    }

    private void MovePlayer()
    {
        if (inputVector.magnitude >= 0.1f)
        {
            // Direção baseada na orientação da câmera
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0;
            camRight.y = 0;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDirection = (camForward * inputVector.y + camRight * inputVector.x).normalized;

            // Aplica velocidade horizontal mantendo a velocidade vertical (gravidade/pulo)
            Vector3 targetVelocity = moveDirection * moveSpeed;
            rb.linearVelocity = new Vector3(targetVelocity.x, rb.linearVelocity.y, targetVelocity.z);

            // Suaviza a rotação do personagem para onde ele está andando
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // Zera o deslize horizontal quando não há input
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }
}

using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0, 90f, 0);

    private void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
```
``
## 2. Rotator.cs

Script C# acoplado aos obstáculos móveis (como cilindros horizontais ou hélices). Gira o objeto continuamente no próprio eixo usando o tempo delta para manter rotação constante independente da taxa de quadros (FPS).

### Código C#

C#

```
using UnityEngine;

public class Rotator : MonoBehaviour
{
    [Header("Configuração de Rotação")]
    [Tooltip("Eixos e velocidade de rotação em graus por segundo")]
    [SerializeField] private Vector3 rotationSpeed = new Vector3(0f, 90f, 0f);

    private void Update()
    {
        // Aplica rotação contínua proporcional ao tempo do frame
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}
```

## 3. Killzone.cs

Script C# acoplado a um colisor com a caixa **Is Trigger** ativada, posicionado logo abaixo da pista. Detecta quando o jogador cai no abismo, anula sua velocidade acumulada e o reposiciona no ponto de spawn.

### Código C#

C#

```
using UnityEngine;

public class Killzone : MonoBehaviour
{
    [Header("Referência de Respawn")]
    [Tooltip("Objeto vazio que marca a posição onde o jogador deve reaparecer")]
    [SerializeField] private Transform spawnPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica se o objeto que caiu possui a tag Player
        if (other.CompareTag("Player"))
        {
            // Zera qualquer inércia e aceleração residual da queda
            Rigidbody playerRb = other.GetComponent<Rigidbody>();
            if (playerRb != null)
            {
                playerRb.linearVelocity = Vector3.zero;
                playerRb.angularVelocity = Vector3.zero;
            }

            // Reposiciona o jogador no ponto de spawn
            other.transform.position = spawnPoint.position;
        }
    }
}
```

## Links

- Voltar para [[00 - GDD]]
    
- Ir para [[02 - Devlog]]