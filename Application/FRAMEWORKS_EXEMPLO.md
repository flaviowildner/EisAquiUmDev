// ?? Exemplos de Integração com Frameworks Populares

## ?? REACT - useAuth Hook

```jsx
import { useState, useContext, createContext } from 'react';

const AuthContext = createContext();

export function AuthProvider({ children }) {
  const [token, setToken] = useState(localStorage.getItem('token'));
  const [user, setUser] = useState(JSON.parse(localStorage.getItem('user') || 'null'));
  const [loading, setLoading] = useState(false);

  const API_URL = 'https://localhost:7126/api';

  const login = async (email, password) => {
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });
      const data = await res.json();
      
      if (res.ok) {
        setToken(data.token);
        setUser(data.user);
        localStorage.setItem('token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        return { success: true };
      }
      return { success: false, error: data };
    } catch (error) {
      return { success: false, error: error.message };
    } finally {
      setLoading(false);
    }
  };

  const loginWithGoogle = async (idToken) => {
    setLoading(true);
    try {
      const res = await fetch(`${API_URL}/auth/google-login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ idToken })
      });
      const data = await res.json();
      
      if (res.ok) {
        setToken(data.token);
        setUser(data.user);
        localStorage.setItem('token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        return { success: true };
      }
      return { success: false, error: data };
    } catch (error) {
      return { success: false, error: error.message };
    } finally {
      setLoading(false);
    }
  };

  const logout = () => {
    setToken(null);
    setUser(null);
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  return (
    <AuthContext.Provider value={{ token, user, login, loginWithGoogle, logout, loading }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
```

### Usar em Componente:

```jsx
import { useAuth } from './AuthContext';

export function LoginPage() {
  const { login, loginWithGoogle, loading } = useAuth();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const handleLogin = async (e) => {
    e.preventDefault();
    const result = await login(email, password);
    if (result.success) {
      // Redirecionar para dashboard
    }
  };

  return (
    <form onSubmit={handleLogin}>
      <input 
        type="email" 
        value={email} 
        onChange={(e) => setEmail(e.target.value)} 
      />
      <input 
        type="password" 
        value={password} 
        onChange={(e) => setPassword(e.target.value)} 
      />
      <button type="submit" disabled={loading}>
        {loading ? 'Entrando...' : 'Entrar'}
      </button>
      <button onClick={() => loginWithGoogle(googleToken)}>
        Google
      </button>
    </form>
  );
}
```

---

## ?? AXIOS INTERCEPTOR - Adicionar Token Automaticamente

```javascript
import axios from 'axios';

const api = axios.create({
  baseURL: 'https://localhost:7126/api',
  timeout: 10000
});

// Adicionar token a todas as requisições
api.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('token');
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Tratar erros de autenticação
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export default api;
```

### Usar:

```javascript
import api from './api';

// Login
const { data } = await api.post('/auth/login', { email, password });
localStorage.setItem('token', data.token);

// Protegido
const profile = await api.get('/auth/profile');
```

---

## ?? VUE 3 - Composable

```javascript
// composables/useAuth.js
import { ref } from 'vue';

export function useAuth() {
  const token = ref(localStorage.getItem('token'));
  const user = ref(JSON.parse(localStorage.getItem('user') || 'null'));
  const loading = ref(false);

  const API_URL = 'https://localhost:7126/api';

  const login = async (email, password) => {
    loading.value = true;
    try {
      const res = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
      });
      
      if (res.ok) {
        const data = await res.json();
        token.value = data.token;
        user.value = data.user;
        localStorage.setItem('token', data.token);
        localStorage.setItem('user', JSON.stringify(data.user));
        return true;
      }
      return false;
    } finally {
      loading.value = false;
    }
  };

  const logout = () => {
    token.value = null;
    user.value = null;
    localStorage.removeItem('token');
    localStorage.removeItem('user');
  };

  return { token, user, loading, login, logout };
}
```

### Usar em Componente:

```vue
<script setup>
import { useAuth } from '@/composables/useAuth';

const { login, loading } = useAuth();

const email = ref('');
const password = ref('');

const handleLogin = async () => {
  await login(email.value, password.value);
};
</script>

<template>
  <form @submit.prevent="handleLogin">
    <input v-model="email" type="email" />
    <input v-model="password" type="password" />
    <button :disabled="loading">
      {{ loading ? 'Entrando...' : 'Entrar' }}
    </button>
  </form>
</template>
```

---

## ?? ANGULAR - Guard + Service

```typescript
// auth.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private API_URL = 'https://localhost:7126/api';
  private currentUserSubject = new BehaviorSubject(JSON.parse(localStorage.getItem('user') || 'null'));
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient) {}

  login(email: string, password: string) {
    return this.http.post(`${this.API_URL}/auth/login`, { email, password }).pipe(
      tap(response: any) => {
        localStorage.setItem('token', response.token);
        localStorage.setItem('user', JSON.stringify(response.user));
        this.currentUserSubject.next(response.user);
      })
    );
  }

  logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('user');
    this.currentUserSubject.next(null);
  }

  getToken() {
    return localStorage.getItem('token');
  }
}

// auth.guard.ts
import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';

@Injectable({ providedIn: 'root' })
export class AuthGuard {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  canActivate() {
    if (this.authService.getToken()) {
      return true;
    }
    this.router.navigate(['/login']);
    return false;
  }
}

// app.routes.ts
import { Routes } from '@angular/router';
import { AuthGuard } from './auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'profile', component: ProfileComponent, canActivate: [AuthGuard] },
];
```

---

## ?? NEXTJS 14 - Auth Middleware

```typescript
// lib/auth.ts
import { jwtVerify } from 'jose';

const secret = new TextEncoder().encode(process.env.JWT_SECRET!);

export async function verifyAuth(token: string) {
  try {
    const verified = await jwtVerify(token, secret);
    return verified.payload;
  } catch (err) {
    return null;
  }
}

// middleware.ts
import { NextRequest, NextResponse } from 'next/server';
import { verifyAuth } from '@/lib/auth';

export async function middleware(request: NextRequest) {
  const token = request.cookies.get('token')?.value;

  if (request.nextUrl.pathname.startsWith('/protected')) {
    if (!token || !(await verifyAuth(token))) {
      return NextResponse.redirect(new URL('/login', request.url));
    }
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/protected/:path*'],
};
```

### Usar em Page:

```typescript
// app/protected/page.tsx
'use client';

import { useRouter } from 'next/navigation';
import { useEffect, useState } from 'react';

export default function Dashboard() {
  const router = useRouter();
  const [user, setUser] = useState(null);

  useEffect(() => {
    const user = JSON.parse(localStorage.getItem('user') || 'null');
    if (!user) router.push('/login');
    setUser(user);
  }, []);

  return <div>Bem-vindo, {user?.name}</div>;
}
```

---

## ? SVELTE - Store

```typescript
// stores/auth.ts
import { writable } from 'svelte/store';

interface User {
  id: number;
  email: string;
  name: string;
}

export const token = writable(localStorage.getItem('token') || '');
export const user = writable<User | null>(
  JSON.parse(localStorage.getItem('user') || 'null')
);

export async function login(email: string, password: string) {
  const res = await fetch('https://localhost:7126/api/auth/login', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ email, password })
  });

  if (res.ok) {
    const data = await res.json();
    token.set(data.token);
    user.set(data.user);
    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user));
  }
}

export function logout() {
  token.set('');
  user.set(null);
  localStorage.removeItem('token');
  localStorage.removeItem('user');
}
```

### Usar em Componente:

```svelte
<script>
  import { token, user, login, logout } from '../stores/auth';

  let email = '';
  let password = '';

  async function handleLogin() {
    await login(email, password);
  }
</script>

{#if $token}
  <div>
    Bem-vindo, {$user.name}
    <button on:click={logout}>Sair</button>
  </div>
{:else}
  <form on:submit|preventDefault={handleLogin}>
    <input type="email" bind:value={email} />
    <input type="password" bind:value={password} />
    <button>Entrar</button>
  </form>
{/if}
```

---

**Escolha seu framework favorito e integre com a API! ??**
