'use server'

import CreateNft from "@/app/nft/create/components/CreateNft";
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
      <div className="max-w-[1200px] mx-auto mt-24 md:my-10 xl:px-5 md:!px-0">
        <div className="mx-auto">
          <CreateNft />
        </div>
      </div>
    </>
  );
}