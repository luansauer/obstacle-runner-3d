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

        // Essencial: define a velocidade angular para que o PhysX e o PlayerController 
        // consigam calcular GetPointVelocity() e a rotação tangencial da superfície
        rb.angularVelocity = worldAxis * (speed * Mathf.Deg2Rad);
    }
}
