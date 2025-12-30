# Project Handoff Checklist

**Next Developer/AI:** Please follow this checklist to continue implementation.

## 1. Context
*   **Architecture**: .NET 10 Isolated Functions (VSA Pattern).
*   **Frontend**: Angular 21 (Zoneless).
*   **AI**: Python 3.14 (Pydantic AI).
*   **Auth**: Easy Auth (App Service) + Trusted Subsystem (for Agent).

## 2. Immediate Next Steps
- [ ] **NuGet Packages**: Install required packages (verify versions for .NET 10 preview if applicable).
    - `Microsoft.Azure.Functions.Worker`
    - `Microsoft.Azure.Functions.Worker.Extensions.Http`
    - `Microsoft.Azure.Functions.Worker.Extensions.SignalRService`
    - `Microsoft.Azure.Functions.Worker.Extensions.ServiceBus`
    - `Microsoft.EntityFrameworkCore.Npgsql` (Postgres)
- [ ] **Shared Layer**: Implement the empty files in `api/Shared/`.
    - `AppDbContext.cs`: Ensure `OnModelCreating` maps Enums correctly.
    - `CurrentUserService.cs`: Implement the Auth Priority logic (See `dotnet-architecture.md`).
- [ ] **Features**: Implement `api/Features/Tickets` first.
    - Verify SignalR messages reach the Angular client.

## 3. Important References
*   `api/docs/dotnet-architecture.md`: **Master Blueprint**. Contains the exact file structure and logic.
*   `docs/dbschema.md`: **Database Source of Truth**. Use these exact column names.
*   `docs/overview.md`: **System High Level**. Explains *why* we are doing things (e.g. Agent flow).

## 4. Nuances
*   **Guids**: We switched to `Guid` for all IDs.
*   **Impersonation**: The Python Agent *must* be able to call the API. Do not block it with standard User Auth checks if the System Key is present.
