# 🚀 Running Sol-Shift with Docker

This guide will walk you through setting up and running the **Sol-Shift** service inside a Docker container.

---

## 🐳 Prerequisites

Before you begin, ensure you have:

- [Docker](https://www.docker.com/get-started) installed

---

## 📦 Build Docker Image

In root directury run

```bash
docker build -t sol-shift .
```

---

## 🚀 Run the Container

In root directury run. For devnet

```bash
docker run -d -p 3001:3001 -e SOLANA_NETWORK=https://api.devnet.solana.com sol-shift
```

or for mainnet

```bash
docker run -d -p 3001:3001 -e SOLANA_NETWORK=https://api.mainnet-beta.solana.com sol-shift
```