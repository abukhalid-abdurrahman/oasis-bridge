"use client";

import React, { useEffect, useMemo, useState } from "react";
import {
  Table,
  TableBody,
  TableCaption,
  TableCell,
  TableFooter,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import Image from "next/image";
import {
  calculatePercentageDifference,
  shortDescription,
} from "@/lib/scripts/script";
import { ChevronDown, ChevronUp } from "lucide-react";
import { Button, buttonVariants } from "@/components/ui/button";
import { PaginationButtons } from "@/components/PaginationButtons";
import {
  useRwaChangesMultiple,
  useRwaMultiple,
  useRwas,
} from "@/requests/getRequests";
import Loading from "@/components/Loading";
import { RwasReq } from "@/lib/types";
import _ from "lodash";
import Link from "next/link";
import Filters from "./Filters";

export default function RWATable() {
  const [tokenIds, setTokenIds] = useState<string[]>([]);
  const [reqParams, setReqParams] = useState<RwasReq>({
    assetType: null,
    priceMin: null,
    priceMax: null,
    sortBy: null,
    sortOrder: null,
    pageSize: 10,
    pageNumber: 1,
  });

  const { data: rwas, isFetching: rwasFetching } = useRwas(reqParams);
  const { data: rwaMultiple, isFetching: rwaMultipleFetching } =
    useRwaMultiple(tokenIds);
  const { data: rwaChangesMultiple, isFetching: rwaChangesMultipleFetching } =
    useRwaChangesMultiple(tokenIds);

  useEffect(() => {
    if (rwas) {
      const getAlltokenIds = (rwas: any) => {
        return rwas?.data?.data.map((rwa: any) => rwa.tokenId);
      };
      setTokenIds(getAlltokenIds(rwas));
    }
  }, [rwas]);

  const fullRwas = useMemo(() => {
    const normalize = (arr: any[]) => {
      return arr
        .filter(Boolean)
        .map((item) => ({
          ...item,
          tokenId: item?.tokenId ?? item?.rwaTokenId,
        }))
        .filter((item) => item.tokenId !== undefined);
    };

    const base = normalize(rwaMultiple.map((rwa) => rwa?.data));
    const changes = normalize(
      rwaChangesMultiple.map((rwa) => rwa?.data?.[rwa.data.length - 1])
    );

    const combinedMap = new Map<string, any>();

    for (const item of [...base, ...changes]) {
      const existing = combinedMap.get(item.tokenId);
      if (existing) {
        combinedMap.set(item.tokenId, {
          ...existing,
          ...item,
        });
      } else {
        combinedMap.set(item.tokenId, item);
      }
    }

    return Array.from(combinedMap.values());
  }, [rwas, rwaMultiple, rwaChangesMultiple]);

  return (
    <div>
      <div className="w-full mb-5 flex justify-end text-sm">
        <Filters reqParams={reqParams} setReqParams={setReqParams} />
      </div>
      {rwaMultipleFetching.some((item) => item === true) ||
      rwaChangesMultipleFetching.some((item) => item === true) ||
      rwasFetching ? (
        <Loading
          className="flex justify-center mt-14"
          classNameLoading="!border-white !border-r-transparent !w-14 !h-14"
        />
      ) : (
        <>
          {fullRwas.length > 0 ? (
            <Table className="min-w-[965px]">
              <TableHeader>
                <TableRow className="hover:bg-transparent border-primary">
                  <TableHead colSpan={2} className="w-[100px]">
                    RWA
                  </TableHead>
                  <TableHead className="text-right">Network</TableHead>
                  <TableHead className="text-right">Price</TableHead>
                  <TableHead className="text-right">Asset Type</TableHead>
                  {/* <TableHead className="text-right">Geolocation</TableHead> */}
                  <TableHead className="text-right">Price Change (%)</TableHead>
                  <TableHead className="text-right"></TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {fullRwas.map((rwa: any) => {
                  return (
                    <TableRow
                      key={rwa.tokenId}
                      className="border-primary hover:bg-transparent"
                    >
                      <TableCell colSpan={2} className="font-medium py-3">
                        <div className="">
                          <Link
                            className="flex gap-3 items-center"
                            href={`/rwa/${rwa.tokenId}`}
                          >
                            <Image
                              src={
                                rwa.image !== "string" ? rwa.image : "/nft.avif"
                              }
                              alt={rwa.title}
                              width={50}
                              height={50}
                              className="rounded-md"
                            />
                            <div className="flex flex-col">
                              <p className="p">{rwa.title}</p>
                              <p className="text-textGray">
                                {shortDescription(rwa.assetDescription)}
                              </p>
                            </div>
                          </Link>
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        {rwa.network}
                      </TableCell>
                      <TableCell className="text-right">
                        {rwa.price}
                        {/* <span className="text-red-600 text-xs flex items-center justify-end">
                        <span className="inline">
                          <ChevronDown size={15} />
                        </span>
                        {rwa.price.secondValue}
                      </span> */}
                      </TableCell>
                      <TableCell className="text-right">
                        {rwa.assetType}
                      </TableCell>
                      {/* <TableCell className="text-right">
                      {rwa.geolocation}
                    </TableCell> */}
                      <TableCell className="text-right">
                        {rwa?.oldPrice - rwa?.price || (
                          <>
                            <p className="p opacity-60">---</p>
                          </>
                        )}
                        {rwa?.oldPrice &&
                          (() => {
                            const diff =
                              ((rwa.price - rwa.oldPrice) / rwa.oldPrice) * 100;
                            const isPositive = diff > 0;
                            const isNeutral = diff === 0
                            const percentage = Math.abs(diff).toFixed(2) + "%";

                            return (
                              <span
                                className={`flex items-center text-xs justify-end ${
                                  isPositive
                                    ? "text-green-500"
                                    : isNeutral
                                    ? "text-textGray"
                                    : "text-red-600"
                                }`}
                              >
                                {isPositive && (
                                  <ChevronUp size={15} className="inline" />
                                )}
                                {(!isPositive && !isNeutral) && (
                                  <ChevronDown size={15} className="inline" />
                                )}
                                {percentage}
                              </span>
                            );
                          })()}
                      </TableCell>
                      <TableCell className="text-right space-x-2">
                        <Link
                          href={`/rwa/${rwa.tokenId}`}
                          className={buttonVariants({
                            variant: "gray",
                            size: "sm",
                          })}
                        >
                          Details
                        </Link>
                        <Button variant="green" size="sm">
                          Purchase
                        </Button>
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          ) : (
            <>
              <h3 className="h3 text-center mt-20 opacity-60">There are no RWAs available yet.</h3>
            </>
          )}
        </>
      )}
      {rwas?.data?.totalPages > 1 && (
        <PaginationButtons
          className="mt-10"
          pages={rwas.data.totalPages}
          currentPage={reqParams.pageNumber}
          setCurrentPage={setReqParams}
        />
      )}
    </div>
  );
}
