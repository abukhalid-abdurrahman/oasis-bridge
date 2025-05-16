"use server";

import Header from "@/components/Header";
import { searchParams } from "@/lib/types";
import RWATable from "./components/RWATable";

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <>
      <Header searchParams={searchParams} />
      <div className="max-w-[1200px] mx-auto pb-10 md:py-10 xl:px-5 md:!px-0 text-white">
        <div className="mx-auto">
          <RWATable />
        </div>
      </div>
    </>
  );
}