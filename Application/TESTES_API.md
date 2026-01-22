# ?? Guia de Testes da API

## API Rest Client - Testar com VS Code ou Postman

### Arquivo de teste: `.http` (VS Code Rest Client Extension)

**Instale a extensão:**
- REST Client (humao.rest-client)

---

## ?? Exemplos de Requisições

### 1. **Registrar Novo Usuário**

```http
POST https://localhost:7126/api/auth/register
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "senha123456",
  "name": "João Silva"
}
```

**Resposta (201):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "joao@example.com",
    "name": "João Silva"
  }
}
```

---

### 2. **Login com Email/Senha**

```http
POST https://localhost:7126/api/auth/login
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "senha123456"
}
```

**Resposta (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "email": "joao@example.com",
    "name": "João Silva"
  }
}
```

---

### 3. **Login com Google** (obter token primeiro via Google Sign-In JS)

```http
POST https://localhost:7126/api/auth/google-login
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjEifQ..."
}
```

**Resposta (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 2,
    "email": "usuario@gmail.com",
    "name": "Usuario Google"
  }
}
```

---

### 4. **Obter Perfil do Usuário (Autenticado)**

```http
GET https://localhost:7126/api/auth/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

**Resposta (200):**
```json
{
  "id": 1,
  "email": "joao@example.com",
  "name": "João Silva",
  "createdAt": "2025-01-21T10:30:00"
}
```

---

## ?? Teste com cURL (Terminal/PowerShell)

### Registrar

```bash
curl -X POST https://localhost:7126/api/auth/register `
  -H "Content-Type: application/json" `
  -d "{\"email\":\"joao@example.com\",\"password\":\"senha123456\",\"name\":\"João\"}" `
  -k
```

### Login

```bash
curl -X POST https://localhost:7126/api/auth/login `
  -H "Content-Type: application/json" `
  -d "{\"email\":\"joao@example.com\",\"password\":\"senha123456\"}" `
  -k
```

### Perfil (substitua o token)

```bash
curl -X GET https://localhost:7126/api/auth/profile `
  -H "Authorization: Bearer SEU_TOKEN_AQUI" `
  -k
```

---

## ?? Teste com Postman

1. **Abra o Postman**
2. **Create Collection** ? "EisAquiUmDev"
3. **Create Request** ? "Register"
   - Method: `POST`
   - URL: `https://localhost:7126/api/auth/register`
   - Body (raw, JSON):
   ```json
   {
     "email": "joao@example.com",
     "password": "senha123456",
     "name": "João Silva"
   }
   ```
4. Salve o token da resposta em uma variável
5. Use em outras requisições

---

## ?? Decodificar JWT Token

Acesse [JWT.io](https://jwt.io) e cole seu token para ver o conteúdo:

**Header:**
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

**Payload:**
```json
{
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "1",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "joao@example.com",
  "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": "João Silva",
  "exp": 1642776600,
  "iss": "sua_api",
  "aud": "seus_clientes"
}
```

---

## ?? Status Esperados

| Código | Descrição |
|--------|-----------|
| 200 | OK - Sucesso |
| 400 | Bad Request - Dados inválidos |
| 401 | Unauthorized - Credenciais inválidas |
| 404 | Not Found - Recurso não encontrado |
| 500 | Internal Server Error - Erro no servidor |

---

## ? Checklist de Testes

- [ ] Registrar novo usuário
- [ ] Login com email/senha
- [ ] Token retornado é válido
- [ ] Acessar /profile com token
- [ ] Acessar /profile sem token (deve retornar 401)
- [ ] Tentar login com senha errada (deve retornar 401)
- [ ] Registrar usuário duplicado (deve retornar 400)
- [ ] Frontend carrega sem erros
- [ ] Botão Google Sign-In funciona
- [ ] Logout limpa localStorage

---

**Bom teste! ??**
