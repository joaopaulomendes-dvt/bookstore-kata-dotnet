# Assessment de migração e arquitetura para a parte de código

## Objetivo

Avaliar a aplicação atual, identificar regras de negócio, gaps de arquitetura e riscos antes da migração para a stack .NET, com foco em execução enxuta, alinhada aos padrões da empresa e com aproveitamento de esforço.

## 1. Visão rápida do estado atual

### Stack atual
- Java 11
- Jersey / JAX-RS
- MongoDB
- estrutura mínima em camadas, mas com acoplamento forte

### Estrutura funcional observada
- listagem de livros
- busca por id/nome
- criação de livro
- atualização de livro
- filtro por categoria
- health check

### Ponto de atenção estratégico
- O sistema está funcional, mas não é arquiteturalmente alinhado ao padrão da empresa em .NET
- Há lógica de negócio e infraestrutura misturada em `BookService`
- Há validações parciais, regras implícitas e comportamento pendente

## 2. Regras de negócio identificadas

### 2.1. Livro
- Possui identificador único
- Tem título, autor, descrição, linguagem, editora e datas
- Possui categoria
- Possui quantidade em estoque
- Pode estar ativo ou inativo
- Quando inativo, registra `inactiveDate`

### 2.2. Categoria
- Existe uma lista estática de categorias:
  - Fiction
  - Non-Fiction
  - Science
  - Biography
  - Children
  - Programming
- A categoria é validada por nome
- O valor da categoria é convertido em `categoryId`

### 2.3. Regras de uso observadas
- `GET /books` pode filtrar por categoria
- `GET /books/single` pode buscar por `id` ou `name`
- `POST /books` cria cadastro
- `PUT /books` atualiza quantidade, status e `inactiveDate`
- Quando o livro é atualizado para inativo, a data de inatividade é preenchida

### 2.4. Gaps de regra detectados
- Filtro por autor foi previsto, mas não implementado
- `categoryId` é validado por nome em um ponto da lógica, mas não há regra explícita centralizada
- `getBook` usa comparações fracas e lógicas redundantes
- `upsertBook` mistura persistência, transformação de domínio e regras de negócio em um único método
- A lógica de criação e atualização não está claramente separada por responsabilidade

## 3. Diagnóstico da estrutura atual

### Pontos positivos
- API simples e direta
- Separação básica entre controller, service e model
- Testes iniciais de controller

### Pontos negativos
- acoplamento da camada de API com persistência direta
- lógica de negócio espalhada em serviço
- uso de `new BookService()` dentro do controller
- uso de `List`/`ArrayList` sem modelagem mais sólida
- ausência de abstrações de repositorio, validação e tratamento de erro
- ausência de política clara de qualidade e cobertura

## 4. Conformidade com padrões da empresa e do hackthon

### Regras que devem ser respeitadas
- usar stack .NET
- manter arquitetura limpa e testável
- preservar regras de negócio do legado
- aplicar Domain-Driven Design e SOLID
- separar camadas: Domain, Application, Infrastructure, API, Tests
- manter documentação e cobertura mínima

### Regras que não podem ser ignoradas
- nenhuma refatoração sem preservar intenção do negócio
- qualquer mudança deve ser protegida por testes
- qualquer endpoint novo ou alteração deve manter documentação atualizada

## 5. Riscos de implementação

### Risco técnico
- migrar a lógica sem primeiro mapear a regra real
- acoplar MongoDB diretamente às entidades de domínio
- perder compatibilidade funcional com a API existente

### Risco de qualidade
- criar solução em .NET sem testabilidade
- deixar cobertura abaixo do mínimo exigido
- documentar depois do código em vez de primeiro

### Risco de esforço
- tentar migrar tudo de uma vez em um único grande bloco
- gastar tokens em implementação inicial sem validar o desenho da arquitetura

## 6. Estratégia recomendada

### Princípio do projeto enxuto
A melhor abordagem é “pequeno escopo, alta qualidade”:
- mapear apenas o que é essencial
- construir a arquitetura mínima necessária
- começar pelos fluxos críticos
- validar regras dentro de testes
- somente depois expandir para documentação e cobertura total

### Abordagem recomendada
- Fase 1: definir domínio e regras
- Fase 2: implementar camada de aplicação e persistência
- Fase 3: expor API .NET
- Fase 4: cobrir testes e qualidade
- Fase 5: documentar e preparar release

## 7. Critérios de decisão para a migração

A migração é recomendada quando:
- a regra de negócio for mapeada e validada
- a estrutura de camadas estiver definida
- os testes críticos forem escritos antes da implementação
- os contratos da API estiverem claros

A migração não deve continuar quando:
- houver ambiguidade de regra de negócio
- os casos de uso não estiverem mapeados
- a solução ainda estiver acoplada ao modelo legado

## 8. Resultado do assessment

### Conclusão
A aplicação legada é pequena, mas tem um bom núcleo funcional. O maior problema não é a escala, e sim a qualidade arquitetural e a separação de responsabilidades. A migração deve começar pela definição correta do domínio e pela separação clara de regras de negócio, infraestrutura e API.

### Decisão arquitetural recomendada
- manter a API como camada de exposição
- mover regras de negócio para domínio/aplicação
- manter persistência em infraestrutura
- deixar os testes como gate de qualidade
- usar documentação e coverage como requisitos de entrega

## 9. Próximo passo

O próximo passo é criar o plano de migração por pequenas fases, com objetivos, entregáveis e critérios de aceite, evitando “big bang” e preservando o aproveitamento de tokens.
