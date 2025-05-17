# Sol-Shift – Solana Transaction Generator

`Sol-Shift` is a NestJS microservice that generates and sends Solana transactions for purchasing NFTs. It supports payments in **SOL** or **SPL tokens**, generates a transaction partially signed by the seller (escrow), and then accepts the fully signed transaction from the buyer to broadcast it to the Solana network.

---

## 🧩 Functionality Overview

- Generates a Solana transaction to purchase an NFT:
  - Transfers **payment** (SOL or SPL token) from buyer to seller
  - Transfers **NFT** from seller (escrow) to buyer
- Signs transaction with seller’s private key
- Returns a **Base64-encoded** partially signed transaction
- Accepts the **fully signed** transaction from the buyer and broadcasts it to the network

---

## ⚙️ API Endpoints

### 📌 `POST /shift/create-transaction`

Generates a transaction for NFT purchase with the seller’s partial signature.

#### 📥 Input Example – `CreateTransactionDto`

```json
{
  "buyerPubkey": "BUYER_PUBLIC_KEY",
  "sellerPubkey": "SELLER_PUBLIC_KEY",
  "sellerSecretkey": "BASE58_SECRET_KEY",
  "nftMint": "NFT_MINT_ADDRESS",
  "price": 1.5,
  "tokenMint": null
}
```

| Field             | Type           | Required | Description                                 |
|------------------|----------------|----------|---------------------------------------------|
| `buyerPubkey`     | string         | ✅        | Buyer's wallet address                      |
| `sellerPubkey`    | string         | ✅        | Seller's wallet address                     |
| `sellerSecretkey` | string         | ✅        | Base58-encoded secret key of seller (escrow)|
| `nftMint`         | string         | ✅        | Mint address of NFT being transferred       |
| `price`           | number         | ✅        | Price in SOL (or smallest unit for tokens)  |
| `tokenMint`       | string \| null | ❌        | SPL token mint address, or null for SOL     |

#### 📤 Output Example – `TransactionResponseDto`

```json
{
  "status": "success",
  "message": "Transaction created successfully.",
  "code": 200,
  "data": {
    "transaction": "<Base64-encoded transaction>"
  }
}
```

---

### 📌 `POST /shift/send-transaction`

Accepts the **signed** transaction from the buyer and sends it to the Solana network.

#### 📥 Input Example – `SendSignedTransactionDto`

```json
{
  "signedTransaction": "<Base64-signed-transaction>"
}
```

| Field               | Type   | Required | Description                                  |
|--------------------|--------|----------|----------------------------------------------|
| `signedTransaction` | string | ✅        | Fully signed transaction in Base64 format    |

#### 📤 Output Example – `TransactionSentResponseDto`

```json
{
  "status": "success",
  "message": "Transaction sent successfully.",
  "code": 200,
  "data": {
    "transactionId": "<Signature>"
  }
}
```

---

## 🔁 Full Transaction Flow

### 1. Get a partially signed transaction from backend

```ts
const res = await fetch('/shift/create-transaction', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    buyerPubkey,
    sellerPubkey,
    sellerSecretkey,
    nftMint,
    price,
    tokenMint
  })
});
const { data } = await res.json();
```

### 2. Sign it with Phantom

```ts
const tx = Transaction.from(Buffer.from(data.transaction, "base64"));
const signedTx = await window.solana.signTransaction(tx);
```

### 3. Send it back to the backend for broadcasting

```ts
await fetch('/shift/send-transaction', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    signedTransaction: Buffer.from(signedTx.serialize()).toString("base64")
  })
});
```

---

## 🛠 Transaction Logic

1. Fetch latest blockhash
2. Set buyer as `feePayer`
3. If `tokenMint` is provided:
   - Use `createTransferInstruction` to transfer **SPL token**
   - Amount should be in **smallest token unit** (e.g. USDC = 1e6)
4. Else (SOL payment):
   - Amount in regular figures (1, 2.43 ...):
5. Transfer **NFT** from escrow to buyer
6. Partially sign transaction with seller’s private key
7. Return the transaction as a Base64 string

---

## 📦 Tech Stack

- **NestJS** – Backend framework
- **@solana/web3.js** – Solana SDK
- **@solana/spl-token** – SPL token utilities
- **Base58 & Base64** – For encoding keys and transactions

