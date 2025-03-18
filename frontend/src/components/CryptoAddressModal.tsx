import {
  useUserVirtualAccounts,
  useVirtualAccountBalance,
} from "@/requests/getRequests";
import { Dispatch, SetStateAction, useEffect, useState } from "react";
import Loading from "./Loading";
import Modal from "./Modal";
import CopyBtn from "./CopyBtn";
import { shortAddress } from "@/lib/scripts/script";
import QRCodeDisplay from "./QrCode";
import CountdownTimer from "./CountdownTimer";
import { VirtualAddressMesages } from "@/lib/helpers/virtualAddressMessages";
import { SelectedCrypto } from "@/lib/cryptoOptions";
import { useUserStore } from "@/store/useUserStore";

interface StatusModalProps {
  setIsStatusModalOpen: any;
  fromNetwork: SelectedCrypto;
  fromAmount: number;
  formData: any;
  isOpen: boolean;
  setIsOpen: any;
  onClose?: () => void;
  orderId?: string;
  orderError: string;
  isOrderCompleted: boolean;
  setIsOrderCompleted: Dispatch<SetStateAction<boolean>>;
}

export default function CryptoAddressModal({
  fromNetwork,
  fromAmount,
  formData,
  isOpen,
  setIsOpen,
  setIsStatusModalOpen,
  orderId,
  orderError,
  isOrderCompleted,
  setIsOrderCompleted,
}: StatusModalProps) {
  const [showAddress, setShowAddress] = useState(false);
  const {
    data: balanceData,
    refetch: balanceRefetch,
    isError: balanceError,
    isFetching: balanceFetching
  } = useVirtualAccountBalance(orderId!, isOpen);
  const user = useUserStore((state) => state.user);
  // const { data } = useVirtualAccount(fromNetwork, toNetwork);
  const { data: accountData } = useUserVirtualAccounts(
    showAddress,
    user?.token!
  );

  const [address, setAddress] = useState("");
  const [timeLeft, setTimeLeft] = useState(600);
  const [message, setMessage] = useState("");
  const [isCanseled, setIsCanceled] = useState(false);

  useEffect(() => {
    if (accountData) {
      const account = accountData.data.find(
        (item: any) => item.token === formData.fromToken
      );
      setAddress(account.address);
    }
  }, [accountData]);

  useEffect(() => {
    const messageInterval = 120;
    const index = Math.floor((600 - timeLeft) / messageInterval);

    if (timeLeft < 600 - messageInterval) {
      if (index < VirtualAddressMesages.length) {
        setMessage(VirtualAddressMesages[index]);
      }
    }
  }, [timeLeft]);

  useEffect(() => {
    if (!balanceData) return;
  
    const status = balanceData.data.status;
  
    if (status === "InsufficientFunds") {
      setShowAddress(true);
      setIsOpen(true);
      setIsStatusModalOpen(false);
    } else if (status === "Expired") {
      setIsCanceled(true);
      setTimeLeft(0);
      // setIsOpen(false);
      // setIsStatusModalOpen(false);
    } else if (status === "Pending" || status === "Completed") {
      setShowAddress(false);
      setIsOpen(false);
      setIsStatusModalOpen(true);
    }
  }, [balanceData]);

  useEffect(() => {
    if (!showAddress || !isOpen || isCanseled) return;

    const interval = setInterval(() => {
      if (!balanceFetching || !balanceError) {
        balanceRefetch();
      }
    }, 10000);

    return () => clearInterval(interval);
  }, [showAddress, isCanseled]);

  useEffect(() => {
    if (isOrderCompleted) {
      setAddress("");
      setShowAddress(false);
    }
  }, [isOrderCompleted]);

  const onClose = () => {
    setAddress("");
    setIsOrderCompleted(true);
    setShowAddress(false);
    setIsOpen(false);
  };

  if (!isOpen) return null;

  return (
    <Modal
      isNonUrlModal={true}
      className={`${(!showAddress || !timeLeft || orderError || balanceError) && "min-h-64"}`}
      onCloseFunc={onClose}
      isNonClosable={!showAddress && !balanceError && !orderError}
      // isNonClosable={!orderId && !showAddress && !orderError}
    >
      {!showAddress && !balanceError && !orderError && (
        <Loading className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2" />
      )}
      {!timeLeft && isCanseled && (
        <>
          <CountdownTimer timeLeft={timeLeft} setTimeLeft={setTimeLeft} />
          <p className="p text-center absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
            Your order has been closed due to insufficient balance.
          </p>
        </>
      )}
      {(orderError || balanceError) && (
        <>
          {!orderError ? (
            <p className="p text-center absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
              Something went wrong. Please try again later.
            </p>
          ): (
            <p className="p text-center absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
              {orderError}
            </p>
          )}
        </>
      )}
      {showAddress && timeLeft && !balanceError && (
        <>
          <CountdownTimer timeLeft={timeLeft} setTimeLeft={setTimeLeft} />
          <div className="">
            <h2 className="h2 text-lg mb-3 text-center max-w-[280px] mx-auto">
              Please deposit your virtual account with{" "}
              <span className="font-bold">{fromAmount}</span> amount of{" "}
              {fromNetwork.token}s
            </h2>
            <div className="">
              <QRCodeDisplay text={address!} />
            </div>
            {message && (
              <p className="text-textGray p-sm text-center mt-2">{message}</p>
            )}
            <div className="flex gap-[5px] mt-5">
              <div className="flex gap-2 bg-gray py-3 px-5 rounded-xl justify-between items-center flex-1 relative">
                <p className="sm:text-sm sm:absolute sm:-top-[21px] sm:left-0">
                  Your {fromNetwork.token} virtual account:
                </p>
                <p className="">{shortAddress(address!)}</p>
              </div>
              <CopyBtn address={address!} />
            </div>
            {/* <button onClick={onSubmit} className="btn btn-lg w-full mt-[10px]">
              Submit
            </button> */}
          </div>
        </>
      )}
    </Modal>
  );
}
