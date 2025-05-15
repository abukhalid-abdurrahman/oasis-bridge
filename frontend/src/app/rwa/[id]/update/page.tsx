"use server";

import Header from "@/components/Header";
import { searchParams } from "@/lib/types";
import ChangeRwa from "./components/ChangeRwa";

export default async function page({
  searchParams,
  params,
}: {
  searchParams: Promise<searchParams>;
  params: any;
}) {
  return (
    <div className="max-w-[1200px] mx-auto">
      <Header searchParams={searchParams} />
      <div className="mt-24 xl:px-5 md:mt-14 md:!px-0 sm:!mt-10">
        <div className="mx-auto">
          <ChangeRwa params={params} />
        </div>
      </div>
    </div>
  );
}
