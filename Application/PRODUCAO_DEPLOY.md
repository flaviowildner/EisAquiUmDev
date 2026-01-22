# ?? Guia de Deploy em Produção

## ?? Configurações Recomendadas

### 1. **Variáveis de Ambiente**

Em vez de usar `appsettings.json` para secrets, use variáveis de ambiente:

```bash
# Windows
set JwtSettings__SecretKey=sua_chave_super_secreta_com_32_caracteres_minimo
set Authentication__Google__ClientId=seu_client_id.apps.googleusercontent.com
set Authentication__Google__ClientSecret=seu_client_secret

# Linux/Mac
export JwtSettings__SecretKey=sua_chave_super_secreta_com_32_caracteres_minimo
export Authentication__Google__ClientId=seu_client_id.apps.googleusercontent.com
export Authentication__Google__ClientSecret=seu_client_secret
```

### 2. **Azure Key Vault (Recomendado)**

```csharp
// Program.cs - Adicionar
builder.Configuration.AddAzureKeyVault(
    new Uri("https://seu-vault.vault.azure.net/"),
    new DefaultAzureCredential()
);
```

### 3. **Docker Deployment**

**Dockerfile:**

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
COPY . .
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/out .
EXPOSE 80 443
ENV ASPNETCORE_URLS=http://+:80;https://+:443
ENTRYPOINT ["dotnet", "Application.dll"]
```

**docker-compose.yml:**

```yaml
version: '3.8'
services:
  api:
    build: .
    ports:
      - "7126:443"
      - "5126:80"
    environment:
      JwtSettings__SecretKey: ${JWT_SECRET_KEY}
      Authentication__Google__ClientId: ${GOOGLE_CLIENT_ID}
      Authentication__Google__ClientSecret: ${GOOGLE_CLIENT_SECRET}
    volumes:
      - ./certs:/app/certs:ro
```

### 4. **HTTPS Certificate**

```bash
# Gerar self-signed certificate
dotnet dev-certs https --export-path ./certs/aspnetapp.pfx --password senha123
```

---

## ?? Melhorias de Segurança para Produção

### 1. **Implementar Refresh Tokens**

Adicione ao `User.cs`:

```csharp
public string? RefreshToken { get; set; }
public DateTime? RefreshTokenExpiry { get; set; }
```

### 2. **Validação de Email**

```csharp
// AuthController.cs - antes de registrar
var code = await _emailService.SendConfirmationEmailAsync(user.Email);
user.EmailConfirmed = false;
user.EmailConfirmationCode = code;
```

### 3. **Rate Limiting**

```bash
dotnet add package AspNetCoreRateLimit
```

```csharp
// Program.cs
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*/api/auth/login",
            Period = "1m",
            Limit = 5
        }
    };
});
```

### 4. **CORS Restrito**

```csharp
// Program.cs
builder.Services.AddCors(options =>
{
    options.AddPolicy("Production", policy =>
    {
        policy.WithOrigins("https://seudominio.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});
```

### 5. **HTTPS Redirect Obrigatório**

```csharp
// Program.cs
app.UseHttpsRedirection();
app.UseHsts(); // HTTP Strict Transport Security
```

### 6. **Logging Centralizado**

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

```csharp
// Program.cs
builder.Host.UseSerilog((context, config) =>
    config
        .MinimumLevel.Information()
        .WriteTo.Console()
        .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
);
```

---

## ?? Banco de Dados Recomendado

### Trocar de InMemory para SQL Server

**1. Instalar Package:**

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

**2. Configurar Connection String:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=seu-server.database.windows.net;Initial Catalog=eisaquiumdev;Persist Security Info=False;User ID=usuario;Password=senha;Encrypt=True;Connection Timeout=30;"
  }
}
```

**3. Atualizar Program.cs:**

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
);
```

**4. Criar Migrations:**

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

---

## ?? Checklist de Deploy

### Segurança
- [ ] JWT Secret é forte (mín. 32 caracteres)
- [ ] Google ClientSecret não está no código
- [ ] HTTPS obrigatório
- [ ] CORS restrito a domínios conhecidos
- [ ] Rate limiting implementado
- [ ] Validação de email implementada
- [ ] Logs centralizados

### Performance
- [ ] Caching implementado
- [ ] CDN configurado
- [ ] Banco de dados otimizado
- [ ] Índices criados nas tabelas

### Monitoramento
- [ ] Application Insights configurado
- [ ] Alertas configurados
- [ ] Backup automático do banco
- [ ] Rollback strategy definida

### Compliance
- [ ] LGPD/GDPR implementado
- [ ] Política de privacidade
- [ ] Termos de serviço
- [ ] Consentimento de cookies

---

## ?? Deploy no Azure App Service

### 1. **Criar App Service**

```bash
az appservice plan create --name EisAquiUmDev-Plan --resource-group seu-rg --sku B1 --is-linux
az webapp create --name EisAquiUmDev-API --resource-group seu-rg --plan EisAquiUmDev-Plan --runtime "DOTNET:10"
```

### 2. **Configurar Variáveis de Ambiente**

```bash
az webapp config appsettings set --name EisAquiUmDev-API --resource-group seu-rg --settings JwtSettings__SecretKey=sua_chave
```

### 3. **Deploy**

```bash
dotnet publish -c Release -o ./publish
cd publish
az webapp up --name EisAquiUmDev-API --resource-group seu-rg
```

---

## ?? Contato e Suporte

Para mais informações sobre deploy, acesse:
- [Microsoft Learn - Deploy ASP.NET Core](https://learn.microsoft.com/aspnet/core/host-and-deploy)
- [Azure App Service Docs](https://learn.microsoft.com/azure/app-service)

---

**Pronto para produção! ??**
