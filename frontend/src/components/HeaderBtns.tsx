"use client";

import { useEffect, useState } from "react";
import { useUserStore } from "@/store/useUserStore";
import Link from "next/link";
import Button from "./Button";
import { useRouter } from "next/navigation";
import Image from "next/image";

export default function HeaderBtns() {
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
        <Link href="?signin=true" className="btn btn-sm">
          Sign In
        </Link>
      ) : (
        <>
          <Link href="/profile" className="btn btn-sm flex gap-[8px] sm:gap-[5px]">
            <Image 
              src='/profile.svg'
              alt={user.UserName}
              width={21}
              height={21}
            />
            {user.UserName}
          </Link>
          <Button className="px-[10px]" onClick={() => {
            logout();
            localStorage.removeItem("user");
            router.push('/');
          }}>
            <Image 
              src='/logout.svg'
              alt='Logout'
              width={24}
              height={24}
            />
          </Button>
        </>
      )}
    </>
  );
}
