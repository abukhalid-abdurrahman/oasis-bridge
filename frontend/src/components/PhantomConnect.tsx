"use client";
import { usePhantomWallet } from "@/hooks/usePhantomWallet";
import { Button } from "./ui/button";

export default function PhantomConnect({ className }: { className?: string }) {
  const { connectWallet, disconnectWallet, publicKey, isPhantomInstalled } =
    usePhantomWallet();

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
            Connected
          </Button>
        ) : (
          <Button className={className} variant="gray" onClick={connectWallet}>
            Connect Phantom
          </Button>
        ))}
    </div>
  );
}
