FROM node:20-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

FROM node:20-alpine AS runtime
WORKDIR /app

COPY --from=build /app/package.json .
COPY --from=build /app/.next/standalone .
COPY --from=build /app/public ./public

EXPOSE 3000

ENV NODE_ENV=production
CMD ["node", "server.js"]
