# CLAUDE.md

## Repository layout

- `Backend/` — ASP.NET Core 10 Web API with EF Core/Npgsql.
- `Frontend/` — Nuxt 4 application using Nuxt UI.

The repository is intentionally business-domain-free. Authentication, users, roles, permissions,
session renewal and auditing are shared infrastructure and must remain reusable. Add business
features in their own model, controller, service, view-model, composable, component and page files.

## Commands

```bash
dotnet restore Backend.sln
dotnet build Backend.sln
dotnet test Backend.IntegrationTests/Backend.IntegrationTests.csproj
dotnet run --project Backend/MbyteWeb.Server.csproj --launch-profile http
pnpm --dir Frontend install
pnpm --dir Frontend dev
pnpm --dir Frontend typecheck
pnpm --dir Frontend lint
pnpm --dir Frontend build
```

## Reusable backend foundations

- `Models/BaseModel.cs` provides integer IDs, timestamps, soft delete and optimistic concurrency.
- `Data/AppDbContext.cs` hosts Identity and applies timestamps, audit records, session versioning
  and soft-delete behavior.
- `Authorization/`, the Admin/Auth services and their controllers are reusable access infrastructure.
- `Services/Interfaces/IRepository.cs` and `Services/Implementations/Repository.cs` provide the
  generic persistence base.
- `Helpers/Filter.cs` and `Helpers/Pagination.cs` provide query filtering and pagination.
- `Middlewares/ExceptionHandlerMiddleware.cs` maps typed exceptions to HTTP responses.

`Migrations/InitialAuth` is the reusable access schema. Add later migrations for business models.

## Reusable frontend foundations

- `composables/useAuth.ts`, `usePermisos.ts` and `useApi.ts` handle JWT sessions and authorization.
- Login, password change and user-management pages are part of the reusable foundation.
- `composables/useCrud.ts` provides conventional CRUD calls.
- `composables/useCrudResource.ts` manages paginated list state and row actions.
- `composables/useCrudFilters.ts` builds declarative filters.
- `components/CrudPanel.vue`, `CrudTable.vue`, `CrudFilters.vue` and `PageSizeSelect.vue` provide
  the reusable list UI.
