export const API = process.env.NEXT_PUBLIC_API_URL;
export const SOLANA_ENVIRONMENT = process.env.NEXT_PUBLIC_SOLANA_ENVIRONMENT;
export const MAX_FILE_SIZE = 100 * 1024 * 1024;
export const MIN_NUMBER = 0.0000000001;
export const ACCEPTED_DOCUMENT_TYPES = [
  "image/jpeg",
  "image/jpg",
  "image/png",
  "image/webp",
];
export const ASSET_TYPES: string[] = [
  "Real Estate",
  // "Automobiles",
  // "Industrial Equipment",
  // "Jewelry and Precious Metals",
  // "Collectibles",
  // "Other",
] as const;
export const PROPERTY_TYPES: string[] = [
  "Residential",
  "Commercial",
  "Industrial",
  "Agricultural",
  "MixedUse",
  "Other",
] as const;
export const INSURANSE_STATUSES: string[] = [
  "Active",
  "Expired",
  "Pending",
  "Cancelled",
] as const;
