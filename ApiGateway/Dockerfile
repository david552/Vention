FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Vention.sln", "./"]
COPY ["Vention.Domain/Vention.Domain.csproj", "Vention.Domain/"]
COPY ["Vention.Application/Vention.Application.csproj", "Vention.Application/"]
COPY ["Vention.API/Vention.API.csproj", "Vention.API/"]
COPY ["Vention.Infrastructure/Vention.Infrastructure.csproj", "Vention.Infrastructure/"]
COPY ["Vention.Application.Tests/Vention.Application.Tests.csproj", "Vention.Application.Tests/"]
COPY ["Vention.Worker/Vention.Worker.csproj", "Vention.Worker/"]
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]

RUN dotnet restore "Vention.sln"

COPY . .

WORKDIR "/src/ApiGateway"

RUN dotnet publish "ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "ApiGateway.dll"]