#!/bin/bash
set -e

echo "Aguardando Postgres estar pronto..."
until pg_isready -h postgres -U postgres; do
  sleep 1
done

echo "Postgres está pronto. Iniciando aplicação..."
exec dotnet Application.dll
