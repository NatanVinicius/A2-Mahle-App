# MAUI Blazor Hybrid Template

Template base para aplicações .NET MAUI Blazor Hybrid utilizando uma arquitetura simples, organizada e escalável.

## Tecnologias

- .NET 9
- .NET MAUI Blazor Hybrid
- Blazor
- Tailwind CSS v4
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Configuration
- Microsoft.Extensions.Logging

## Arquitetura

O template segue uma arquitetura em quatro projetos:

- Client
- Application
- Domain
- Infrastructure

Mais detalhes:

- docs/ARCHITECTURE.md
- docs/CONVENTIONS.md

---

# Pré-requisitos

- .NET 9 SDK
- Visual Studio 2022 17.14+
- Node.js 22 LTS (ou superior)

---

# Primeira execução

Clone o repositório.

Instale as dependências do Tailwind.

```bash
npm install
```

Execute normalmente pelo Visual Studio ou CLI.

O Tailwind é compilado automaticamente durante o Build.

---

# Desenvolvimento de estilos

Durante o desenvolvimento recomenda-se executar:

```bash
npm run watch:css
```

Arquivos fonte:

```
Resources/Styles
Resources/Themes
```

CSS gerado:

```
wwwroot/app.css
```

---

# Estrutura da solução

```
Client
Application
Domain
Infrastructure
```

---

# Filosofia

Este template possui apenas a infraestrutura comum entre aplicações.

Ele não inclui:

- CQRS
- MediatR
- AutoMapper
- EF Core
- SQLite
- Autenticação
- Componentes reutilizáveis
- Serviços genéricos

Cada aplicação adiciona apenas o que realmente necessita.

---

# Convenções

Consulte:

- docs/CONVENTIONS.md

---

# Licença

MIT