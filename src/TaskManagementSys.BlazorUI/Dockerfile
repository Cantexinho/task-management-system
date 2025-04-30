FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TaskManagementSys.BlazorUI/TaskManagementSys.BlazorUI.csproj", "TaskManagementSys.BlazorUI/"]
COPY ["TaskManagementSys.Core/TaskManagementSys.Core.csproj", "TaskManagementSys.Core/"]
RUN dotnet restore "TaskManagementSys.BlazorUI/TaskManagementSys.BlazorUI.csproj"
COPY . .
WORKDIR "/src/TaskManagementSys.BlazorUI"
RUN dotnet build "TaskManagementSys.BlazorUI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TaskManagementSys.BlazorUI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskManagementSys.BlazorUI.dll"]