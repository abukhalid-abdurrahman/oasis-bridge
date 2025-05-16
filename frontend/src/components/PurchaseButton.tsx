import CopyBtn from "@/components/CopyBtn";
import Loading from "@/components/Loading";
import Modal from "@/components/Modal";
import { Button, buttonVariants } from "@/components/ui/button";
import { shortAddress } from "@/lib/scripts/script";
import {
  mutateRwaPurchase,
  mutateRwaTransaction,
} from "@/requests/postRequests";
import Image from "next/image";
import { useState } from "react";
import { Connection, Transaction } from "@solana/web3.js";
import { useWalletStore } from "@/store/useWalletStore";
import { SOLANA_NET } from "@/lib/constants";

interface PurchaseButtonProps {
  tokenId: string;
  usageInMarketPage?: boolean;
}

export default function PurchaseButton({
  tokenId,
  usageInMarketPage,
}: PurchaseButtonProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSuccessfullyDone, setIsSuccessfullyDone] = useState(false);
  const [isError, setIsError] = useState(false);
  const [transactionId, setTransactionId] = useState("");
  const [errorMessage, setErrorMessage] = useState("");
  const { publicKey } = useWalletStore();

  const purchase = mutateRwaPurchase();
  const submitTransaction = mutateRwaTransaction();

  const handlePurchase = async () => {
    setIsModalOpen(true);
    setIsError(false);
    setIsSuccessfullyDone(false);
    setErrorMessage("");

    try {
      const provider = (window as any).solana;
      if (!provider || !provider.isPhantom) {
        setErrorMessage("Phantom wallet not found");
        setIsError(true);
        return;
      }

      if (!publicKey) {
        setIsError(true);
        setErrorMessage("Please connect your wallet firstly.");
        return;
      }

      const connectRes = await provider.connect();

      purchase.mutate(
        {
          rwaId: tokenId,
          buyerPubKey: publicKey,
        },
        {
          onSuccess: async (res) => {
            try {
              const connection = new Connection(SOLANA_NET!, "confirmed");
              const txBase64 = res.data;
              const transaction = Transaction.from(
                Buffer.from(txBase64, "base64")
              );

              const signedTransaction = await provider
                .signTransaction(transaction)
                .catch((err: any) => {
                  throw new Error(
                    err?.message === "User rejected the request."
                      ? "Transaction signing was cancelled"
                      : err?.message || "Failed to sign transaction"
                  );
                });

              const rawTx = signedTransaction.serialize();
              const txBase64Signed = Buffer.from(rawTx).toString("base64");

              submitTransaction.mutate(
                {
                  transactionHash: txBase64,
                  transactionSignature: txBase64Signed,
                },
                {
                  onSuccess: (res) => {
                    setTransactionId(res.data);
                    setIsSuccessfullyDone(true);
                  },
                  onError: (err: any) => {
                    setErrorMessage(
                      err?.response?.data?.error?.message ||
                        "Transaction submission failed"
                    );
                    setIsError(true);
                  },
                }
              );
            } catch (err: any) {
              console.error("Signing or submission error:", err);
              setErrorMessage(err.message || "Something went wrong");
              setIsError(true);
            }
          },
          onError: (error: any) => {
            console.error("Transaction fetch error:", error);
            setErrorMessage(
              error?.response?.data?.error?.message ||
                "Could not prepare transaction"
            );
            setIsError(true);
          },
        }
      );
    } catch (err: any) {
      console.error("Connection or general error:", err);
      setErrorMessage(
        err?.message === "User rejected the request."
          ? "Wallet connection was cancelled"
          : err?.message || "Something went wrong"
      );
      setIsError(true);
    }
  };

  return (
    <>
      <Button
        variant="green"
        size={`${usageInMarketPage ? "sm" : "lg"}`}
        onClick={handlePurchase}
      >
        Purchase
      </Button>
      {isModalOpen && (
        <Modal
          isNonClosable={!isError}
          isNonUrlModal
          className={`${
            (!isSuccessfullyDone || isError) &&
            "min-h-64 flex justify-center items-center"
          } relative z-[10000]`}
          onCloseFunc={() => setIsModalOpen(false)}
        >
          <div className="flex flex-col items-center justify-center">
            {!isSuccessfullyDone && !isError && <Loading />}
            {isSuccessfullyDone && !isError && (
              <>
                <Image
                  src="/done.svg"
                  alt="Done"
                  width={100}
                  height={100}
                  className="mt-5 sm:w-20"
                />
                <h2 className="h2 my-5 !block text-black">
                  You have successfully purchased your RWA
                </h2>
                <div className="flex gap-[5px] mb-[10px] w-full mt-5">
                  <div
                    className={`${buttonVariants({
                      variant: "empty",
                      size: "xl",
                    })} flex gap-2 bg-gray py-3 px-5 rounded-xl justify-between items-center flex-1 relative`}
                  >
                    <p className="sm:text-sm xxs:text-xs">
                      Your transaction ID:
                    </p>
                    <p className="">{shortAddress(transactionId)}</p>
                  </div>
                  <CopyBtn address={transactionId} />
                </div>
                <Button
                  variant="gray"
                  size="xl"
                  onClick={() => {
                    setIsSuccessfullyDone(false);
                    setIsModalOpen(false);
                  }}
                  className="w-full mt-2"
                >
                  Done
                </Button>
              </>
            )}
            {!isSuccessfullyDone && isError && (
              <p className="p text-black text-center max-w-sm whitespace-pre-line">
                {errorMessage ||
                  "Something went wrong. Please try again later."}
              </p>
            )}
          </div>
        </Modal>
      )}
    </>
  );
}
