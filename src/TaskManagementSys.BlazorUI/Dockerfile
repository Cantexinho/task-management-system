FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TaskManagementSys.BlazorUI/TaskManagementSys.BlazorUI.csproj", "TaskManagementSys.BlazorUI/"]
COPY ["TaskManagementSys.Core/TaskManagementSys.Core.csproj", "TaskManagementSys.Core/"]
RUN dotnet restore "TaskManagementSys.BlazorUI/TaskManagementSys.BlazorUI.csproj"
COPY . .
WORKDIR "/src/TaskManagementSys.BlazorUI"

RUN dotnet publish "TaskManagementSys.BlazorUI.csproj" -c Release -o /app/publish

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY TaskManagementSys.BlazorUI/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]