"use client";

import { useEffect, useState } from "react";
import { useUserStore } from "@/store/useUserStore";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { buttonVariants } from "./ui/button";
import { Avatar, AvatarFallback, AvatarImage } from "./ui/avatar";
import Cookies from "js-cookie";

export default function MobileHeaderBtns() {
  const router = useRouter();
  const { user, logout, setUser } = useUserStore();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, [setUser]);

  useEffect(() => {
    if (!Cookies.get("oasisToken")) {
      setUser(null);
    }
  }, [Cookies.get("oasisToken")]);

  if (loading) return null;

  return (
    <>
      {!user ? (
        <Link
          href="?signin=true"
          className={`${buttonVariants({
            variant: "gray",
            size: "default",
          })} !items-start !justify-start !px-5`}
        >
          Sign In
        </Link>
      ) : (
        <>
          <Link href="/profile">
            <div className="flex items-center gap-4 mb-5">
              <Avatar className="w-12 h-12">
                <AvatarImage src="/profile.svg" className="invert" />
                <AvatarFallback className="">AV</AvatarFallback>
              </Avatar>
              <div className="">
                <p className="font-semibold text-lg">{user.UserName}</p>
                <p className="text-textGray text-sm -mt-1">{user.Email}</p>
              </div>
            </div>
          </Link>
        </>
      )}
    </>
  );
}
