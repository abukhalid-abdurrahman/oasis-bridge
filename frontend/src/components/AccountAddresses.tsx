"use server";

import CryptoItem from "./CryptoItem";
import CopyBtn from "./CopyBtn";
// import { useUserStore } from "@/store/useUserStore";
// import { useUserVirtualAccounts } from "@/requests/getRequests";
import { getUserVirtualAccountsOnServer } from "@/requests/getRequestsOnServer";
import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import Link from "next/link";
import { buttonVariants } from "./ui/button";
// import { useEffect } from "react";
// import { redirectOnUnauthorize } from "@/lib/scripts/script";

export default async function AccountAddresses() {
  const cookiesStore = await cookies();
  const token = cookiesStore.get("oasisToken")?.value;
  // const user = useUserStore((state) => state.user)
  // const { data } = useUserVirtualAccounts(true, user?.token!)
  const data: any = [];
  try {
    const res = await getUserVirtualAccountsOnServer(token!);
    if (res.status === 401) {
      const create = async () => {
        cookiesStore.delete("oasisToken");
      };
      redirect("/?sigin=true");
    } else {
      data.push(res.data);
    }
  } catch (error) {
    const create = async () => {
      cookiesStore.delete("oasisToken");
    };
    redirect("/?signin=true");
  }

  if (data[0].data.length <= 0) {
    return (
      <div>
        <h2 className="h2 text-white mb-6">Account Addresses</h2>
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
  } else if (data[0].data.length > 0) {
    return (
      <div>
        <h2 className="h2 text-white mb-6">Account Addresses</h2>
        <div className="flex flex-col gap-[5px]">
          {data[0]?.data.map((address: any) => (
            <div key={address.token} className="flex gap-[5px]">
              <CryptoItem
                image={`/${address.token}.png`}
                crypto={address.token}
              />
              <div
                className={`${buttonVariants({
                  variant: "empty",
                  size: "xl",
                })} flex gap-2 bg-gray px-5 rounded-xl justify-between items-center flex-1 sm:py-1 xxs:px-3 relative`}
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
