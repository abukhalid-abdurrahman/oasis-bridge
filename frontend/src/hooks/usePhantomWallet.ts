import { useEffect, useState } from "react";
import { PublicKey } from "@solana/web3.js";
import { useWalletStore } from "@/store/useWalletStore";

declare global {
  interface Window {
    solana?: any;
  }
}

export const usePhantomWallet = () => {
  const [isPhantomInstalled, setIsPhantomInstalled] = useState(false);
  const [publicKey, setPublicKey] = useState<PublicKey | null>(null);
  const { setPublicKey: setKey } = useWalletStore()

  useEffect(() => {
    if (typeof window !== "undefined" && window.solana?.isPhantom) {
      setIsPhantomInstalled(true);
    }
  }, []);

  const connectWallet = async () => {
    if (!window.solana) {
      alert("Phantom is not installed");
      return;
    }

    try {
      const resp = await window.solana.connect();
      const pubKey = new PublicKey(resp.publicKey.toString());
      setPublicKey(pubKey);

      setKey(pubKey.toBase58());

    } catch (err) {
      console.error("Wallet connection error:", err);
    }
  };

  const disconnectWallet = async () => {
    try {
      await window.solana.disconnect();
      setPublicKey(null);
      setKey(null);
    } catch (err) {
      console.error("Wallet disconnection error:", err);
    }
  };

  return {
    isPhantomInstalled,
    publicKey,
    connectWallet,
    disconnectWallet,
  };
};
