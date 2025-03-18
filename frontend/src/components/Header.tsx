'use server'

import Link from "next/link";
import SignInModal from "./SignInModal";
import SignUpModal from "./SignUpModal";
import { searchParams } from "@/lib/types";
import HeaderBtns from "./HeaderBtns";

export default async function Header({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  const { signin, signup } = await searchParams;

  return (
    <header
      className="flex justify-between items-center bg-white rounded-[20px] 
    px-[35px] py-[30px] mb-20 sm:py-[25px] sm:px-[20px]"
    >
      <Link href="/" className="h1 text-xl">
        OASIS Celestial Bridge
      </Link>
      <div className="flex gap-5 sm:gap-2">
        {/* <Button onClick={() => openModal("signup")}>Sign Up</Button> */}
        {/* <Button onClick={() => openModal("signin")}>Sign In</Button> */}
        <HeaderBtns />
      </div>
      {signin && <SignInModal />}
      {signup && <SignUpModal />}
    </header>
  );
}
