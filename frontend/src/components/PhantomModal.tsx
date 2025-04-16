import Image from "next/image";
import Modal from "./Modal";
import { Button } from "./ui/button";
import { useEffect } from "react";
import Loading from "./Loading";

export default function PhantomModal({
  onClose,
  publicKey,
  walletDenied,
  connectWallet,
  setWalletDenied
}: {
  onClose: () => void;
  publicKey: string | null;
  walletDenied: boolean;
  connectWallet: () => void;
  setWalletDenied: (value: boolean) => void;
}) {
  useEffect(() => {
    setTimeout(() => {
      if (publicKey) {
        onClose();
      }
    }, 3000);
  }, [publicKey]);

  return (
    <Modal
      onCloseFunc={onClose}
      isNonUrlModal={true}
      className="flex flex-col items-center justify-center text-black gap-7"
    >
      <div
        className={`relative transition-all mt-10 mb-7 ${
          (publicKey || walletDenied) && "mt-5 mb-0"
        }`}
      >
        <Loading
          className={`absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 transition-all ${
            (publicKey || walletDenied) && "opacity-0"
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
      {publicKey ? (
        <>
          <div className="text-center flex flex-col items-center gap-2">
            <h3 className="h3 font-semibold">Connected</h3>
            <p className="p max-w-[90%]">
              You’re connected to your Phantom wallet.
            </p>
          </div>
          <Button variant="gray" size="xl" className="w-full" disabled={true}>
            Connected
          </Button>
        </>
      ) : (
        <>
          {!walletDenied ? (
            <>
              <div className="text-center flex flex-col items-center gap-2">
                <h3 className="h3 font-semibold">Sign to verify</h3>
                <p className="p max-w-[90%]">
                  Don’t see your wallet? Check your other browser windows.
                </p>
              </div>
              <Button
                variant="gray"
                size="xl"
                className="w-full"
                disabled={true}
              >
                Connecting...
              </Button>
            </>
          ) : (
            <>
              <div className="text-center flex flex-col items-center gap-2">
                <h3 className="h3 font-semibold">Connection failed</h3>
                <p className="p max-w-[90%]">
                  It seems you have declined the request. Please try again.
                </p>
              </div>
              <Button
                variant="gray"
                size="xl"
                className="w-full"
                onClick={() => {
                  connectWallet();
                  setWalletDenied(false)
                }}
              >
                Retry
              </Button>
            </>
          )}
        </>
      )}
    </Modal>
  );
}
