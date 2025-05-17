"use client";

import { useForm } from "react-hook-form";
import { useEffect, useRef, useState } from "react";
import Image from "next/image";

import CryptoModal from "./CryptoModal";
import StatusModal from "./StatusModal";
import CryptoAddressModal from "./CryptoAddressModal";

import { useSwap } from "@/hooks/swapHooks";
import { useExchangeRate } from "@/requests/getRequests";
import { useUserStore } from "@/store/useUserStore";
import { mutateOrders } from "@/requests/postRequests";
import LoadingAlt from "./LoadingAlt";
import { useRouter } from "next/navigation";
import { Button } from "./ui/button";

export default function SwapForm() {
  const user = useUserStore((state) => state.user);
  const router = useRouter();
  const [isOrderCompleted, setIsOrderCompleted] = useState<boolean>(false);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors },
  } = useForm();
  const fromAmount = watch("fromAmount");

  const {
    selectedNetwork,
    selectedFrom,
    selectedTo,
    isCryptoModalOpen,
    openCryptoModal,
    closeCryptoModal,
    selectNetwork,
    selectCrypto,
    setSelectedFrom,
    setSelectedTo,
  } = useSwap();

  const { data, isFetching } = useExchangeRate(
    selectedFrom.token,
    selectedTo.token
  );

  const submit = mutateOrders();

  const [isStatusModalOpen, setIsStatusModalOpen] = useState(false);
  const [isCryptoAddressModalOpen, setIsCryptoAddressModalOpen] =
    useState(false);
  const [orderError, setOrderError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [formData, setFormData] = useState<any>();
  const [responseData, setResponseData] = useState<{
    data: { orderId: string };
  } | null>(null);

  const prevSelectedFrom = useRef(selectedFrom);
  const prevSelectedTo = useRef(selectedTo);

  useEffect(() => {
    if (selectedFrom.token === selectedTo.token) {
      setSelectedFrom(prevSelectedTo.current);
      setSelectedTo(prevSelectedFrom.current);
    }

    prevSelectedFrom.current = selectedFrom;
    prevSelectedTo.current = selectedTo;
  }, [selectedFrom, selectedTo]);

  useEffect(() => {
    if (data) {
      const convertedAmount = fromAmount * data.data.rate || "";
      setValue("toAmount", convertedAmount);
    }
  }, [fromAmount, data]);

  useEffect(() => {
    if (isOrderCompleted) {
      setOrderError('');
      setFormData(null);
      setResponseData(null);
      setIsStatusModalOpen(false);
      setIsCryptoAddressModalOpen(false);
    }
  }, [isOrderCompleted]);

  const onSubmit = (data: any) => {
    if (!user?.Id) {
      router.push("?signin=true");
    } else {
      setIsLoading(true);
      const formData = {
        userId: user?.Id,
        fromToken: selectedFrom.token,
        toToken: selectedTo.token,
        amount: data.fromAmount,
        fromNetwork: selectedFrom.network,
        toNetwork: selectedTo.network,
        destinationAddress: data.destinationAddress,
      };
      setFormData(formData);
      submit.mutate(formData, {
        onSuccess: (response) => {
          setResponseData(response);
        },
        onError: (error: any) => {
          setOrderError(error.response?.data?.error?.message || 'An error occurred');
        },
      });
      setIsCryptoAddressModalOpen(true);
    }
  };

  const closeStatusModal = () => {
    setIsOrderCompleted(true);
    setIsStatusModalOpen(false);
  };

  return (
    <div className="">
      <form
        onSubmit={handleSubmit(onSubmit)}
        className="flex flex-col gap-5 sm:gap-3"
      >
        {/* From field */}
        <div className="flex flex-col gap-5 relative sm:gap-3">
          <div className="flex gap-2 items-center bg-white rounded-2xl p-5">
            <div className="flex flex-col w-full">
              <p className="p-sm">From</p>
              <input
                className="input-swap !pt-2 !pb-4 sm:!pb-2"
                type="number"
                step="0.00001"
                defaultValue={1}
                placeholder={selectedFrom.token}
                onKeyDown={(e) => {
                  if (
                    !/[0-9.]/.test(e.key) &&
                    e.key !== "Backspace" &&
                    e.key !== "Delete"
                  ) {
                    e.preventDefault();
                  }
                }}
                {...register("fromAmount", {
                  required: "Amount is required",
                  validate: (value) => value > 0,
                  // valueAsNumber: true,
                  min: { value: 0.00001, message: "Amount must be positive" },
                })}
              />
              {errors.fromAmount && (
                <p className="text-red-500 text-sm">
                  {errors.fromAmount.message as any}
                </p>
              )}
            </div>
            <Button
              variant='empty'
              size='lg'
              type="button"
              className="text-lg rounded-full bg-darkGray px-4 flex justify-center gap-[7px] items-center sm:text-base xxs:py-1 hover:shadow-md transition-all"
              onClick={() => openCryptoModal("from")}
            >
              <Image
                src={`/${selectedFrom.token}.png`}
                alt=""
                width={24}
                height={24}
                className="sm:w-5"
              />
              {selectedFrom.token}
            </Button>
          </div>

          {/* Swap button */}
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2">
            <button
              type="button"
              className="bg-white w-10 h-10 flex justify-center items-center rounded-xl border border-[#878787] shadow-sm group sm:w-8 sm:h-8"
              onClick={() => {
                const temp = selectedFrom;
                setSelectedFrom(selectedTo);
                setSelectedTo(temp);
              }}
            >
              <Image
                src="/arrow.svg"
                alt="Arrow"
                width={16}
                height={16}
                className="group-hover:rotate-180 transition-transform duration-200 sm:w-3"
              />
            </button>
          </div>

          {/* To field */}
          <div className="flex gap-2 items-center bg-white rounded-2xl p-5">
            <div className="flex flex-col w-full">
              <p className="p-sm">To</p>
              <input
                className="input-swap !pt-2 !pb-4 sm:!pb-2"
                type="text"
                placeholder={`Result in ${selectedTo.token}`}
                disabled={true}
                {...register("toAmount")}
              />
            </div>
            <Button
              variant='empty'
              size='lg'
              type="button"
              className="text-lg rounded-full bg-darkGray py-2 px-4 flex gap-[7px] justify-center items-center sm:text-base xxs:py-1 hover:shadow-md transition-all"
              onClick={() => openCryptoModal("to")}
            >
              <Image
                src={`/${selectedTo.token}.png`}
                alt=""
                width={24}
                height={24}
                className="sm:w-5"
              />
              {selectedTo.token}
            </Button>
          </div>
        </div>

        {/* Recipient address field */}
        <div className="flex gap-2 items-center bg-white rounded-2xl p-5">
          <div className="flex flex-col w-full">
            <p className="p-sm">Recipient Address</p>
            <input
              className="input-swap w-full !pt-2"
              type="text"
              placeholder="Your wallet address"
              {...register("destinationAddress", {
                required: "Address is required",
              })}
            />
            {errors.destinationAddress && (
              <p className="text-red-500 text-sm">
                {errors.destinationAddress.message as any}
              </p>
            )}
          </div>
        </div>

        <Button
          type="submit"
          variant='gray'
          size='xxl'
          onClick={() => {
            setIsOrderCompleted(false);
          }}
        >
          Swap
        </Button>
      </form>

      {/* Modals */}
      <CryptoModal
        isOpen={isCryptoModalOpen}
        onClose={closeCryptoModal}
        selectNetwork={selectNetwork}
        selectedNetwork={selectedNetwork as any}
        onSelect={selectCrypto as any}
        selectedFrom={selectedFrom}
        selectedTo={selectedTo}
        setSelectedFrom={setSelectedFrom}
        setSelectedTo={setSelectedTo}
      />
      {isCryptoAddressModalOpen && (
        <CryptoAddressModal
          isOrderCompleted={isOrderCompleted}
          setIsOrderCompleted={setIsOrderCompleted}
          fromNetwork={selectedFrom}
          fromAmount={fromAmount}
          formData={formData}
          isOpen={isCryptoAddressModalOpen}
          setIsOpen={setIsCryptoAddressModalOpen}
          onClose={closeCryptoModal}
          setIsStatusModalOpen={setIsStatusModalOpen}
          orderId={responseData?.data?.orderId}
          orderError={orderError}
        />
      )}
      {isStatusModalOpen && (
        <StatusModal
          isOrderCompleted={isOrderCompleted}
          orderId={responseData?.data?.orderId!}
          isOpen={isStatusModalOpen}
          onClose={closeStatusModal}
        />
      )}
      {isFetching && <LoadingAlt />}
    </div>
  );
}
