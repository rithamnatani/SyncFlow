---
created: 2025-12-20 03:20
category:
  - "[[LLM Coding]]"
status:
---
# dotNET Delta 8-10

### TLDR

Change: use ComplexProperty with ToJson() for JSON columns instead of OwnsOne 
Change: use field keyword for properties instead of manual private backing variables 
Change: use implicit span conversions instead of explicit .AsSpan() 
Change: handle nulls explicitly in ExecuteUpdate/Delete due to strict nullability enforcement Change: use native .LeftJoin() and .RightJoin() instead of GroupJoin with SelectMany 
Deprecated: dont use SqlVector or EF.Functions.VectorDistance 
Feature: use Pgvector.EntityFrameworkCore with HasPostgresExtension("vector") and options.UseVector() 
Feature: use ExecuteUpdate to set specific JSON fields without retrieving entities 
Feature: use strongly-typed Hub\<T> with Redis Backplane for SignalR 
Feature: use params ReadOnlySpan\<T> for memory optimization Deprecated: dont use MD5 auth, use scram-sha-256 in pg_hba.conf 
Deprecated: dont use old_snapshot_threshold config (removed in PG17) 
Change: use VACUUM ONLY to target specific tables excluding children 
Change: place ago strictly at the end of interval strings 
Change: use native uuidv7() default instead of gen_random_uuid() or client-side gen 
Feature: use RETURNING OLD.* and NEW.* in UPDATE/DELETE statements 
Feature: use WITHOUT OVERLAPS in constraints for temporal data 
Feature: use JSON_TABLE to convert JSON payloads to relational rowsets 
Feature: use transaction_timeout to prevent stuck locks Feature: use halfvec type in pgvector for reduced storage 
Feature: use io_method = worker for concurrent I/O scans


# System Instruction: Yantra Tech Stack & Reference (.NET 10 / C# 14)

Context: You are assisting with Yantra, a real-time Kanban board application.

Current Date: December 2025.

Framework: .NET 10 (Preview/RC features are now stable release).

Language: C# 14.

Database: PostgreSQL (Npgsql) with pgvector and Redis.

## 🚨 Critical Breaking Changes & Anti-Patterns

_Do not use legacy patterns from .NET 8/9. Strictly adhere to these constraints:_

### 1. Entity Framework Core 10 (No `OwnsOne` for JSON)

- **❌ Anti-Pattern:** Do **NOT** use `builder.OwnsOne()` to map JSON columns. It causes identity resolution issues and breaks `ExecuteUpdate`.
    
- **✅ New Standard:** Use **Complex Types** with `ToJson()`.
    
    C#
    
    ```
    // Correct Mapping in OnModelCreating
    builder.Entity<TaskItem>()
        .ComplexProperty(t => t.Metadata, b => b.ToJson());
    ```
    

### 2. C# 14 Syntax Constraints

- **❌ Anti-Pattern:** Do not declare manual `private _backingField` variables for properties.
    
- **✅ New Standard:** Use the **`field`** keyword.
    
    C#
    
    ```
    public string Status 
    { 
        get; 
        set => field = value ?? "Todo"; // 'field' accesses the compiler-generated backing store
    }
    ```
    
- **❌ Anti-Pattern:** Do not call `.AsSpan()` explicitly on arrays.
    
- **✅ New Standard:** Rely on **Implicit Span Conversions**. `ReadOnlySpan<byte> data = myByteArray;` works natively.
    

### 3. LINQ & Database Updates

- **Strict Nullability:** `ExecuteUpdate` and `ExecuteDelete` now strictly enforce nullability. You must handle nulls in your LINQ expressions explicitly to avoid runtime exceptions.
    
- **Native Joins:** Use `.LeftJoin()` and `.RightJoin()` directly in LINQ queries. Do not use `GroupJoin` + `SelectMany` workarounds.

### 4. Vector Search (PostgreSQL/Npgsql Specifics)

- **❌ Anti-Pattern:** Do **NOT** use .NET 10's new `SqlVector` or `EF.Functions.VectorDistance` (these are SQL Server only).
    
- **✅ Library:** Use `Pgvector.EntityFrameworkCore`.
    
- **✅ Configuration:**
    
    - **Context:** `modelBuilder.HasPostgresExtension("vector");`
        
    - **Setup:** `options.UseNpgsql(connString, o => o.UseVector());`
        
    - **Mapping:** `[Column(TypeName="vector(384)")] public Vector? Embedding { get; set; }`
        
- **✅ Querying:** Use specific extension methods for distance.
    
    C#
    
    ```
    // Correct Npgsql/pgvector pattern
    var result = await context.Tasks
        .OrderBy(x => x.Embedding!.CosineDistance(queryVector))
        .Select(x => x.Title)
        .ToListAsync();
    ```
    

---

## 🚀 Yantra Architecture & Patterns

### 1. Vertical Slice Architecture

Organize code by **Feature**, not Layer.

- `src/Yantra.Server/Features/Board/GetBoard.cs` (Endpoint + Request + Response)
    
- `src/Yantra.Server/Features/Board/MoveTask.cs` (Command + Handler)
    
- `src/Yantra.Server/Features/Chat/Hubs/ChatHub.cs`
    

### 2. High-Performance Data Access

- **JSON Updates:** Update specific fields inside a JSON document without retrieving the entity.
    
    C#
    
    ```
    // Updates ONLY the IsAiGenerated field inside the Metadata JSON column
    await context.Tasks
        .Where(t => t.Id == taskId)
        .ExecuteUpdateAsync(s => s.SetProperty(t => t.Metadata.IsAiGenerated, true));
    ```
    


        

### 3. Real-Time Collaboration (SignalR)

- **Hub Design:** Use strongly-typed hubs (`Hub<IBoardClient>`) to ensure type safety with the Angular client.
    
- **Scaling:** configured with a Redis Backplane for horizontal scaling.
    
- **Auth:** Hubs must be protected via `[Authorize]`.
    

---

## 🛠️ Quick Copy-Paste Examples

**Example: C# 14 DTO with `field` validation**

C#

```
public class UpdateTaskRequest
{
    public required Guid TaskId { get; init; }
    
    // C# 14: Validation without manual backing fields
    public string Title 
    { 
        get; 
        set => field = value.Trim().Length > 0 ? value : throw new ArgumentException("Empty title"); 
    }
}
```

**Example: .NET 10 Minimal API Endpoint (Vertical Slice)**

C#

```
// Features/Board/Endpoints.cs
public class BoardEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPatch("/tasks/{id}/move", async (int id, MoveTaskRequest req, YantraDbContext db) => 
        {
            // .NET 10: ExecuteUpdate with Implicit Span/Params support
            await db.Tasks
                .Where(t => t.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.ColumnId, req.NewColumnId)
                    .SetProperty(t => t.UpdatedAt, DateTime.UtcNow));
            
            return Results.NoContent();
        })
        .RequireAuthorization();
    }
}
```

---
1. **`params` Performance:** C# 14 allows `params ReadOnlySpan<T>`, which saves massive memory in a high-throughput app like a Kanban board (updates happen frequently).
    

This addendum updates the database section of your reference sheet for the **PostgreSQL 17 & 18** era (current as of Dec 2025).

---

# 🐘 PostgreSQL 17 & 18 Addendum (Dec 2025)

Context: You are running PostgreSQL 18 (latest stable) or 17.

Upgrade Note: If migrating from PG16 or older, a full pg_dumpall / pg_upgrade is required.

## 🚨 Critical Breaking Changes (PG 17/18)

### 1. Authentication & Security

- **MD5 Auth Deprecated:** PostgreSQL 18 emits warnings if you create/alter users with MD5 passwords.
    
    - **Action:** Ensure your `pg_hba.conf` uses `scram-sha-256` and your local connection strings do not rely on legacy MD5 hashing.
        

### 2. Maintenance & Ops

- **`VACUUM` Behavior:** In PG18, `VACUUM` (and `ANALYZE`) now processes inheritance children by default.
    
    - **Fix:** If you have maintenance scripts targeting specific parent tables, use `VACUUM ONLY table_name` to restore old behavior.
        
- **Removed Configs:** `old_snapshot_threshold` was removed in PG17. Remove it from your `postgresql.conf` to avoid startup errors.
    

### 3. Syntax Strictness

- **Intervals:** `ago` is now strictly restricted to the **end** of an interval string (e.g., `'1 day ago'`). Placing it elsewhere causes a syntax error.
    

---

## ⚡ New Features for Yantra

### 1. Native UUIDv7 (PostgreSQL 18)

- **Feature:** Native `uuidv7()` generation is now built-in.
    
- **Why:** Replaces client-side "CombGUIDs" or extension-based UUIDs. UUIDv7 is time-ordered, offering significantly better B-Tree index performance than random UUIDv4.
    
    SQL
    
    ```
    -- Use this default instead of gen_random_uuid()
    id uuid PRIMARY KEY DEFAULT uuidv7()
    ```
    

### 2. Audit & Event Logs (`RETURNING OLD/NEW`)

- **Feature:** PostgreSQL 18 allows `RETURNING OLD.*` and `NEW.*` in `UPDATE`/`DELETE` statements.
    
- **Yantra Use Case:** Perfect for the "Task Activity Log" (e.g., moving a card). You can fetch the _previous_ column state and the _new_ state in a single query without a pre-fetch.
    
    SQL
    
    ```
    -- Single query move + audit
    UPDATE tasks 
    SET status = 'Done' 
    WHERE id = @id 
    RETURNING OLD.status as prev_status, NEW.status as new_status;
    ```
    

### 3. Temporal Constraints (PostgreSQL 18)

- **Feature:** Native support for `WITHOUT OVERLAPS` in primary/unique keys.
    
- **Yantra Use Case:** Critical for **Sprints** or **Time-Blocking**. You can natively prevent a user from having overlapping "Focus Modes" or "Sprint Dates" at the DB level.
    
    SQL
    
    ```
    CREATE TABLE sprints (
        project_id uuid,
        duration daterange,
        -- Prevents any two sprints for the same project from overlapping dates
        EXCLUDE USING gist (project_id WITH =, duration WITH &&) -- Pre-PG18 Way
        -- PG18 WAY: UNIQUE (project_id, duration WITHOUT OVERLAPS)
    );
    ```
    

### 4. `JSON_TABLE` (PostgreSQL 17)

- **Feature:** Convert JSON data into a relational rowset on the fly.
    
- **Yantra Use Case:** Bulk importing tasks from a JSON payload or analyzing "AI Metadata" blobs without complex CTEs.
    
    SQL
    
    ```
    SELECT * FROM JSON_TABLE(
        @json_input, '$[*]' 
        COLUMNS (
            title text PATH '$.title',
            severity int PATH '$.meta.severity'
        )
    ) as jt;
    ```
    

### 5. Performance: Async I/O & Transaction Timeouts

- **Async I/O (PG18):** New `io_method = worker` (default) allows concurrent I/O operations for scans. Excellent for vector search workloads on disk.
    
- **Transaction Timeout (PG17):** New `transaction_timeout` setting. Use this to safeguard against "stuck" transactions holding locks on the Board, distinct from query timeouts.
    

---

## 🤖 Pgvector Compatibility Note

- **Compatibility:** `pgvector` works seamlessly with PG17/18.
    
- **Optimization:** If storing millions of small embeddings, consider the `halfvec` type (2 bytes per dim) introduced in `pgvector 0.7.0` (supported in PG17+) to cut storage by 50%.