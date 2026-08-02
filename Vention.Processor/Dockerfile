FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

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

RUN dotnet restore "Vention.Processor/Vention.Processor.csproj"

COPY . .

WORKDIR "/src/Vention.Processor"
RUN dotnet publish "Vention.Processor.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Vention.Processor.dll"]