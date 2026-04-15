# EventFlowX 🚀

EventFlowX is a **.NET 10 based distributed event-driven system** implementing:
- Outbox Pattern
- Inbox Pattern
- RabbitMQ messaging
- SQL Server persistence
- Worker-based consumers

It demonstrates how microservices communicate reliably using event-driven architecture with guaranteed delivery.

---

## 📌 Architecture Overview

System consists of:

### 🧩 Services

- **EventFlowX.Host**
  - API entry point
  - Creates Outbox events
  - Publishes events via RabbitMQ

- **EventFlowX.Consumer**
  - Background worker service
  - Consumes RabbitMQ messages
  - Writes to Inbox table
  - Ensures idempotent processing

- **RabbitMQ**
  - Message broker

- **SQL Server**
  - Stores:
    - Outbox events
    - Inbox events
    - Processing state

---

## 🔁 Event Flow

1. Client calls API:

POST /orders


2. Host:
- Creates Outbox Event
- Saves to SQL Server
- Publishes event to RabbitMQ

3. Consumer:
- Receives event
- Checks Inbox (idempotency)
- Processes event
- Stores result in Inbox table

4. DB becomes **source of truth for event tracking**

---
<img width="1521" height="867" alt="image" src="https://github.com/user-attachments/assets/8c048d7b-bcf0-442f-937c-80169bfbcdde" />

## 📡 API Endpoints

### ➤ Create Order Event

```http
POST http://localhost:8080/orders


Creates a new order event and publishes it.

➤ Get Events
GET http://localhost:8080/events

Returns all processed events from Outbox + Inbox tracking.

🐳 Run with Docker
1. Start system
docker compose up --build
2. Services
Service	URL
Host API	http://localhost:8080

RabbitMQ UI	http://localhost:15672

SQL Server	localhost:1433
🔐 Default Credentials
RabbitMQ
Username: admin
Password: admin
SQL Server
User: sa
Password: Admin1234!
🧠 Key Concepts Implemented
✔ Outbox Pattern

Ensures reliable event publishing by storing events before sending.

✔ Inbox Pattern

Prevents duplicate processing (idempotency).

✔ Worker Services

Background consumers processing events asynchronously.

✔ Event Driven Architecture

Services communicate via events instead of direct calls.

📊 Database Tables
OutboxEvents
InboxEvents
Pods (worker tracking)
🧪 How to Test
1. Send event
curl -X POST http://localhost:8080/orders
2. Check database
SELECT * FROM EventFlowX_Outbox.dbo.OutboxEvents;
SELECT * FROM EventFlowX_Inbox.dbo.InboxEvents;
3. Verify flow
Outbox → Created by Host
Inbox → Processed by Consumer
📷 Example Flow
API call creates event
RabbitMQ delivers message
Consumer processes it
DB shows:
Outbox: Created
Inbox: Processed
🛠 Tech Stack
.NET 10
ASP.NET Core
Entity Framework Core
SQL Server
RabbitMQ
Docker
Background Services
📌 Author

Built by Hüseyin Kazım Tosun

📄 License

This project is for learning and demonstration purposes.
