FROM node:20-alpine AS build
WORKDIR /app

COPY frontend/package.json frontend/package-lock.json ./
RUN npm ci

COPY frontend/ ./

ARG VITE_HUB_URL=/racehub
ENV VITE_HUB_URL=${VITE_HUB_URL}

RUN npm run build

FROM nginx:1.27-alpine AS final

ARG BACKEND_ORIGIN

COPY frontend/nginx.conf.template /tmp/nginx.conf.template
COPY --from=build /app/dist /usr/share/nginx/html

RUN sed "s|__BACKEND_ORIGIN__|${BACKEND_ORIGIN}|g" /tmp/nginx.conf.template > /etc/nginx/conf.d/default.conf \
    && rm /tmp/nginx.conf.template

EXPOSE 8080
