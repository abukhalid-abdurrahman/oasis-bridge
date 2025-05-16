"use client";

import CryptoItem from "@/components/CryptoItem";
import CopyBtn from "@/components/CopyBtn";
import { useUserStore } from "@/store/useUserStore";
import { useUserVirtualAccounts } from "@/requests/getRequests";
import Link from "next/link";
import { buttonVariants } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";

export default function AccountAddresses() {
  const user = useUserStore((state) => state.user);
  const { data, isFetching } = useUserVirtualAccounts(user?.token!);

  if (isFetching) {
    return (
      <div className="flex flex-col space-y-3">
        <Skeleton className="w-2/3 h-10 rounded-xl" />
        <div className="space-y-2">
          <Skeleton className="w-full h-20" />
        </div>
      </div>
    );
  }

  if (data.data.length <= 0) {
    return (
      <div>
        <h2 className="h2 text-white mb-4">Account Addresses</h2>
        <div className="flex flex-col gap-[5px]">
          <p className="p text-white">
            You don't have any virtual accounts yet. Make your first swap to
            create one.
          </p>
          <Link
            href="/"
            className={buttonVariants({ variant: "gray", size: "xl" })}
          >
            Make a Swap
          </Link>
        </div>
      </div>
    );
  } else if (data.data.length > 0) {
    return (
      <div>
        <h2 className="h2 text-white mb-6">Account Addresses</h2>
        <div className="flex flex-col gap-[5px]">
          {data?.data.map((address: any) => (
            <div key={address.token} className="flex gap-[5px]">
              <CryptoItem
                image={`/${address.token}.png`}
                crypto={address.token}
              />
              <div
                className={`${buttonVariants({
                  variant: "empty",
                  size: "xl",
                })} flex gap-2 bg-gray px-5 rounded-xl justify-between items-center flex-1 sm:py-1 sm:px-3 sm:h-[46px] relative`}
              >
                <p className="p sm:absolute sm:text-textGray sm:top-1 sm:text-xs">
                  Balance:
                </p>
                <p className="p sm:pt-3 sm:text-sm">{address.balance}</p>
              </div>
              <CopyBtn address={address.address} />
            </div>
          ))}
        </div>
      </div>
    );
  }
}
