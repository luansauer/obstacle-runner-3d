using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pendulum : MonoBehaviour
{
    [Header("Pendulum Settings")]
    [Tooltip("Eixo de rotação local do pêndulo. Normalmente (0, 0, 1) para balançar como um relógio.")]
    public Vector3 swingAxis = Vector3.forward;
    
    [Tooltip("Ângulo máximo de inclinação para cada lado (em graus).")]
    public float maxAngle = 75f;
    
    [Tooltip("Velocidade da oscilação.")]
    public float swingSpeed = 2f;
    
    [Tooltip("Deslocamento de tempo para dessincronizar vários pêndulos na mesma fase.")]
    public float timeOffset = 0f;

    [Header("Knockback Settings")]
    [Tooltip("Força com que o pêndulo arremessa o jogador para longe no impacto.")]
    public float knockbackForce = 25f;
    
    [Tooltip("Pequeno empurrão vertical extra para fazer o jogador 'voar' ao tomar o hit.")]
    public float upwardBoost = 8f;

    private Rigidbody rb;
    private Quaternion initialRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Pêndulos precisam ser Kinematic para ignorarem a gravidade padrão 
        // e se moverem perfeitamente pela nossa matemática senoidal
        rb.isKinematic = true; 
        
        initialRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        // A função Mathf.Sin vai de -1 a 1 suavemente ao longo do tempo.
        // Multiplicando pelo maxAngle, garantimos que o martelo vá e volte.
        float currentAngle = maxAngle * Mathf.Sin((Time.time + timeOffset) * swingSpeed);
        
        // Criamos o novo ângulo em torno do eixo escolhido (swingAxis)
        Quaternion deltaRotation = Quaternion.AngleAxis(currentAngle, swingAxis.normalized);
        
        // Movemos fisicamente o pêndulo, garantindo que objetos em cima ou batendo nele reajam fisicamente
        rb.MoveRotation(initialRotation * deltaRotation);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlayerController>(out var player))
            {
                // Descobre a direção que o pêndulo estava indo em relação ao jogador
                Vector3 hitDirection = collision.GetContact(0).normal * -1f;
                hitDirection.y = 0f; // Zera o Y inicial para normalizar apenas o empurrão 2D
                hitDirection.Normalize();

                // Adiciona o empurrão pra cima (upwardBoost) e a força pra trás (knockbackForce)
                Vector3 finalForce = (hitDirection * knockbackForce) + (Vector3.up * upwardBoost);

                player.AddImpact(finalForce);
            }
        }
    }
}
