import Loading from "@/components/Loading";
import Modal from "@/components/Modal";
import { Button, buttonVariants } from "@/components/ui/button";
import { mutateRwaUpdate } from "@/requests/putRequests";
import Image from "next/image";
import Link from "next/link";
import { Dispatch, SetStateAction, useEffect, useState } from "react";
import { UseFormReturn } from "react-hook-form";

interface UpdatingModalProps {
  setIsUpdated: Dispatch<SetStateAction<boolean>>;
  form: UseFormReturn<
    {
      [x: string]: any;
    },
    any,
    {
      [x: string]: any;
    }
  >;
  tokenId: string;
  isError: boolean;
  errorMessage: string;
  isSuccessfullyDone: boolean;
  setIsSuccessfullyDone: Dispatch<SetStateAction<boolean>>;
}

export default function UpdatingModal({
  setIsUpdated,
  form,
  tokenId,
  isError,
  errorMessage,
  isSuccessfullyDone,
  setIsSuccessfullyDone
}: UpdatingModalProps) {
  return (
    <Modal
      isNonClosable={!isError}
      isNonUrlModal
      className={`${
        (!isSuccessfullyDone || isError) && "min-h-64 flex justify-center items-center"
      }`}
      onCloseFunc={() => setIsUpdated(false)}
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
              You have successfully updated your RWA
            </h2>
            <div
              className={`${buttonVariants({
                variant: "empty",
                size: "xl",
              })} flex gap-2 w-full bg-gray pl-5 pr-0 rounded-xl justify-between items-center flex-1 relative`}
            >
              <p className="sm:text-sm xxs:text-xs">You can check it here:</p>
              <Link
                href={`/rwa/${tokenId}`}
                className={`${buttonVariants({
                  variant: "gray",
                  size: "xl",
                })}`}
              >
                Check
              </Link>
            </div>
            <Button
              variant="gray"
              size="xl"
              onClick={() => {
                setIsUpdated(false);
                setIsSuccessfullyDone(false);
                form.reset();
              }}
              className="w-full mt-2"
            >
              Done
            </Button>
          </>
        )}
        {!isSuccessfullyDone && isError && <p className="p">{errorMessage}</p>}
      </div>
    </Modal>
  );
}
