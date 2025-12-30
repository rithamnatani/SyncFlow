
# Database Schema (EF Core)

## Enum: `TicketStatus`
Hardcoded columns based on the UI.
- `Todo` (Value: 0)
- `InProgress` (Value: 1)
- `Testing` (Value: 2)
- `Done` (Value: 3)

## Enum: `PriorityLevel`
- `Low` (Value: 0)
- `Medium` (Value: 1)
- `High` (Value: 2)
- `Critical` (Value: 3)

---

## 1. Project
Represents a Kanban board.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Id` | `Guid` | PK. |
| `Name` | `string` | MaxLength(100). Required. |
| `Description` | `string` | MaxLength(500). |
| `OwnerId` | `string` | **Indexed**. From Entra ID (Subject). |
| `InviteCode` | `string` | **Unique**. Random 6-8 char code for public links (optional usage). |
| `CreatedAt` | `DateTimeOffset` | UTC. |
| `UpdatedAt` | `DateTimeOffset` | UTC. |

*(Navigation Properties)*
- `List<Ticket> Tickets`
- `List<ProjectMember> Members`
- `List<JoinRequest> JoinRequests`

---

## 2. ProjectMember
Join table for Users <-> Projects.

| Property | Type | Description |
| :--- | :--- | :--- |
| `ProjectId` | `Guid` | Pk, FK to Project. |
| `UserId` | `string` | PK. Entra ID. |
| `Role` | `string` | "Member" or "Admin". |
| `JoinedAt` | `DateTimeOffset` | UTC. |

*(Navigation Properties)*
- `Project Project`

---

## 3. JoinRequest
Pending invitations/requests.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Id` | `Guid` | PK. |
| `ProjectId` | `Guid` | FK to Project. |
| `UserId` | `string` | Entra ID of requester. |
| `Status` | `string` | "Pending", "Declined". |
| `CreatedAt` | `DateTimeOffset` | UTC. |

*(Navigation Properties)*
- `Project Project`

---

## 4. Ticket
Represents a task on the board.

| Property | Type | Description |
| :--- | :--- | :--- |
| `Id` | `Guid` | PK. |
| `ProjectId` | `Guid` | **FK** to `Project`. Indexed. |
| `TicketNumber`| `int` | Friendly ID (User friendly, per project 1, 2, 3..). |
| `Title` | `string` | MaxLength(200). Required. |
| `Description` | `string` | MaxLength(4000). (Markdown support). |
| `Status` | `TicketStatus` | Enum. **Indexed**. |
| `Priority` | `PriorityLevel` | Enum. |
| `Type` | `string` | "Story", "Bug", etc. |
| `AssigneeId` | `string` | nullable. Entra ID. |
| `Tags` | `List<string>` | JSONB. |
| `Estimate` | `double` | |
| `Color` | `string` | |
| `RankId` | `double` | |
| `CreatedAt` | `DateTimeOffset` | UTC. |
| `UpdatedAt` | `DateTimeOffset` | UTC. |

*(Navigation Properties)*
- `Project Project`

---

## 5. TicketAttachment (Optional)

| Property | Type | Description |
| :--- | :--- | :--- |
| `Id` | `Guid` | PK. |
| `TicketId` | `Guid` | FK to Ticket. |
| `FileName` | `string` | |
| `BlobUrl` | `string` | |
| `UploadedBy` | `string` | |