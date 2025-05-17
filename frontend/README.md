# 🚀 Running with Docker

This guide will walk you through setting up and running the Next.js-app inside a Docker container.

---

## 🐳 Prerequisites

Before you begin, ensure you have:

- [Docker](https://www.docker.com/get-started) installed

---

## 📦 Build Docker Image

In root directury run

```bash
docker build -t app-name .
```

---

## 🚀 Run the Container

In root directury run. For devnet

```bash
docker run -d -p 3000:3000 -e NEXT_PUBLIC_API_URL=http://31.222.229.159:3000/api/v1 -e NEXT_PUBLIC_SOLANA_NET=https://api.devnet.solana.com app-name
```

or for mainnet

```bash
docker run -d -p 3000:3000 -e NEXT_PUBLIC_API_URL=http://31.222.229.159:3000/api/v1 -e NEXT_PUBLIC_SOLANA_NET=https://api.mainnet-beta.solana.com app-name
```