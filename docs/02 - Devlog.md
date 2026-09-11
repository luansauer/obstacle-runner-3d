# 📝 02 - Devlog: Obstacle Runner 3D

Registro contínuo de progresso, decisões técnicas, problemas encontrados e soluções implementadas ao longo do projeto.

---

## 📅 Sessão 01 — Planejamento, GDD e Infraestrutura

### O que foi feito:
- [x] Definição do escopo: jogo de plataforma 3D focado em física (estilo *Fall Guys*).
- [x] Escolha da stack técnica: **Unity (URP)** + **C#** + **Antigravity**.
- [x] Repositório Git criado no GitHub (`obstacle-runner-3d`) com template de `.gitignore` para Unity.
- [x] Quadro Kanban configurado no **GitHub Projects** integrado ao repositório com 5 issues iniciais.
- [x] Cofre criado no **Obsidian** com a estrutura base de documentação:
  - `00 - GDD.md`: Escopo, controles e catálogo de obstáculos.
  - `01 - Arquitetura & Scripts.md`: Código base C# de `PlayerController`, `Rotator` e `Killzone`.
  - `02 - Devlog.md`: Registro de atividades e próximos passos.

### Decisões Técnicas Tomadas:
* **Física com Rigidbody:** Optado por usar `Rigidbody` em vez de `CharacterController` para permitir reações naturais e caóticas de impacto com os obstáculos.
* **Camera Follow via Cinemachine:** Uso da ferramenta nativa da Unity para evitar código manual complexo de câmera em 3ª pessoa.
* **Antigravity como IDE:** Editor de código principal aproveitando a assistência contextual com Gemini Pro.

### Próximos Passos (Sessão 02 - Computador Pessoal):
- [ ] Instalar o Unity Hub e baixar a versão Unity LTS mais recente com suporte a URP.
- [ ] Clonar o repositório `obstacle-runner-3d` localmente e configurar o Git LFS (`git lfs install`).
- [ ] Criar a cena inicial de prototipagem (greybox) com chão e luz básica.
- [ ] Implementar e testar o script `PlayerController.cs`.

---

## 📅 Sessão 02 — [A Preencher]

### O que foi feito:
- [ ] 

### Dificuldades & Soluções:
- 

### Próximos Passos:
- [ ] 

---

## Links
* Voltar para [[00 - GDD]]
* Acessar [[01 - Arquitetura & Scripts]]