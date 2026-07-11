# ============================================
# BUILD STAGE
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Install Node.js 20 for TailwindCSS
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /src

# Copy solution and project files for layer caching
COPY *.sln .
COPY Domain/Domain.csproj Domain/
COPY Application/Application.csproj Application/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY API/API.csproj API/
COPY Front/Front.csproj Front/

RUN dotnet restore

# Copy everything else
COPY . .

# Install npm dependencies and build TailwindCSS for Front
WORKDIR /src/Front
RUN npm install
RUN npx tailwindcss -i ./wwwroot/css/input.css -o ./wwwroot/css/app.css

# Build and publish both projects
WORKDIR /src
RUN dotnet publish API/API.csproj -c Release -o /app/api --no-restore
RUN dotnet publish Front/Front.csproj -c Release -o /app/front --no-restore

# ============================================
# RUNTIME STAGE
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

COPY --from=build /app/api ./api
COPY --from=build /app/front ./front
COPY --from=build /src/entrypoint.sh .

RUN chmod +x entrypoint.sh

EXPOSE 8080
EXPOSE 8081

ENTRYPOINT ["./entrypoint.sh"]
