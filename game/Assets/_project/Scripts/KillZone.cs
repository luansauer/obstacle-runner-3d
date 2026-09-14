using UnityEngine;

public class KillZone : MonoBehaviour
{
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
}