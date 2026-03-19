FROM node:20-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./

ARG VITE_HUB_URL
ENV VITE_HUB_URL=${VITE_HUB_URL}

RUN npm run build

FROM nginxinc/nginx-unprivileged:1.27-alpine AS final

COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 8080
