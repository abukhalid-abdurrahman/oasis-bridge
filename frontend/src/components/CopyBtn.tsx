"use client";

import { useState } from "react";
import Image from "next/image";
import { Button } from "./ui/button";
import { handleCopy } from "@/lib/scripts/script";

interface CopyBtnProps {
  address: string;
}

export default function CopyBtn({ address }: CopyBtnProps) {
  const [copied, setCopied] = useState(false);

  return (
    <div className="relative">
      <Button
        variant='empty' size='icon'
        className="flex justify-center gap-2 bg-gray w-[48px] h-[48px] rounded-xl items-center 
        aspect-square sm:w-[46px] sm:h-[46px] hover:bg-darkGray transition-all"
        onClick={() => {
          handleCopy(address, setCopied)
        }}
      >
        <Image src="/copy.svg" alt="copy" width={22} height={22} className="xxs:w-5" />
      </Button>

      {/* Всплывающее уведомление */}
      {copied && (
        <span className="absolute -top-8 left-1/2 -translate-x-1/2 bg-black text-white text-xs px-2 py-1 rounded-md opacity-90 transition">
          Copied
        </span>
      )}
    </div>
  );
}
