# Plano de documentação do código legado

## Objetivo

Documentar a aplicação atual em Java de forma compreensiva, preservando o contexto técnico e de negócio antes da refatoração para .NET.

## Escopo

- OpenAPI Specification v3
- Functional Documentation
- Technical Documentation

## Ordem de execução

### 1. Levantamento do sistema atual

- Identificar todos os endpoints expostos pelo controller
- Listar entidades, DTOs e regras implícitas em `BookService`
- Mapear integrações, especialmente com MongoDB
- Identificar endpoints incompletos ou comportamentos pendentes

### 2. Documentação de API (OpenAPI)

- Definir base path da API
- Registrar endpoints:
  - `GET /books`
  - `GET /books/health`
  - `GET /books/single`
  - `POST /books`
  - `PUT /books`
- Definir payloads de entrada e saída
- Registrar status codes esperados
- Documentar erros e casos limites
- Incluir filtros e validações previstos

### 3. Documentação funcional

- Descrever os cenários de negócio: listagem, busca, criação e atualização
- Explicar a regra por categoria
- Explicar regras de status do livro (`active` e `inactiveDate`)
- Registrar comportamentos pendentes e não implementados
- Identificar casos de uso e regras de negócio para Product Owner

### 4. Documentação técnica

#### Alto nível

- Arquitetura atual e camadas
- Fluxo de requisição
- Dependências externas
- Estratégia de persistência no MongoDB

#### Baixo nível

- Estrutura de pacotes
- Descrição de classes e responsabilidades
- Padrões de validação e controle de erros
- Observações sobre acoplamento e pontos de melhoria

### 5. Revisão e aprovação

- Validar com equipe técnica
- Validar com stakeholder de negócio
- Ajustar ambiguidades
- Consolidar base documental para a refatoração

## Entregáveis

- `openapi.yaml` ou equivalente
- `functional-documentation.md`
- `technical-documentation.md`
- `legacy-architecture-map.md`

## Critérios de aceite

- Todos os endpoints mapeados
- Payloads e erros detalhados
- Regras de negócio explicadas em linguagem executiva
- Arquitetura e integrações descritas em nível técnico
- Documento pronto para guiar a migração para .NET

## Entregáveis mínimos para a fase

- OpenAPI v3 finalizada
- Documento funcional completo
- Documento técnico completo
- Checklist de revisão para aprovação
