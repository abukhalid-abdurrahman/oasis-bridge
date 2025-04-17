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
      <div className="max-w-[1200px] md:my-10 xl:px-5 md:!px-0 translate-y-1/3 lg:translate-y-0 sm:!mb-0 sm:pb-5">
        <div className="mx-auto">
          <div className="grid grid-cols-3 gap-10 lg:gap-10 lg:flex lg:flex-col-reverse">
            <Chart className="col-span-2 lg:col-span-1" />
            <div className="flex flex-col gap-5 text-white items-start col-span-1 lg:flex-row sm:!flex-col">
              <div className="flex gap-5 shrink-0 lg:flex-col lg:gap-3 sm:w-full">
                <Image 
                  src='/nft.avif'
                  alt='NFT'
                  width={100}
                  height={100}
                  className="rounded-2xl lg:w-[200px] sm:!w-full sm:!aspect-square sm:!h-auto"
                />
                <div className="sm:flex sm:justify-between sm:mt-3 sm:mb-1">
                  <h3 className="h1 sm:!text-2xl">NFT Title</h3>
                  <p className="p text-textGray -mb-2 mt-2 lg:mt-0 lg:-mb-1 sm:hidden">Price</p>
                  <h2 className="h1 text-green-500 font-bold sm:!text-2xl">126.22$</h2>
                </div>
              </div>
              <div className="w-full flex flex-col gap-2">
                <span className="text-gray-500 text-sm -mb-1">Asset Description:</span>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between flex-wrap`}>
                   Hello my name is Famous
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between flex-wrap`}>
                  <span className="text-gray-500">Unique Identifier:</span> Jfex03Fjeo9wFFkfe
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between flex-wrap`}>
                  <span className="text-gray-500">Royalty:</span> 2%
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between flex-wrap`}>
                  <span className="text-gray-500">Network:</span> Solana
                </div>
                <div className={`${buttonVariants({ variant: 'gray', size: 'lg' })} !px-5 !w-full flex justify-between flex-wrap`}>
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