# Development Conventions

Este documento define os padrões de desenvolvimento utilizados neste template.

O objetivo é manter todos os projetos consistentes, simples e fáceis de manter.

---

# Filosofia

Antes de criar qualquer classe, pasta ou abstração, responda:

- Isso resolve um problema real?
- Isso reduz complexidade?
- Isso facilita a manutenção?

Se a resposta for não, não implemente.

A simplicidade sempre deve ser priorizada.

---

# Arquitetura

A solução é dividida em quatro projetos.

```
Client
Application
Domain
Infrastructure
```

Cada camada possui uma responsabilidade única.

## Client

Responsável apenas pela interface do usuário.

Nunca deve conter regra de negócio.

## Application

Responsável pelos casos de uso da aplicação.

Coordena a execução das funcionalidades.

## Domain

Contém as regras de negócio.

Não conhece UI, banco de dados ou frameworks.

## Infrastructure

Responsável por integrações externas.

Exemplos:

- APIs
- Banco de dados
- Arquivos
- Hardware
- SDKs

---

# Organização do Client

```
Client
│
├── Components
│   ├── Layout
│   └── Shared
│
├── Pages
│   └── Home
│       ├── HomePage.razor
│       ├── HomeViewModel.cs
│       └── Components
│
├── Resources
│
└── wwwroot
```

## Components/Layout

Contém apenas componentes estruturais da aplicação.

Exemplos:

- MainLayout
- NavMenu
- Header
- Footer

## Components/Shared

Componentes reutilizáveis.

Exemplos:

- PrimaryButton
- LoadingOverlay
- ConfirmationDialog

Nunca colocar páginas aqui.

## Pages

Cada funcionalidade possui sua própria pasta.

Tudo relacionado àquela página permanece junto.

Exemplo:

```
Inspection
│
├── InspectionPage.razor
├── InspectionViewModel.cs
├── Models
└── Components
```

---

# Organização da Application

```
Application
│
├── Common
└── Features
```

Cada Feature contém apenas o necessário para aquela funcionalidade.

Evite compartilhar código sem necessidade.

---

# Organização da Infrastructure

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
│
├── Camera
├── ControlId
├── Ftp
└── Modbus
```

---

# Convenções de nomenclatura

## Interfaces

Sempre utilizar prefixo I.

Exemplo:

```
ISettingsService
ILogService
```

---

## Pages

Sempre terminar com "Page".

Exemplo:

```
HomePage
InspectionPage
SettingsPage
```

---

## ViewModels

Sempre terminar com "ViewModel".

Exemplo:

```
HomeViewModel
InspectionViewModel
```

---

## Requests

Sempre terminar com "Request".

Exemplo:

```
LoginRequest
CreateUserRequest
```

---

## Responses

Sempre terminar com "Response".

Exemplo:

```
LoginResponse
CreateUserResponse
```

---

## Models

Sempre utilizar nomes no singular.

Exemplo:

```
InspectionModel
UserModel
```

---

# Organização por Feature

Sempre que possível, manter tudo relacionado à funcionalidade na mesma pasta.

Evite espalhar arquivos pelo projeto.

---

# Componentes

## Componentes reutilizáveis

Devem ficar em:

```
Components/Shared
```

## Componentes específicos

Devem ficar dentro da própria Page.

Exemplo:

```
Pages
└── Inspection
    └── Components
```

---

# Responsabilidade das pastas

Cada pasta deve possuir apenas uma responsabilidade.

Se uma pasta começar a armazenar arquivos sem relação entre si, provavelmente ela precisa ser reorganizada.

---

# Evitar

Não criar pastas genéricas como:

- Helpers
- Utils
- Managers
- Base
- Core

Essas pastas tendem a se tornar locais onde qualquer código é colocado.

Prefira nomes que representem claramente a responsabilidade.

---

# Abstrações

Não criar abstrações antecipadamente.

Toda abstração deve existir para resolver um problema real.

Código explícito é preferível a abstrações desnecessárias.

---

# Princípios

- Simplicidade acima de sofisticação.
- Uma responsabilidade por classe.
- Uma responsabilidade por pasta.
- Organização por Feature.
- Componentes reutilizáveis em Shared.
- Componentes específicos junto da Feature.
- Evitar duplicação desnecessária.
- Evitar complexidade desnecessária.
- Priorizar legibilidade.
