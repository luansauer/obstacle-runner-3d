# 📝 02 - Devlog: Obstacle Runner 3D

Registro contínuo de progresso, decisões técnicas, problemas encontrados e soluções implementadas ao longo do projeto.

---

## 📅 Sessão 01 — Planejamento, GDD e Infraestrutura
### O que foi feito:
* Definição do escopo: jogo de plataforma 3D focado em física (estilo Fall Guys).
* Escolha da stack técnica: Unity 6 (URP) + C# + Antigravity.
* Repositório Git criado no GitHub (`obstacle-runner-3d`) com template de `.gitignore` para Unity.
* Quadro Kanban configurado no GitHub Projects com issues iniciais de fundação.
* Estrutura de notas configurada no cofre do Obsidian.

---

## 📅 Sessão 02 — Setup Unity 6, Cinemachine 3 e Obstáculo Giratório
### O que foi feito:
* Configuração do projeto no Unity 6 URP e importação do New Input System e Cinemachine 3.
* Criação da cena de testes (passarela de greybox, iluminação básica e cápsula do jogador).
* Implementação do `PlayerController.cs` com detecção de chão estável via `SphereCast`.
* Calibração da câmera orbital em 3ª pessoa:
  * Resolução do bloqueio de órbita 360° (ajuste de `Target Offset Z: 0` e ativação do `Wrap` no Horizontal Axis).
  * Implementação da órbita de 3 anéis (*Three Ring*): `Top` (6, 6.5), `Center` (2.5, 6.0) e `Bottom` (-2.0, 2.0), permitindo que a câmera mergulhe até o chão e se aproxime dos pés do boneco.
  * Ajuste de foco no `Rotation Composer` com `Target Offset Y: 0.7` para manter a visão limpa.
* Criação do obstáculo giratório (`Rotator.cs`) com cilindro cinemático.
* Criação do sistema de respawn básico (`KillZone.cs` no `KillFloor`).
* Criação do material de física `HighFrictionMat` (`Friction Combine: Maximum`) para garantir aderência ao obstáculo.
* Atualização do `PlayerController.cs` para herdar velocidade tangencial via `Rigidbody.GetPointVelocity()` ao pisar no cilindro em movimento.

### Dificuldades & Soluções:
* **Problema:** A câmera colidia e esmagava o raio ao tentar girar para a frente do personagem.
  * **Solução:** O `Target Offset Z` estava negativo empurrando a esfera para trás. Foi zerado o Z e configurado o raio puro no componente Orbital Follow.
* **Problema:** Ao pisar no cilindro giratório, o boneco deslizava e não era arrastado pela rotação.
  * **Solução:** O script do player zerava a velocidade horizontal a cada frame sem input. Foi adicionada a captura do `hit.rigidbody` no `SphereCast` somando `platformVelocity` ao vetor de movimento, além da aplicação de Physics Material com atrito máximo.

---

## 📅 Próximos Passos (Sessão 03 — Próximo Desenvolvimento)
- [ ] Aplicar o `HighFrictionMat` nos colliders do Player e do ObstacleSpinner na cena local.
- [ ] Fazer commit e push das alterações da Sessão 02 no Git (`main`).
- [ ] Criar o obstáculo de martelo pendular oscilante (*Pendulum*).
- [ ] Implementar o sistema modular de Checkpoints vinculado ao `KillZone.cs`.
- [ ] Criar trigger de Linha de Chegada e tela/lógica de vitória.

---

## Links
* Voltar para [[docs/00 - GDD]]
* Acessar [[docs/01 - Arquitetura & Scripts]]