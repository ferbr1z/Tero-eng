#!/usr/bin/env bash
#
# Corre la batería de tests de integración del backend (.NET + Testcontainers/Postgres real).
# Uso:  ./test.sh
# Devuelve exit code != 0 si fallan (sirve para CI).
#
set -uo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$ROOT"

BE_INTEGRATION_PROJECT="Backend.IntegrationTests/Backend.IntegrationTests.csproj"

bold() { printf "\n\033[1m==> %s\033[0m\n" "$1"; }
fail() { printf "\n\033[31m✗ %s\033[0m\n" "$1"; exit 1; }

# ---------------------------------------------------------------------------
# Backend — tests de integración (.NET + Testcontainers/Postgres real)
# ---------------------------------------------------------------------------
bold "Backend (.NET integration)"
if [ ! -f "$BE_INTEGRATION_PROJECT" ]; then
  fail "No existe $BE_INTEGRATION_PROJECT."
fi

if ! docker info >/dev/null 2>&1; then
  fail "Docker no está corriendo — los tests de integración lo necesitan (Testcontainers). Levantá Docker Desktop y reintentá."
fi

dotnet test "$BE_INTEGRATION_PROJECT" || fail "Fallaron tests de integración de backend."

printf "\n\033[32m✓ Toda la batería pasó.\033[0m\n"
