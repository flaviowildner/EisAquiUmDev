'use client';

import Script from 'next/script';
import { useEffect, useState } from 'react';

const API_URL = process.env.NEXT_PUBLIC_API_URL ?? '/api';
const GOOGLE_CLIENT_ID =
  process.env.NEXT_PUBLIC_GOOGLE_CLIENT_ID ??
  '51993202531-q0541gq40uaib12fimt5srqmn9fr48q1.apps.googleusercontent.com';

const initialLoginForm = {
  email: '',
  password: ''
};

const initialRegisterForm = {
  name: '',
  email: '',
  password: ''
};

function GoogleIcon() {
  return (
    <svg className="googleIcon" viewBox="0 0 24 24" aria-hidden="true">
      <path
        fill="#4285F4"
        d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
      />
      <path
        fill="#34A853"
        d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
      />
      <path
        fill="#FBBC05"
        d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
      />
      <path
        fill="#EA4335"
        d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
      />
    </svg>
  );
}

function Alert({ alert }) {
  if (!alert) {
    return null;
  }

  return <div className={`alert alert-${alert.type}`}>{alert.message}</div>;
}

export default function HomePage() {
  const [activeTab, setActiveTab] = useState('login');
  const [loginForm, setLoginForm] = useState(initialLoginForm);
  const [registerForm, setRegisterForm] = useState(initialRegisterForm);
  const [user, setUser] = useState(null);
  const [googleReady, setGoogleReady] = useState(false);
  const [busyAction, setBusyAction] = useState('');
  const [loginAlert, setLoginAlert] = useState(null);
  const [registerAlert, setRegisterAlert] = useState(null);

  useEffect(() => {
    const token = window.localStorage.getItem('token');
    const storedUser = window.localStorage.getItem('user');

    if (!token || !storedUser) {
      return;
    }

    try {
      setUser(JSON.parse(storedUser));
    } catch (error) {
      console.error('Erro ao carregar usuario salvo:', error);
      window.localStorage.removeItem('token');
      window.localStorage.removeItem('user');
    }
  }, []);

  useEffect(() => {
    if (!googleReady || !window.google?.accounts?.id) {
      return;
    }

    window.google.accounts.id.initialize({
      client_id: GOOGLE_CLIENT_ID,
      callback: handleGoogleCallback
    });
  }, [googleReady]);

  function showAlert(scope, message, type) {
    const setter = scope === 'register' ? setRegisterAlert : setLoginAlert;
    setter({ message, type });

    window.setTimeout(() => setter(null), 5000);
  }

  async function parseApiResponse(response) {
    const text = await response.text();

    if (!text) {
      return null;
    }

    try {
      return JSON.parse(text);
    } catch {
      return text;
    }
  }

  function persistSession(data) {
    window.localStorage.setItem('token', data.token);
    window.localStorage.setItem('user', JSON.stringify(data.user));
    setUser(data.user);
  }

  function clearSession() {
    if (window.google?.accounts?.id) {
      window.google.accounts.id.cancel();
      window.google.accounts.id.disableAutoSelect();
    }

    window.localStorage.removeItem('token');
    window.localStorage.removeItem('user');
    setUser(null);
    setLoginForm(initialLoginForm);
    setRegisterForm(initialRegisterForm);
    setActiveTab('login');
  }

  async function handleGoogleCallback(response) {
    if (!response?.credential) {
      showAlert('login', 'Google nao retornou credenciais. Tente novamente.', 'error');
      return;
    }

    setBusyAction('google');

    try {
      const apiResponse = await fetch(`${API_URL}/auth/google-login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify({ idToken: response.credential })
      });

      const data = await parseApiResponse(apiResponse);

      if (!apiResponse.ok) {
        throw new Error(data?.message || data || 'Erro ao autenticar com Google.');
      }

      persistSession(data);
      showAlert('login', 'Autenticacao com Google concluida com sucesso.', 'success');
    } catch (error) {
      console.error(error);
      showAlert('login', error.message || 'Erro na comunicacao com o servidor.', 'error');
    } finally {
      setBusyAction('');
    }
  }

  async function signInWithGoogle() {
    if (!googleReady || !window.google?.accounts?.id) {
      showAlert('login', 'Google Sign-In ainda nao inicializou. Tente novamente em instantes.', 'error');
      return;
    }

    try {
      window.google.accounts.id.cancel();
      window.google.accounts.id.prompt((notification) => {
        if (notification.isNotDisplayed()) {
          console.warn('Google prompt nao exibido:', notification.getNotDisplayedReason());
        }

        if (notification.isSkippedMoment()) {
          console.warn('Google prompt ignorado:', notification.getSkippedReason());
        }

        if (notification.isDismissedMoment()) {
          console.warn('Google prompt dispensado:', notification.getDismissedReason());
        }
      });
    } catch (error) {
      console.error(error);
      showAlert('login', 'Erro ao abrir o prompt do Google.', 'error');
    }
  }

  async function handleLogin(event) {
    event.preventDefault();
    setBusyAction('login');

    try {
      const response = await fetch(`${API_URL}/auth/login`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(loginForm)
      });

      const data = await parseApiResponse(response);

      if (!response.ok) {
        throw new Error(data?.message || data || 'Erro ao fazer login.');
      }

      persistSession(data);
      showAlert('login', 'Login realizado com sucesso.', 'success');
    } catch (error) {
      console.error(error);
      showAlert('login', error.message || 'Erro na comunicacao com o servidor.', 'error');
    } finally {
      setBusyAction('');
    }
  }

  async function handleRegister(event) {
    event.preventDefault();

    if (registerForm.password.length < 6) {
      showAlert('register', 'A senha precisa ter no minimo 6 caracteres.', 'error');
      return;
    }

    setBusyAction('register');

    try {
      const response = await fetch(`${API_URL}/auth/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json'
        },
        body: JSON.stringify(registerForm)
      });

      const data = await parseApiResponse(response);

      if (!response.ok) {
        throw new Error(data?.message || data || 'Erro ao criar conta.');
      }

      persistSession(data);
      showAlert('register', 'Conta criada com sucesso.', 'success');
    } catch (error) {
      console.error(error);
      showAlert('register', error.message || 'Erro na comunicacao com o servidor.', 'error');
    } finally {
      setBusyAction('');
    }
  }

  return (
    <>
      <Script
        src="https://accounts.google.com/gsi/client"
        strategy="afterInteractive"
        onLoad={() => setGoogleReady(true)}
      />

      <main className="pageShell">
        <section className="heroPanel">
          <p className="eyebrow">Autenticacao</p>
          <h1>EisAquiUmDev</h1>
          <p className="heroText">
            Refeito em Next.js para deixar a interface mais organizada, mantendo o backend .NET e
            o login com Google.
          </p>
          <div className="heroCard">
            <span className="heroChip">JWT 24h</span>
            <span className="heroChip">Google OAuth</span>
            <span className="heroChip">ASP.NET Core API</span>
          </div>
        </section>

        <section className="authCard">
          {user ? (
            <div className="profilePanel">
              <p className="profileEyebrow">Sessao ativa</p>
              <h2>Bem-vindo de volta</h2>
              <div className="profileInfo">
                <div>
                  <span className="profileLabel">Nome</span>
                  <strong>{user.name}</strong>
                </div>
                <div>
                  <span className="profileLabel">Email</span>
                  <strong>{user.email}</strong>
                </div>
                <div>
                  <span className="profileLabel">ID</span>
                  <strong>{user.id}</strong>
                </div>
              </div>
              <button className="primaryButton dangerButton" type="button" onClick={clearSession}>
                Sair
              </button>
            </div>
          ) : (
            <>
              <div className="tabRow">
                <button
                  className={activeTab === 'login' ? 'tabButton active' : 'tabButton'}
                  type="button"
                  onClick={() => setActiveTab('login')}
                >
                  Login
                </button>
                <button
                  className={activeTab === 'register' ? 'tabButton active' : 'tabButton'}
                  type="button"
                  onClick={() => setActiveTab('register')}
                >
                  Registrar
                </button>
              </div>

              {activeTab === 'login' ? (
                <form className="authForm" onSubmit={handleLogin}>
                  <div className="formHeading">
                    <h2>Entre na sua conta</h2>
                    <p>Use email e senha ou siga direto com Google.</p>
                  </div>

                  <Alert alert={loginAlert} />

                  <label className="field">
                    <span>Email</span>
                    <input
                      type="email"
                      placeholder="seu@email.com"
                      value={loginForm.email}
                      onChange={(event) =>
                        setLoginForm((current) => ({ ...current, email: event.target.value }))
                      }
                      required
                    />
                  </label>

                  <label className="field">
                    <span>Senha</span>
                    <input
                      type="password"
                      placeholder="Digite sua senha"
                      value={loginForm.password}
                      onChange={(event) =>
                        setLoginForm((current) => ({ ...current, password: event.target.value }))
                      }
                      required
                    />
                  </label>

                  <button
                    className="primaryButton"
                    type="submit"
                    disabled={busyAction === 'login'}
                  >
                    {busyAction === 'login' ? 'Entrando...' : 'Entrar'}
                  </button>

                  <button
                    className="googleButton"
                    type="button"
                    onClick={signInWithGoogle}
                    disabled={busyAction === 'google'}
                  >
                    <GoogleIcon />
                    {busyAction === 'google' ? 'Conectando...' : 'Continuar com Google'}
                  </button>
                </form>
              ) : (
                <form className="authForm" onSubmit={handleRegister}>
                  <div className="formHeading">
                    <h2>Crie sua conta</h2>
                    <p>Cadastre seu usuario ou use o Google para acelerar.</p>
                  </div>

                  <Alert alert={registerAlert} />

                  <label className="field">
                    <span>Nome</span>
                    <input
                      type="text"
                      placeholder="Seu nome"
                      value={registerForm.name}
                      onChange={(event) =>
                        setRegisterForm((current) => ({ ...current, name: event.target.value }))
                      }
                      required
                    />
                  </label>

                  <label className="field">
                    <span>Email</span>
                    <input
                      type="email"
                      placeholder="seu@email.com"
                      value={registerForm.email}
                      onChange={(event) =>
                        setRegisterForm((current) => ({ ...current, email: event.target.value }))
                      }
                      required
                    />
                  </label>

                  <label className="field">
                    <span>Senha</span>
                    <input
                      type="password"
                      placeholder="No minimo 6 caracteres"
                      value={registerForm.password}
                      onChange={(event) =>
                        setRegisterForm((current) => ({
                          ...current,
                          password: event.target.value
                        }))
                      }
                      required
                    />
                  </label>

                  <button
                    className="primaryButton"
                    type="submit"
                    disabled={busyAction === 'register'}
                  >
                    {busyAction === 'register' ? 'Criando conta...' : 'Criar conta'}
                  </button>

                  <button
                    className="googleButton"
                    type="button"
                    onClick={signInWithGoogle}
                    disabled={busyAction === 'google'}
                  >
                    <GoogleIcon />
                    {busyAction === 'google' ? 'Conectando...' : 'Cadastrar com Google'}
                  </button>
                </form>
              )}
            </>
          )}
        </section>
      </main>
    </>
  );
}
