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
COPY ["Vention.Processor/Vention.Processor.csproj", "Vention.Processor/"]
COPY ["ApiGateway/ApiGateway.csproj", "ApiGateway/"]
COPY ["Vention.Observability/Vention.Observability.csproj", "Vention.Observability/"]
COPY ["Vention.Presentation.Common/Vention.Presentation.Common.csproj", "Vention.Presentation.Common/"]
COPY ["Vention.GraphQL/Vention.GraphQL.csproj", "Vention.GraphQL/"]
COPY ["Vention.StressTests/Vention.StressTests.csproj", "Vention.StressTests/"]

RUN dotnet restore "Vention.sln"

COPY . .

WORKDIR "/src/Vention.GraphQL"
RUN dotnet publish "Vention.GraphQL.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "Vention.GraphQL.dll"]