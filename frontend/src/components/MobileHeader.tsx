"use client";

import Link from "next/link";
import { useEffect, useRef, useState } from "react";
import { ArrowLeftRight, ChartCandlestick, ChevronsUp, Menu, X } from "lucide-react";
import { Button, buttonVariants } from "./ui/button";
import SignInModal from "./SignInModal";
import SignUpModal from "./SignUpModal";
import MobileHeaderBtns from "./MobileHeaderBtns";
import { useRouter } from "next/navigation";
import { useUserStore } from "@/store/useUserStore";
import Image from "next/image";
import WalletConnect from "./WalletConnect";

export default function MobileHeader({
  signin,
  signup,
}: {
  signin?: string;
  signup?: string;
}) {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);
  const router = useRouter();
  const { user, logout, setUser } = useUserStore();

  useEffect(() => {
    const storedUser = localStorage.getItem("user");
    if (storedUser) {
      setUser(JSON.parse(storedUser));
    }
  }, [setUser]);

  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (
        isOpen &&
        menuRef.current &&
        !menuRef.current.contains(event.target as Node)
      ) {
        setIsOpen(false);
      }
    };

    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [isOpen]);

  return (
    <>
      <header className="justify-between items-center py-4 w-full text-white hidden md:flex">
        <Link href="/" className="text-lg font-semibold">
          Quantum Street Bridge
        </Link>
        <button onClick={() => setIsOpen(true)} aria-label="Open menu">
          <Menu className="w-6 h-6" />
        </button>
      </header>

      {/* Sidebar / Drawer */}
      <div
        className={`fixed -left-full top-0 bottom-0 z-50 bg-black/60 backdrop-blur-sm transition-all ${
          isOpen && "!right-0 !left-0"
        }`}
      >
        <div
          ref={menuRef}
          className={`absolute -left-full top-0 h-full w-4/5 bg-white text-black py-4 px-2 shadow-xl transition-all
          ${isOpen && "!left-0"}`}
        >
          <nav className="flex flex-col gap-2">
            <MobileHeaderBtns />
            <WalletConnect className="w-full !justify-start" />
            <div
              className={`${buttonVariants({
                variant: "empty",
                size: "default",
              })} flex-col h-auto gap-3 !px-5 !py-3`}
            >
              <p className="p w-full mb-2 font-semibold">Sections</p>
              <Link href="/" className="w-full flex gap-2 items-center">
                <ArrowLeftRight size={5} strokeWidth={1} className="mr-1" />
                Swap
              </Link>
              <Link
                href="/nft"
                className="w-full flex gap-2 items-center"
              >
                <ChartCandlestick size={5} strokeWidth={1} className="mr-1" />
                NFT Market
              </Link>
              <Link
                href={`${user ? "/create-nft" : "?signin=true"}`}
                className="w-full flex gap-2 items-center"
              >
                <ChevronsUp size={5} strokeWidth={1} className="mr-1" />
                Create NFT
              </Link>
            </div>
          </nav>
          {user && (
            <Button
              variant="gray"
              size="default"
              className="mt-2 !justify-between w-full"
              onClick={() => {
                logout();
                localStorage.removeItem("user");
                router.push("/");
              }}
            >
              Logout
              <Image src="/logout.svg" alt="Logout" width={20} height={20} />
            </Button>
          )}
        </div>
      </div>

      {signin && <SignInModal />}
      {signup && <SignUpModal />}
    </>
  );
}
