# User Stories detalhadas da Fase 2 — Refatoração e otimização em .NET

## Visão geral

Esta fase cobre exclusivamente a parte de implementação do código, em .NET, seguindo os padrões da empresa e o uso consciente do harness. O objetivo é transformar a aplicação legada em uma solução enxuta, testável, bem estruturada e alinhada às regras de qualidade exigidas pelo hackthon.

A execução deve seguir uma sequência lógica, fase a fase, com validação explícita antes de avançar para a próxima etapa.

---

## User Story 1 — Definir a arquitetura alvo e os limites da solução

### Como usuário técnico
Quero definir a arquitetura alvo da aplicação em .NET antes de alterar o código,
para que a solução siga os padrões da empresa, mantenha baixo acoplamento e evite retrabalho.

### Critérios de aceitação
- A solução deve ser organizada em camadas: Domain, Application, Infrastructure, API e Tests.
- A arquitetura deve respeitar princípios DDD e SOLID.
- A API não deve conter regras de negócio pesadas.
- A persistência não deve misturar regras de domínio.
- O desenho deve ser revisado antes da implementação real.

### Regras de execução
- O desenho deve ser validado antes da escrita do código funcional.
- O harness usado deve ser o que melhor apoiar a arquitetura e a organização, em especial o uso de documentação e estrutura de projeto.
- O trabalho deve ser enxuto: sem criar camadas extras desnecessárias.

### Validação
- revisão técnica da arquitetura antes de iniciar codificação
- confirmação de que as responsabilidades estão corretamente separadas

### Harness recomendado
- uso do framework de documentação/estratégia de implementação já disponível no ambiente
- utilização leve de apoio para alinhamento arquitetural, sem excesso de processo

---

## User Story 2 — Mapear as regras de negócio do legado

### Como usuário técnico
Quero consolidar as regras de negócio observadas no código legado,
para garantir que a refatoração preserve o comportamento funcional e não “invente” regras novas.

### Critérios de aceitação
- Devem estar listadas as regras de livro, categoria e estado do livro.
- Devem estar identificados os pontos pendentes e não implementados do legado.
- Devem ser confirmadas as regras de criação, atualização, listagem e busca.
- O comportamento do sistema legado deve servir como base de comparação da nova implementação.

### Regras de execução
- regras devem ser documentadas antes da migração
- qualquer regra ambígua deve ser explicitamente tratada antes da codificação
- o escopo deve permanecer focado no sistema de livros

### Validação
- checklist de regras correlacionadas ao domínio
- comparação entre comportamento legado e nova solução

### Harness recomendado
- uso do harness de diagnóstico e documentação da solução, para manter registro do que foi validado e do que foi preservado

---

## User Story 3 — Criar a estrutura mínima da solução em .NET

### Como usuário técnico
Quero iniciar a solução em .NET com a estrutura mínima necessária,
para preparar a base da aplicação sem dispersão de esforço.

### Critérios de aceitação
- a solução deve possuir projetos separados por camada
- a solução deve compilar corretamente com a estrutura mínima
- a injeção de dependência deve estar preparada
- existem arquivos de configuração básicos para a API e a infraestrutura

### Regras de execução
- não criar projetos desnecessários
- não ampliar a arquitetura para componentes que não serão usados
- manter a base enxuta e funcional

### Validação
- build da solução com sucesso
- estrutura pronta e consistente com a arquitetura definida

### Harness recomendado
- uso do harness de setup/estrutura de projeto para manter a solução organizada e consistente com o padrão da empresa

---

## User Story 4 — Modelar o domínio e as entidades principais

### Como usuário técnico
Quero consolidar o domínio do bookstore em entidades e valores claros,
para que a regra de negócio permaneça centralizada e reutilizável.

### Critérios de aceitação
- devem existir entidades para Livro e Categoria
- o modelo de domínio deve refletir regras de negócio e não detalhes de infraestrutura
- o estado do livro deve ser representado de forma explícita
- as regras de categoria e ativação devem estar no domínio

### Regras de execução
- não misturar MongoDB ou DTOs dentro do domínio
- evitar lógica de persistência dentro das entidades
- manter nomes consistentes com a linguagem ubíqua do negócio

### Validação
- revisão da modelagem do domínio
- testes unitários das regras principais

### Harness recomendado
- uso de padrões de domain modeling e arquitetura DDD para manter consistência de linguagem e design

---

## User Story 5 — Centralizar regras de negócio em serviços de aplicação

### Como usuário técnico
Quero mover a lógica de negócio para a camada de aplicação,
para separar regras de uso do sistema das entidades e da infraestrutura.

### Critérios de aceitação
- criação de livro passa pela camada de aplicação
- atualização de livro passa pela camada de aplicação
- listagem e busca são orquestradas por serviços com responsabilidade clara
- não existem regras distribuídas em controller, domínio e infra ao mesmo tempo

### Regras de execução
- a aplicação deve orquestrar casos de uso
- o domínio deve conter a regra central
- a infraestrutura deve apenas persistir e recuperar dados

### Validação
- testes de aplicação exercitando casos de uso reais
- verificação de isolamento entre regras de negócio e infraestrutura

### Harness recomendado
- uso de recursos de arquitetura e organização para manter a separação de responsabilidades bem definida

---

## User Story 6 — Encapsular persistência no repositório

### Como usuário técnico
Quero isolar o acesso ao MongoDB por meio de repositórios,
para evitar acoplamento do domínio com a infraestrutura e facilitar a manutenção.

### Critérios de aceitação
- a infraestrutura deve expor abstrações de repositorio
- o domínio não deve conhecer a implementação do banco
- a API não deve acionar MongoDB diretamente
- os dados de persistência devem ser adaptados ao modelo de domínio

### Regras de execução
- cada repositório deve ter responsabilidade única
- o código de acesso ao banco deve ser mínimo e objetivo
- as operações devem refletir o domínio do negócio e não a estrutura técnica do banco

### Validação
- testes de integração com base em dados controlados
- verificação de que a lógica de negócio não ficou acoplada ao driver MongoDB

### Harness recomendado
- uso de padrões de infraestrutura e integração, com foco na organização e no desenho técnico correto

---

## User Story 7 — Implementar os endpoints .NET da API

### Como usuário técnico
Quero expor os endpoints em .NET respeitando o contrato funcional do legado,
para que a solução mantenha compatibilidade de uso e permita evolução controlada.

### Critérios de aceitação
- endpoints para listagem, busca, criação e atualização devem existir
- payloads e respostas devem ser consistentes
- status codes adequados devem ser usados
- o controller deve delegar para a camada de aplicação

### Regras de execução
- controller não deve ter regra de negócio
- não deve haver lógica de persistência no endpoint
- mensagens, respostas e erros devem seguir convenções da empresa

### Validação
- testes de API para cenários positivos e negativos
- verificação dos status esperados

### Harness recomendado
- uso do harness de Web API para manter qualidade de endpoints, status code e documentação de contrato

---

## User Story 8 — Garantir cobertura de testes da fase

### Como usuário técnico
Quero validar que o código atinge a cobertura mínima exigida,
para atender ao requisito de qualidade e reduzir risco de regressão.

### Critérios de aceitação
- cobertura mínima de 70% na solução
- testes para regras de domínio e casos críticos
- testes para cenários negativos e borda
- testes de integração para endpoints e persistência

### Regras de execução
- testes devem ser escritos antes ou junto com a implementação funcional
- os testes devem seguir o padrão de nomeação da empresa
- a métrica deve ser verificada no fim da fase

### Validação
- relatório de cobertura gerado
- execução de testes com sucesso
- verificação de que o mínimo necessário foi cumprido

### Harness recomendado
- uso do harness de execução e validação de testes para garantir execução consistente e evidência documental

---

## User Story 9 — Validar qualidade, padrões da empresa e readiness da fase

### Como usuário técnico
Quero validar ao fim da fase que o código segue os padrões da empresa,
para que a refatoração possa avançar com segurança para a documentação final e QA.

### Critérios de aceitação
- arquitetura e regras atendem à política da empresa
- código está otimizado e organizado
- cobertura mínima foi atingida
- regras do legado estão preservadas
- a fase está pronta para a próxima etapa

### Regras de execução
- a validação deve ser feita ao final de cada bloco, e não só no fim
- qualquer falha deve bloquear a etapa seguinte
- o resultado final precisa ser mensurável e documentado

### Validação
- checklist final da fase
- execução de testes e revisão arquitetural
- aprovação da fase antes de avançar

### Harness recomendado
- uso do harness de qualidade e verificação para centralizar evidências e garantir aderência aos padrões

---

## Critérios globais de execução da fase

### Sequência obrigatória
1. arquitetura alvo
2. regras de negócio do legado
3. estrutura em .NET
4. domínio
5. aplicação
6. infraestrutura
7. API
8. testes e cobertura
9. validação final

### Regras da empresa a serem seguidas
- usar stack .NET
- seguir DDD e SOLID
- separar responsabilidades por camada
- manter linguagem ubíqua consistente
- garantir cobertura mínima
- usar documentação e evidência de validação

### Uso do harness
O harness deve ser usado com foco em apoiar a execução, não em criar sobrecarga. O ideal é:
- para arquitetura e organização: uso leve e estratégico
- para Web API e contratos: uso adequado da camada REST
- para testes e validação: execução automatizada e evidências
- para documentação e acompanhamento: uso do mais relevante ao contexto

## Definition of Done da Fase 2

A fase 2 só é considerada concluída quando:
- a solução está em .NET
- a arquitetura está alinhada à empresa
- o código está enxuto e otimizado
- as regras do legado foram preservadas
- cobertura mínima de 70% foi alcançada
- os testes passam
- existe evidência clara de validação da fase

## Observação final

Esta fase deve ser executada em blocos pequenos, um por um, com validação de aceite antes de seguir para o próximo. Isso reduz retrabalho, melhora qualidade e respeita o objetivo do hackthon: entregar uma refatoração enxuta, robusta e conforme padrões da empresa.
