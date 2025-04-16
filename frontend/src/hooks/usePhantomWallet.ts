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
  const [walletDenied, setWalletDenied] = useState(false);
  const { setPublicKey: setKey } = useWalletStore();

  useEffect(() => {
    if (typeof window !== "undefined" && window.solana?.isPhantom) {
      setIsPhantomInstalled(true);
    }
  }, []);

  const connectWallet = async () => {
    setWalletDenied(false);

    if (!window.solana) {
      alert("Phantom is not installed");
      return;
    }

    try {
      const resp = await window.solana.connect();
      const pubKey = new PublicKey(resp.publicKey.toString());
    
      setKey(pubKey.toBase58());
      setWalletDenied(false);
    } catch (err: any) {
      if (err?.code === 4001 || err?.message?.includes("User rejected the request")) {
        setWalletDenied(true);
        // Не логируем ошибку — она ожидаемая
      } else {
        console.error("Unexpected wallet error:", err);
      }
    }
  };

  const disconnectWallet = async () => {
    try {
      await window.solana.disconnect();
      setKey(null);
    } catch (err) {
      console.error("Wallet disconnection error:", err);
    }
  };

  return {
    isPhantomInstalled,
    connectWallet,
    disconnectWallet,
    walletDenied,
    setWalletDenied
  };
};
