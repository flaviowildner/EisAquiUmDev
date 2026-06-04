/** @type {import('next').NextConfig} */
const nextConfig = {
  output: 'standalone',
  reactStrictMode: true,
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: `${process.env.NEXT_PUBLIC_BACKEND_ORIGIN ?? 'http://localhost:5001'}/api/:path*`
      }
    ];
  }
};

export default nextConfig;
