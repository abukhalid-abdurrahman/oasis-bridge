"use client";

import { Dispatch, SetStateAction, useEffect, useRef, useState } from "react";
import FiltersForm from "./FiltersForm";
import { Button } from "@/components/ui/button";
import { Funnel } from "lucide-react";
import Image from "next/image";

interface FiltersProps {
  reqParams: any;
  setReqParams: Dispatch<SetStateAction<any>>;
}

export default function Filters({ reqParams, setReqParams }: FiltersProps) {
  const [isOpen, setIsOpen] = useState(false);
  const menuRef = useRef<HTMLDivElement>(null);

  return (
    <>
      <div className="lg:hidden">
        <FiltersForm reqParams={reqParams} setReqParams={setReqParams} />
      </div>

      <Button
        variant="gray"
        size="sm"
        className="hidden lg:flex"
        onClick={() => setIsOpen(true)}
        aria-label="Open menu"
      >
        <Funnel /> Filters
      </Button>
      {/* Sidebar / Drawer */}
      <div
        className={`fixed -right-full top-0 bottom-0 z-50 bg-black/60 backdrop-blur-sm transition-all ${
          isOpen && "!left-0 !right-0"
        }`}
      >
        <div
          ref={menuRef}
          className={`absolute -right-full top-0 h-full w-4/5 bg-white text-black py-4 px-5 shadow-xl transition-all
          ${isOpen && "!right-0"}`}
        >
          <div className="flex justify-between items-center mb-8">
            <h3 className="h3">Filters</h3>
            <Button
              variant="ghost"
              size="icon"
              onClick={() => setIsOpen(false)}
              aria-label="Close menu"
            >
              <Image src="/close.svg" alt="Close" width={12} height={12} />
            </Button>
          </div>
          <FiltersForm
            reqParams={reqParams}
            setReqParams={setReqParams}
            setIsFiltersOpen={setIsOpen}
          />
        </div>
      </div>
    </>
  );
}
