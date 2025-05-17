import { PublicKey } from "@solana/web3.js";
import { ZodType } from "zod";

export type searchParams = {
  signin?: string;
  signup?: string;
};

export interface User {
  token: string;
  expiresAt: string;
  startTime: string;
  Id: string;
  UserName: string;
  Email: string;
}

export interface TokenizationField {
  name: string;
  placeholder: string;
  type: string;
  validation: ZodType;
  group?: number;
  defaultValue?: string;
  HTMLType?: string;
  selectItems?: string[];
}

export type PostWallet = {
  walletAddress: PublicKey;
  network: string;
};

export type RwasReq = {
  assetType?: string | null;
  priceMin?: number | null;
  priceMax?: number | null;
  sortBy?: string | null;
  sortOrder?: string | null;
  pageSize: number;
  pageNumber: number;
};
