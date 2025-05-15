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
import { Dispatch, SetStateAction, useState } from "react";
import { Connection, Transaction } from "@solana/web3.js";
import { SOLANA_NET } from "@/lib/constants";

interface PurchaseButtonProps {
  setIsOpen: Dispatch<SetStateAction<boolean>>;
  tokenId: string;
}

export default function PurchaseButton({
  setIsOpen,
  tokenId,
}: PurchaseButtonProps) {
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isSuccessfullyDone, setIsSuccessfullyDone] = useState(false);
  const [isError, setIsError] = useState(false);
  const [transactionId, setTransactionId] = useState("");

  const purchase = mutateRwaPurchase();
  const submitTransaction = mutateRwaTransaction();

  const handlePurchase = async () => {
    setIsModalOpen(true);
    setIsError(false);
    setIsSuccessfullyDone(false);

    try {
      const provider = (window as any).solana;
      if (!provider || !provider.isPhantom) {
        alert("Phantom wallet not found");
        setIsError(true);
        return;
      }

      await provider.connect();

      purchase.mutate(tokenId, {
        onSuccess: async (res) => {
          try {
            const txBase64 = res.data;
            const transaction = Transaction.from(
              Buffer.from(txBase64, "base64")
            );

            const signedTransaction = await provider.signTransaction(
              transaction
            );

            // const connection = new Connection(SOLANA_NET!);
            // await connection.confirmTransaction(signature, "confirmed");

            submitTransaction.mutate(Buffer.from(signedTransaction.serialize()).toString("base64"), {
              onSuccess: (res) => {
                setTransactionId(res.data);
                setIsSuccessfullyDone(true);
              },
              onError: () => {
                setIsError(true);
              },
            });
          } catch (err) {
            console.error(err);
            setIsError(true);
          }
        },
        onError: () => {
          setIsError(true);
        },
      });
    } catch (err) {
      console.error(err);
      setIsError(true);
    }
  };

  return (
    <>
      <Button variant="green" size="lg" onClick={handlePurchase}>
        Purchase
      </Button>
      {isModalOpen && (
        <Modal
          isNonClosable={!isError}
          isNonUrlModal
          className={`${
            (!isSuccessfullyDone || isError) &&
            "min-h-64 flex justify-center items-center"
          }`}
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
                <h2 className="h2 my-5 !block">
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
                    setIsOpen(false);
                  }}
                  className="w-full mt-2"
                >
                  Done
                </Button>
              </>
            )}
            {!isSuccessfullyDone && isError && (
              <p className="p text-black">Something went wrong. Please try again later.</p>
            )}
          </div>
        </Modal>
      )}
    </>
  );
}
