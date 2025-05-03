"use client";
import { usePhantomWallet } from "@/hooks/usePhantomWallet";
import { Button } from "./ui/button";
import { useState } from "react";
import { useWalletStore } from "@/store/useWalletStore";
import Image from "next/image";
import { shortAddress } from "@/lib/scripts/script";
import { useUserStore } from "@/store/useUserStore";
import WalletSelector from "./WalletSelector";

export default function WalletConnect({ className }: { className?: string }) {
  const [showWalletSelector, setShowWalletSelector] = useState(false);
  const {
    disconnectWallet,
  } = usePhantomWallet();
  const { publicKey } = useWalletStore();
  const { user } = useUserStore();

  if (user) {
    return (
      <div>
        {publicKey ? (
          <div className="flex gap-2">
            <Button
              className={`group relative ${className}`}
              variant="gray"
              onClick={disconnectWallet}
            >
              <Image
                src="/phantom.svg"
                alt="Phantom Wallet"
                width={20}
                height={20}
                className="mr-2"
              />
              <span className="group-hover:hidden md:group-hover:inline">
                {shortAddress(publicKey as any)}
              </span>
              <span className="hidden group-hover:inline md:group-hover:hidden">
                Disconnect
              </span>
            </Button>

            <Button
              className={`group relative hidden md:block ${className}`}
              variant="gray"
              onClick={disconnectWallet}
            >
              Disconnect
            </Button>
          </div>
        ) : (
          <Button
            className={className}
            variant="gray"
            onClick={() => {
              setShowWalletSelector(true);
            }}
          >
            Connect wallet
          </Button>
        )}
        {showWalletSelector && (
          <WalletSelector
            showWalletSelector={showWalletSelector}
            setShowWalletSelector={setShowWalletSelector}
          />
        )}
      </div>
    );
  }
}
