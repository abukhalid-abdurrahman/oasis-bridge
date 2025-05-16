export const API = process.env.NEXT_PUBLIC_API_URL;
export const SOLANA_NET = process.env.NEXT_PUBLIC_SOLANA_NET;
export const MAX_FILE_SIZE = 50000000;
export const ACCEPTED_DOCUMENT_TYPES = [
  "image/jpeg",
  "image/jpg",
  "image/png",
  "image/webp",
];
export const ASSET_TYPES = [
  "Real Estate",
  "Automobiles",
  // "Industrial Equipment",
  // "Jewelry and Precious Metals",
  // "Collectibles",
  // "Other",
] as const;

export const PROPERTY_TYPES = ["Residential", "Commercial", "Land"];
export const INSURANSE_STATUSES = ['Active', 'Expired', 'Pending', 'Cancelled']
