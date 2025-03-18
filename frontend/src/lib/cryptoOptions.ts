export interface CryptoOption {
  id: string;
  name: string;
  description: string;
  tokens: string[];
}

export interface SelectedCrypto {
  network: string;
  token: string;
}

export const defaultSelectedFrom: SelectedCrypto = {
  network: "Solana",
  token: "SOL",
};

export const defaultSelectedTo: SelectedCrypto = {
  network: "Radix",
  token: "XRD",
};

export const defaultSelectedNetwork = {
  name: "Solana",
  description: "Solana network",
};
