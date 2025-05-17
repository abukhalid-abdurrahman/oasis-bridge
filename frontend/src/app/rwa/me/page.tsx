"use server";

import Header from "@/components/Header";
import PageTitle from "@/components/PageTitle";
import { searchParams } from "@/lib/types";
import RwaTableMe from "./components/RwaTableMe";

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <>
      <Header searchParams={searchParams} />
      <div className="max-w-[1200px] mx-auto pb-10 md:py-10 xl:px-5 md:!px-0 text-white">
        <PageTitle title='My RWAs' />
        <RwaTableMe />
      </div>
    </>
  );
}
