# obstacle-runner-3d

Protótipo jogável de plataforma e corrida de obstáculos 3D no estilo *Fall Guys*, desenvolvido em Unity 6 URP com foco em movimentação puramente física, câmera orbital em terceira pessoa e obstáculos cinemáticos interativos.

---

## 🎮 Controles

| Ação | Teclado & Mouse | Gamepad |
| :--- | :--- | :--- |
| **Mover** | `W`, `A`, `S`, `D` / Setas | Analógico Esquerdo |
| **Pular** | `Espaço` | Botão Sul (`A` / `X`) |
| **Girar Câmera** | Mover Mouse | Analógico Direito |

---

## 🛠️ Stack Técnica

* **Engine:** Unity 6 (6000.x) — Universal Render Pipeline (URP)
* **Linguagem:** C#
* **Controle de Entrada:** New Input System (`com.unity.inputsystem`)
* **Câmera:** Cinemachine 3 (`com.unity.cinemachine`)
* **Física:** Rigidbody 3D com `MoveRotation` cinemático e atrito elevado

---

## 📂 Estrutura de Documentação

* 🎮 **Game Design Document:** `[[docs/00 - GDD]]`
* 🏗️ **Arquitetura Técnica & Scripts:** `[[docs/01 - Arquitetura & Scripts]]`
* 📝 **Registro Diário de Desenvolvimento:** `[[docs/02 - Devlog]]`
* 🤖 **Diretrizes Rápidas / Prompt Context:** `[[GEMINI]]`