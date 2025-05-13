"use client";

import {
  Pagination,
  PaginationContent,
  PaginationEllipsis,
  PaginationItem,
  PaginationLink,
  PaginationNext,
  PaginationPrevious,
} from "@/components/ui/pagination";
import { getVisiblePages } from "@/lib/scripts/script";
import { Dispatch, SetStateAction } from "react";

interface PaginationButtonsProps {
  className?: string;
  pages: number;
  currentPage: number;
  setCurrentPage: Dispatch<SetStateAction<any>>;
}

export function PaginationButtons({
  className,
  pages,
  currentPage,
  setCurrentPage,
}: PaginationButtonsProps) {
  const visiblePages = getVisiblePages(currentPage, pages);

  const setPage = (i: number) => {
    setCurrentPage((prevState: any) => {
      return {
        ...prevState,
        pageNumber: i
      }
    });
  };

  const previuosPage = () => {
    setCurrentPage((prevState: any) => {
      if (prevState.pageNumber <= 1) {
        return prevState
      } else {
        return {
          ...prevState,
          pageNumber: prevState.pageNumber -= 1
        }
      }
    });
  };

  const nextPage = () => {
    setCurrentPage((prevState: any) => {
      if (prevState.pageNumber >= pages) {
        return prevState
      } else {
        return {
          ...prevState,
          pageNumber: prevState.pageNumber += 1
        }
      }
    });
  };

  return (
    <Pagination className={className}>
      <PaginationContent>
        <PaginationItem>
          <PaginationPrevious onClick={previuosPage} href="#" />
        </PaginationItem>

        {visiblePages.map((p, i) => (
          <PaginationItem key={i}>
            {p === "..." ? (
              <PaginationEllipsis />
            ) : (
              <PaginationLink
                onClick={() => setPage(p)}
                href="#"
                isActive={p === currentPage}
              >
                {p}
              </PaginationLink>
            )}
          </PaginationItem>
        ))}

        <PaginationItem>
          <PaginationNext onClick={nextPage} href="#" />
        </PaginationItem>
      </PaginationContent>
    </Pagination>
  );
}