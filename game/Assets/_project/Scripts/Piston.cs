using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Piston : MonoBehaviour
{
    [Header("Configurações do Movimento")]
    [Tooltip("Direção local para onde o soco é dado (ex: X=0, Y=0, Z=1 para avançar na sua frente).")]
    public Vector3 punchDirection = Vector3.forward;
    
    [Tooltip("Distância total que a parede avança.")]
    public float punchDistance = 5f;
    
    [Tooltip("Velocidade do 'soco' (bem rápido).")]
    public float punchSpeed = 25f;
    
    [Tooltip("Velocidade de recuo (lento).")]
    public float retractSpeed = 3f;
    
    [Tooltip("Tempo que ele fica parado antes de socar novamente.")]
    public float waitTime = 2f;
    
    [Tooltip("Tempo de atraso inicial (para dessincronizar vários pistões numa parede).")]
    public float startDelay = 0f;

    [Header("Força de Impacto")]
    [Tooltip("Força com que arremessa o jogador para longe caso encoste na parede em movimento.")]
    public float knockbackForce = 35f;
    
    [Tooltip("Bônus de altura ao ser atingido para o jogador voar.")]
    public float upwardBoost = 8f;

    private Rigidbody rb;
    private Vector3 startPosition;
    private Vector3 targetPosition;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        // Pistões precisam ser Kinematic para varrerem o espaço fisicamente 
        // e atropelarem o jogador, sem serem parados pela gravidade.
        rb.isKinematic = true; 
        
        startPosition = transform.position;
        // Calcula a posição exata de até onde o soco vai chegar
        targetPosition = startPosition + transform.TransformDirection(punchDirection.normalized) * punchDistance;
    }

    private void Start()
    {
        // Inicia a rotina infinita de socar e voltar
        StartCoroutine(PistonRoutine());
    }

    private IEnumerator PistonRoutine()
    {
        // Atraso inicial para criar padrões legais se colocar vários lado a lado
        if (startDelay > 0) yield return new WaitForSeconds(startDelay);

        while (true)
        {
            // 1. Espera antes do soco
            yield return new WaitForSeconds(waitTime);

            // 2. SOCO RÁPIDO! Vai movendo até chegar no alvo
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                // Usamos Time.fixedDeltaTime e rb.MovePosition para garantir que a física empurre o jogador certinho
                Vector3 newPos = Vector3.MoveTowards(transform.position, targetPosition, punchSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }

            // Garante alinhamento final no alvo
            rb.MovePosition(targetPosition);

            // Pequena pausa com o soco esticado
            yield return new WaitForSeconds(0.2f);

            // 3. RECUO LENTO! Volta para a origem
            while (Vector3.Distance(transform.position, startPosition) > 0.01f)
            {
                Vector3 newPos = Vector3.MoveTowards(transform.position, startPosition, retractSpeed * Time.fixedDeltaTime);
                rb.MovePosition(newPos);
                yield return new WaitForFixedUpdate();
            }

            // Garante alinhamento final na base
            rb.MovePosition(startPosition);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (collision.gameObject.TryGetComponent<PlayerController>(out var player))
            {
                // Descobre a direção oposta do impacto
                Vector3 hitDirection = collision.GetContact(0).normal * -1f;
                hitDirection.y = 0f; // Queremos controlar o eixo Y manualmente
                hitDirection.Normalize();

                Vector3 finalForce = (hitDirection * knockbackForce) + (Vector3.up * upwardBoost);
                player.AddImpact(finalForce);
            }
        }
    }
}

