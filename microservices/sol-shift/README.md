# ShiftService – Solana Transaction Generator

`ShiftService` is a NestJS service for generating a **Solana transaction** to purchase an NFT. It supports payments in **SOL** or **SPL tokens**, and returns a partially signed Base64 transaction ready for the buyer to sign and send.

---

## 🧩 Functionality Overview

- Transfers **payment** (SOL or SPL token) from buyer to seller
- Transfers **NFT** from seller (escrow wallet) to buyer
- Uses seller's secret key to **partially sign** the transaction
- Returns a Base64-encoded transaction string

---

## 🛠 Transaction Logic

1. Get latest blockhash
2. Set buyer as `feePayer`
3. If `tokenMint` is provided:
   - Use `createTransferInstruction` to transfer **SPL token**
   - Amount should be in **base units** (e.g., USDC = 1e6)
4. Else:
   - Transfer **SOL**, converting to lamports:
     ```
     lamports = price * LAMPORTS_PER_SOL
     ```
     `LAMPORTS_PER_SOL = 1_000_000_000`
5. Transfer **NFT** from escrow to buyer
6. Sign transaction with `escrow` key (sellerSecretkey)
7. Return serialized transaction as Base64

---

## 📥 Input – `CreateTransactionDto`

| Field            | Type     | Required | Description |
|------------------|----------|----------|-------------|
| `buyerPubkey`     | string   | ✅        | Buyer's wallet address |
| `sellerPubkey`    | string   | ✅        | Seller's wallet address |
| `sellerSecretkey` | string   | ✅        | Base58-encoded secret key of seller (escrow) |
| `nftMint`         | string   | ✅        | Mint address of NFT being transferred |
| `price`           | number   | ✅        | Price in SOL (if no token) or smallest token unit (if SPL token) |
| `tokenMint`       | string\|null | ✅   | Mint address of SPL token, or null for SOL |

---

## 📤 Output – `TransactionResponseDto`

Success:
```json
{
  "status": "success",
  "message": "Transaction created successfully.",
  "code": 200,
  "data": {
    "base64": "<Base64-encoded transaction>"
  }
}
