# Quantum Street Exchange

Quantum Street exchange is an RWA (Real World Asset) platform built on Solana, allowing users to tokenise, fractionalise, buy and sell real world assets. Platform provides tools for asset tokenisation, ownership verification, and secure trading of physical assets. A key feature of the platform is the ability to purchase RWA tokens using zBTC, leveraging Zeus Network’s Bitcoin-to-Solana integration. 


## Project Overview

The platform focuses on two core functions:

1. RWA Marketplace:
   - Tokenization of real-world assets (Real Estate, Automobiles, Collectibles, etc.)
   - Trading platform for tokenized assets with SOL or zBTC payment options
   - **Native zBTC Support**: Purchase assets directly with Bitcoin wrapped on Solana via [Zeus Network](https://zeusnetwork.xyz)
   - Verification and documentation of asset ownership
   - Secure storage of ownership documents on IPFS
   - Tracking of price history and ownership transfers


Key features include:
- Comprehensive RWA tokenization tools with support for various asset types
- Secure ownership verification and documentation
- Trading interface with price history tracking
- **zBTC Integration**: Enabling Bitcoin holders to purchase RWA tokens without converting to SOL
- Identity management (registration, authentication, account recovery)
- Network management for different blockchains
- Wallet linking and management
- IPFS integration for decentralized storage

## zBTC Integration via Zeus Network

Quantum Street Exchange leverages [Zeus Network](https://zeusnetwork.xyz) to provide native Bitcoin support for RWA purchases. This integration allows Bitcoin holders to participate in the RWA marketplace without having to convert their BTC to SOL first.

### Key Benefits:

- **Direct BTC Utilization**: Users can purchase RWA tokens directly using their Bitcoin holdings
- **Cross-Chain Efficiency**: Leverages Zeus Network's multi-chain layer on Solana
- **Reduced Friction**: Eliminates the need for multiple token conversions
- **Broader Market Access**: Opens the RWA marketplace to Bitcoin's extensive user base

### How It Works:

1. Bitcoin holders wrap their BTC into zBTC tokens on Zeus Network
2. These zBTC tokens can be used directly on Quantum Street Exchange
3. The Sol-Shift service generates transactions that accept zBTC as payment
4. RWA token ownership is transferred upon completion of the zBTC payment transaction

## Architecture

The project follows a microservice architecture with:

- **Backend API**: Core service exposing RESTful endpoints
- **Database Migrator**: Service for database schema management
- **Bridge SDK**: Client library for blockchain interactions
- **Frontend**: User interface built with Next.js for interacting with the platform
- **Sol-Shift**: Microservice for Solana transaction generation and processing

### Real-World Asset (RWA) Tokenization

Quantum Street Exchange enables the tokenization of real-world assets as NFTs on the blockchain. The platform supports:

- Tokenizing various types of assets (Real Estate, Automobiles, Collectibles, etc.)
- Defining asset-specific attributes and metadata
- Storing proof of ownership documents on IPFS
- Trading RWA tokens using SOL or zBTC as payment currencies
- Tracking price history and ownership transfers

#### RWA Token Features:

- **Asset Types**: Support for various real-world assets including Real Estate, Automobiles, Industrial Equipment, Jewelry, and Collectibles
- **Ownership Verification**: Documents and proofs stored on IPFS with references on the blockchain
- **Price Management**: Ability for token owners to update prices and track price history
- **Purchase Options**: Tokens can be purchased using SOL or zBTC tokens
- **Royalties**: Support for royalties on secondary sales

#### RWA Token API Endpoints:

- `POST /api/v1/rwa/tokenize`: Create and tokenize a real-world asset
- `GET /api/v1/rwa/{token_id}`: Get details of a specific RWA token
- `GET /api/v1/rwa`: List all RWA tokens with filtering and pagination
- `PUT /api/v1/rwa/{token_id}`: Update an RWA token's details
- `GET /api/v1/rwa/{token_id}/history`: Get transaction and price history for a token

### Microservices

#### Sol-Shift

Sol-Shift is a NestJS microservice that generates and sends Solana transactions for purchasing NFTs and RWA tokens. It supports:

- Generating Solana transactions for NFT/RWA token purchases with payments in SOL or SPL tokens (e.g., zBTC)
- Creating partially signed transactions from the seller side (escrow)
- Broadcasting fully signed transactions to the Solana network

The transaction flow works as follows:
1. Sol-Shift creates a transaction for transferring an RWA token to the buyer
2. The transaction allows payment in either SOL or zBTC tokens from Zeus Network
3. The service returns the transaction in base64 format to the user
4. The user signs the transaction with their wallet
5. The signed transaction is sent back to Sol-Shift for broadcasting to the network

##### API Endpoints:

- `POST /shift/create-transaction`: Generates a transaction with the seller's partial signature
- `POST /shift/send-transaction`: Accepts the fully signed transaction and broadcasts it to the Solana network

More details can be found in the [Sol-Shift documentation](microservices/sol-shift/README.md).

### Frontend

The frontend application is built with:

- **Next.js**: React framework for server-rendered applications
- **Tailwind CSS**: Utility-first CSS framework for styling
- **Web3 Libraries**: Integration with various blockchain wallets and networks
- **Axios**: HTTP client for API requests

The frontend provides a user-friendly interface for interacting with the Quantum Street Exchange backend services, allowing users to manage their accounts, link wallets, perform transactions, and monitor order status.

### Docker Images

The project is containerized using Docker with the following images:

- **quantum-exchange**: Main backend API service that handles API requests and business logic
  - Image: `nazarovqurbonali/quantum-exchange:v23`
  - Exposes REST API endpoints on port 80

- **quantum-db-migrator**: Service responsible for database schema migrations
  - Image: `nazarovqurbonali/quantum-db-migrator:v4`
  - Runs once to initialize and update the database schema
  - Creates necessary tables, indexes, and seed data
  - Automatically exits after completing migrations

- **quantum-exchange-frontend**: Frontend application for user interaction
  - Image: `nazarovqurbonali/quantum-exchange-frontend:v17`
  - Exposes the web interface on port 3000

- **sol-shift**: Solana transaction microservice
  - Image: `nazarovqurbonali/sol-shift:v1`
  - Processes Solana-specific transaction requests
  - Handles zBTC payment transactions
  - Exposes API on port 3001

## Setup and Installation

### Prerequisites

- Docker and Docker Compose
- Git
- Node.js (for local development)

### Clone the Repository

```bash
git clone https://github.com/abukhalid-abdurrahman/oasis-bridge.git
cd oasis-bridge
```

### Environment Configuration

1. Create `.env` files for each service according to the examples in their respective directories.

2. Configure database connection settings in the main `.env` file:

```
POSTGRES_USER=admin
POSTGRES_PASSWORD=yourpassword
POSTGRES_DB=quantum_bridge
```

### Running with Docker Compose

The simplest way to run the entire stack is using Docker Compose:

```bash
docker-compose up -d
```

This command will start all services defined in the `docker-compose.yaml` file.

### Local Development Setup

For development purposes, you can run specific services locally:

1. Backend API:
```bash
cd backend
dotnet restore
dotnet run --project src/api/API/API.csproj
```

2. Frontend:
```bash
cd frontend
npm install
npm run dev
```

3. Sol-Shift:
```bash
cd microservices/sol-shift
npm install
npm run start:dev
```

## API Documentation

The API documentation is available at the following URLs:

- Main Quantum Street Bridge/Exchange API: [http://31.222.229.159:3000/swagger/index.html](http://31.222.229.159:3000/swagger/index.html)
- Sol-Shift API: [http://31.222.229.159:3001/swagger](http://31.222.229.159:3001/swagger)
- IPFS Node: [http://86.105.252.126:5001/webui](http://86.105.252.126:5001/webui)

The main application is accessible at: [http://31.222.229.159](http://31.222.229.159)

### Authentication and Identity Management

#### Register a New User
- **Endpoint**: `POST /api/v1/auth/register`
- **Description**: Registers a new user with the provided registration details
- **Request Body**:
```json
{
  "username": "newuser",
  "email": "newuser@example.com",
  "password": "SecurePassword123!"
}
```
- **Response**:
```json
{
  "success": true,
  "message": "User registered successfully."
}
```

#### User Login
- **Endpoint**: `POST /api/v1/auth/login`
- **Description**: Logs in a user using the provided credentials
- **Request Body**:
```json
{
  "email": "newuser@example.com",
  "password": "SecurePassword123!"
}
```
- **Response**:
```json
{
  "success": true,
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

#### User Logout
- **Endpoint**: `POST /api/v1/auth/logout`
- **Description**: Logs out a currently authenticated user
- **Authorization**: Required
- **Response**:
```json
{
  "success": true,
  "message": "Logged out successfully."
}
```

#### Change Password
- **Endpoint**: `POST /api/v1/auth/change-password`
- **Description**: Changes the password for an authenticated user
- **Authorization**: Required
- **Request Body**:
```json
{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewPassword456!"
}
```
- **Response**:
```json
{
  "success": true,
  "message": "Password changed successfully."
}
```

#### Forgot Password
- **Endpoint**: `POST /api/v1/auth/forgot-password`
- **Description**: Initiates the password reset process
- **Request Body**:
```json
{
  "email": "user@example.com"
}
```
- **Response**:
```json
{
  "success": true,
  "message": "Reset code sent to your email."
}
```

#### Reset Password
- **Endpoint**: `POST /api/v1/auth/reset-password`
- **Description**: Resets the password using a reset token
- **Request Body**:
```json
{
  "email": "user@example.com",
  "code": "123456",
  "newPassword": "NewPassword456!"
}
```
- **Response**:
```json
{
  "success": true,
  "message": "Password reset successfully."
}
```

#### Delete Account
- **Endpoint**: `DELETE /api/v1/auth`
- **Description**: Deletes the authenticated user's account
- **Authorization**: Required
- **Response**:
```json
{
  "success": true,
  "message": "Account deleted successfully."
}
```

### Network Management

#### Get Networks
- **Endpoint**: `GET /api/v1/networks`
- **Description**: Retrieves a list of networks based on filter criteria
- **Query Parameters**: Optional filter parameters
- **Response**:
```json
{
  "items": [
    {
      "id": "123e4567-e89b-12d3-a456-426614174000",
      "name": "Solana Devnet",
      "description": "Solana development network",
      "isActive": true
    },
    {
      "id": "223e4567-e89b-12d3-a456-426614174001",
      "name": "Radix Stokenet",
      "description": "Radix stokenet network",
      "isActive": true
    }
  ],
  "totalCount": 2
}
```

#### Get Network Details
- **Endpoint**: `GET /api/v1/networks/{networkId}`
- **Description**: Retrieves detailed information about a specific network
- **Response**:
```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Solana Devnet",
  "description": "Solana development network",
  "isActive": true,
  "tokens": [
    {
      "id": "323e4567-e89b-12d3-a456-426614174002",
      "symbol": "SOL",
      "name": "Solana",
      "decimals": 9
    }
  ]
}
```

#### Create Network (Admin only)
- **Endpoint**: `POST /api/v1/networks`
- **Description**: Creates a new network
- **Authorization**: Admin role required
- **Request Body**:
```json
{
  "name": "Ethereum Mainnet",
  "description": "Ethereum main network",
  "isActive": true
}
```
- **Response**:
```json
{
  "success": true,
  "id": "423e4567-e89b-12d3-a456-426614174003"
}
```

### RWA Token Management

#### Create and Tokenize RWA
- **Endpoint**: `POST /api/v1/rwa/tokenize`
- **Description**: Creates a new RWA token with the provided details
- **Authorization**: Required
- **Request Body**:
```json
{
  "title": "Real Estate Property XYZ",
  "asset_description": "Description of the RWA asset.",
  "proof_of_ownership_document": "ipfs://<CID>",
  "unique_identifier": "PROPERTY12345",
  "royalty": 5,
  "price": 100000,
  "network": "SOL",
  "image": "ipfs://<CID>",
  "owner_contact": "owner@example.com",
  "asset_type": "Real Estate",
  "geolocation": "latitude,longitude",
  "valuation_date": "2023-01-01",
  "property_type": "Residential",
  "area": 2000,
  "construction_year": 1990
}
```
- **Response**:
```json
{
  "status": "success",
  "message": "RWA token successfully created and tokenized",
  "data": {
    "token_id": "abcd1234",
    "title": "Real Estate Property XYZ",
    "price": 100000,
    "network": "SOL",
    "royalty": 5,
    "owner_contact": "owner@example.com",
    "metadata": "ipfs://<CID>",
    "mint_account": "abcd1234=",
    "transaction_hash": "abcd1234",
    "version": 1
  }
}
```

#### Get RWA Token Details
- **Endpoint**: `GET /api/v1/rwa/{token_id}`
- **Description**: Retrieves detailed information about a specific RWA token
- **Response**:
```json
{
  "status": "success",
  "data": {
    "token_id": "abcd1234",
    "title": "Real Estate Property XYZ",
    "asset_description": "Description of the RWA asset.",
    "proof_of_ownership_document": "ipfs://<CID>",
    "unique_identifier": "PROPERTY12345",
    "royalty": 5,
    "price": 100000,
    "network": "SOL",
    "image": "ipfs://<CID>",
    "owner_contact": "owner@example.com",
    "asset_type": "Real Estate",
    "metadata": "ipfs://<CID>",
    "mint_account": "abcd1234=",
    "transaction_hash": "abcd1234",
    "version": 1
  }
}
```

#### List RWA Tokens
- **Endpoint**: `GET /api/v1/rwa`
- **Description**: Retrieves a list of RWA tokens with filtering and pagination
- **Query Parameters**: 
  - `asset_type`: Filter by asset type
  - `price_min`: Minimum price
  - `price_max`: Maximum price
  - `sort_by`: Sort field
  - `sort_order`: Sort direction (asc/desc)
  - `page`: Page number
  - `page_size`: Results per page
- **Response**:
```json
{
  "status": "success",
  "data": {
    "total": 100,
    "page": 1,
    "page_size": 10,
    "items": [
      {
        "token_id": "abcd1234",
        "title": "Real Estate Property XYZ",
        "price": 100000,
        "asset_type": "Real Estate",
        "image": "ipfs://<CID>",
        "version": 1
      }
    ]
  }
}
```

#### Update RWA Token
- **Endpoint**: `PUT /api/v1/rwa/{token_id}`
- **Description**: Updates an RWA token's details
- **Authorization**: Required (token owner only)
- **Request Body**:
```json
{
  "title": "Updated Real Estate Property XYZ",
  "asset_description": "Updated description of the RWA asset.",
  "price": 120000
}
```
- **Response**:
```json
{
  "status": "success",
  "message": "RWA token successfully updated",
  "data": {
    "token_id": "abcd1234",
    "title": "Updated Real Estate Property XYZ",
    "price": 120000,
    "version": 2
  }
}
```

#### Get RWA Token History
- **Endpoint**: `GET /api/v1/rwa/{token_id}/history`
- **Description**: Retrieves transaction and price history for an RWA token
- **Response**:
```json
{
  "status": "success",
  "data": {
    "sell_buy_history": [
      {
        "transaction_type": "buy",
        "price": 100000,
        "date": "2023-01-01",
        "buyer": "user123"
      }
    ],
    "price_change_history": [
      {
        "old_price": 100000,
        "new_price": 110000,
        "date": "2023-02-01",
        "percentage_change": 10
      }
    ]
  }
}
```

### Orders

#### Create Order
- **Endpoint**: `POST /api/v1/orders`
- **Description**: Creates a new order
- **Authorization**: Required
- **Request Body**:
```json
{
  "tokenId": "323e4567-e89b-12d3-a456-426614174002",
  "amount": 1.5,
  "networkId": "123e4567-e89b-12d3-a456-426614174000"
}
```
- **Response**:
```json
{
  "success": true,
  "orderId": "523e4567-e89b-12d3-a456-426614174004"
}
```

#### Check Order Balance
- **Endpoint**: `GET /api/v1/orders/{orderId}/check-balance`
- **Description**: Checks the balance of an existing order
- **Authorization**: Required
- **Response**:
```json
{
  "orderId": "523e4567-e89b-12d3-a456-426614174004",
  "balance": 1.5,
  "status": "Pending"
}
```

### File Management

#### Upload File
- **Endpoint**: `POST /api/v1/files/upload`
- **Description**: Uploads a file to IPFS storage
- **Request Body**: 
  - multipart/form-data with `file` and `type` fields
- **Response**:
```json
{
  "status": "success",
  "message": "File successfully uploaded.",
  "data": {
    "file_id": "abc123xyz",
    "file_url": "https://ipfs.io/ipfs/abc123xyz"
  }
}
```

#### Get Full-Quality File
- **Endpoint**: `GET /api/v1/files/full/{file_id}`
- **Description**: Downloads a file in full quality
- **Response**: Raw file content

#### Get Optimized NFT Logo
- **Endpoint**: `GET /api/v1/files/nft-logo/{file_id}/optimized`
- **Description**: Gets an optimized version of an NFT logo
- **Response**: Optimized image file

### Wallet Linked Accounts

#### Create Linked Account
- **Endpoint**: `POST /api/v1/linked-accounts`
- **Description**: Creates a new linked wallet account
- **Authorization**: Required
- **Request Body**:
```json
{
  "networkId": "123e4567-e89b-12d3-a456-426614174000",
  "publicKey": "9XyZKG9RVL5vZqTfSveCyZMUeexM1mcvGvwYz8NG4Jch"
}
```
- **Response**:
```json
{
  "success": true,
  "id": "623e4567-e89b-12d3-a456-426614174005"
}
```

#### Get User's Linked Accounts
- **Endpoint**: `GET /api/v1/linked-accounts/me`
- **Description**: Retrieves the current user's linked wallet accounts
- **Authorization**: Required
- **Response**:
```json
{
  "items": [
    {
      "id": "623e4567-e89b-12d3-a456-426614174005",
      "networkId": "123e4567-e89b-12d3-a456-426614174000",
      "networkName": "Solana Devnet",
      "publicKey": "9XyZKG9RVL5vZqTfSveCyZMUeexM1mcvGvwYz8NG4Jch"
    }
  ],
  "totalCount": 1
}
```

## Bridge Operations

2. Cross-chain bridge between Solana and Radix networks:
   - Seamless transfers of assets between SOL and XRD
   - Secure transaction validation and processing
   - Support for native tokens and wrapped assets

Quantum Street Bridge provides a unified interface for blockchain operations through its Bridge SDK:

- Account creation and restoration
- Balance checking
- Deposits and withdrawals
- Transaction status queries

The Bridge interface abstracts the complexities of different blockchain networks, providing a standardized way to interact with chains like Solana and Radix.

## Deployment

The project can be deployed using Docker. Sample docker-compose.yaml is included in the repository:

```yaml
services:
  postgres:
    image: postgres:latest
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: admin
      POSTGRES_DB: postgres
    ports:
      - "5432:5432"

  db-migrator:
    image: nazarovqurbonali/quantum-db-migrator:v4
    depends_on:
      - postgres
    environment:
      - ConnectionStrings__DefaultConnection=...

  quantum-exchange:
    image: nazarovqurbonali/quantum-exchange:v23
    ports:
      - "3000:80"
    environment:
      - ConnectionStrings__DefaultConnection=...
    depends_on:
      - postgres
      - db-migrator

  quantum-exchange-frontend:
    image: nazarovqurbonali/quantum-exchange-frontend:v17
    ports:
      - "80:3000"
    environment:
      - NEXT_PUBLIC_API_URL=http://...
    depends_on:
      - quantum-exchange
      
  sol-shift:
    image: nazarovqurbonali/sol-shift:v1
    container_name: sol-shift
    restart: always
    environment:
      - SOLANA_NETWORK=https://api.devnet.solana.com
    ports:
      - "3001:3001"
```

For more detailed deployment instructions, please refer to the docker deployment guides in the `backend/deployments/` directory.
