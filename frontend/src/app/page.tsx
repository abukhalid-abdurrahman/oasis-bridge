'use server'

import Header from "@/components/Header";
import SwapForm from "@/components/SwapForm";
import { searchParams } from "@/lib/types";

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <>
      <Header searchParams={searchParams} />
      <div className="">
        <div className="mx-auto">
          <SwapForm />
        </div>
      </div>
    </>
  );
}
