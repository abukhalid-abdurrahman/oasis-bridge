import { useVirtualAccountBalance } from "@/requests/getRequests";
import { Dispatch, SetStateAction, useEffect, useState } from "react";
import Loading from "./Loading";
import Modal from "./Modal";
import CopyBtn from "./CopyBtn";
import { shortAddress } from "@/lib/scripts/script";
import Image from "next/image";
import { statusMessages } from "@/lib/helpers/statusMessages";

interface StatusModalProps {
  orderId: string;
  isOpen: boolean;
  onClose: () => void;
  isOrderCompleted: boolean
}

export default function StatusModal({
  orderId,
  isOpen,
  onClose,
  isOrderCompleted
}: StatusModalProps) {
  // const [isNonCompleted, setIsNonCompleted] = useState(true)
  const [address, setAddress] = useState("");
  const [messageIndex, setMessageIndex] = useState(0);
  const { data, refetch } = useVirtualAccountBalance(orderId!, !address);
  
  useEffect(() => {
    if (data?.data.status === "Completed") {
      setAddress(data.data.transactionId);
      // setIsNonCompleted(false)
    }
  }, [data]);

  useEffect(() => {
    if (isOrderCompleted) {
      setAddress('')
    }
  }, [isOrderCompleted])

  useEffect(() => {
    if (!orderId || address || isOrderCompleted) return;
  
    const interval = setInterval(() => {
      refetch();
    }, 10000);
  
    return () => clearInterval(interval);
  }, [orderId, address]);
  

  useEffect(() => {
    if (!isOpen || (data && data.data.status === "Completed")) return;

    const messageInterval = setInterval(() => {
      setMessageIndex((prevIndex) => (prevIndex + 1) % statusMessages.length);
    }, 5000);

    return () => clearInterval(messageInterval);
  }, [isOpen, data]);

  useEffect(() => {
    console.log(data)
  }, [data])

  if (!isOpen) return null;

  return (
    <Modal isNonClosable={true} className="grid">
      <div className="flex flex-col items-center justify-center">
        {!address && (
          <div className="flex flex-col items-center justify-center py-10">
            <Loading />
            <h2 className="h2 text-center mt-5">
              {statusMessages[messageIndex]}
            </h2>
          </div>
        )}
        {address && (
          <>
            <Image
              src="/done.svg"
              alt="Done"
              width={100}
              height={100}
              className="mt-5 sm:w-20"
            />
            <h2 className="h2 mt-5 !block">
              Your transaction was successful
            </h2>
            <div className="flex gap-[5px] mb-[10px] w-full mt-5">
              <div className="flex gap-2 bg-gray py-3 px-5 rounded-xl justify-between items-center flex-1 relative">
                <p className="sm:text-sm xxs:text-xs">Your transaction ID:</p>
                <p className="">{shortAddress(data?.data.transactionId)!}</p>
              </div>
              <CopyBtn address={address!} />
            </div>
            <button onClick={() => {
              onClose()
              setAddress('')
            }} className="btn btn-lg w-full !block">
              Done
            </button>
          </>
        )}
      </div>
    </Modal>
  );
}
