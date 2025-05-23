"use client";

import { useEffect, useState } from "react";
import { useUserStore } from "@/store/useUserStore";
import Link from "next/link";
import { useRouter } from "next/navigation";
import Image from "next/image";
import { Button, buttonVariants } from "./ui/button";
import { useWalletStore } from "@/store/useWalletStore";
import Cookies from 'js-cookie'

export default function HeaderBtns() {
  const router = useRouter();
  const { user, logout, setUser } = useUserStore();
  const { disconnectWallet } = useWalletStore();
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
    setLoading(false);
  }, [setUser]);
  useEffect(() => {
    if (!Cookies.get('oasisToken')) {
      setUser(null)
    }
  }, [Cookies.get('oasisToken')])

  if (loading) return null;

  return (
    <>
      {!user ? (
        <Link
          href="?signin=true"
          className={buttonVariants({ variant: "gray", size: "default" })}
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
              disconnectWallet();
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
