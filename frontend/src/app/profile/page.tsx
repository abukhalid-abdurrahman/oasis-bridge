"use server";

import Header from "@/components/Header";
import { searchParams } from "@/lib/types";
import AccountAddresses from "./components/AccountAddresses";
import ChangePasswordForm from "./components/ChangePasswordForm";

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
