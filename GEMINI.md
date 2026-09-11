# Diretrizes do Projeto: Obstacle Runner 3D

## Stack Técnica
- Engine: Unity (LTS - URP)
- Linguagem: C#
- Física: Rigidbody 3D
- Câmera: Unity Cinemachine (3ª pessoa orbital)

## Padrões de Código
- Inputs no Update(), física no FixedUpdate().
- Usar [SerializeField] private para inspeção.
- Scripts desacoplados e foco em prototipagem modular.

## Documentação
- GDD completo em: docs/00 - GDD.md
- Arquitetura de scripts em: docs/01 - Arquitetura & Scripts.md
- Devlog em: docs/02 - Devlog.md