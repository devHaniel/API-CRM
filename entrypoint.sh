#!/bin/bash
set -e

echo "Starting API on port 8080..."
ASPNETCORE_URLS=http://+:8080 ASPNETCORE_ENVIRONMENT=Production dotnet /app/api/API.dll &
PID_API=$!

echo "Starting Front on port 8081..."
ASPNETCORE_URLS=http://+:8081 ASPNETCORE_ENVIRONMENT=Production dotnet /app/front/Front.dll &
PID_FRONT=$!

echo "Both services started. API on :8080, Front on :8081"

shutdown() {
    echo "Shutting down..."
    kill $PID_API $PID_FRONT 2>/dev/null
    wait
}

trap shutdown SIGTERM SIGINT

wait
