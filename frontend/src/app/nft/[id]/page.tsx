"use server";

import Header from "@/components/Header";
import { searchParams } from "@/lib/types";
import RwaData from "./components/RwaData";

export default async function page({
  searchParams,
  params,
}: {
  searchParams: Promise<searchParams>;
  params: any;
}) {
  return (
    <>
      <Header searchParams={searchParams} />
      <div className="max-w-[1200px] md:my-10 xl:px-5 md:!px-0 mt-40 lg:mt-10 sm:!mb-0 sm:pb-5">
        <div className="mx-auto">
          <RwaData params={params} />
        </div>
      </div>
    </>
  );
}
