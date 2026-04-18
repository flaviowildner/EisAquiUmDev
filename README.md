# EisAquiUmDev

Projeto com frontend em Next.js e backend em ASP.NET Core.

## Estrutura

- `Frontend/`: interface web em Next.js
- `Application/`: API em ASP.NET Core com autenticacao JWT e login com Google

## Requisitos

- Node.js 20+
- npm
- .NET SDK 10

## Portas usadas em desenvolvimento

- Frontend: `http://localhost:5000`
- Backend: `http://localhost:5001`
- Backend HTTPS: `https://localhost:5002`

## Como rodar

### 1. Subir o backend

```powershell
cd Application
dotnet run
```

### 2. Subir o frontend

Em outro terminal:

```powershell
cd Frontend
npm.cmd install
npm.cmd run dev
```

### 3. Abrir no navegador

```text
http://localhost:5000
```

## Como frontend e backend se comunicam

O frontend chama `/api/...`.
Em desenvolvimento, o Next.js faz rewrite dessas rotas para o backend em `http://localhost:5001`.

Se precisar mudar a origem do backend, crie `Frontend/.env.local` com:

```env
NEXT_PUBLIC_BACKEND_ORIGIN=http://localhost:5001
NEXT_PUBLIC_API_URL=/api
NEXT_PUBLIC_GOOGLE_CLIENT_ID=seu_client_id.apps.googleusercontent.com
```

Existe um exemplo em [Frontend/.env.local.example](/C:/Users/flavi/source/repos/EisAquiUmDev/Frontend/.env.local.example).

## Configuracao do backend

Revise [Application/appsettings.json](/C:/Users/flavi/source/repos/EisAquiUmDev/Application/appsettings.json):

- `JwtSettings.SecretKey`
- `JwtSettings.Issuer`
- `JwtSettings.Audience`
- `Authentication.Google.ClientId`
- `Authentication.Google.ClientSecret`

Para desenvolvimento, o ideal e usar User Secrets para as credenciais sensiveis.

## Google Login

Para o login com Google funcionar localmente:

1. Crie ou use um OAuth Client do tipo `Web application` no Google Cloud Console
2. Em `Authorized JavaScript origins`, adicione:
   - `http://localhost:5000`
3. Use o mesmo `client_id` no frontend e no backend

Se aparecer `origin_mismatch`, normalmente a origem cadastrada no Google nao bate com a origem real aberta no navegador.

## Endpoints principais

- `POST /api/auth/register`
- `POST /api/auth/login`
- `POST /api/auth/google-login`
- `GET /api/auth/profile`

## Observacoes

- O banco atual e `InMemory`, entao os dados sao perdidos ao reiniciar a API
- O frontend usa proxy do Next em desenvolvimento
- A raiz do backend nao serve interface web; a interface roda pelo Next

## O que mais vale documentar depois

- como configurar User Secrets no .NET
- como publicar frontend e backend
- como trocar o banco `InMemory` por um banco real
- variaveis de ambiente de producao
- fluxo de autenticacao e expiracao do JWT
- troubleshooting com Google OAuth e portas locais
