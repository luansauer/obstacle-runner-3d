using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    [Header("Configurações do Checkpoint")]
    [Tooltip("Ponto exato onde o jogador vai renascer. Se deixar vazio, usa a própria posição desta bandeira/plataforma.")]
    public Transform spawnPoint;
    
    [Header("Feedback Visual")]
    [Tooltip("Modelos 3D da bandeira ou luzes para trocarem de cor ao ser ativado.")]
    public Renderer[] visualRenderers;
    [Tooltip("Material verde ou neon que indica que o checkpoint foi salvo.")]
    public Material activatedMaterial;

    private bool isActivated = false;

    private void Awake()
    {
        // Garante automaticamente que o collider está em modo Trigger
        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isActivated) return; // Ignora se já pegamos esse checkpoint

        if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent<PlayerController>(out var player))
            {
                isActivated = true;
                
                // Pega a posição do spawnPoint, se não houver, pega o centro do próprio objeto
                Vector3 respawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                
                // Avisa ao jogador que o ponto de retorno mudou!
                player.SetRespawnPoint(respawnPos);

                // Feedback visual de sucesso
                if (visualRenderers != null && activatedMaterial != null)
                {
                    foreach (Renderer rend in visualRenderers)
                    {
                        if (rend != null)
                        {
                            rend.material = activatedMaterial;
                        }
                    }
                }

                Debug.Log($"Checkpoint alcançado! Novo Respawn em: {respawnPos}");
            }
        }
    }
}
