"use client";
import { usePhantomWallet } from "@/hooks/usePhantomWallet";
import { Button } from "./ui/button";
import PhantomModal from "./PhantomModal";
import { useEffect, useState } from "react";
import { useWalletStore } from "@/store/useWalletStore";
import Image from "next/image";
import { shortAddress } from "@/lib/scripts/script";

export default function PhantomConnect({ className }: { className?: string }) {
  const [showModal, setShowModal] = useState(false);
  const { connectWallet, disconnectWallet, isPhantomInstalled } =
    usePhantomWallet();
  const { publicKey } = useWalletStore();

  return (
    <div>
      {/* {isPhantomInstalled ? (
        publicKey ? (
          <div>
            <p>Connected: {publicKey.toBase58()}</p>
            <button onClick={disconnectWallet}>Disconnect</button>
          </div>
        ) : (
          <button onClick={connectWallet}>Connect Phantom</button>
        )
      ) : (
        <p>Please install Phantom wallet</p>
      )} */}
      {isPhantomInstalled &&
        (publicKey ? (
          <Button className={className} variant="gray" onClick={disconnectWallet}>
            <Image 
              src='/phantom.svg'
              alt='Phantom Wallet'
              width={20}
              height={20}
            />
            {shortAddress(publicKey)}
          </Button>
        ) : (
          <Button className={className} variant="gray" onClick={() => {
            connectWallet()
            setShowModal(true);
          }}>
            Connect Phantom
          </Button>
        ))}
      {showModal && <PhantomModal publicKey={publicKey} onClose={() => setShowModal(false)} />}
    </div>
  );
}
