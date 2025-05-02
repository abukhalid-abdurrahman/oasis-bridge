import { usePhantomWallet } from "@/hooks/usePhantomWallet";
import Modal from "./Modal";
import PhantomModal from "./PhantomModal";
import { SetStateAction, useState } from "react";
import { useWalletStore } from "@/store/useWalletStore";
import { walletsForConnection } from "@/lib/helpers/walletsForConnection";
import { buttonVariants } from "./ui/button";
import Image from "next/image";

interface WalletSelectorProps {
  showWalletSelector: boolean;
  setShowWalletSelector: (value: SetStateAction<boolean>) => void;
}

export default function WalletSelector({
  showWalletSelector,
  setShowWalletSelector,
}: WalletSelectorProps) {
  const [whatWallet, setWhatWallet] = useState<string | null>(null);
  const { connectPhantomWallet, walletDenied, setWalletDenied, errorMessage } =
    usePhantomWallet();
  const { publicKey } = useWalletStore();
  return (
    <Modal
      onCloseFunc={() => setShowWalletSelector(false)}
      isNonUrlModal
      className="flex flex-col justify-center text-black"
    >
      <h3 className="h3 mb-7">Choose your wallet</h3>
      <div className="w-full">
        {walletsForConnection.map((wallet) => (
          <div
            key={wallet.id}
            className={`${buttonVariants({
              variant: "empty",
              size: "xxl",
            })} !p-3 h-auto w-full !justify-between cursor-pointer`}
            onClick={() => {
              setWhatWallet(wallet.walletName)
              connectPhantomWallet(publicKey)
            }}
          >
            <div className="flex gap-5 items-center">
              <Image
                src={wallet.img}
                alt={wallet.walletName}
                width={35}
                height={35}
              />
              <p className="p">{wallet.walletName}</p>
            </div>
            <div className="">
              <p className="text:sm opacity-50">Detected</p>
            </div>
          </div>
        ))}
      </div>
      {whatWallet === 'Phantom' && (
        <PhantomModal
          errorMessage={errorMessage}
          setWalletDenied={setWalletDenied}
          connectPhantomWallet={connectPhantomWallet}
          walletDenied={walletDenied}
          publicKey={publicKey}
          onClose={() => setShowWalletSelector(false)}
        />
      )}
    </Modal>
  );
}
