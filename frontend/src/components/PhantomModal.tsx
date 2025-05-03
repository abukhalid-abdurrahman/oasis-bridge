import Image from "next/image";
import Modal from "./Modal";
import { Button } from "./ui/button";
import { useEffect } from "react";
import Loading from "./Loading";

type PhantomModalProps = {
  onClose: () => void;
  publicKey: string | null;
  walletDenied: boolean;
  connectPhantomWallet: (publicKey: string | null) => void;
  setWalletDenied: (value: boolean) => void;
  errorMessage: string;
};

export default function PhantomModal({
  onClose,
  publicKey,
  walletDenied,
  connectPhantomWallet,
  setWalletDenied,
  errorMessage,
}: PhantomModalProps) {
  useEffect(() => {
    const timer = setTimeout(() => {
      if (publicKey) onClose();
    }, 3000);
    return () => clearTimeout(timer);
  }, [publicKey, onClose]);

  const renderContent = () => {
    if (publicKey) {
      return {
        title: "Connected",
        description: "You’re connected to your Phantom wallet.",
        buttonText: "Connected",
        buttonDisabled: true,
        onClick: undefined,
      };
    }

    if (!walletDenied && !errorMessage) {
      return {
        title: "Sign to verify",
        description: "Don’t see your wallet? Check your other browser windows.",
        buttonText: "Connecting...",
        buttonDisabled: true,
        onClick: undefined,
      };
    }

    return {
      title: "Connection failed",
      description: errorMessage || 'Something went wrong',
      buttonText: "Retry",
      buttonDisabled: false,
      onClick: () => {
        connectPhantomWallet(publicKey);
        setWalletDenied(false);
      },
    };
  };

  const { title, description, buttonText, buttonDisabled, onClick } =
    renderContent();

  return (
    <Modal
      onCloseFunc={onClose}
      isNonUrlModal
      className="flex flex-col items-center justify-center text-black gap-7"
    >
      <div
        className={`relative transition-all mt-10 mb-7 ${
          (publicKey || walletDenied || errorMessage) && "!mt-5 !mb-0"
        }`}
      >
        <Loading
          className={`absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 transition-all ${
            (publicKey || walletDenied || errorMessage) && "opacity-0"
          }`}
          classNameLoading="!w-[130px] !h-[130px] !border-textGray !border-r-transparent !border-4"
        />
        <Image
          src="/phantom.svg"
          alt="Phantom Wallet"
          width={70}
          height={70}
          className="-mt-2"
        />
      </div>

      <div className="text-center flex flex-col items-center gap-2">
        <h3 className="h3 font-semibold">{title}</h3>
        <p className="p">{description}</p>
      </div>

      <Button
        variant="gray"
        size="xl"
        className="w-full"
        disabled={buttonDisabled}
        onClick={onClick}
      >
        {buttonText}
      </Button>
    </Modal>
  );
}
