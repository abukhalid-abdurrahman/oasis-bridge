# Билдим как раньше
FROM node:22-alpine AS builder

WORKDIR /app

COPY package*.json ./
RUN npm install --frozen-lockfile --force

COPY . .

ENV NEXT_PUBLIC_API_URL=NEXT_PUBLIC_API_URL
ENV NEXT_PUBLIC_SOLANA_NET=NEXT_PUBLIC_SOLANA_NET

RUN npm run build

FROM node:22-alpine AS runner

WORKDIR /app

COPY --from=builder /app ./

COPY src/lib/scripts/entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 3000

ENTRYPOINT ["/entrypoint.sh"]
CMD ["npm", "start"]
