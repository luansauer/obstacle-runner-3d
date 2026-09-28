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

    /// <summary>
    /// Permite reiniciar o estado da linha de chegada (útil para reiniciar partidas).
    /// </summary>
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
