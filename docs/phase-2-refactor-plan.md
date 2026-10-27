# Fase 2 — Plano de refatoração e otimização do código

## Objetivo

Executar a parte de código do hackthon, conforme o milestone de refatoração, com foco em:

- stack em .NET
- aderência às diretrizes da empresa
- otimização do código
- cobertura mínima de 70%
- manutenção das regras de negócio do legado

## Escopo estrito desta fase

Esta fase cobre apenas a parte de implementação e qualidade da aplicação, não a documentação geral do projeto nem a entrega final de release.

## Requisitos obrigatórios desta fase

### 1. Stack Refactorization
- remover a dependência de Java da solução
- aplicar arquitetura .NET alinhada à empresa
- seguir princípios de DDD, SOLID e boas práticas .NET
- separar camadas de responsabilidade

### 2. Code Optimizations
- eliminar duplicações e lógica acoplada
- reduzir acoplamento entre API, regras de negócio e persistência
- melhorar validações e tratamento de erros
- estruturar modelo de domínio e serviços de forma clara

### 3. Code Coverage
- garantir cobertura mínima de 70%
- priorizar testes de domínio, aplicação e integração crítica
- tratar casos de negócio e cenários negativos

## Arquitetura alvo recomendada

### Camadas
- Domain
  - entidades e regras de negócio
  - categorias
  - status do livro
  - validações centrais
- Application
  - casos de uso
  - DTOs
  - orquestração de ações
- Infrastructure
  - acesso a MongoDB
  - repositórios
  - persistência
- API
  - controllers/endpoints
  - contratos HTTP
- Tests
  - unitários e integração

## Regras de negócio que devem ser preservadas

- listagem de livros
- filtro por categoria
- busca por id e nome
- criação de livro
- atualização de livro
- validação da categoria
- controle de status `active` / `inactiveDate`
- regra de inativação no update
- caso pendente de filtro por autor deve ser explícito e tratado com clareza

## Critérios de arquitetura para a implementação

### Não pode acontecer
- lógica de negócio no controller
- persistência em camada de apresentação
- entidades de domínio acopladas ao MongoDB
- validações espalhadas sem padrão
- `new` direto de serviços em pontos de uso

### Deve acontecer
- injeção de dependência
- serviços com responsabilidade única
- abstrações para repositórios
- DTOs específicos de entrada/saída
- tratamento consistente de erros

## Planejamento por pequenas fases

### Fase 2.1 — Diagnóstico e desenho da solução
Objetivo:
- confirmar regras do legado
- definir arquitetura alvo
- alinhar nomenclatura e língua ubíqua

Entregáveis:
- domínio identificado
- camadas definidas
- lista de regras de negócio validadas

Critério de aceite:
- todas as regras críticas foram confirmadas antes da implementação

### Fase 2.2 — Estrutura mínima em .NET
Objetivo:
- criar a base da solução em .NET
- definir projetos e dependências
- preparar DI e configuração

Entregáveis:
- estrutura de solução pronta
- projetos de API, Domain, Application, Infrastructure, Tests
- configuração básica

Critério de aceite:
- solução compila sem dependências quebradas

### Fase 2.3 — Domínio e regras de negócio
Objetivo:
- mover as regras do negócio para o domínio
- centralizar validações
- criar modelos de domínio claros

Entregáveis:
- entidades de domínio
- enumerações e validadores
- regras de categoria e status

Critério de aceite:
- business rules não dependem de controller ou MongoDB

### Fase 2.4 — Persistência e infraestrutura
Objetivo:
- encapsular acesso ao MongoDB
- estabelecer repositórios para livros
- isolar dependência externa

Entregáveis:
- interface do repositório
- implementação no MongoDB
- adaptação do modelo de persistência

Critério de aceite:
- camada de infraestrutura recebe e retorna domínio, sem vazamento de detalhes externos

### Fase 2.5 — Aplicação e API
Objetivo:
- expor endpoints HTTP .NET
- conectar aplicação e infraestrutura
- manter contrato funcional compatível com o legado

Entregáveis:
- endpoints da API
- DTOs de request/response
- tratamento de erros e status codes

Critério de aceite:
- API expõe funcionamento funcional equivalente ao legado

### Fase 2.6 — Testes e qualidade
Objetivo:
- garantir que as regras do contexto sejam validadas
- atingir cobertura mínima de 70%
- validar cenários de erro e borda

Entregáveis:
- testes de unidade e integração
- relatório de cobertura
- listagem de cenários cobertos

Critério de aceite:
- cobertura >= 70%
- métodos e regras críticas cobertas

## Padrões que devem ser aplicados em toda a fase

### Test naming
Seguir o padrão:
- `MethodName_Condition_ExpectedResult()`

### Estrutura de testes
- usar testes unitários para regras de negócio
- usar testes de integração para endpoints e persistência
- validar cenários positivos e negativos

### Qualidade de código
- nomes simples e consistentes
- responsabilidade única por classe
- tratamento explícito de nulos e erros
- evitar código duplicado
- aplicar async/await quando apropriado para I/O

## Definition of Done da fase 2

A fase 2 só pode ser considerada concluída quando:

- a solução está em .NET
- a arquitetura segue as diretrizes da empresa
- o código foi otimizado e simplificado
- regras do legado foram preservadas
- cobertura >= 70%
- os testes relevantes passam
- a aplicação está pronta para a fase seguinte de documentação e release

## Entregáveis finais mínimos da fase 2

- projeto .NET funcional
- arquitetura organizada por camadas
- código otimizado
- testes automatizados
- relatório de cobertura
- evidências de alinhamento aos padrões da empresa

## Conclusão

A fase 2 deve ser tratada como uma migração orientada por qualidade, não apenas por mudança de linguagem. O sucesso está em preservar o comportamento de negócio enquanto melhora arquitetura, manutenção e confiabilidade do código.
