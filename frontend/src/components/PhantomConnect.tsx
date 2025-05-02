"use client";
import { usePhantomWallet } from "@/hooks/usePhantomWallet";
import { Button } from "./ui/button";
import PhantomModal from "./PhantomModal";
import { useEffect, useState } from "react";
import { useWalletStore } from "@/store/useWalletStore";
import Image from "next/image";
import { shortAddress } from "@/lib/scripts/script";
import { set } from "zod";
import { mutateWallet } from "@/requests/postRequests";
import { useUserStore } from "@/store/useUserStore";

export default function PhantomConnect({ className }: { className?: string }) {
  const [showModal, setShowModal] = useState(false);
  const {
    connectWallet,
    disconnectWallet,
    walletDenied,
    setWalletDenied,
    errorMessage,
  } = usePhantomWallet();
  const { publicKey } = useWalletStore();
  const { user } = useUserStore();

  const submit = mutateWallet();

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
              connectWallet(publicKey!);
              setShowModal(true);
            }}
          >
            Connect Phantom
          </Button>
        )}
        {showModal && (
          <PhantomModal
            errorMessage={errorMessage}
            setWalletDenied={setWalletDenied}
            connectWallet={connectWallet}
            walletDenied={walletDenied}
            publicKey={publicKey}
            onClose={() => setShowModal(false)}
          />
        )}
      </div>
    );
  }
}
