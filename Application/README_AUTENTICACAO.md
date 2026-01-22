# ?? Autenticação com Google OAuth 2.0 + JWT - Guia Completo

## ? Implementação Concluída

Seu backend está 100% pronto com:
- ? Autenticação JWT
- ? Login com Email/Senha
- ? Login com Google OAuth 2.0
- ? Banco de dados em memória
- ? Frontend HTML/JavaScript pronto
- ? CORS configurado

---

## ?? Próximos Passos

### 1. **Configurar seu Client ID do Google**

No arquivo `Application/wwwroot/index.html`, procure pela linha:

```javascript
client_id: 'SEU_CLIENT_ID.apps.googleusercontent.com', // ?? COLOQUE SEU CLIENT ID AQUI
```

Substitua `SEU_CLIENT_ID` pelo seu Client ID real do Google Cloud Console.

**Exemplo:**
```javascript
client_id: '123456789-abcdef.apps.googleusercontent.com',
```

### 2. **Configurar appsettings.json com suas credenciais**

```json
{
  "JwtSettings": {
    "SecretKey": "sua_chave_super_secreta_com_minimo_32_caracteres_aqui_mudar_em_producao",
    "Issuer": "sua_api",
    "Audience": "seus_clientes"
  },
  "Authentication": {
    "Google": {
      "ClientId": "seu_client_id_aqui.apps.googleusercontent.com",
      "ClientSecret": "seu_client_secret_aqui_MANTER_SECRETO"
    }
  }
}
```

### 3. **Usar User Secrets (RECOMENDADO PARA DESENVOLVIMENTO)**

```bash
dotnet user-secrets init

# Guardar a chave secreta com segurança
dotnet user-secrets set "Authentication:Google:ClientId" "seu_client_id.apps.googleusercontent.com"
dotnet user-secrets set "Authentication:Google:ClientSecret" "seu_client_secret"
```

### 4. **Rodar a API**

```bash
cd Application
dotnet run
```

A API estará disponível em:
- **HTTPS:** `https://localhost:7126`
- **HTTP:** `http://localhost:5126`

### 5. **Acessar o Frontend**

Abra no navegador:
```
https://localhost:7126/index.html
```

---

## ?? Endpoints da API

### ?? Públicos (sem autenticação)

| Método | URL | Descrição |
|--------|-----|-----------|
| POST | `/api/auth/register` | Criar conta com email/senha |
| POST | `/api/auth/login` | Login com email/senha |
| POST | `/api/auth/google-login` | Login com Google OAuth |

**Exemplos de uso:**

```bash
# Registrar
curl -X POST https://localhost:7126/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"senha123","name":"João"}'

# Login
curl -X POST https://localhost:7126/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"senha123"}'

# Google Login
curl -X POST https://localhost:7126/api/auth/google-login \
  -H "Content-Type: application/json" \
  -d '{"idToken":"SEU_ID_TOKEN_DO_GOOGLE"}'
```

### ?? Protegidos (requer token JWT)

| Método | URL | Descrição |
|--------|-----|-----------|
| GET | `/api/auth/profile` | Obter perfil do usuário autenticado |

**Uso:**
```bash
curl -X GET https://localhost:7126/api/auth/profile \
  -H "Authorization: Bearer SEU_JWT_TOKEN"
```

---

## ?? Respostas da API

### ? Sucesso no Login/Registro

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "user@example.com",
    "name": "João"
  }
}
```

### ? Erro

```json
{
  "message": "Descrição do erro"
}
```

---

## ?? Testar no Frontend

### Opção 1: Login Tradicional
1. Clique na aba "Login"
2. Insira seu email e senha
3. Clique em "Entrar"

### Opção 2: Registrar Nova Conta
1. Clique na aba "Registrar"
2. Insira nome, email e senha
3. Clique em "Criar Conta"

### Opção 3: Login com Google
1. Clique no botão "Google"
2. Escolha sua conta Google
3. Autorize o acesso

---

## ?? Segurança - Checklist

### ? Implementado
- ? JWT com assinatura HMAC-SHA256
- ? Tokens com expiração (24 horas)
- ? Verificação de token Google com biblioteca oficial
- ? CORS restrito aos localhosts
- ? Senhas com hash BCrypt (salto 10)

### ?? Para Produção
- [ ] Usar HTTPS obrigatório
- [ ] Mover secrets para Azure Key Vault
- [ ] Implementar refresh tokens
- [ ] Configurar CORS com domínios reais
- [ ] Usar banco de dados real (SQL Server/PostgreSQL)
- [ ] Adicionar rate limiting
- [ ] Implementar 2FA
- [ ] Usar cookies HttpOnly para JWT

---

## ??? Estrutura de Arquivos

```
Application/
??? Models/
?   ??? User.cs                 # Modelo de usuário
??? Data/
?   ??? AppDbContext.cs         # Contexto do banco
??? Services/
?   ??? AuthService.cs          # Serviço de autenticação
??? Controllers/
?   ??? AuthController.cs       # Endpoints da API
??? wwwroot/
?   ??? index.html              # Frontend (HTML/JS)
??? Program.cs                  # Configuração da API
??? appsettings.json            # Configurações
```

---

## ?? Troubleshooting

### ? "Token do Google inválido"
- Verifique se o `ClientId` está correto em `index.html` e `appsettings.json`
- Certifique-se que o token ainda não expirou
- Verifique se o domínio está autorizado no Google Cloud Console

### ? "CORS error"
- Verifique se a API está rodando em `https://localhost:7126` ou `http://localhost:5126`
- Confirme que o frontend está sendo servido da API (não por outro servidor)

### ? "Autenticação falhou"
- Limpe o localStorage do navegador: `localStorage.clear()`
- Faça uma nova autenticação

### ? "Banco de dados vazio após reiniciar"
- Isso é normal! O banco em memória é perdido ao reiniciar
- Para produção, use um banco de dados real

---

## ?? Recursos Úteis

- [Google Sign-In Docs](https://developers.google.com/identity/sign-in/web)
- [JWT.io](https://jwt.io)
- [ASP.NET Core Auth Docs](https://learn.microsoft.com/aspnet/core/security)
- [Microsoft Identity Web](https://github.com/AzureAD/microsoft-identity-web)

---

## ?? Próximas Melhorias

- [ ] Implementar Refresh Tokens
- [ ] Adicionar validação de email
- [ ] Implementar reset de senha
- [ ] Adicionar autenticação com Microsoft/GitHub
- [ ] Adicionar 2FA com QR Code
- [ ] Dashboard de usuário
- [ ] Migrar para banco de dados real

---

**Criado com ?? para EisAquiUmDev**
