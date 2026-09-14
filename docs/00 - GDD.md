# 🎮 GDD: Obstacle Runner 3D

## 1. Visão Geral
* **Gênero:** Plataforma 3D / Gincana física
* **Inspiração:** Fall Guys
* **Engine:** Unity 6 (Universal Render Pipeline - URP)
* **Linguagem:** C#
* **Objetivo:** Controlar uma cápsula em terceira pessoa através de um percurso flutuante, superando obstáculos cinemáticos até cruzar a linha de chegada.

---

## 2. Movimentação & Física do Personagem
* **Controles:**
  * Andar: `W`, `A`, `S`, `D` (Teclado) / Analógico Esquerdo (Gamepad).
  * Pular: `Espaço` (Teclado) / Botão Sul `A`/`X` (Gamepad).
  * Câmera: Movimento do Mouse / Analógico Direito (Gamepad).
  * Mergulho / Dive (Planejado): Tecla `Ctrl` arremessa o personagem para frente rente ao chão.
* **Câmera:** Cinemachine 3 em modo Orbital Follow (Three Ring) com mira em Rotation Composer sem zonas mortas.
* **Física:** Manipulação direta via `Rigidbody.linearVelocity` com soma vetorial de superfícies móveis via `GetPointVelocity`.
* **Respawn:** Gatilho de queda (`KillFloor`) em `Y: -15` que zera a inércia e teleporta para o último checkpoint registrado.

---

## 3. Catálogo de Obstáculos

### 3.1 Em Produção / Validados
* **Barra Giratória / Sweeper (`Rotator.cs`):** Cilindro horizontal posicionado na pista rodando de forma contínua via `rb.MoveRotation()`, transferindo velocidade tangencial por atrito elevado.
* **Zona de Queda (`KillZone.cs`):** Gatilho que captura quedas no abismo e reseta as coordenadas do player.

### 3.2 Próximas Implementações
* **Pêndulo Físico Oscilante (Hammer / Pendulum):** Martelo oscilando perpendicularmente à pista por função senoidal em `FixedUpdate`.
* **Pistão Empurrador:** Bloco que avança e recua periodicamente em linha reta.
* **Gangorra Suspensa (See-Saw):** Plataformas articuladas com `Hinge Joint` que reagem dinamicamente à massa do jogador.
* **Portas Falsas:** Três portas lado a lado onde apenas uma possui colisor físico com física de quebra/abertura.

---

## 4. Navegação
* 🧩 [[docs/01 - Arquitetura & Scripts]]
* 📝 [[docs/02 - Devlog]]