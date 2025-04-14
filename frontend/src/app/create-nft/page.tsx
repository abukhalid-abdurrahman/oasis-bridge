'use server'

import CreateNft from "@/components/CreateNft";
import Header from "@/components/Header";
import { searchParams } from "@/lib/types";

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <>
      <Header searchParams={searchParams} />
      <div className="max-w-[512px] mx-auto mt-36">
        <div className="mx-auto">
          <CreateNft />
        </div>
      </div>
    </>
  );
}