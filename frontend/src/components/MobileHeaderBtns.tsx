"use client";

import { useEffect, useState } from "react";
import { useUserStore } from "@/store/useUserStore";
import Link from "next/link";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { Button, buttonVariants } from "./ui/button";

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
          <Link
            href="/profile"
            className={buttonVariants({ variant: "gray", size: "default" })}
          >
            <Image
              src="/profile.svg"
              alt={user.UserName}
              width={21}
              height={21}
            />
            {user.UserName}
          </Link>
          <Button
            variant="gray"
            size="icon"
            onClick={() => {
              logout();
              localStorage.removeItem("user");
              router.push("/");
            }}
          >
            <Image src="/logout.svg" alt="Logout" width={24} height={24} />
          </Button>
        </>
      )}
    </>
  );
}
