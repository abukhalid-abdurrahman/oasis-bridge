# Quantum Street Bridge

Quantum Street Bridge is a blockchain interoperability solution that enables seamless asset transfers and interactions between different blockchain networks. The platform provides a robust API and microservices architecture to facilitate identity management, network integration, and secure cross-chain transactions.

## Project Overview

Quantum Street Bridge is designed to connect various blockchain networks including Solana and Radix, allowing users to manage accounts, transfer assets, and perform cross-chain operations. The platform consists of a backend API service, database migrations, and client SDK for blockchain interactions.

Key features include:
- Identity management (registration, authentication, account recovery)
- Network management for different blockchains
- Wallet linking and management
- Order creation and balance checking
- Token operations and transfers
- IPFS integration for decentralized storage

## Architecture

The project follows a microservice architecture with:

- **Backend API**: Core service exposing RESTful endpoints
- **Database Migrator**: Service for database schema management
- **Bridge SDK**: Client library for blockchain interactions
- **Frontend**: User interface built with Next.js for interacting with the platform
- **Sol-Shift**: Microservice for Solana transaction generation and processing

### Microservices

#### Sol-Shift

Sol-Shift is a NestJS microservice that generates and sends Solana transactions for purchasing NFTs. It supports:

- Generating Solana transactions for NFT purchases with payments in SOL or SPL tokens
- Creating partially signed transactions from the seller side (escrow)
- Broadcasting fully signed transactions to the Solana network

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

The frontend provides a user-friendly interface for interacting with the Quantum Street Bridge backend services, allowing users to manage their accounts, link wallets, perform transactions, and monitor order status.

### Docker Images

The project is containerized using Docker with the following images:

- **oasis-bridge**: Main backend API service that handles API requests and business logic
  - Image: `nazarovqurbonali/oasis-bridge:v23`
  - Exposes REST API endpoints on port 80

- **oasis-db-migrator**: Service responsible for database schema migrations
  - Image: `nazarovqurbonali/oasis-db-migrator:v4`
  - Runs once to initialize and update the database schema
  - Creates necessary tables, indexes, and seed data
  - Automatically exits after completing migrations

- **oasis-bridge-frontend**: Frontend application for user interaction
  - Image: `nazarovqurbonali/oasis-bridge-frontend:v17`
  - Exposes the web interface on port 3000

- **sol-shift**: Solana transaction microservice
  - Image: `nazarovqurbonali/sol-shift:v1`
  - Processes Solana-specific transaction requests
  - Exposes API on port 3001

## API Documentation

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
    image: nazarovqurbonali/oasis-db-migrator:v4
    depends_on:
      - postgres
    environment:
      - ConnectionStrings__DefaultConnection=...

  oasisbridge:
    image: nazarovqurbonali/oasis-bridge:v23
    ports:
      - "3000:80"
    environment:
      - ConnectionStrings__DefaultConnection=...
    depends_on:
      - postgres
      - db-migrator

  oasisbridge-frontend:
    image: nazarovqurbonali/oasis-bridge-frontend:v17
    ports:
      - "80:3000"
    environment:
      - NEXT_PUBLIC_API_URL=http://...
    depends_on:
      - oasisbridge
      
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
