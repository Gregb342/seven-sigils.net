# ── Build stage ──────────────────────────────────────────────────────────────
FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
RUN npm ci

COPY . .
# Back-office « citadel » : visible par défaut (dev), désactivé par le workflow
# de déploiement (--build-arg VITE_CITADEL_ENABLED=false).
ARG VITE_CITADEL_ENABLED=true
ENV VITE_CITADEL_ENABLED=$VITE_CITADEL_ENABLED
RUN npm run build

# ── Runtime stage (nginx) ─────────────────────────────────────────────────────
FROM nginx:1.27-alpine AS runtime

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
