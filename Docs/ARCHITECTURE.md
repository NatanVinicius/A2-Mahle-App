# Architecture

## Objetivo

Este template foi criado para servir como base para aplicações .NET MAUI Blazor Hybrid.

O foco é fornecer uma arquitetura simples, organizada e de fácil manutenção, evitando abstrações desnecessárias e permitindo evolução conforme o projeto cresce.

A arquitetura prioriza:

- Simplicidade
- Baixo acoplamento
- Separação de responsabilidades
- Organização por funcionalidade
- Facilidade de manutenção

---

# Estrutura da solução

A solução é dividida em quatro projetos.

```
Client
Application
Domain
Infrastructure
```

Cada projeto possui uma responsabilidade bem definida.

---

# Client

Responsável exclusivamente pela interface do usuário.

Contém:

- Pages
- Components
- Resources
- wwwroot

O Client nunca deve conter regras de negócio.

Seu papel é apenas exibir informações ao usuário e encaminhar ações para a camada de Application.

---

## Estrutura

```
Client
│
├── Components
│   ├── Layout
│   └── Shared
│
├── Pages
│
├── Resources
│
└── wwwroot
```

### Components/Layout

Componentes estruturais da aplicação.

Exemplos:

- MainLayout
- NavMenu
- Header
- Footer

### Components/Shared

Componentes reutilizáveis por toda a aplicação.

Exemplos:

- PrimaryButton
- Dialog
- LoadingOverlay

### Pages

Cada funcionalidade possui sua própria pasta.

Exemplo:

```
Pages
└── Inspection
    ├── InspectionPage.razor
    ├── InspectionViewModel.cs
    ├── Components
    └── Models
```

Todos os arquivos específicos daquela funcionalidade permanecem juntos.

---

# Application

A camada de Application coordena a execução das funcionalidades.

Ela representa os casos de uso da aplicação.

Não conhece detalhes da interface nem da infraestrutura.

É responsável por orquestrar o fluxo entre Domain e Infrastructure.

Estrutura:

```
Application
│
├── Common
└── Features
```

Cada Feature contém apenas os arquivos necessários para aquela funcionalidade.

---

# Domain

O Domain representa o negócio.

Contém apenas conceitos do domínio.

Não possui dependência de:

- UI
- Banco de dados
- APIs
- Frameworks

Estrutura:

```
Domain
│
├── Entities
├── Enums
└── ValueObjects
```

Toda regra de negócio deve permanecer aqui sempre que possível.

---

# Infrastructure

Responsável pelas integrações externas.

Exemplos:

- APIs
- Banco de dados
- Arquivos
- Hardware
- SDKs

Estrutura:

```
Infrastructure
│
├── Configuration
├── DependencyInjection
└── Features
```

Cada integração possui sua própria pasta.

Exemplo:

```
Features
├── Camera
├── ControlId
├── Modbus
└── Ftp
```

---

# Dependências entre projetos

As referências seguem uma única direção.

```
          Client
         /      \
Application   Infrastructure
        \
        Domain
```

Regras:

Client

↓

Application

↓

Domain

Infrastructure

↓

Application

↓

Domain

O Domain nunca referencia nenhum outro projeto.

---

# Fluxo da aplicação

Uma requisição percorre a aplicação seguindo este fluxo.

```
Usuário

↓

Page

↓

ViewModel

↓

Application

↓

Domain

↓

Infrastructure (quando necessário)

↓

Application

↓

ViewModel

↓

Interface
```

Cada camada possui apenas uma responsabilidade.

---

# Injeção de Dependência

Toda configuração de serviços deve ocorrer durante a inicialização da aplicação.

Cada projeto possui sua própria extensão de registro.

Exemplo:

```
builder.Services
    .AddApplication()
    .AddInfrastructure(configuration);
```

Isso mantém o MauiProgram limpo e facilita a manutenção.

---

# Organização por funcionalidade

O projeto utiliza organização por Feature.

Sempre que possível, todos os arquivos relacionados à mesma funcionalidade permanecem próximos.

Exemplo:

```
Pages
└── Inspection
```

em vez de

```
Pages
ViewModels
Services
Models
```

Essa abordagem reduz a quantidade de navegação entre pastas e melhora a manutenção.

---

# Crescimento da solução

A arquitetura foi projetada para crescer conforme a necessidade.

Novas pastas somente devem ser criadas quando houver uma responsabilidade clara.

Evite criar estruturas antecipadamente.

Exemplo:

Hoje:

```
Infrastructure
└── Features
```

Quando surgir integração com banco:

```
Infrastructure
└── Persistence
```

Quando surgir uma API:

```
Infrastructure
└── Api
```

A estrutura deve evoluir junto com o projeto.

# Utilização das APIs do .NET

Este template prioriza o uso das APIs nativas do ecossistema .NET.

Sempre que houver uma solução oficial para determinado problema, ela deve ser utilizada antes da criação de abstrações próprias.

Exemplos:

- NavigationManager para navegação.
- IConfiguration para configuração.
- ILogger<T> para logs.
- IHttpClientFactory para comunicação HTTP.
- Dependency Injection nativa para gerenciamento de dependências.

Abstrações devem ser criadas apenas quando resolverem um problema concreto ou simplificarem o desenvolvimento da aplicação.

---

# Filosofia

Este template evita abstrações desnecessárias.

O objetivo é que qualquer desenvolvedor consiga compreender rapidamente a solução.

Antes de criar uma nova camada, pasta ou abstração, pergunte:

- Existe um problema real?
- Essa mudança reduz complexidade?
- Ela melhora a manutenção?

Caso contrário, mantenha a solução simples.

---

# Objetivo final

Este template busca servir como uma base sólida para aplicações MAUI Blazor Hybrid.

Ele não pretende antecipar todos os cenários possíveis.

Novas tecnologias, bibliotecas e padrões podem ser incorporados conforme a necessidade do projeto.

A arquitetura deve evoluir junto com a aplicação, sempre preservando simplicidade, organização e clareza.
