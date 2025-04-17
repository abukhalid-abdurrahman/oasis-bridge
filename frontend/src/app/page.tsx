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
    <div className='max-w-[1200px] mx-auto md:!px-5'>
      <Header searchParams={searchParams} />
      <div className="max-w-[512px] mx-auto mt-36">
        <div className="mx-auto">
          <SwapForm />
        </div>
      </div>
    </div>
  );
}
