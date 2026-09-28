# 🎮 GDD: Obstacle Runner 3D

## 1. Visão Geral & Direção de Arte
* **Gênero:** Plataforma 3D / Gincana física
* **Inspiração:** Fall Guys
* **Engine:** Unity 6 (Universal Render Pipeline - URP)
* **Linguagem:** C#
* **Objetivo:** Controlar uma cápsula em terceira pessoa através de um percurso flutuante, superando obstáculos cinemáticos até cruzar a linha de chegada.

### 1.1 Tema e Identidade Visual (Novo!)
* **O Personagem:** Animais cilíndricos fofos e peludos (ex: Urso, Cachorro, Gato), inspirados na proporção do Fall Guys. 
* **O Mundo de Gosma:** O jogo gira em torno da interação dos "pelos" do animal com poças de "Gosmas" coloridas espalhadas pelo cenário. O animal se suja da gosma ao tocar nela, alterando visualmente sua pelagem e concedendo habilidades físicas específicas.

### 1.2 O Sistema de Gosmas (Slimes)
* 🔵 **Gosma Azul (Gelo/Escorregar):** Zera o atrito. Faz o jogador deslizar muito rápido pelo chão. Perfeita para ladeiras ou para ganhar impulso usando o *Dive*.
* 🟡 **Gosma Amarela (Cola/Aderência):** Muito grudenta. Se colocada na parede, permite que o jogador se agarre, ande pelos lados e deslize devagar. Se colocada no chão, reduz drasticamente a velocidade de corrida do personagem.
* 🔴 **Gosma Rosa (Trampolim/Bouncy):** Elástica. Lança o jogador para o alto ou para longe quando ele pula ou cai sobre ela.
* 🟣 **Gosma Roxa (Mortal/KillZone):** Tóxica. Se o jogador encostar nela (seja em paredes, chão ou obstáculos estáticos/móveis), ele morre instantaneamente e retorna ao último Checkpoint.
  * *Mecânica Especial (Inundação):* Em poços verticais ou fases de sobrevivência, uma "maré" de gosma roxa pode começar a descer do teto ou subir do chão lentamente, servindo como um "Timer da Morte" que força o jogador a continuar se movendo rápido para não ser engolido.

## 2. Movimentação & Física do Personagem
* **Controles:**
  * Andar: `W`, `A`, `S`, `D` (Teclado) / Analógico Esquerdo (Gamepad).
  * Pular: `Espaço` (Teclado) / Botão Sul `A`/`X` (Gamepad).
  * Câmera: Movimento do Mouse / Analógico Direito (Gamepad).
  * Mergulho (*Air Dive*): Pressionar Pular novamente enquanto estiver no ar.
  * Agarrar Parede (*Wall Grab*): Segurar `Shift` ou `Botão Direito do Mouse` (Teclado) / `RT` (Gamepad) contra superfícies agarráveis.
  * Pulo da Parede (*Wall Jump*): Pressionar Pular enquanto estiver agarrado a uma parede.
* **Câmera:** Cinemachine 3 em modo Orbital Follow (Three Ring) com mira em Rotation Composer sem zonas mortas.
* **Física:** Manipulação direta via `Rigidbody.linearVelocity` com soma vetorial de superfícies móveis via `GetPointVelocity`.
* **Respawn:** Gatilho de queda (`KillFloor`) em `Y: -15` que zera a inércia e teleporta para o último checkpoint registrado.

---

## 3. Catálogo de Obstáculos

### 3.1 Em Produção / Validados
* **Barra Giratória / Sweeper (`Rotator.cs`):** Cilindro horizontal posicionado na pista rodando de forma contínua via `rb.MoveRotation()`, transferindo velocidade tangencial por atrito elevado.
* **Zona de Queda (`KillZone.cs`):** Gatilho que captura quedas no abismo e reseta as coordenadas do player no spawn/checkpoint.
* **Linha de Chegada (`FinishLine.cs`):** Gatilho de conclusão do percurso que detecta vitória, desativa inputs do jogador e dispara eventos de fim de fase.

### 3.2 Próximas Implementações
* **Pêndulo Físico Oscilante (Hammer / Pendulum - `Pendulum.cs`):** Martelo oscilando perpendicularmente à pista por função senoidal em `FixedUpdate`.
* **Checkpoints Modulares Dinâmicos (`Checkpoint.cs`):** Totens/plataformas intermediárias que atualizam dinamicamente o ponto de respawn no `KillZone`.
* **Temporizador & Contagem Regressiva (`GameTimer.cs`):** Sistema de largada 3, 2, 1, GO! com congelamento temporário de input e cronômetro de conclusão.
* **Pistão Empurrador:** Bloco que avança e recua periodicamente em linha reta.
* **Gangorra Suspensa (See-Saw):** Plataformas articuladas com `Hinge Joint` que reagem dinamicamente à massa do jogador.
* **Portas Falsas:** Três portas lado a lado onde apenas uma possui colisor físico com física de quebra/abertura.

---

## 4. Navegação
* 🧩 [[docs/01 - Arquitetura & Scripts]]
* 📝 [[docs/02 - Devlog]]