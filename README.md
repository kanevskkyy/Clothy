# Clothy — Cloud-Native Fashion E-Commerce Platform

<div align="center">

![System Context](docs/images/C4%20L1%20%E2%80%94%20System%20Context.png)

[![CI/CD](https://github.com/kanevskkyy/Clothy/actions/workflows/ci-cd.yml/badge.svg)](https://github.com/kanevskkyy/Clothy/actions/workflows/ci-cd.yml)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![React](https://img.shields.io/badge/React-19-61DAFB?logo=react)
![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)
![License](https://img.shields.io/badge/License-MIT-green)

**Clothy** is a production-grade, event-driven microservices e-commerce platform for fashion retail — built with .NET 8, React 19, and a full cloud-native infrastructure stack.

</div>

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Microservices](#microservices)
- [Tech Stack](#tech-stack)
- [Key Features](#key-features)
- [Getting Started](#getting-started)
- [Environment Variables](#environment-variables)
- [Fraud Detection](#fraud-detection)
- [CI/CD Pipeline](#cicd-pipeline)
- [Testing Strategy](#testing-strategy)
- [Domain Diagrams](#domain-diagrams)

---

## Overview

Clothy is a full-stack, microservices-based fashion e-commerce application featuring virtual AI try-on, multi-provider payment processing, real-time notifications, and advanced order management. The platform is built around Domain-Driven Design principles with a clear bounded-context model and event-driven communication between services.

---

## Architecture

### C4 Level 1 — System Context

![System Context](docs/images/C4%20L1%20%E2%80%94%20System%20Context.png)

### C4 Level 2 — Container Diagram

![Container Diagram](docs/images/C4%20L2%20-%20Container%20Diagram.jpg)

The system is composed of 9 business microservices, an API Gateway (YARP), an Aggregator service, a Python AI service, and a React frontend — all orchestrated via Docker Compose and developed locally with .NET Aspire.

### Component Diagram

![Component Diagram](docs/images/component-diagram.jpg)

### Domain Context Map

![Context Map](docs/images/context%20map.jpg)

---

## Microservices

| Service | Responsibility | Database | Communication |
|---|---|---|---|
| **Catalog** | Products, stock, brands, collections | PostgreSQL + Redis | REST + gRPC Server |
| **Basket** | Shopping cart management | Redis | REST + gRPC Client/Server |
| **Order** | Order lifecycle, delivery, regions | PostgreSQL | REST + gRPC Client/Server |
| **Payment** | Stripe & crypto payment processing | PostgreSQL | REST + gRPC Client |
| **Review** | Product reviews & Q&A | MongoDB | REST + gRPC Client/Server |
| **Auth** | Authentication via Keycloak | Redis | REST |
| **User** | User profile management | PostgreSQL | REST |
| **Notification** | Email notifications | — (stateless) | RabbitMQ consumer |
| **Identity Server** | OAuth2 / OpenID Connect | PostgreSQL | Keycloak |
| **Aggregator** | Composite queries for the frontend | — (stateless) | gRPC Client |
| **Gateway** | Routing, auth enforcement | — | YARP reverse proxy |
| **AI Try-On** | Virtual clothing try-on | — | HTTP (LightX + Cloudinary) |

### Service-Layer Architecture

#### Catalog — Layered Architecture
![Catalog Layered Architecture](docs/images/C4%20L3%20-%20CatalogService%20%28Layered%20Architecture%29.jpg)

#### Review — Clean Architecture
![Review Clean Architecture](docs/images/C4%20L3%20%E2%80%94%20ReviewService%20%28Clean%20Architecture%29.jpg)

### Package Overview

![Package Diagram](docs/images/package.png)

---

## Tech Stack

### Backend
| Category | Technology |
|---|---|
| Framework | .NET 8, ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Service mesh | gRPC (Protobuf), MassTransit 8.5 |
| Message broker | RabbitMQ 3 |
| Caching | Redis 7, StackExchange.Redis |
| Databases | PostgreSQL 16, MongoDB 7 |
| Identity | Keycloak 26 (OAuth2 / OIDC), JWT |
| Payments | Stripe, NOWPayments (crypto) |
| Reverse proxy | YARP 2.3 |
| Observability | OpenTelemetry, Serilog, Prometheus |
| Orchestration | .NET Aspire 9, Docker Compose |
| Cloud storage | Cloudinary |
| Delivery | Nova Poshta API |

### Frontend
| Category | Technology |
|---|---|
| UI | React 19, TypeScript 5.9 |
| Build | Vite 7.2 |
| Styling | Tailwind CSS |
| State | Zustand 5 |
| Data fetching | TanStack React Query 5.90 |
| Routing | React Router 7.12 |
| Validation | Zod 4.3 |
| HTTP | Axios |

### AI Service
| Category | Technology |
|---|---|
| Language | Python 3.11 |
| Framework | FastAPI, Uvicorn |
| Try-on API | LightX |
| Images | Cloudinary |

---

## Key Features

- **Virtual AI Try-On** — Upload a photo and preview how clothes look on you via the LightX API
- **Event-Driven Architecture** — All cross-service side effects are propagated via RabbitMQ events through MassTransit
- **Multi-Provider Payments** — Stripe (cards) and NOWPayments (crypto) with webhook handling
- **Real-Time Stock Notifications** — Customers subscribe to out-of-stock items and receive email alerts when restocked
- **gRPC-First Inter-Service Communication** — Strongly typed contracts for all service-to-service calls
- **Resilience Patterns** — Circuit breakers, retries, and health checks on all services

### Resilience — Circuit Breaker

![Circuit Breaker](docs/images/circuit-breaker.jpg)

- **Role-Based Access** — Keycloak-backed OAuth2/OIDC with JWT enforcement at the Gateway layer
- **Structured Observability** — Distributed tracing (OpenTelemetry), structured logging (Serilog), and metrics (Prometheus)
- **Nova Poshta Integration** — Live pickup-point and settlement lookup for Ukrainian delivery
- **Cloudinary Image Management** — Automatic image optimization, transformation, and CDN delivery

---

## Getting Started

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) 24+
- [.NET SDK 8.0](https://dotnet.microsoft.com/download)
- [Node.js 20+](https://nodejs.org/) (for local frontend development)

### Quick Start with Docker Compose

```bash
# 1. Clone the repository
git clone https://github.com/kanevskkyy/Clothy.git
cd Clothy

# 2. Copy and fill in environment variables
cp .env.example .env

# 3. Start all services
docker compose up -d

# 4. Open the app
# Frontend:        http://localhost:3000
# API Gateway:     http://localhost:8094
# Keycloak:        http://localhost:8080
# RabbitMQ UI:     http://localhost:15672
```

### Local Development with .NET Aspire

```bash
# Run the orchestration host (starts all .NET services with hot-reload)
dotnet run --project src/Clothy.AppHost
```

The Aspire dashboard provides live logs, traces, and health status for all services.

---

## Environment Variables

Copy `.env.example` to `.env` and fill in the required values:

```env
# Cloudinary (image storage)
CLOUDINARYSETTINGS__CLOUDNAME=
CLOUDINARYSETTINGS__APIKEY=
CLOUDINARYSETTINGS__APISECRET=

# Keycloak (identity)
KEYCLOAK__URL=http://keycloak:8080
KEYCLOAK__REALM=clothy-realm
KEYCLOAK__CLIENTID=clothy-api
KEYCLOAK__CLIENTSECRET=          # obtain from Pulumi output

# Gmail (notifications)
GMAIL__APP_PASSWORD=
GMAIL__FROM=
GMAIL__FROM_NAME=Clothy

# Stripe (payments)
STRIPE__SECRET_KEY=
STRIPE__PUBLISHABLE_KEY=
STRIPE__WEBHOOK_SECRET=

# NOWPayments (crypto payments)
NOWPAYMENTS__API_KEY=
NOWPAYMENTS__CALLBACK_URL=
NOWPAYMENTS__WEBHOOK_SECRET=

# AI Try-On
LIGHT_X_API_KEY=

# Nova Poshta (delivery)
NOVAPOSHTA__API_KEY=

# URLs
FRONTEND__URL=http://localhost:3000
VITE_API_URL=http://localhost:8094

# Fraud detection (all optional, defaults shown)
FRAUD__URL=http://fraud-service:8097
FRAUD__MODEL_VARIANT=main
FRAUD__FLAG_RATE=0.01
FRAUD__RING_SIGNAL=on
FRAUD__LOG_LEVEL=INFO
```

---

## Fraud Detection

`Clothy.AI.FraudService` (Python, FastAPI, XGBoost) scores every card payment after it succeeds and flags
risky orders for manual review in the admin panel. It is the demo service of a bachelor's thesis
(graph-antifraud): features come from the history of the card, card + postal code and card + email pairs,
disputes of neighbouring transactions (known after 7 days, 30-day window) and the device fingerprint.
The answer is a fraud probability, a decision against a threshold chosen on validation data, the top-3
reasons and a transparent "ring" signal (one device paying with several new cards).

Buyers never see anything: scoring happens after the payment, and the payment never depends on it.

```mermaid
flowchart LR
    B[Buyer] -->|checkout + device data| P[PaymentService]
    P -->|Checkout Session<br/>metadata: order_id, device| S[(Stripe)]
    S -->|checkout.session.completed| P
    P -->|POST /score<br/>timeout 2 s, circuit breaker| F[FraudService<br/>XGBoost + SQLite history]
    S -->|charge.dispute.created / closed| P
    P -->|POST /labels fraud=true/false| F
    P -->|Disputed / DisputeWon / DisputeLost| DB[(ClothyPaymentDb)]
    A[Admin panel] -->|GET /api/fraud/tx, decisions<br/>role Admin| G[Gateway]
    G --> F
    A -->|GET /api/payments/admin/statuses| G --> P
```

| Piece | Where |
|---|---|
| Service, tests, Dockerfile | `src/Clothy.AI.FraudService` (own [README](src/Clothy.AI.FraudService/README.md)) |
| Scoring client and Stripe mapping | `PaymentService.BLL/Fraud` (`IFraudScoringClient`, `FraudRequestMapper`) |
| Dispute webhook | `PaymentService.BLL/Services/StripeDisputeHandler.cs` |
| Admin UI | `/admin/orders` (risk badge, "Лише на перевірку" filter), `/admin/orders/:id` (reasons, ring signal, dispute status) |

**How a payment is scored.** On `checkout.session.completed` PaymentService marks the payment as Paid and
publishes `OrderPaidEvent` exactly as before, then reads the charge from Stripe and sends `POST /score`:
`txid` = order id, `ts` = charge time (Unix seconds), `TransactionAmt` = order total,
`card1` = 53-bit hash of `card.fingerprint`, `card4`/`card6` = brand/funding, `addr1` = billing postal code
if it is numeric, `P_emaildomain` = buyer email domain, `DeviceType`/`DeviceInfo`/`id_30`/`id_31`/`id_33` =
device data from the storefront, `ProductCD` = `W`. If FraudService is down, slow (> 2 s), returns an error
or rejects a late webhook (409 for a `ts` earlier than the last one), the order stays **not scored** and
only a warning is logged. A 409 for a duplicate txid counts as "already scored". Card fingerprint, email,
address and device fields are never logged.

**Disputes.** `charge.dispute.created` sets the payment to `Disputed` and labels the order as fraud
(`at` = dispute creation time); `charge.dispute.closed` sets `DisputeWon` (label removed) or `DisputeLost`.
The order is found through `order_id` / `payment_id` in PaymentIntent metadata (older payments: through
their Checkout Session). Repeated events change nothing. Events with an invalid signature are not processed.
The order status in OrderService is not changed.

### Running it

```powershell
# 1. Model files are not in git: copy them from the graph-antifraud repository
$src = "D:\graph-antifraud\models"; $dst = "src\Clothy.AI.FraudService\models"
"service_main.json","service_device.json","encoders.json","service_config.yaml","ring_signal.yaml","fixture_synthetic.npz" |
  ForEach-Object { Copy-Item (Join-Path $src $_) $dst }

# 2a. Docker: fraud-service is built with the rest; it is reachable only inside clothy-network
docker compose up --build

# 2b. Local development (Aspire or dotnet run): start the service on port 8097,
#     PaymentService and the gateway use http://localhost:8097 by default
cd src\Clothy.AI.FraudService
python -m venv .venv; .venv\Scripts\Activate.ps1; pip install -r requirements.txt
uvicorn src.service:create_app --factory --port 8097
```

The decision history lives in SQLite in the `frauddata` volume (`/data/history.db`) and survives restarts.

### Stripe setup (test mode only)

In Stripe Dashboard → Developers → Webhooks, the endpoint `<gateway>/api/payments/webhook/stripe` needs
these events: `checkout.session.completed`, `charge.dispute.created`, `charge.dispute.closed`.
Other events are ignored with 200.

Local test with the Stripe CLI:

```powershell
stripe login
stripe listen --forward-to localhost:8094/api/payments/webhook/stripe `
  --events checkout.session.completed,charge.dispute.created,charge.dispute.closed
# put the printed whsec_... into STRIPE__WEBHOOK_SECRET and restart payments
```

1. Pay for an order in the store with the test card `4000 0000 0000 0259` (any future date, any CVC).
   The payment succeeds, the order gets a risk badge, and Stripe opens a dispute automatically:
   the payment becomes `Disputed` and the order is labelled in FraudService.
2. Close the dispute: submit evidence `winning_evidence` (→ `DisputeWon`, label removed) or
   `losing_evidence` (→ `DisputeLost`) in the Dashboard or with
   `stripe disputes update dp_... -d "evidence[uncategorized_text]=winning_evidence" -d submit=true`.
3. `stripe trigger charge.dispute.created` only checks signature and routing: the triggered payment has no
   Clothy order, so the handler logs "No payment found" and changes nothing.

### Limitations

- The model is trained on **IEEE-CIS Fraud Detection**, not on shop data. Stripe fields map to IEEE-CIS
  fields only approximately (e.g. `card1` is a hash, IEEE-CIS card attributes `card2/3/5` and distances are
  missing), so scores are **for demonstration only**; real use needs retraining on the shop's own data.
- The thresholds give the chosen flag rate only on the validation distribution; on live traffic it shifts.
- The ring signal turns on after 14 days of history in the store.
- Payments must reach the service in time order; a late webhook leaves the order not scored.
- SQLite history means one FraudService instance.

---

## CI/CD Pipeline

![CI/CD Pipeline](docs/images/ci-cd.jpg)

The pipeline runs on every push to `main` and every pull request, with four sequential jobs:

```
Unit Tests  →  Integration Tests  →  Contract Tests  →  Docker Build & Push
   (7 services)      (5 services)       (5 services)       (11 images → DockerHub)
```

- **Unit Tests** — 7 service matrices run in parallel with NuGet caching
- **Integration Tests** — 5 services, require passing unit tests
- **Contract Tests** — Consumer-driven contract tests, require all prior tests
- **Docker Build & Push** — Builds and pushes 11 Docker images to DockerHub using GitHub Actions cache; only runs on `main` after all tests pass

---

## Testing Strategy

The project follows a comprehensive testing pyramid:

| Layer | Services Covered | What is tested |
|---|---|---|
| **Unit Tests** | 7 services | Business logic, domain rules, handlers |
| **Integration Tests** | 5 services | Database interactions, repository layer |
| **Contract Tests** | 5 services | gRPC & REST API contracts between services |
| **E2E Tests** | Full flow | Critical user journeys end-to-end |

```bash
# Run all tests for a specific service
dotnet test tests/UnitTests/Clothy.CatalogService.UnitTests
dotnet test tests/IntegrationTests/Clothy.CatalogService.IntegrationTests
dotnet test tests/ContractTests/Clothy.CatalogService.ContractTests
```

---

## Domain Diagrams

### Use Case Diagram

![Use Case Diagram](docs/images/use-case-diagram.jpg)

### Order Aggregate — Class Diagram

![Order Aggregate Class Diagram](docs/images/order-aggregate-class-diagram.jpg)

### State Diagram — Order Lifecycle

![Order State Diagram](docs/images/state-order.jpg)

### Activity — Place Order

![Activity Place Order](docs/images/activity-place-order.jpg)

### Activity — Pay for Order

![Activity Pay for Order](docs/images/activity-pay-for-order.jpg)

### Sequence — Add to Cart

![Sequence Add to Cart](docs/images/sequence-add-to-cart.jpg)

### Communication — Add to Cart

![Communication Add to Cart](docs/images/communication-add-to-cart.jpg)

### Sequence — Order Happy Path

![Sequence Happy Path](docs/images/sequence-diagram-happy-path.jpg)

### Sequence — Order Failure Path

![Sequence Failure Path](docs/images/sequence-diagram-failure-path.jpg)

### Sequence — Event Flow

![Sequence Event Flow](docs/images/sequence-diagram-event-flow.jpg)

### Auth Flows

**Login**

![Auth Login](docs/images/auth-sequence-login.jpg)

**Get Current User**

![Auth Get Me](docs/images/auth-sequence-get-me.jpg)

**Refresh Token**

![Auth Refresh Token](docs/images/auth-sequence-refresh-token.jpg)

### Deployment Diagram

![Deployment Diagram](docs/images/deployment.jpg)

---

<div align="center">
  Built with ❤️ using .NET 8, React 19, and cloud-native technologies.
</div>
