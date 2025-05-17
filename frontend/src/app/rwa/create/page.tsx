"use server";

import CreateRwa from "@/app/rwa/create/components/CreateRwa";
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
      <div className="mx-auto mt-24 lg:mt-16 md:!mt-10 xxs:!mt-5">
        <CreateRwa />
      </div>
    </>
  );
}
