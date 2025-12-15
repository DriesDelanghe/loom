# Loom

Loom is a workflow-driven integration platform built around event sourcing and configurable workflows.

Instead of hardcoding ingestion and delivery pipelines, Loom lets you define workflows declaratively, while maintaining full auditability, replayability, and tenant-specific evolution of business data.

⸻

## Core principles
	•	Event-sourced master data
All business data owned by Loom (orders, customers, users, …) is stored as immutable events and rebuilt deterministically.
	•	Config-driven behavior
Workflows, validation rules, projections, and transformations are defined in configuration, not code.
	•	Strong execution guarantees
Workflow execution history is event-sourced, enabling debugging, retries, and replay.
	•	Multi-tenant by design
Each tenant evolves independently without schema migrations.

⸻

## What Loom is (and is not)
	•	Loom is not a task automation tool (Zapier / n8n).
	•	Loom is a state orchestration platform:
	•	workflows cause events
	•	events define truth
	•	projections expose state

⸻

## Architecture overview

External Systems
      ↓
   Webhooks / API
      ↓
 Workflow Engine
      ↓
 Event Store (ES)
      ↓
 Projection Engine
      ↓
 Read Models / APIs

	•	Execution: orchestrated via workflow definitions
	•	State: stored as immutable events
	•	Meaning: interpreted via projection rules

⸻

## Event sourcing model
	•	Event-sourced
	•	Master data (domain aggregates)
	•	Workflow execution history
	•	CRUD (records)
	•	Workflow definitions & versions
	•	Validation rules
	•	Projection rules
	•	Connector configuration
	•	Tenant metadata

⸻

## Technology stack
	•	.NET / C#
	•	PostgreSQL
	•	Marten (Event Store)
	•	JSON-first data model
	•	Azure Durable Task (planned)

⸻

## Development philosophy
	•	Prefer explicitness over magic
	•	Prefer configuration over code
	•	Prefer replayability over convenience
	•	Optimize for long-term velocity, not short-term shortcuts

⸻

## Status

🚧 Early development / architectural phase
The focus is currently on:
	•	configuration services
	•	event sourcing foundations
	•	projection & replay mechanics

⸻

Workflows cause events.
Events define truth.
Projections make truth usable.

⸻