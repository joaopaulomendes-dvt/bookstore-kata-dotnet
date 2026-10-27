# Plano mestre de execução do hackthon

## Objetivo

Executar a entrega em três frentes paralelas, mas ordenadas:

1. Documentar o código legado
2. Refatorar e otimizar a solução para a stack .NET
3. Documentar a aplicação final, validar qualidade e preparar o release

> Esta fase inicial é de planejamento e análise. Não será executado o programa ou a aplicação neste momento.

## Contexto do projeto atual

- Stack atual: Java 11 + Jersey + MongoDB
- Estrutura principal:
  - `BookController`: endpoints REST de consulta e escrita
  - `BookService`: regras de negócio e acesso ao MongoDB
  - `Book`, `Category`, DTOs: modelo da aplicação
- Requisitos observados no código legado:
  - listagem por categoria
  - filtro por nome e id em `GET /books/single`
  - criação e atualização de livro
  - validação de categoria
  - comportamento incompleto para filtro por autor
  - lógica de `inactiveDate` em update

## Ordem de execução recomendada

### Fase 0 — Diagnóstico e alinhamento

- Revisar o código legado e mapear endpoints, regras e dependências
- Confirmar regras de negócio em código e em documentação do legado
- Definir critérios de aceite para cada entrega
- Definir e aprovar a arquitetura alvo .NET

### Fase 1 — Documentação do legado

- Produzir OpenAPI v3 com endpoints, payloads e erros
- Produzir documentação funcional para Product Owner / Business Manager
- Produzir documentação técnica de alto e baixo nível

### Fase 2 — Refatoração para .NET

- Estruturar a arquitetura em camadas alinhadas à política da empresa
- Portar domínio, serviços, validações e integrações
- Melhorar performance, clareza e robustez do código
- Aumentar cobertura de testes para no mínimo 70%

### Fase 3 — Documentação da nova aplicação

- Atualizar a documentação com a versão nova
- Definir gates de qualidade para novos recursos
- Preparar release, checklist de QA e evidências de validação

## Critérios de saída por fase

### Fase 1

- OpenAPI validada por endpoint
- Documentação funcional aprovada pelo responsável de negócio
- Documentação técnica com arquitetura e integrações

### Fase 2

- Aplicação em .NET funcionando conforme regras de negócio
- Código revisado e padronizado
- Testes com cobertura >= 70%
- Pipeline / validação automatizada demonstrável

### Fase 3

- Documentação final publicada
- Checklist de QA e release note concluídos
- Evidências de qualidade e cobertura disponíveis

## Entregáveis esperados

- Documento de legado: OpenAPI, funcional e técnico
- Código refatorado em .NET
- Testes automatizados com cobertura medível
- Documento atualizado do projeto
- Release plan e checklist para QA

## Riscos e mitigação

- Risco: regras ocultas no código legado sem documentação formal
  - Mitigação: mapeamento por endpoint e análise de comportamento real
- Risco: refatoração sem preservar regras de negócio
  - Mitigação: testes de regressão e comparação com documentos de negócio
- Risco: cobertura insuficiente
  - Mitigação: foco em testes de domínio e serviços antes da validação final
- Risco: documentação não refletir a solução final
  - Mitigação: revisão final após estabilização da implementação

## Próximo passo imediato

Executar a documentação do legado como primeiro bloco, porque ela fornece a base para a refatoração e para a validação de regressão da solução final.
