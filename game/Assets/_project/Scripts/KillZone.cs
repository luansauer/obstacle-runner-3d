using UnityEngine;

public class KillZone : MonoBehaviour
{
    [Header("Referência de Respawn")]
    [Tooltip("Ponto ou coordenadas para onde o jogador deve retornar ao cair")]
    [SerializeField] private Vector3 respawnPosition = new Vector3(0, 1.5f, -40f);

    private void OnTriggerEnter(Collider other)
    {
        HandleKill(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleKill(collision.gameObject);
    }

    private void HandleKill(GameObject target)
    {
        if (target.CompareTag("Player"))
        {
            if (target.TryGetComponent<PlayerController>(out var player))
            {
                player.Die();
            }
            else
            {
                if (target.TryGetComponent<Rigidbody>(out var rb))
                {
                    rb.linearVelocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                target.transform.position = respawnPosition;
            }
        }
    }
}