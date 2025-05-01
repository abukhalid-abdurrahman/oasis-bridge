import { useEffect, useState } from "react";
import { PublicKey } from "@solana/web3.js";
import { useWalletStore } from "@/store/useWalletStore";
import { UseMutateFunction } from "@tanstack/react-query";
import { PostWallet } from "@/lib/types";
import { mutateWallet } from "@/requests/postRequests";

declare global {
  interface Window {
    solana?: any;
  }
}

export const usePhantomWallet = () => {
  const [isPhantomInstalled, setIsPhantomInstalled] = useState(false);
  const [walletDenied, setWalletDenied] = useState(false);
  const { setPublicKey: setKey } = useWalletStore();
  const submit = mutateWallet()

  useEffect(() => {
    if (typeof window !== "undefined" && window.solana?.isPhantom) {
      setIsPhantomInstalled(true);
    }
  }, []);

  const connectWallet = async (pubKey: string | null) => {
    setWalletDenied(false);

    if (!window.solana) {
      alert("Phantom is not installed");
      return;
    }

    try {
      const resp = await window.solana.connect();
      const pubKey = new PublicKey(resp.publicKey.toString());
    
      submit.mutate({
        walletAddress: pubKey,
        network: 'Solana'
      }, {
        onSuccess: () => {
          setKey(pubKey.toBase58());
          setWalletDenied(false);
        },
        onError: () => {
          setWalletDenied(true);
        }
      })
    } catch (err: any) {
      if (err?.code === 4001 || err?.message?.includes("User rejected the request")) {
        setWalletDenied(true);
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
