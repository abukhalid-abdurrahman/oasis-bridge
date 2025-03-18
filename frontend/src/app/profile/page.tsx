'use server'

import AccountAddresses from "@/components/AccountAddresses";
import ChangePasswordForm from "@/components/ChangePasswordForm";
import Header from "@/components/Header";
import ProfileForm from "@/components/ProfileForm";
import { searchParams } from "@/lib/types";

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <div className="">
      <Header searchParams={searchParams} />
      <AccountAddresses />
      <ChangePasswordForm />
    </div>
  );
}
