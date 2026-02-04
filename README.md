# 🏦 AtlasBank OS

**Enterprise-Grade Core Banking Platform for Digital Financial Institutions**

AtlasBank OS is a production-ready, microservices-based core banking system designed for modern digital banks operating in South Africa and beyond. Built with .NET 8, PostgreSQL, Kafka, and Kubernetes.

## 🎯 Key Features

- **Account Management**: Full lifecycle management for all account types
- **Transaction Processing**: Real-time, reliable transaction pipeline with idempotency
- **Financial Products**: Configurable loans, savings, credit cards, and more
- **Compliance Engine**: Built-in KYC, AML, sanctions screening, and PEP checks
- **Multi-Tenancy**: Secure tenant isolation for multiple banking institutions
- **Event-Driven Architecture**: Kafka-based event streaming for real-time processing
- **High Availability**: Kubernetes-native with horizontal scaling
- **OAuth2/JWT**: Secure authentication and authorization
- **Domain-Driven Design**: Enterprise patterns for financial domain

## 🏗️ Architecture

```
┌──────────────────────┐
│   API Gateway        │
│    (Ocelot)          │
└──────────┬───────────┘
           │
    ┌──────┴─────────────────────┐
    │       Microservices        │
    └──────┬─────────────────────┘
           │
┌──────────┼──────────┬──────────┬──────────┬──────────┐
│          │          │          │          │          │
▼          ▼          ▼          ▼          ▼          ▼
Accounts   Txns       Products   Compliance Tenancy   Shared
Services   Services   Services   Services   Services   Libs
│          │          │          │          │          │
└──────────┴──────────┴──────────┴──────────┴──────────┘
           │
┌──────────┼──────────────────────┐
│   PostgreSQL    │    Kafka      │
│   Redis         │    Zookeeper  │
└─────────────────┴───────────────┘
```

## 📁 Project Structure

```
AtlasBank-OS/
├── shared/
│   ├── AtlasBank.Core/              # Domain models, enums, value objects
│   └── AtlasBank.Infrastructure/    # EF Core, Kafka, JWT, base classes
├── services/
│   ├── accounts/                    # Account lifecycle management
│   ├── transactions/                # Transaction processing
│   ├── products/                    # Financial products & loans
│   ├── compliance/                  # KYC, AML, risk scoring
│   └── tenancy/                     # Multi-tenant management & auth
├── gateway/
│   └── AtlasBank.Gateway/          # Ocelot API Gateway
├── infra/
│   ├── docker/                     # Docker Compose files
│   ├── kubernetes/                 # K8s manifests
│   └── terraform/                  # IaC for Azure
├── tests/
│   ├── AtlasBank.Tests.Unit/
│   └── AtlasBank.Tests.Integration/
├── .github/
│   └── workflows/                  # CI/CD pipelines
├── AtlasBank.sln
└── README.md
```

## 🚀 Quick Start

### Prerequisites

- .NET 8 SDK ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- Docker Desktop ([Download](https://www.docker.com/products/docker-desktop))
- PostgreSQL 16+ (or use Docker)
- Kafka 7.5+
- Postman or Curl for API testing

### Installation

#### 1. Clone and Navigate to Project

```powershell
cd "c:\Users\obake\Downloads\AtlasBank OS"
```

#### 2. Start Infrastructure (Docker)

```powershell
cd infra/docker
docker-compose up -d
```

Wait for all services to be healthy (check with `docker-compose ps`).

#### 3. Build and Restore All Services

```powershell
# From root directory
dotnet restore
dotnet build
```

#### 4. Run Database Migrations

```powershell
# Accounts Service
cd services/accounts/AtlasBank.Accounts.API
dotnet ef database update

# Transactions Service
cd ../../transactions/AtlasBank.Transactions.API
dotnet ef database update

# Products Service
cd ../../products/AtlasBank.Products.API
dotnet ef database update

# Compliance Service
cd ../../compliance/AtlasBank.Compliance.API
dotnet ef database update

# Tenancy Service
cd ../../tenancy/AtlasBank.Tenancy.API
dotnet ef database update
```

#### 5. Run Services

Run each in a separate terminal:

```powershell
# Terminal 1 - Accounts Service (Port 5001)
cd services/accounts/AtlasBank.Accounts.API
dotnet run

# Terminal 2 - Transactions Service (Port 5002)
cd services/transactions/AtlasBank.Transactions.API
dotnet run

# Terminal 3 - Products Service (Port 5003)
cd services/products/AtlasBank.Products.API
dotnet run

# Terminal 4 - Compliance Service (Port 5004)
cd services/compliance/AtlasBank.Compliance.API
dotnet run

# Terminal 5 - Tenancy Service (Port 5005)
cd services/tenancy/AtlasBank.Tenancy.API
dotnet run

# Terminal 6 - API Gateway (Port 5000)
cd gateway/AtlasBank.Gateway
dotnet run
```

### API Endpoints

Once running, access the following:

| Service      | Swagger UI                        | Base URL                  |
| ------------ | --------------------------------- | ------------------------- |
| Accounts     | http://localhost:5001/swagger     | http://localhost:5001/api |
| Transactions | http://localhost:5002/swagger     | http://localhost:5002/api |
| Products     | http://localhost:5003/swagger     | http://localhost:5003/api |
| Compliance   | http://localhost:5004/swagger     | http://localhost:5004/api |
| Tenancy      | http://localhost:5005/swagger     | http://localhost:5005/api |
| **Gateway**  | **http://localhost:5000/swagger** | **http://localhost:5000** |

## 📊 Technical Stack

### Backend

- **.NET 8** - Modern, high-performance framework
- **C# 12** - Latest language features
- **Entity Framework Core 8** - ORM with migrations
- **PostgreSQL 16** - Reliable relational database

### Architecture & Patterns

- **Domain-Driven Design (DDD)** - Rich domain models
- **CQRS** - Command Query Responsibility Segregation
- **MediatR 12** - In-process messaging
- **Value Objects** - Money, AccountNumber encapsulation
- **Aggregate Roots** - Transaction consistency
- **Domain Events** - Event-driven communication
- **Repository Pattern** - Data access abstraction

### Libraries & Tools

- **FluentValidation 11** - Fluent validation rules
- **Ocelot 20** - API Gateway with routing
- **Kafka** - Event streaming & pub/sub
- **Redis 7** - Caching & sessions
- **JWT/OAuth2** - Authentication
- **Serilog** - Structured logging
- **BCrypt.Net** - Secure password hashing

### DevOps & Infrastructure

- **Docker** - Containerization
- **Kubernetes (K8s)** - Orchestration
- **Docker Compose** - Local development
- **Terraform** - Infrastructure as Code
- **GitHub Actions** - CI/CD pipeline

## 🔐 Security & Compliance

### Multi-Tenancy

- Tenant isolation at database level
- Tenant ID in all queries
- Separate connection strings per database

### Authentication & Authorization

- JWT token-based authentication
- OAuth2/OpenID Connect ready
- RBAC (Role-Based Access Control)
- Password hashing with BCrypt
- Account lockout after failed attempts
- Token expiration & refresh

### Financial Security

- Transaction idempotency keys
- Soft deletes for audit trails
- Created/Updated timestamps & user tracking
- Money value object prevents currency mixing
- Pessimistic locking on critical operations

### Compliance Ready

- POPIA-compliant data handling
- SARB (South African Reserve Bank) ready
- Audit logging for all operations
- Compliance check enforcement
- KYC/AML/Sanctions screening integration

## 🧪 Testing

```powershell
# Unit tests
dotnet test tests/AtlasBank.Tests.Unit

# Integration tests
dotnet test tests/AtlasBank.Tests.Integration
```

## 📈 Example API Calls

### 1. Create Tenant

```bash
POST http://localhost:5005/api/tenants
Content-Type: application/json

{
  "tenantCode": "TESTBANK",
  "name": "Test Digital Bank",
  "legalName": "Test Digital Bank (Pty) Ltd",
  "registrationNumber": "2024/123456",
  "country": "South Africa",
  "primaryContact": "John Doe",
  "email": "info@testbank.co.za",
  "phone": "+27123456789"
}

# Response
{
  "tenantId": "550e8400-e29b-41d4-a716-446655440000"
}
```

### 2. Register User

```bash
POST http://localhost:5005/api/auth/register
Content-Type: application/json

{
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@testbank.co.za",
  "password": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+27123456789",
  "roles": ["Admin", "AccountManager"]
}

# Response
{
  "userId": "660e8400-e29b-41d4-a716-446655440001"
}
```

### 3. Login & Get Token

```bash
POST http://localhost:5005/api/auth/login
Content-Type: application/json

{
  "email": "user@testbank.co.za",
  "password": "SecurePass123!"
}

# Response
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "userId": "660e8400-e29b-41d4-a716-446655440001",
  "email": "user@testbank.co.za",
  "roles": ["Admin", "AccountManager"]
}
```

### 4. Create Account

```bash
POST http://localhost:5001/api/accounts
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "customerId": "CUST123",
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "productType": 1,
  "interestRate": 3.5
}

# Response
{
  "value": "f47ac10b-58cc-4372-a567-0e02b2c3d479"
}
```

### 5. Create Transfer

```bash
POST http://localhost:5002/api/transactions
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
Content-Type: application/json

{
  "tenantId": "550e8400-e29b-41d4-a716-446655440000",
  "sourceAccountId": "f47ac10b-58cc-4372-a567-0e02b2c3d479",
  "destinationAccountId": "g47ac10b-58cc-4372-a567-0e02b2c3d480",
  "amount": 1000.00,
  "currency": "ZAR",
  "description": "Payment for services"
}

# Response
{
  "value": "TXN20260203120000ABC123D4"
}
```

## 🐳 Docker Deployment

### Build Images

```powershell
docker build -t atlasbank/accounts-api:latest -f services/accounts/AtlasBank.Accounts.API/Dockerfile .
docker build -t atlasbank/transactions-api:latest -f services/transactions/AtlasBank.Transactions.API/Dockerfile .
docker build -t atlasbank/products-api:latest -f services/products/AtlasBank.Products.API/Dockerfile .
docker build -t atlasbank/compliance-api:latest -f services/compliance/AtlasBank.Compliance.API/Dockerfile .
docker build -t atlasbank/tenancy-api:latest -f services/tenancy/AtlasBank.Tenancy.API/Dockerfile .
docker build -t atlasbank/gateway:latest -f gateway/AtlasBank.Gateway/Dockerfile .
```

### Run with Docker Compose

```powershell
docker-compose -f infra/docker/docker-compose.yml up -d
```

## ☸️ Kubernetes Deployment

### Prerequisites

- kubectl installed and configured
- Access to a Kubernetes cluster

### Deploy to Kubernetes

```powershell
# Create namespace
kubectl apply -f infra/kubernetes/namespace.yaml

# Create configuration
kubectl apply -f infra/kubernetes/configmap.yaml

# Create secrets
kubectl apply -f infra/kubernetes/secrets.yaml

# Deploy all services
kubectl apply -f infra/kubernetes/accounts-deployment.yaml
kubectl apply -f infra/kubernetes/transactions-deployment.yaml
kubectl apply -f infra/kubernetes/products-deployment.yaml
kubectl apply -f infra/kubernetes/compliance-deployment.yaml
kubectl apply -f infra/kubernetes/tenancy-deployment.yaml
kubectl apply -f infra/kubernetes/gateway-deployment.yaml

# Check deployments
kubectl get deployments -n atlasbank
kubectl get services -n atlasbank

# Port forward for testing
kubectl port-forward -n atlasbank svc/gateway-service 5000:80
```

## 🔄 CI/CD Pipeline

GitHub Actions pipeline automatically:

1. Restores dependencies
2. Builds solution
3. Runs unit tests
4. Builds Docker images
5. Pushes to registry (when configured)

See `.github/workflows/build-deploy.yml` for details.

## 📚 Database Schema

### Accounts Service

- `Accounts` - Account records with balances
- `Account_Balance` - Embedded Money value object
- `Account_AvailableBalance` - Available funds

### Transactions Service

- `Transactions` - All transaction records
- `Transaction_Amount` - Embedded Money value object

### Products Service

- `Products` - Financial product definitions
- `Loans` - Loan agreements with terms

### Compliance Service

- `ComplianceChecks` - KYC, AML, PEP checks

### Tenancy Service

- `Tenants` - Bank tenant organizations
- `Users` - Tenant users with roles

## 🎓 Learning Resources

- **DDD**: [Domain-Driven Design by Eric Evans](https://www.domainlanguage.com/ddd/)
- **.NET 8**: [Microsoft .NET Documentation](https://learn.microsoft.com/en-us/dotnet/)
- **Entity Framework Core**: [EF Core Documentation](https://learn.microsoft.com/en-us/ef/core/)
- **CQRS**: [Microsoft CQRS Pattern](https://learn.microsoft.com/en-us/azure/architecture/patterns/cqrs)
- **Microservices**: [SAM Newman's Building Microservices](https://samnewman.io/books/building_microservices/)

## 🛠️ Troubleshooting

### Database Connection Issues

```powershell
# Test PostgreSQL connection
$env:PGPASSWORD = "postgres"
psql -h localhost -U postgres -d atlasbank_accounts -c "SELECT 1;"
```

### Kafka Issues

```bash
# Check Kafka is running
docker-compose logs kafka

# Create test topic
docker exec -it kafka kafka-topics --create --topic test --bootstrap-server kafka:9092
```

### Port Conflicts

If ports are in use, modify ports in:

- `appsettings.json` (change Kestrel port)
- `docker-compose.yml` (change port mappings)
- Service startup commands

## 📈 Performance Optimization

- Connection pooling configured in EF Core
- Indexes on frequently queried columns
- Soft delete query filters
- Async/await throughout
- Value object comparisons optimized
- JSON serialization caching

## 🔒 Security Checklist

- [ ] Change JWT secret in appsettings.json
- [ ] Use environment-specific configuration
- [ ] Enable HTTPS in production
- [ ] Configure CORS policies
- [ ] Set up database backups
- [ ] Enable audit logging
- [ ] Review compliance check rules
- [ ] Test account lockout mechanisms

## 📞 Support & Contribution

For issues, feature requests, or contributions, please follow standard git practices.

## 📄 License

MIT License - Built for portfolio demonstration

## 🚀 Production Readiness

This codebase is production-ready with:

- ✅ Error handling & logging
- ✅ Data validation
- ✅ Authentication & authorization
- ✅ Database migrations
- ✅ API documentation (Swagger)
- ✅ Containerization
- ✅ Kubernetes manifests
- ✅ Infrastructure as Code
- ✅ CI/CD pipeline ready
- ✅ Financial accuracy guarantees

---

**Built to showcase enterprise-level software engineering capabilities in the fintech domain** 🚀
