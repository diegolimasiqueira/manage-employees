# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar arquivos de projeto
COPY ["src/ManageEmployees.API/ManageEmployees.API.csproj", "ManageEmployees.API/"]
COPY ["src/ManageEmployees.Application/ManageEmployees.Application.csproj", "ManageEmployees.Application/"]
COPY ["src/ManageEmployees.Domain/ManageEmployees.Domain.csproj", "ManageEmployees.Domain/"]
COPY ["src/ManageEmployees.Infrastructure/ManageEmployees.Infrastructure.csproj", "ManageEmployees.Infrastructure/"]

# Restaurar dependências
RUN dotnet restore "ManageEmployees.API/ManageEmployees.API.csproj"

# Copiar todo o código
COPY src/ .

# Build da aplicação
WORKDIR "/src/ManageEmployees.API"
RUN dotnet build "ManageEmployees.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ManageEmployees.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 5000

# Copiar aplicação publicada
COPY --from=publish /app/publish .

# Criar diretório para uploads
RUN mkdir -p /app/wwwroot/uploads/photos

# Variáveis de ambiente padrão
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "ManageEmployees.API.dll"]

