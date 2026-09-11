# 🎮 GDD: Obstacle Runner 3D

## 1. Visão Geral
* **Gênero:** Plataforma 3D / Gincana com física
* **Inspiração:** *Fall Guys*
* **Engine:** Unity (URP)
* **Objetivo:** Controlar uma cápsula em terceira pessoa, desviar de obstáculos móveis e cruzar a linha de chegada.

---

## 2. Movimentação & Física do Personagem
* **Controles:** `WASD` para andar, `Espaço` para pular.
* **Câmera:** Terceira pessoa orbital com Unity Cinemachine.
* **Física:** Controlado via `Rigidbody` para interagir organicamente com colisões e empurrões.
* **Mergulho / Dive:** Tecla `Ctrl` arremessa o personagem para frente em linha reta rente ao chão.
* **Respawn:** Queda da pista aciona respawn no último checkpoint.

---

## 3. Catálogo de Obstáculos
* **Hélices Varredoras (Sweepers):** Cilindros horizontais girando no eixo vertical que empurram o jogador para fora da pista.
* **Pistão Empurrador:** Bloco que avança e recua periodicamente.
* **Gangorra Suspensa (See-Saw):** Plataformas com `Hinge Joint` que inclinam conforme o peso do jogador.
* **Portas Falsas:** Fileira de 3 portas onde apenas uma abre por colisão física.

---

## 4. Próximas Notas
* [[# 🧩 01 - Arquitetura & Scripts]]
* [[02 - Devlog]]