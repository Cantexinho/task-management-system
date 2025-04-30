FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TaskManagementSys.Api/TaskManagementSys.Api.csproj", "TaskManagementSys.Api/"]
COPY ["TaskManagementSys.Core/TaskManagementSys.Core.csproj", "TaskManagementSys.Core/"]
COPY ["TaskManagementSys.Infrastructure/TaskManagementSys.Infrastructure.csproj", "TaskManagementSys.Infrastructure/"]
RUN dotnet restore "TaskManagementSys.Api/TaskManagementSys.Api.csproj"
COPY . .
WORKDIR "/src/TaskManagementSys.Api"
RUN dotnet build "TaskManagementSys.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagementSys.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagementSys.Api.dll"]