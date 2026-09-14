using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Rotator : MonoBehaviour
{
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0); // Eixo Y global
    [SerializeField] private float speed = 90f; // Graus por segundo

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