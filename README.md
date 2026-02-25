# Real-Time Financial Monitor (MVP)

Full-stack MVP for ingesting financial transactions and streaming them live to a monitoring dashboard.

## Transaction Model

```json
{
  "transactionId": "guid-string",
  "amount": 1500.50,
  "currency": "USD",
  "status": "Pending | Completed | Failed",
  "timestamp": "2024-01-15T10:00:00Z"
}
```

## Stack

- Backend: ASP.NET Core (.NET 9), SignalR
- Frontend: React + TypeScript + Vite
- Storage: In-memory (thread-safe, bounded to latest 1000)
- Tests: xUnit, FluentAssertions, Moq

## Implemented Requirements

- `POST /transactions` ingestion endpoint
- Real-time broadcast via SignalR hub (`/hub/transactions`, `ReceiveTransaction`)
- Thread-safe in-memory transaction repository
- Frontend routes:
  - `/add` transaction simulator
  - `/monitor` live dashboard
- UX:
  - status colors
  - client-side filtering
  - animated new-row entry and status-change pulse
- Performance:
  - buffered live updates using `requestAnimationFrame`
- Automated unit tests for validation, service flow, repository, and hub broadcast

## Local Run

Backend:

```bash
dotnet run --project RealTimeMonitor
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

Tests:

```bash
dotnet test mid-fullstack-project.sln
```

## Docker

Build:

```bash
docker build -t real-time-monitor:latest .
```

Run:

```bash
docker run --rm -p 8080:8080 real-time-monitor:latest
```

Health endpoint:

- `GET /health`

## Kubernetes

Manifests:

- `k8s/deployment.yaml`
- `k8s/service.yaml`

Apply:

```bash
kubectl apply -f k8s/deployment.yaml
kubectl apply -f k8s/service.yaml
```

## Distributed Multi-Pod Sync (Cloud-Ready Design)

Problem: with multiple replicas, clients connected to Pod A may miss events ingested by Pod B.

Recommended architecture:

1. Use shared SignalR backplane (Redis) or managed SignalR service.
2. Publish each accepted transaction to the shared bus.
3. Every pod consumes the event and broadcasts to its local clients.

This removes pod-local event isolation and keeps dashboards consistent across replicas.
