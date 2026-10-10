# Proyecto base

Base reutilizable para una API ASP.NET Core y una interfaz Nuxt. Conserva la infraestructura común
de usuarios y acceso, pero no incluye recursos ni datos de un dominio de negocio.

## Incluye

- Backend ASP.NET Core con PostgreSQL, Swagger, CORS, NLog y manejo centralizado de errores.
- Autenticación JWT, usuarios, roles, permisos, renovación de sesión y auditoría.
- Entidades base con timestamps, borrado lógico y concurrencia optimista.
- Repositorio y ejecutor de consultas genéricos.
- Helpers de filtros y paginación.
- Frontend Nuxt UI con login, administración de usuarios, cambio de contraseña, guards de rutas,
  cliente HTTP y componentes/composables CRUD genéricos.

## Requisitos

- .NET SDK 10
- Node.js 20 o superior y pnpm 12.4.1
- PostgreSQL 16 o compatible

## Primer recurso

1. Creá el modelo en `Backend/Models` y agregá su `DbSet` y configuración en `AppDbContext`.
2. Creá el controlador y, si hace falta, un repositorio específico sobre `IRepository<T>`.
3. Generá una migración para incorporar el recurso al esquema base.
4. En el frontend, creá sus tipos, composable API y página usando las bases CRUD existentes.

```bash
dotnet ef migrations add NombreDelRecurso --project Backend/MbyteWeb.Server.csproj --startup-project Backend/MbyteWeb.Server.csproj
dotnet ef database update --project Backend/MbyteWeb.Server.csproj --startup-project Backend/MbyteWeb.Server.csproj
```

## Desarrollo

```bash
dotnet restore Backend.sln
dotnet run --project Backend/MbyteWeb.Server.csproj --launch-profile http
pnpm --dir Frontend install
pnpm --dir Frontend dev
```

La API queda en `http://localhost:5122`, Swagger en `/swagger` y Nuxt en
`http://localhost:3000`. Ajustá la conexión de ejemplo de `Backend/appsettings.json` antes de
crear la base y reemplazá las claves de ejemplo de JWT/autorización antes de desplegar.

## Validación

```bash
dotnet build Backend.sln
dotnet test Backend.IntegrationTests/Backend.IntegrationTests.csproj
pnpm --dir Frontend typecheck
pnpm --dir Frontend lint
pnpm --dir Frontend build
```
