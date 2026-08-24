# Especificação Técnica e Escopo do MVP - MAHLE App

## 1. Visão Geral do Produto
É uma aplicação desktop desenvolvida para monitorar linhas de inspeção industrial em tempo real. O software recebe dados de câmeras e sensores, processa os resultados de inspeção (Peças Aprovadas vs. Reprovadas), exibe as informações dinamicamente na Interface Gráfica (UI), persiste o histórico e executa ações de salvamento de evidências visuais e relatórios.

---

## 2. Requisitos Funcionais (Regras de Negócio)

### 2.1. Recepção de Dados e Inspeção em Tempo Real
- O sistema deve se conectar à fonte de dados da câmera/controlador para receber os dados de cada inspeção realizada.
- Cada ciclo de inspeção deve conter: **Status da Peça (Aprovado / Reprovado)** e a **Imagem capturada**.
- A Interface do Usuário (UI) deve atualizar instantaneamente o status atual e os contadores da inspeção sempre que novos dados forem recebidos.

### 2.2. Salvamento de Evidências (Reprovadas)
- Quando uma peça for classificada como **Reprovada**, o sistema deve salvar automaticamente a imagem correspondente em uma pasta dedicada de logs/evidências no disco local, com timestamp ou identificador único.

### 2.3. Persistência de Dados
- Todos os resultados das inspeções (data/hora, status de aprovação/reprovação e metadados) devem ser salvos em um banco de dados local (ex: SQLite) para consulta posterior e histórico.

### 2.4. Exportação de Relatórios (PDF)
- O operador deve ter a opção de exportar o resumo das inspeções ou o histórico filtrado em formato **PDF** para auditoria e controle de qualidade.

---

## 3. Requisitos Não-Funcionais e Tecnologias
- **Plataforma:** Aplicação Desktop (C# / .NET / WPF).
- **Arquitetura:** Clean Architecture / Separação clara de camadas (Domain, Application, Infrastructure, UI).
- **Armazenamento:** Banco de dados relacional leve (SQLite) para dados textuais e sistema de arquivos local para imagens de reprovação.

---

## 4. Definição do Escopo do MVP (O que entra e o que NÃO entra)

### ✅ O que entra no MVP (Foco atual):
1. Conexão e recebimento de dados da câmera (Status + Imagem).
2. Atualização em tempo real na tela (UI) dos contadores e status.
3. Salvamento automático da imagem no disco se o status for **Reprovado**.
4. Gravação dos dados em banco local (Persistência).
5. Geração e exportação do relatório em **PDF**.

### ❌ O que NÃO entra no MVP (Deixar para versões futuras):
- Gráficos estatísticos avançados de tendência de falhas.