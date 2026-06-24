FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080 


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src  

COPY ["Vention.Domain/Vention.Domain.csproj", "Vention.Domain/"]
COPY ["Vention.Application/Vention.Application.csproj", "Vention.Application/"]
COPY ["Vention.API/Vention.API.csproj", "Vention.API/"]

RUN dotnet restore "Vention.API/Vention.API.csproj"

COPY . .

WORKDIR "/src/Vention.API"

RUN dotnet publish "Vention.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
ENTRYPOINT ["dotnet", "Vention.API.dll"]