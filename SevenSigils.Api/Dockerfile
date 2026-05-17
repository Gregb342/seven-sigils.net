# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files for layer-cached restore
COPY SevenSigils.Domain/SevenSigils.Domain.csproj           SevenSigils.Domain/
COPY SevenSigils.Application/SevenSigils.Application.csproj SevenSigils.Application/
COPY SevenSigils.Infrastructure/SevenSigils.Infrastructure.csproj SevenSigils.Infrastructure/
COPY SevenSigils.Api/SevenSigils.Api.csproj                 SevenSigils.Api/

RUN dotnet restore SevenSigils.Api/SevenSigils.Api.csproj

# Copy full source and publish
COPY SevenSigils.Domain/       SevenSigils.Domain/
COPY SevenSigils.Application/  SevenSigils.Application/
COPY SevenSigils.Infrastructure/ SevenSigils.Infrastructure/
COPY SevenSigils.Api/          SevenSigils.Api/

RUN dotnet publish SevenSigils.Api/SevenSigils.Api.csproj \
    -c Release -o /app/publish --no-restore

# ── Runtime stage ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SevenSigils.Api.dll"]
