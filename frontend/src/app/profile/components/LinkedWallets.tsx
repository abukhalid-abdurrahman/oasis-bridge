"use client";

import CopyBtn from "@/components/CopyBtn";
import CryptoItem from "@/components/CryptoItem";
import { buttonVariants } from "@/components/ui/button";
import { Skeleton } from "@/components/ui/skeleton";
import { shortAddress } from "@/lib/scripts/script";
import { useLinkedWallets } from "@/requests/getRequests";
import { useUserStore } from "@/store/useUserStore";
import { use } from "react";

export default function LinkedWallets() {
  const { user } = useUserStore();
  const { data, isFetching } = useLinkedWallets(user?.token!);

  if (isFetching) {
    return (
      <div className="flex flex-col space-y-3 mt-16">
        <Skeleton className="w-2/3 h-10 rounded-xl" />
        <div className="space-y-2">
          <Skeleton className="w-full h-20" />
        </div>
      </div>
    );
  }

  return (
    <div className="mt-10">
      <h2 className="h2 text-white mb-4">Linked Wallets</h2>
      <div className="flex flex-col gap-[5px]">
        {data?.data.map((wallet: any) => (
          <div key={wallet.walletAddress} className="flex gap-[5px]">
            <CryptoItem
              image={`/${wallet.network}.png`}
              crypto={wallet.network}
            />
            <div
              className={`${buttonVariants({
                variant: "empty",
                size: "xl",
              })} flex gap-2 bg-gray px-5 rounded-xl flex-1 sm:py-1 sm:px-3 relative sm:h-[46px] !justify-between`}
            >
              <p className="p sm:absolute sm:text-textGray sm:top-1 sm:text-xs">
                Address:
              </p>
              <p className="p sm:pt-3 sm:text-sm">
                {shortAddress(wallet.walletAddress)}
              </p>
            </div>
            <CopyBtn address={wallet.walletAddress} />
          </div>
        ))}
      </div>
    </div>
  );
}
