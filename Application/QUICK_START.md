# ?? QUICK START - 5 MINUTOS

## Passo 1: Obter seu Client ID do Google (se não tem)

1. Acesse: https://console.cloud.google.com/
2. Crie um projeto novo
3. Vá para "APIs & Services" ? "Credentials"
4. Clique "Create Credentials" ? "OAuth 2.0 Client ID"
5. Escolha "Web application"
6. Adicione URIs autorizadas:
   ```
   http://localhost:5000
   http://localhost:5001
   https://localhost:7126
   https://localhost:5126
   ```
7. Clique "Create" e copie seu `Client ID`

---

## Passo 2: Configurar o Frontend (30 segundos)

**Abra:** `Application/wwwroot/index.html`

**Procure (linha ~110):**
```javascript
client_id: 'SEU_CLIENT_ID.apps.googleusercontent.com', // ?? COLOQUE AQUI
```

**Substitua** `SEU_CLIENT_ID` pelo seu Client ID real:
```javascript
client_id: '123456789-abcdefg.apps.googleusercontent.com',
```

**Salve o arquivo!**

---

## Passo 3: Configurar Credenciais (1 minuto)

**Opção A: Teste Rápido (appsettings.json)**

Abra: `Application/appsettings.json`

```json
{
  "Authentication": {
    "Google": {
      "ClientId": "seu_client_id.apps.googleusercontent.com",
      "ClientSecret": "seu_client_secret"
    }
  }
}
```

**?? NÃO faça commit com o secret!**

---

**Opção B: Seguro (user-secrets - Recomendado)**

```bash
cd Application

# Inicializar
dotnet user-secrets init

# Configurar
dotnet user-secrets set "Authentication:Google:ClientId" "seu_client_id.apps.googleusercontent.com"
dotnet user-secrets set "Authentication:Google:ClientSecret" "seu_client_secret"

# Verificar
dotnet user-secrets list
```

---

## Passo 4: Rodar a API (2 minutos)

```bash
cd Application
dotnet run
```

**Você verá:**
```
Building...
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7126
      Now listening on: http://localhost:5126
```

---

## Passo 5: Abrir no Navegador (1 minuto)

```
https://localhost:7126/index.html
```

---

## ? Pronto! Teste as funcionalidades:

### 1. Registrar
- Clique em "Registrar"
- Insira nome, email e senha
- Clique em "Criar Conta"
- ? Deve aparecer sua informação no perfil

### 2. Login
- Clique em "Login"
- Insira email e senha
- Clique em "Entrar"
- ? Deve aparecer sua informação no perfil

### 3. Google Sign-In
- Clique no botão "Google"
- Escolha sua conta Google
- Autorize o acesso
- ? Deve fazer login automático

### 4. Logout
- Clique em "Sair"
- ? Deve voltar para a tela de login

---

## ?? Testar API com cURL

```bash
# Registrar
curl -X POST https://localhost:7126/api/auth/register `
  -H "Content-Type: application/json" `
  -d "{\"email\":\"test@example.com\",\"password\":\"senha123\",\"name\":\"Test\"}" `
  -k

# Login
curl -X POST https://localhost:7126/api/auth/login `
  -H "Content-Type: application/json" `
  -d "{\"email\":\"test@example.com\",\"password\":\"senha123\"}" `
  -k

# Copie o token da resposta e use em:

# Perfil (substitua o token)
curl -X GET https://localhost:7126/api/auth/profile `
  -H "Authorization: Bearer SEU_TOKEN_AQUI" `
  -k
```

---

## ?? Problemas Comuns

### ? "CORS error"
**Solução:** Certifique-se que está acessando via `https://localhost:7126/index.html`

### ? "Token do Google inválido"
**Solução:** Verifique se o Client ID em `index.html` está correto

### ? "Banco vazio após reiniciar"
**Solução:** É normal! Banco em memória é perdido ao parar a API

### ? "Unauthorized 401"
**Solução:** Verifique se o token está sendo enviado no header:
```
Authorization: Bearer <seu_token>
```

---

## ?? Arquivos Importantes

```
Application/
??? wwwroot/index.html           ? Frontend (adicione Client ID aqui)
??? Program.cs                    ? Configuração
??? appsettings.json              ? Credenciais (OU use user-secrets)
??? Controllers/AuthController.cs ? Endpoints da API
??? Services/AuthService.cs       ? Lógica de autenticação
??? Models/User.cs                ? Modelo de usuário
```

---

## ?? Segurança: NÃO ESQUEÇA

- ? Use `dotnet user-secrets` para guardar ClientSecret
- ? NÃO faça commit de `appsettings.json` com secrets
- ? Mude a `SecretKey` em produção
- ? Use HTTPS obrigatório

---

## ?? Precisa de Ajuda?

Consulte os arquivos de documentação:
- `COMECE_AQUI.md` - Guia completo
- `TESTES_API.md` - Como testar
- `PRODUCAO_DEPLOY.md` - Deploy

---

**É isso! Seu backend está pronto para usar! ??**
