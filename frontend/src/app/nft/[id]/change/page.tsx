'use server'

import ChangeNft from "@/components/ChangeNFT";
import Header from "@/components/Header";
import { searchParams } from "@/lib/types";

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <div className='max-w-[1200px] mx-auto'>
      <Header searchParams={searchParams} />
      <div className="mt-24 xl:px-5 md:mt-14 md:!px-0 sm:!mt-10">
        <div className="mx-auto">
          <ChangeNft />
        </div>
      </div>
    </div>
  );
}
