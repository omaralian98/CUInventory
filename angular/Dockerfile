# Multi-stage build for Angular Frontend
# Generated from Aspire AppHost configuration

FROM node:20-alpine AS build
WORKDIR /app

# Copy package files
COPY package*.json ./
COPY yarn.lock ./

# Install dependencies
RUN yarn install --frozen-lockfile

# Copy source code
COPY . .

# Build the application
RUN yarn build:prod

# Runtime stage
FROM nginx:alpine
WORKDIR /usr/share/nginx/html

# Copy nginx configuration
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built application from build stage
COPY --from=build /app/dist/CUInventory/browser .

# Copy dynamic environment configuration
COPY dynamic-env.json .

# Expose port
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]
