import { useEffect, useState } from "react";
import { PublicKey } from "@solana/web3.js";
import { useWalletStore } from "@/store/useWalletStore";
import { mutateWallet } from "@/requests/postRequests";
import { useUserStore } from "@/store/useUserStore";

declare global {
  interface Window {
    solana?: any;
  }
}

export const usePhantomWallet = () => {
  const [isPhantomInstalled, setIsPhantomInstalled] = useState(false);
  const [walletDenied, setWalletDenied] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");
  const { setPublicKey: setKey } = useWalletStore();
  const { user } = useUserStore();
  const submit = mutateWallet();

  useEffect(() => {
    if (typeof window !== "undefined" && window.solana?.isPhantom) {
      setIsPhantomInstalled(true);
    }
  }, []);

  const connectPhantomWallet = (pubKey: string | null) => {
    setErrorMessage("");
    setWalletDenied(false);

    if (!window.solana) {
      setErrorMessage("Phantom is not installed.");
      setIsPhantomInstalled(false);
      return;
    }

    window.solana
      .connect()
      .then((resp: any) => {
        const pubKey = new PublicKey(resp.publicKey.toString());

        return submit.mutateAsync({
          walletAddress: pubKey,
          network: "Solana",
        }).then(() => {
          setKey(pubKey.toBase58());
          setWalletDenied(false);
        }).catch((error: any) => {
          if (error?.response?.data?.error?.errorType === "AlreadyExist") {
            setKey(pubKey.toBase58());
            setWalletDenied(false);
          } else {
            setWalletDenied(true);
            setErrorMessage("Failed to connect wallet.");
          }
        });
      })
      .catch((err: any) => {
        if (
          err?.code === 4001 ||
          err?.message?.includes("User rejected the request")
        ) {
          setErrorMessage("It seems you have declined the request. Please try again.");
          setWalletDenied(true);
        } else {
          setErrorMessage("Unexpected wallet error. Please try again later.");
          console.error("Unexpected wallet error:", err);
        }
      });
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
    connectPhantomWallet,
    disconnectWallet,
    walletDenied,
    setWalletDenied,
    errorMessage,
  };
};
