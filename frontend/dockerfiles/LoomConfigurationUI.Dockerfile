FROM node:20-alpine AS build

WORKDIR /app

COPY loom-configuration-ui/package*.json ./
RUN npm ci

COPY loom-configuration-ui .

# ARG VITE_API_BASE_URL=/api
# ARG VITE_LAYOUT_API_URL=/api/layout

# ENV VITE_API_BASE_URL=${VITE_API_BASE_URL}
# ENV VITE_LAYOUT_API_URL=${VITE_LAYOUT_API_URL}

RUN npm run build

FROM nginx:alpine

COPY --from=build /app/dist /usr/share/nginx/html

# COPY loom-configuration-ui/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

