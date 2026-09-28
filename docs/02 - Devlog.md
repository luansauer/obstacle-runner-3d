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

## 📌 Sessão 03 — Refatoração de Pastas, Linha de Chegada e Bloqueio de Input
### O que foi feito:
* Refatoração da estrutura do repositório: pasta do projeto Unity renomeada de `Obstacle Runner 3D` para `game/` para eliminar redundâncias e evitar conflitos com caminhos contendo espaços.
* Correção e ajuste definitivo dos arquivos `.gitignore` (isolando `Library/`, `Logs/`, `UserSettings/` e `.obsidian/`).
* Atualização do `README.md` com visão geral, controles e stack técnica.
* Implementação do script `FinishLine.cs` com trigger de chegada, validação de tag, disparo de `UnityEvent` de vitória e renderização de Gizmo no Editor.
* Atualização do `PlayerController.cs` com o método `SetInputActive(bool)`, permitindo congelar o movimento do personagem ao cruzar a chegada ou durante contagens regressivas.
* **Status do Roadmap:** Issue **#5 (Triggers de Killzone e Linha de Chegada)** concluída.

---

## 📅 Sessão 04 — Mecânicas Avançadas de Movimentação
### O que foi feito:
* Implementação do sistema de **Mergulho (Air Dive)** no `PlayerController.cs`: ativado ao pressionar Pulo novamente enquanto no ar. Confere impulso frontal com breve recuperação ao tocar o chão.
* Implementação do sistema de **Wall Grab & Slide**: agarrar paredes segurando `Shift`, `E` ou Gatilho do Gamepad. Desacelera a queda e permite **Wall Jump**.
* Criação do script `GrabbableSurface.cs` para customizar fricção/deslizamento de paredes específicas.
* Adição de suporte a **Knockback e Forças Externas** (`AddImpact`) com decaimento exponencial, resolvendo o bug onde colisões de obstáculos eram imediatamente canceladas pelo movimento no `FixedUpdate`.
* **Polimento & Correções de Física (Sessão 04b):**
  * Eliminação de fricção estática fantasma nas paredes criando um `PhysicsMaterial` sem atrito dinamicamente no player.
  * Correção do vetor de checagem de parede (`horizontalFacing`) durante o Dive, permitindo agarrar paredes mesmo com o corpo inclinado.
  * Adição de colisão "Bonk" em paredes durante o Dive para restaurar postura vertical caso não se agarre.
  * Integração do método `Respawn()` e `ResetState()` no `PlayerController.cs` e `KillZone.cs`, restaurando imediatamente a rotação vertical, inércia zero e limpando estados de dive ao morrer/cair.

---

## 📅 Sessão 05 — Level Design de Gosmas, Wall Jump Orgânico e KillZone Físico
### O que foi feito:
* **Fim do Wall Grab por botão:** Implementação de auto-grab puramente físico (baseado na detecção pelo `OnCollisionStay` filtrando normais de parede entre `-0.1f` e `0.25f`), dispensando o uso de Raycasts baseados em câmera e input de botão.
* **Movimentação ao redor de Paredes:** Implementado movimento horizontal deslizando pela parede usando cruzamento vetorial do plano normal da parede com a câmera. Deslize vertical acelerado (segurar para baixo) ou freiado (segurar para cima).
* **Wall Jump Orgânico:** 
  * Refatoração completa para remover travas artificiais de *air control*. O Wall Jump agora aplica uma Força Externa (`externalForce`) gigantesca oposta à parede, forçando um decaimento balístico suave.
  * O `MovePlayer` agora **soma** o direcional à força de repulsão da parede, devolvendo 100% de controle aéreo ao jogador no mesmo instante do pulo para realizar curvas perfeitas no ar (ou voltar para a parede).
* **Direção de Arte e Level Design (Sistema de Gosmas):** Atualização massiva do GDD estabelecendo as gosmas como centro do design:
  * 🔵 **Azul (Gelo):** Física material 0 atrito (veloz).
  * 🟡 **Amarela (Cola):** Parede de auto-grab / Reduz velocidade no chão.
  * 🔴 **Rosa (Trampolim):** Alto Bounciness.
  * 🟣 **Roxa (Mortal):** Inundação da morte / Obstáculos hit-kill.
* **Refatoração do KillZone:** O script `KillZone.cs` foi atualizado para matar por colisão física (`OnCollisionEnter`) e não apenas *Triggers*, preparando terreno para obstáculos com gosma roxa e inundações.

---

## 📌 Próximos Passos (Sessão 06)
- [ ] Implementar obstáculo de martelo/pêndulo oscilante (`Pendulum.cs`) — Issue #6.
- [ ] Implementar sistema modular de checkpoints dinâmicos com respawn (`Checkpoint.cs`) — Issue #7.
- [ ] Implementar temporizador de corrida e contagem regressiva 3, 2, 1, GO! (`GameTimer.cs`) — Issue #8.

## Links
* Voltar para [[docs/00 - GDD]]
* Acessar [[docs/01 - Arquitetura & Scripts]]