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
  useNftChangesMultiple,
  useNftMultiple,
  useNfts,
} from "@/requests/getRequests";
import Loading from "@/components/Loading";
import { RwasReq } from "@/lib/types";
import _ from "lodash";
import Link from "next/link";
import Filters from "./Filters";

const nftsExample = [
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
  {
    image: "/nft.avif",
    title: "Shiza Quick",
    description:
      "Shiza Quick is a digital artist known for her vibrant and surreal artwork.",
    network: "Solana",
    price: {
      value: "3.12 SOL",
      secondValue: "0.25%",
    },
    assetType: "Automobiles",
    geolocation: "USA",
    priceChangePercentage: {
      value: "4.00%",
      secondValue: "0.12%",
    },
  },
];

export default function NFTTable() {
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

  const { data: nfts, isFetching: nftsFetching } = useNfts(reqParams);
  const { data: nftMultiple, isFetching: nftMultipleFetching } =
    useNftMultiple(tokenIds);
  const { data: nftChangesMultiple, isFetching: nftChangesMultipleFetching } =
    useNftChangesMultiple(tokenIds);

  useEffect(() => {
    if (nfts) {
      const getAlltokenIds = (nfts: any) => {
        return nfts?.data?.data.map((nft: any) => nft.tokenId);
      };
      setTokenIds(getAlltokenIds(nfts));
    }
  }, [nfts]);

  const fullNfts = useMemo(() => {
    const normalize = (arr: any[]) => {
      return arr
        .filter(Boolean)
        .map((item) => ({
          ...item,
          tokenId: item?.tokenId ?? item?.rwaTokenId,
        }))
        .filter((item) => item.tokenId !== undefined);
    };

    const base = normalize(nftMultiple.map((nft) => nft?.data));
    const changes = normalize(
      nftChangesMultiple.map((nft) => nft?.data?.[nft.data.length - 1])
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
  }, [nfts, nftMultiple, nftChangesMultiple]);

  return (
    <div>
      <div className="w-full mb-5 flex justify-end text-sm">
        <Filters reqParams={reqParams} setReqParams={setReqParams} />
      </div>
      {nftMultipleFetching.some((item) => item === true) ||
      nftChangesMultipleFetching.some((item) => item === true) ||
      nftsFetching ? (
        <Loading
          className="flex justify-center mt-14"
          classNameLoading="!border-white !border-r-transparent !w-14 !h-14"
        />
      ) : (
        <>
          {fullNfts.length > 0 ? (
            <Table className="min-w-[965px]">
              <TableHeader>
                <TableRow className="hover:bg-transparent border-primary">
                  <TableHead colSpan={2} className="w-[100px]">
                    NFT
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
                {fullNfts.map((nft: any) => {
                  return (
                    <TableRow
                      key={nft.tokenId}
                      className="border-primary hover:bg-transparent"
                    >
                      <TableCell colSpan={2} className="font-medium py-3">
                        <div className="">
                          <Link
                            className="flex gap-3 items-center"
                            href={`/nft/${nft.tokenId}`}
                          >
                            <Image
                              src={
                                nft.image !== "string" ? nft.image : "/nft.avif"
                              }
                              alt={nft.title}
                              width={50}
                              height={50}
                              className="rounded-md"
                            />
                            <div className="flex flex-col">
                              <p className="p">{nft.title}</p>
                              <p className="text-textGray">
                                {shortDescription(nft.assetDescription)}
                              </p>
                            </div>
                          </Link>
                        </div>
                      </TableCell>
                      <TableCell className="text-right">
                        {nft.network}
                      </TableCell>
                      <TableCell className="text-right">
                        {nft.price}
                        {/* <span className="text-red-600 text-xs flex items-center justify-end">
                        <span className="inline">
                          <ChevronDown size={15} />
                        </span>
                        {nft.price.secondValue}
                      </span> */}
                      </TableCell>
                      <TableCell className="text-right">
                        {nft.assetType}
                      </TableCell>
                      {/* <TableCell className="text-right">
                      {nft.geolocation}
                    </TableCell> */}
                      <TableCell className="text-right">
                        {nft?.oldPrice - nft?.price || (
                          <>
                            <p className="p opacity-60">---</p>
                          </>
                        )}
                        {nft?.oldPrice &&
                          (() => {
                            const diff =
                              ((nft.price - nft.oldPrice) / nft.oldPrice) * 100;
                            const isPositive = diff >= 0;
                            const percentage = Math.abs(diff).toFixed(2) + "%";

                            return (
                              <span
                                className={`flex items-center text-xs justify-end ${
                                  isPositive ? "text-green-600" : "text-red-600"
                                }`}
                              >
                                {isPositive ? (
                                  <ChevronUp size={15} className="inline" />
                                ) : (
                                  <ChevronDown size={15} className="inline" />
                                )}
                                {percentage}
                              </span>
                            );
                          })()}
                      </TableCell>
                      <TableCell className="text-right space-x-2">
                        <Link
                          href={`/nft/${nft.tokenId}`}
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
              <h3 className="h3 text-center mt-20 opacity-60">There are no NFTs available yet.</h3>
            </>
          )}
        </>
      )}
      {nfts?.data?.totalPages > 1 && (
        <PaginationButtons
          className="mt-10"
          pages={nfts.data.totalPages}
          currentPage={reqParams.pageNumber}
          setCurrentPage={setReqParams}
        />
      )}
    </div>
  );
}
