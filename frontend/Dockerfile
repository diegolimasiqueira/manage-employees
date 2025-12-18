# Build stage
FROM node:18-alpine AS build
WORKDIR /app

# Argumentos de build
ARG VITE_API_URL=http://localhost:5000/api

# Copiar package.json e package-lock.json
COPY package*.json ./

# Instalar dependências
RUN npm ci

# Copiar código fonte
COPY . .

# Definir variável de ambiente para o build
ENV VITE_API_URL=$VITE_API_URL

# Build da aplicação
RUN npm run build

# Production stage com nginx
FROM nginx:alpine AS production
WORKDIR /usr/share/nginx/html

# Remover arquivos padrão do nginx
RUN rm -rf ./*

# Copiar build do React
COPY --from=build /app/dist .

# Copiar configuração customizada do nginx
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expor porta 80
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]

