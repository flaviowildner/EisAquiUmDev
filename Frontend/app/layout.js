import './globals.css';

export const metadata = {
  title: 'EisAquiUmDev',
  description: 'Frontend Next.js para autenticacao com email, senha e Google.'
};

export default function RootLayout({ children }) {
  return (
    <html lang="pt-BR">
      <body>{children}</body>
    </html>
  );
}
