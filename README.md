# Sistema de Gestão de Funcionários

Sistema completo para gerenciamento de funcionários desenvolvido com .NET 8 (Backend) e React (Frontend), seguindo os princípios de Clean Architecture e boas práticas de desenvolvimento.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-12+-4169E1?logo=postgresql)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)

## 📋 Índice

- [Funcionalidades](#-funcionalidades)
- [Arquitetura](#️-arquitetura)
- [Tecnologias](#️-tecnologias)
- [Início Rápido](#-início-rápido)
  - [Docker (Recomendado)](#-opção-1-docker-recomendado)
  - [Instalação Local](#-opção-2-instalação-local)
- [API REST](#-api-rest)
- [Regras de Negócio](#-regras-de-negócio)
- [Testes](#-testes)
- [Docker](#-docker)

## 🎯 Funcionalidades

### Gestão de Funcionários
- ✅ Cadastro completo com validação de dados
- ✅ Edição de informações pessoais
- ✅ Upload de foto de perfil (JPG, PNG, GIF, WebP)
- ✅ Múltiplos telefones por funcionário
- ✅ Busca e filtro por nome, email, CPF e cargo
- ✅ Soft delete (exclusão lógica)
- ✅ Validação de maioridade (18 anos)

### Sistema de Cargos e Permissões
- ✅ Gestão de cargos (CRUD completo)
- ✅ Hierarquia de cargos configurável
- ✅ Permissões granulares por cargo:
  - Aprovar cadastros
  - Criar funcionários
  - Editar funcionários
  - Excluir funcionários
  - Gerenciar cargos

### Autenticação e Segurança
- ✅ Login com JWT
- ✅ Auto-cadastro com fluxo de aprovação
- ✅ Reset de senha por gestores
- ✅ Alteração de senha pelo próprio usuário
- ✅ Validação de hierarquia em todas as operações
- ✅ Senhas criptografadas com BCrypt

### Experiência do Usuário
- ✅ Interface moderna e responsiva
- ✅ Breadcrumbs para navegação
- ✅ Feedback visual (SweetAlert2)
- ✅ Tema claro profissional
- ✅ Busca em tempo real

## 🏗️ Arquitetura

### Backend (.NET 8)

```
backend/
├── src/
│   ├── ManageEmployees.Domain/          # Entidades, Interfaces, Exceções
│   ├── ManageEmployees.Application/     # Serviços, DTOs, Validadores
│   ├── ManageEmployees.Infrastructure/  # DbContext, Repositórios, JWT
│   └── ManageEmployees.API/             # Controllers, Middlewares
└── tests/
    └── ManageEmployees.Tests/           # Testes unitários
```

**Princípios aplicados:**
- Clean Architecture (4 camadas)
- SOLID
- Repository Pattern
- Unit of Work
- Dependency Injection
- FluentValidation

### Frontend (React + TypeScript)

```
frontend/
├── src/
│   ├── components/     # Componentes reutilizáveis
│   ├── pages/          # Páginas da aplicação
│   ├── services/       # Integração com API
│   ├── store/          # Redux Toolkit
│   └── router/         # Configuração de rotas
```

## 🛠️ Tecnologias

### Backend
| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| .NET | 8.0 | Framework principal |
| Entity Framework Core | 8.0 | ORM |
| PostgreSQL | 12+ | Banco de dados |
| JWT | - | Autenticação |
| FluentValidation | 11.9 | Validação de dados |
| Serilog | - | Logging estruturado |
| Swagger/OpenAPI | - | Documentação da API |

### Frontend
| Tecnologia | Versão | Propósito |
|------------|--------|-----------|
| React | 18 | Framework principal |
| TypeScript | 5 | Tipagem estática |
| Vite | 5 | Build tool |
| Redux Toolkit | - | Estado global |
| Tailwind CSS | 3 | Estilização |
| React Router | 6 | Roteamento |
| SweetAlert2 | - | Feedback visual |

### Infraestrutura
| Tecnologia | Propósito |
|------------|-----------|
| Docker | Containerização |
| Docker Compose | Orquestração de containers |
| Nginx | Servidor web para frontend |
| PgAdmin | Gerenciamento do banco (opcional) |

## 🚀 Início Rápido

### 🐳 Opção 1: Docker (Recomendado)

**Pré-requisitos:**
- Docker Desktop ou Docker Engine (20.10+)
- Docker Compose:
  - **v2.0+** (plugin integrado) - comando: `docker compose` ← Recomendado
  - **v1.x** (standalone) - comando: `docker-compose`

**Verificar sua versão:**
```bash
# Docker Compose v2+ (integrado)
docker compose version
# Output: Docker Compose version v2.x.x ou superior

# Docker Compose v1.x (standalone)
docker-compose version
# Output: docker-compose version 1.x.x
```

**1. Clone o repositório**

```bash
git clone <seu-repositorio>
cd manage-employees
```

**2. Inicie toda a aplicação**

O arquivo `docker-compose.yml` está na **raiz do projeto**.

```bash
# A partir da raiz do projeto (manage-employees/)

# Docker Compose v2+ (recomendado)
docker compose up -d

# OU Docker Compose v1.x (se você tem a versão antiga)
docker-compose up -d
```

**3. Aguarde ~30 segundos para as migrations e seed inicial**

**4. Aguarde ~30 segundos** para migrations e seed inicial

**5. Acesse a aplicação:**

| Serviço | URL | Credenciais |
|---------|-----|-------------|
| Frontend | http://localhost | - |
| Backend API | http://localhost:5000 | - |
| Swagger | http://localhost:5000/swagger | - |
| **Dozzle (Logs)** | http://localhost:8888 | Sem login |
| PostgreSQL | localhost:5433 | postgres / postgres123 |
| PgAdmin | http://localhost:5050 | admin@admin.com / admin123 |

> **💡 Nota:** O PostgreSQL usa a porta **5433** externamente para evitar conflito com instalações locais (porta 5432).

**Credenciais padrão do sistema:**
- **Email:** `admin@admin.com`
- **Senha:** `Master@123`

---

### 🔄 Para Limpar e Começar do Zero

Se precisar **apagar tudo e recomeçar** com banco limpo:

```bash
# Parar e remover TUDO (containers + volumes + banco)
docker compose down -v

# Subir novamente (banco novo)
docker compose up -d
```

**Comandos úteis:**

| Ação | Docker Compose v2+ | Docker Compose v1.x |
|------|-------------------|---------------------|
| **Iniciar** | `docker compose up -d` | `docker-compose up -d` |
| Ver logs | `docker compose logs -f` | `docker-compose logs -f` |
| Ver logs do backend | `docker compose logs -f backend` | `docker-compose logs -f backend` |
| Ver status | `docker compose ps` | `docker-compose ps` |
| **Parar** (mantém dados) | `docker compose stop` | `docker-compose stop` |
| **Parar e remover** (mantém volumes) | `docker compose down` | `docker-compose down` |
| **Parar e LIMPAR TUDO** | `docker compose down -v` | `docker-compose down -v` |
| Reconstruir | `docker compose up -d --build` | `docker-compose up -d --build` |

### 🧹 Comandos para Limpar Completamente

```bash
# Docker Compose v2+ (recomendado)
# 1. Parar todos os containers
docker compose stop

# 2. Remover containers, redes E volumes (APAGA O BANCO!)
docker compose down -v

# 3. (Opcional) Remover também as imagens construídas
docker compose down -v --rmi all

# 4. Verificar se tudo foi removido
docker compose ps -a
docker volume ls | grep manage-employees
```

```bash
# Docker Compose v1.x (legado)
docker-compose stop
docker-compose down -v
docker-compose down -v --rmi all
```

> ⚠️ **ATENÇÃO:** O comando `docker compose down -v` remove os volumes, **apagando permanentemente**:
> - Todo o banco de dados PostgreSQL
> - Fotos de perfil dos usuários
> - Configurações do PgAdmin
>
> Use este comando quando quiser começar **do zero** com um banco limpo.

---

### 💻 Opção 2: Instalação Local

**Pré-requisitos:**
- .NET 8 SDK
- Node.js 18+
- PostgreSQL 12+

#### Backend

**1. Configure a connection string em `backend/src/ManageEmployees.API/appsettings.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=manage_employees;Username=postgres;Password=SuaSenha"
  },
  "JwtSettings": {
    "Secret": "YourSuperSecretKeyForJWTTokenGenerationWithAtLeast32Characters123456",
    "Issuer": "ManageEmployeesAPI",
    "Audience": "ManageEmployeesClient",
    "ExpirationInMinutes": 60
  }
}
```

**2. Execute a aplicação:**

```bash
cd backend/src/ManageEmployees.API
dotnet run
```

> ✨ As migrations são executadas automaticamente na inicialização.
> 
> 👤 Um usuário admin é criado: `admin@admin.com` / `Master@123`

#### Frontend

**1. Instale as dependências:**

```bash
cd frontend
npm install
```

**2. Execute o servidor de desenvolvimento:**

```bash
npm run dev
```

**3. Acesse:** `http://localhost:5173`

## 📖 API REST

### Documentação Swagger
Acesse: `http://localhost:5000/swagger`

### Endpoints Principais

#### Autenticação
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/api/Auth/login` | Realiza login e retorna JWT |
| POST | `/api/Auth/self-register` | Auto-cadastro (aguarda aprovação) |
| GET | `/api/Auth/has-director` | Verifica se existe administrador |

#### Funcionários (requer autenticação)
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/Employees` | Lista todos os funcionários |
| GET | `/api/Employees/{id}` | Busca funcionário por ID |
| POST | `/api/Employees` | Cria novo funcionário |
| PUT | `/api/Employees/{id}` | Atualiza funcionário |
| DELETE | `/api/Employees/{id}` | Exclui funcionário (soft delete) |
| POST | `/api/Employees/{id}/photo` | Upload de foto de perfil |
| DELETE | `/api/Employees/{id}/photo` | Remove foto de perfil |
| PUT | `/api/Employees/profile` | Atualiza próprio perfil |
| POST | `/api/Employees/change-password` | Altera própria senha |
| POST | `/api/Employees/{id}/reset-password` | Reseta senha (gestor) |
| GET | `/api/Employees/pending-approvals` | Lista cadastros pendentes |
| POST | `/api/Employees/approve` | Aprova/rejeita cadastro |
| GET | `/api/Employees/managers` | Lista gerentes disponíveis |

#### Cargos (requer permissão)
| Método | Endpoint | Descrição |
|--------|----------|-----------|
| GET | `/api/Roles` | Lista todos os cargos |
| GET | `/api/Roles/public` | Lista cargos públicos |
| GET | `/api/Roles/assignable` | Lista cargos atribuíveis |
| GET | `/api/Roles/{id}` | Busca cargo por ID |
| POST | `/api/Roles` | Cria novo cargo |
| PUT | `/api/Roles/{id}` | Atualiza cargo |
| DELETE | `/api/Roles/{id}` | Exclui cargo |

## 🔐 Regras de Negócio

### Sistema de Hierarquia

| Cargo | Nível | Pode gerenciar |
|-------|-------|----------------|
| Administrador | 100 | Níveis < 100 |
| Gerente | 50 | Níveis < 50 |
| Funcionário | 10 | Nenhum |

**Regras:**
- Um usuário só pode criar/editar/excluir outro com nível hierárquico inferior
- Funcionário não pode criar Gerente
- Gerente não pode criar Administrador

### Validações Obrigatórias

| Campo | Validação |
|-------|-----------|
| Nome | Obrigatório, nome e sobrenome |
| E-mail | Obrigatório, único, formato válido |
| CPF | Obrigatório, único, 11 dígitos |
| Data Nascimento | Obrigatório, idade ≥ 18 anos |
| Telefone | Obrigatório, mínimo 1, formato válido |
| Senha | 8+ caracteres, maiúsculas, minúsculas, números, especiais |
| Cargo | Obrigatório |
| Foto | Opcional, JPG/PNG/GIF/WebP, máx 5MB |

### Fluxo de Aprovação

1. Funcionário faz auto-cadastro via `/api/Auth/self-register`
2. Cadastro fica pendente (`Enabled = false`)
3. Superior com permissão aprova via `/api/Employees/approve`
4. Funcionário recebe acesso ao sistema
5. Pode fazer login via `/api/Auth/login`

## 🧪 Testes

### Executar Testes Unitários

```bash
cd backend
dotnet test
```

### Cobertura de Testes

```bash
# Executar com cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Gerar relatório HTML
dotnet tool run reportgenerator -reports:./tests/ManageEmployees.Tests/TestResults/coverage/coverage.opencover.xml -targetdir:./tests/ManageEmployees.Tests/TestResults/coverage/report
```

### Estrutura de Testes

- ✅ Testes de Unidade (Domain, Application, Infrastructure)
- ✅ Testes de Integração (API)
- ✅ Testes de Validação (FluentValidation)
- ✅ Testes de Serviços (Business Logic)
- ✅ Testes de Repositórios (Data Access)

## 📊 Visualização de Logs (Dozzle)

O projeto inclui o **Dozzle** - uma ferramenta simples e eficiente para visualizar logs em tempo real!

### 🚀 Acesse os Logs

**URL:** http://localhost:8888

**Sem login necessário!** Interface limpa e intuitiva.

### ✨ Funcionalidades

- 📱 **Interface Responsiva** - Funciona em qualquer dispositivo
- 🔴 **Tempo Real** - Veja logs conforme acontecem (live streaming)
- 🔍 **Busca Integrada** - Filtre logs por texto
- 📊 **Multi-Container** - Visualize logs de vários containers simultaneamente
- 💾 **Histórico** - Acesse até 300 últimas linhas de cada container
- 🎨 **Colorização** - Logs coloridos por nível (erro, warn, info)
- 📥 **Download** - Baixe logs para análise offline

### 📋 Containers Monitorados

O Dozzle monitora automaticamente todos os containers:

| Container | Logs |
|-----------|------|
| **manage-employees-api** | Logs do Backend (.NET 8) |
| **manage-employees-web** | Logs do Frontend (React/Nginx) |
| **manage-employees-db** | Logs do PostgreSQL |
| **manage-employees-pgadmin** | Logs do PgAdmin |

### 🎯 Como Usar

1. **Acesse** http://localhost:8888
2. **Selecione** um container no menu lateral
3. **Veja** os logs em tempo real!
4. **Use a busca** (Ctrl+F ou ícone 🔍) para filtrar
5. **Clique no ícone ⬇️** para baixar logs

### 💡 Dicas

- **Multi-visualização**: Clique em "+" para abrir múltiplos containers lado a lado
- **Pause automático**: Os logs pausam automaticamente quando você rola para cima
- **Busca avançada**: Suporta regex para buscas complexas
- **Tema escuro**: Interface moderna em dark mode

## 🐳 Docker

### Compatibilidade de Versões

| Versão | Comando | Status |
|--------|---------|--------|
| Docker Compose v2.0+ | `docker compose` (sem hífen) | ✅ Recomendado |
| Docker Compose v1.x | `docker-compose` (com hífen) | ⚠️ Legado |

**Como identificar sua versão:**
```bash
docker compose version    # v2+ retorna: "Docker Compose version v2.x.x"
docker-compose version    # v1.x retorna: "docker-compose version 1.x.x"
```

> **💡 Dica:** Se você tem Docker Desktop ou Docker Engine recente, provavelmente já tem a v2+ integrada. Use `docker compose` (sem hífen).

### Estrutura de Arquivos Docker

```
manage-employees/                    # ← Raiz do projeto
├── docker-compose.yml              # ← Orquestração de todos os serviços
├── backend/
│   ├── Dockerfile                  # Build do backend .NET 8
│   └── .dockerignore
└── frontend/
    ├── Dockerfile                  # Build do frontend React
    ├── nginx.conf                  # Configuração do Nginx
    └── .dockerignore
```

### Arquitetura de Containers

```
                ┌─────────────────────────────────────┐
                │        Docker Compose                │
                │       (raiz do projeto)              │
                └────────────┬────────────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
┌───────▼────────┐  ┌────────▼────────┐  ┌───────▼────────┐
│   Frontend     │  │    Backend      │  │   PostgreSQL   │
│   (Nginx)      │  │    (.NET 8)     │  │   (Database)   │
│   Port: 80     │  │   Port: 5000    │  │   Port: 5433*  │
└────────────────┘  └─────────────────┘  └────────────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
┌───────▼────────┐  ┌────────▼────────┐  ┌───────▼────────┐
│    PgAdmin     │  │     Dozzle      │  │                │
│   Port: 5050   │  │   Port: 8888    │  │   (Logs em     │
│                │  │  (Visualizador  │  │  Tempo Real)   │
└────────────────┘  │   de Logs)      │  └────────────────┘
                    └─────────────────┘

* PostgreSQL: 5433 (externa) → 5432 (interna do container)
  Evita conflito com PostgreSQL local
```

### Volumes Persistentes

| Volume | Conteúdo |
|--------|----------|
| `postgres_data` | Dados do banco PostgreSQL |
| `backend_uploads` | Fotos de perfil dos funcionários |
| `pgadmin_data` | Configurações do PgAdmin |

### Configurando PgAdmin

1. Acesse http://localhost:5050
2. Login: `admin@admin.com` / `admin123`
3. Add New Server:
   - **General > Name:** `Manage Employees`
   - **Connection > Host:** `postgres` (dentro do Docker) ou `localhost` (fora do Docker)
   - **Connection > Port:** `5432` (dentro do Docker) ou `5433` (fora do Docker)
   - **Connection > Database:** `manage_employees`
   - **Connection > Username:** `postgres`
   - **Connection > Password:** `postgres123`

> **💡 Dica:** Quando conectar pelo PgAdmin que está dentro do Docker, use `postgres:5432`. Se conectar com cliente externo (DBeaver, pgAdmin local, etc.), use `localhost:5433`.

### O que está sendo logado?

A aplicação possui **logging estruturado completo** em todos os níveis:

#### 📝 Logs de Autenticação
- ✅ **Login bem-sucedido** (usuário, IP, timestamp)
- ❌ **Tentativas de login falhadas** (e-mail inexistente, senha incorreta, usuário não aprovado)
- 🔐 **Mudanças de senha** (quem mudou, quando)
- 🔄 **Reset de senha** (quem resetou, para quem)
- 📋 **Auto-registro** (novo usuário aguardando aprovação)

#### 👥 Logs de Funcionários
- ➕ **Criação de funcionário** (quem criou, dados do novo funcionário)
- ✏️ **Atualização de funcionário** (quem editou, quais dados foram alterados)
- 🗑️ **Exclusão de funcionário** (quem excluiu, funcionário excluído)
- ✅ **Aprovação de cadastro** (quem aprovou, funcionário aprovado)
- ❌ **Rejeição de cadastro** (quem rejeitou, motivo da rejeição)
- 📸 **Upload de foto** (funcionário, timestamp)
- 🗑️ **Remoção de foto** (funcionário, timestamp)
- 👤 **Atualização de perfil** (dados alterados)

#### 🎭 Logs de Cargos
- ➕ **Criação de cargo** (nome do cargo, quem criou, permissões)
- ✏️ **Atualização de cargo** (alterações realizadas, quem atualizou)
- 🗑️ **Exclusão de cargo** (cargo excluído, quem excluiu)

#### 🌐 Logs HTTP (Todas as Requisições)
- 📊 **Método HTTP** (GET, POST, PUT, DELETE)
- 🛣️ **Endpoint acessado** (path completo)
- 🔢 **Status code** (200, 201, 400, 401, 403, 404, 500, etc.)
- ⏱️ **Tempo de resposta** (em milissegundos)
- 👤 **Usuário que fez a requisição** (e-mail ou "Anonymous")
- 🌍 **Endereço IP** do cliente

#### ⚠️ Logs de Erros e Exceções
- 🚨 **Erros não tratados** (exceção, stack trace, contexto completo)
- ⚠️ **Exceções de domínio** (validação, não autorizado, não encontrado, conflito)
- 🚫 **Acessos não autorizados** (tentativas de acesso sem permissão)
- ❌ **Validações falhadas** (campos inválidos, regras de negócio)

#### 💾 Logs de Banco de Dados
- 🔄 **Migrations aplicadas** (sucesso ou falha)
- 🌱 **Seed inicial** (criação de dados padrão)
- ⚠️ **Erros de conexão** com o banco


### Variáveis de Ambiente

Configuráveis em `docker-compose.yml`:

```yaml
# PostgreSQL
POSTGRES_USER: postgres
POSTGRES_PASSWORD: postgres123
POSTGRES_DB: manage_employees

# Backend
ConnectionStrings__DefaultConnection: Host=postgres;Port=5432;...
JwtSettings__Secret: YourSecretKey...
JwtSettings__ExpirationInMinutes: 60

# Frontend
VITE_API_URL: http://localhost:5000/api
```

### 🔧 Comandos de Manutenção e Troubleshooting

**🧹 Limpar TUDO e reiniciar do zero (banco limpo):**
```bash
# 1. Parar containers
docker compose stop

# 2. Remover containers, redes E volumes (APAGA O BANCO!)
docker compose down -v

# 3. Verificar que tudo foi removido
docker volume ls | grep manage-employees
# (não deve retornar nada)

# 4. Subir novamente (banco novo, migrations e seed executados)
docker compose up -d

# 5. Acompanhar logs para ver migrations e seed
docker compose logs -f backend
```

> ⚠️ **ATENÇÃO:** O `-v` remove **PERMANENTEMENTE**:
> - Todo o banco de dados PostgreSQL
> - Fotos de perfil dos usuários  
> - Configurações do PgAdmin

**Backup do banco:**
```bash
# Criar backup
docker exec manage-employees-db pg_dump -U postgres manage_employees > backup.sql

# Restaurar backup
docker exec -i manage-employees-db psql -U postgres manage_employees < backup.sql
```

**Acessar containers:**
```bash
# Backend
docker exec -it manage-employees-api bash

# Frontend
docker exec -it manage-employees-web sh

# PostgreSQL
docker exec -it manage-employees-db psql -U postgres -d manage_employees
```

### Troubleshooting

**Porta já em uso:**
```bash
# Verificar portas em uso
sudo lsof -ti:5000 | xargs kill -9  # Backend
sudo lsof -ti:80 | xargs kill -9    # Frontend
sudo lsof -ti:5433 | xargs kill -9  # PostgreSQL

# Ou alterar portas no docker-compose.yml
ports:
  - "5001:5000"  # Backend
  - "8080:80"    # Frontend
  - "5434:5432"  # PostgreSQL (se 5433 estiver em uso)
```

> **💡 Nota:** O PostgreSQL já usa porta 5433 por padrão para não conflitar com instalações locais (porta 5432).

**Containers não iniciam:**
```bash
# Ver logs detalhados (v2+)
docker compose logs
# ou (v1.x)
docker-compose logs

# Remover tudo e reconstruir (v2+)
docker compose down -v --rmi all
docker compose up -d --build

# ou (v1.x)
docker-compose down -v --rmi all
docker-compose up -d --build
```

**Verificar se está tudo funcionando:**
```bash
# 1. Ver status de todos os containers
docker compose ps
# Todos devem estar "Up" e o postgres "healthy"

# 2. Ver logs do backend
docker compose logs backend --tail 50
# Deve mostrar "Migrations aplicadas" e "Seed inicial concluído"

# 3. Verificar volumes criados
docker volume ls | grep manage-employees
# Deve mostrar 3 volumes: postgres_data, backend_uploads, pgadmin_data

# 4. Testar API
curl -s http://localhost:5000/swagger | grep "Swagger"
# Deve retornar HTML do Swagger

# 5. Testar frontend
curl -s http://localhost | head -5
# Deve retornar HTML
```

## 👤 Usuário Padrão

Ao iniciar o sistema pela primeira vez, é criado automaticamente:

| Campo | Valor |
|-------|-------|
| Nome | Administrador |
| Email | admin@admin.com |
| Senha | Master@123 |
| Cargo | Administrador |
| Permissões | Todas |

## 📱 Responsividade

A interface é totalmente responsiva, adaptando-se a:

- 📱 **Mobile** (< 768px) - Layout simplificado, menu hambúrguer
- 💻 **Tablet** (768px - 1024px) - Layout intermediário
- 🖥️ **Desktop** (> 1024px) - Layout completo com sidebar

## 🔒 Segurança

### Implementações de Segurança

- ✅ Senhas criptografadas com BCrypt
- ✅ Tokens JWT com expiração configurável
- ✅ Validação de entrada em todas as operações
- ✅ Proteção contra SQL Injection (EF Core)
- ✅ CORS configurado
- ✅ Headers de segurança no Nginx
- ✅ Validação de hierarquia em todas as operações
- ✅ Logs estruturados de todas as ações

### Recomendações para Produção

⚠️ **IMPORTANTE:** As credenciais padrão são apenas para desenvolvimento.

Em produção, você deve:

1. ✅ Alterar todas as senhas e secrets
2. ✅ Usar variáveis de ambiente ou secrets manager
3. ✅ Configurar HTTPS/SSL (certbot + nginx)
4. ✅ Restringir CORS para domínios específicos
5. ✅ Usar chave JWT forte (64+ caracteres)
6. ✅ Configurar firewall e limitar portas expostas
7. ✅ Implementar rate limiting
8. ✅ Configurar backup automático do banco

## 📄 Licença

Este projeto foi desenvolvido como avaliação técnica para demonstração de habilidades em:

- Desenvolvimento de APIs REST com .NET 8
- Frontend moderno com React e TypeScript
- Arquitetura limpa e boas práticas
- Containerização com Docker
- Testes unitários e de integração

---

**Desenvolvido com ❤️ usando .NET 8, React e Docker**
