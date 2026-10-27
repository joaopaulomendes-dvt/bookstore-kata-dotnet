# Plano de refatoração e otimização para .NET

## Objetivo

Refatorar a solução legada para a stack .NET da empresa, preservando regras de negócio e elevando qualidade, desempenho e cobertura de testes.

## Premissas

- A solução atual em Java não atende mais ao padrão tecnológico da empresa
- A nova solução deve seguir as diretrizes da arquitetura .NET corporativa
- A refatoração deve ser feita com atenção às regras e ao comportamento observado no sistema legado
- A cobertura mínima deve ser 70%

## Ordem de execução

### 1. Definição da arquitetura alvo

- Definir estrutura de projetos .NET:
  - `Api`
  - `Application`
  - `Domain`
  - `Infrastructure`
  - `Tests`
- Definir padrões de injeção de dependência, validação e logging
- Definir padrão de resposta HTTP e tratamento de erros
- Definir convenções de nomenclatura e organização

### 2. Mapeamento de regras de negócio

- Migrar as regras da camada de serviço do legado para a nova arquitetura
- Validar:
  - criação de livro
  - atualização de quantidade/estado
  - categoria válida
  - data de inatividade
  - filtros por categoria e nome
- Registrar exceções e casos pendentes

### 3. Portabilidade do modelo de domínio

- Criar entidades e contratos de domínio compatíveis com a aplicação
- Definir entidades para `Book` e `Category`
- Criar DTOs de request/response
- Separar regras de domínio de detalhes de infraestrutura

### 4. Refatoração de infraestrutura

- Substituir uso direto do MongoDB em camada de acesso
- Criar abstrações para repositório e persistência
- Centralizar configuração de connection string e banco
- Melhorar segurança e robustez da integração

### 5. Otimização de código

- Eliminar duplicações e trechos desnecessários
- Simplificar lógica de busca e listagem
- Tratar casos nulos e validações explícitas
- Padronizar tratamento de erros e respostas HTTP
- Eliminar anti-padrões de código legado

### 6. Testes e qualidade

- Escrever testes de unidade para regras de domínio e serviços
- Escrever testes de integração para endpoints e persistência
- Testar cenários positivos e negativos
- Medir cobertura com ferramenta .NET apropriada
- Garantir cobertura mínima de 70%

### 7. Verificação de qualidade

- Rodar análise estática e validação do código
- Revisar código contra padrões da empresa
- Validar que a nova aplicação preserva a intenção do negócio

## Critérios de aceite

- Aplicação em .NET compilando e funcionando
- Arquitetura alinhada às diretrizes da empresa
- Código mais legível, bem estruturado e otimizado
- Cobertura >= 70%
- Regras do legado preservadas e validadas

## Entregáveis

- Projeto .NET estruturado
- Código refatorado e otimizado
- Testes automatizados
- Relatório de cobertura
- Evidências de alinhamento com as diretrizes da empresa
