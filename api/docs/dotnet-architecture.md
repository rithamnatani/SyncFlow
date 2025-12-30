# .NET 10 Vertical Slice Architecture (VSA)

This architecture organizes code by **Features** (Vertical Slices) rather than technical layers. Each feature is self-contained.

## 1. Project Structure
The root `api/` folder differs from traditional MVC.

```
/api
  /Features
     /Tickets
        /CreateTicket              <-- "Slice" for creating a ticket
           CreateTicket.cs         (The Azure Function Trigger)
           CreateTicketDto.cs      (The JSON Contract)
           CreateTicketHandler.cs  (The Business Logic)
        /MoveTicket
           MoveTicket.cs
           ...
     /Projects
        /JoinProject
           ...
  /Shared
     /Domain                       <-- Shared Entities (Ticket, Project)
     /Infrastructure               <-- Shared DB Context & Services
        AppDbContext.cs
        SignalRService.cs
  Program.cs                       <-- DI Container
```

## 2. Anatomy of a Slice (The "Why 3 Files?" Answer)

You asked: *"Why are there 3 files for creating a ticket?"*
**Answer**: To separate **Infrastructure** (Azure) from **Logic** (C#) from **Contract** (JSON).

### File 1: `CreateTicket.cs` (The Trigger)
*   **Role**: The "dumb" entry point.
*   **Responsibility**:
    *   Listens to HTTP POST `/api/tickets`.
    *   Auth: Checks `FunctionContext` / Keys.
    *   **Deserialization**: Reads the JSON body into the DTO.
    *   **Handoff**: Calls `Handler.HandleAsync(dto)`.
*   **Why split?**: If you ever switch from HTTP to a Queue Trigger, you *only* change this file. The logic stays the same.

### File 2: `CreateTicketDto.cs` (The Contract)
*   **Role**: The "Shape" of the data.
*   **Responsibility**: Defines exactly what the UI sends (e.g., `Title`, `Description`).
*   **Why split?**: Your internal database `Ticket` entity might have fields (`CreatedAt`, `OwnerId`) that you *never* want the user to set. The DTO protects your DB schema.

### File 3: `CreateTicketHandler.cs` (The Logic)
*   **Role**: The "Brain".
*   **Responsibility**:
    *   Validates rules (e.g., "Title cannot be empty").
    *   Talks to DB (`AppDbContext`).
    *   Saves the entity.
    *   Talks to SignalR (External Service).
*   **Why split?**: This is a pure C# class. You can **Unit Test** this easily without mocking the entire Azure Functions runtime.

---

## 3. Shared Layers
Some things *must* be shared to avoid duplication.

*   **/Shared/Domain**: The EF Core Entities (`Ticket`, `Project`). Everyone needs to know what a "Ticket" is.
*   **/Shared/Infrastructure**: `AppDbContext`, `SignalRService`, `EmailService`. These are **Injected** into the Handlers.

## 4. Master File Plan (Execution Blueprint)

This is the exact list of files we will create.

### A. Core Configuration (Root)
| File | Function |
| :--- | :--- |
| `Program.cs` | **App Entry**. Configures DI (`AddScoped`), middleware (`WorkerApplication`), and Service Bus / SignalR clients. |
| `host.json` | **Runtime Config**. Logging levels, extension bundles. |
| `local.settings.json` | **Secrets**. connection strings (Postgres, Redis, ServiceBus). |

### B. Shared Layer (`/Shared`)
**1. Domain (Entities)** - *Pure POCOs*
| File | Function |
| :--- | :--- |
| `Ticket.cs` | EF Entity. `Id` (Guid), `Title`, `Status` (Enum). |
| `Project.cs` | EF Entity. `Id` (Guid), `OwnerId` (String). |
| `ProjectMember.cs` | EF Entity. Join table user <-> project. |
| `JoinRequest.cs` | EF Entity. Pending invites. |
| `TicketStatus.cs` | Enum. (Todo, InProgress, Done). |

**2. Infrastructure (Services)** - *Heavy Lifting*
| File | Function |
| :--- | :--- |
| `AppDbContext.cs` | **EF Core**. `DbSet<Ticket>`, `OnModelCreating` (Config). |
| `SignalRService.cs` | **Realtime**. Wraps `IAsyncCollector` to push messages to Users/Groups. |
| `CurrentUserService.cs` | **Auth**. Parses `X-MS-CLIENT-PRINCIPAL` or `X-SyncFlow-Acting-User-Id`. |
| `ServiceBusService.cs` | **Messaging**. Sends commands to `q-agent-commands` (Agent Queue). |

### C. Features (Slices) - *The Application Logic*

**1. Ticket Slices (`/Features/Tickets`)**
| Slice | Files | Description |
| :--- | :--- | :--- |
| **GetTickets** | `GetTickets.cs` | [HttpTrigger] GET /projects/{id}/tickets. Returns `List<TicketDto>`. |
|  | `GetTicketsHandler.cs` | Queries Db.Where(ProjectId == id). |
| **CreateTicket** | `CreateTicket.cs` | [HttpTrigger] POST. |
|  | `CreateTicketDto.cs` | `Title`, `Priority`. |
|  | `CreateTicketHandler.cs` | Validates, Saves to DB, **SignalR Broadcast**. |
| **MoveTicket** | `MoveTicket.cs` | [HttpTrigger] PUT /tickets/{id}/move. |
|  | `MoveTicketDto.cs` | `NewStatus`, `RankId`. |
|  | `MoveTicketHandler.cs` | Updates Status/Rank. **SignalR Broadcast**. |
| **DeleteTicket** | `DeleteTicket.cs` | [HttpTrigger] DELETE. |
|  | `DeleteTicketHandler.cs` | Soft/Hard delete. **SignalR Broadcast**. |

**2. Project Slices (`/Features/Projects`)**
| Slice | Files | Description |
| :--- | :--- | :--- |
| **CreateProject** | `CreateProject.cs` | [HttpTrigger] POST. Sets `OwnerId`. |
| **JoinProject** | `JoinProject.cs` | [HttpTrigger] POST /join. Creates `JoinRequest`. |
|  | `JoinProjectHandler.cs` | Checks if user already member. Notifies Owner via **SignalR**. |
| **ApproveJoin** | `ApproveJoin.cs` | [HttpTrigger] POST /approve. |
|  | `ApproveJoinHandler.cs` | Moves Request -> Member. Notifies Guest. |

**3. Agent Slices (`/Features/Agent`)**
| Slice | Files | Description |
| :--- | :--- | :--- |
| **AgentCommand** | `AgentCommand.cs` | [HttpTrigger] POST. "Delete done tasks". |
|  | `AgentCommandHandler.cs` | Pushes message to **Service Bus**. |

**4. Realtime Slices (`/Features/Realtime`)**
| Slice | Files | Description |
| :--- | :--- | :--- |
| **Negotiate** | `Negotiate.cs` | [HttpTrigger] POST /negotiate. Returns SignalR Connection Token. |

---

## 5. Next Steps
1.  **Refactor**: I will delete the old temp files.
2.  **Scaffold**: I will create the `Shared` folder and `AppDbContext` first (Foundation).

---

## 6. Critical Implementation Details (Handoff Notes)

### A. Authentication Logic (`CurrentUserService`)
The `CurrentUserService` must handle **two** scenarios with specific priority:
1.  **Impersonation (High Priority)**: Check for `X-SyncFlow-Acting-User-Id`.
    *   *Security Condition*: This header is **ONLY** valid if the request also has a valid System/Function Key (System-to-System trust).
    *   If present and secure, Set `UserId = HeaderValue`.
2.  **User Login (Standard)**: Check for `X-MS-CLIENT-PRINCIPAL` (Easy Auth).
    *   Base64 decode -> JSON Deserialize -> Set `UserId = Principal.UserId`.
3.  **Anonymous**: If neither, throw `UnauthorizedAccessException` for protected endpoints.

### B. SignalR Configuration
*   **Hub Name**: Must be `"chat"` (matches Frontend).
*   **Attributes**: 
    *   Input: `[SignalRConnectionInfoInput(HubName = "chat")]`
    *   Output: `[SignalROutput(HubName = "chat")]`

### C. Service Bus Configuration
*   **Queue Name**: `"q-agent-commands"`.
*   **Message Format**: JSON.
    ```json
    {
      "command": "Delete all done tickets",
      "userId": "user-guid-123", /* Essential for Agent to know who is asking */
      "projectId": "project-guid-456"
    }
    ```

### D. Generic Repository vs Direct EF
*   **Decision**: Use **Direct EF Core** usage in Handlers.

---

## 7. Coding Standards (.NET 10 / C# 14)

> [!IMPORTANT]
> Strictly adhere to these patterns. Do not use legacy .NET 8/9 approaches.

### A. Entity Framework Core
1.  **JSON Mapping**: Do **NOT** use `OwnsOne`. Use `ComplexProperty` with `ToJson()`.
    ```csharp
    builder.Entity<Ticket>()
        .ComplexProperty(t => t.Tags, b => b.ToJson());
    ```
2.  **Vector Store**: Use `Pgvector.EntityFrameworkCore`.
    *   Config: `modelBuilder.HasPostgresExtension("vector");`
    *   Type: `vector(384)` (Matches models).
3.  **IDs**: Use PostgreSQL 18 Native **UUIDv7**.
    *   `id uuid PRIMARY KEY DEFAULT uuidv7()`

### B. C# 14 Features
1.  **Properties**: Use the `field` keyword. No private backing fields.
    ```csharp
    public string Title { get; set => field = value.Trim(); }
    ```
2.  **Performance**: Use `params ReadOnlySpan<T>` for array arguments.

### C. SignalR
1.  **Strong Typing**: Use `Hub<IChatClient>` interfaces. Do not use `SendAsync("stringName")`.



