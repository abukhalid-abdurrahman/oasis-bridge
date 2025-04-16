'use server'

import Chart from "@/components/Chart";
import Header from "@/components/Header";
import { Button, buttonVariants } from "@/components/ui/button";
import { searchParams } from "@/lib/types";
import Image from "next/image";

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
          <div className="flex gap-10">
            <Chart />
            <div className="text-white w-full">
              <div className="flex gap-5">
                <Image 
                  src='/nft.avif'
                  alt='NFT'
                  width={150}
                  height={150}
                  className="rounded-2xl"
                />
                <div className="">
                  <h3 className="h1">NFT Title</h3>
                  <p className="p text-textGray -mb-2 mt-2">Price</p>
                  <h2 className="h1 text-green-500 font-bold">126.22$</h2>
                </div>
              </div>
              <div className="mt-5 w-full flex flex-col gap-2">
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between`}>
                  <span className="text-gray-500">Asset Description:</span> Hello my name is Famous
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between`}>
                  <span className="text-gray-500">Unique Identifier:</span> Jfex03Fjeo9wFFkfe
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between`}>
                  <span className="text-gray-500">Royalty:</span> 2%
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between`}>
                  <span className="text-gray-500">Network:</span> Solana
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between`}>
                  <span className="text-gray-500">IPFS CID:</span> http://solana.com/ipfs
                </div>
                <Button variant='empty' size='lg'>
                  Purchase
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </>
  );
}