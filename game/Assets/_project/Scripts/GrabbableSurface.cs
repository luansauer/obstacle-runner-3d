using UnityEngine;

public class GrabbableSurface : MonoBehaviour
{
    [Header("Configurações de Deslizamento")]
    [Tooltip("Velocidade máxima de descida vertical enquanto agarrado a esta superfície.")]
    public float customSlideSpeed = -2.5f;

    [Tooltip("Permite pular (Wall Jump) a partir desta superfície?")]
    public bool allowWallJump = true;

    [Header("Debug")]
    [Tooltip("Cor do Gizmo no Editor para identificar a superfície agarrável.")]
    [SerializeField] private Color debugColor = new Color(0.2f, 0.8f, 0.2f, 0.3f);

    private void OnDrawGizmos()
    {
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = debugColor;
            Gizmos.matrix = transform.localToWorldMatrix;

            if (col is BoxCollider box)
            {
                Gizmos.DrawCube(box.center, box.size);
            }
            else if (col is MeshCollider meshCollider && meshCollider.sharedMesh != null)
            {
                Gizmos.DrawMesh(meshCollider.sharedMesh);
            }
        }
    }
}

