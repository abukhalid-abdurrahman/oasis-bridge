"use server";

import AccountAddresses from "@/components/AccountAddresses";
import ChangePasswordForm from "@/components/ChangePasswordForm";
import Header from "@/components/Header";
import { searchParams } from "@/lib/types";

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <div className="">
      <Header searchParams={searchParams} />
      <div className="max-w-[512px] mx-auto mt-36">
        <AccountAddresses />
        <ChangePasswordForm />
      </div>
    </div>
  );
}
