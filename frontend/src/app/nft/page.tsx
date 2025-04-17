import Header from "@/components/Header";
import { Button } from "@/components/ui/button";
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
import { shortDescription } from "@/lib/scripts/script";
import { searchParams } from "@/lib/types";
import { ChevronDown, ChevronUp } from "lucide-react";
import Image from "next/image";

const nfts = [
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

export default async function page({
  searchParams,
}: {
  searchParams: Promise<searchParams>;
}) {
  return (
    <>
      <Header searchParams={searchParams} />
      <div className="max-w-[1200px] mx-auto md:py-10 xl:px-5 md:!px-0 text-white">
        <div className="mx-auto">
          <Table className="min-w-[965px]">
            <TableHeader>
              <TableRow className="hover:bg-transparent border-primary">
                <TableHead colSpan={2} className="w-[100px]">
                  NFT
                </TableHead>
                <TableHead className="text-right">Network</TableHead>
                <TableHead className="text-right">Price</TableHead>
                <TableHead className="text-right">Asset Type</TableHead>
                <TableHead className="text-right">Geolocation</TableHead>
                <TableHead className="text-right">Price Change (%)</TableHead>
                <TableHead className="text-right"></TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {nfts.map((nft, i) => (
                <TableRow
                  key={i}
                  className="border-primary hover:bg-transparent"
                >
                  <TableCell colSpan={2} className="font-medium py-3">
                    <div className="flex gap-3 items-center">
                      <Image
                        src={nft.image}
                        alt={nft.title}
                        width={50}
                        height={50}
                        className="rounded-md"
                      />
                      <div className="flex flex-col">
                        <p className="p">{nft.title}</p>
                        <p className="text-textGray">
                          {shortDescription(nft.description)}
                        </p>
                      </div>
                    </div>
                  </TableCell>
                  <TableCell className="text-right">{nft.network}</TableCell>
                  <TableCell className="text-right">
                    {nft.price.value}
                    <span className="text-red-600 text-xs flex items-center justify-end">
                      <span className="inline">
                        <ChevronDown size={15} />
                      </span>
                      {nft.price.secondValue}
                    </span>
                  </TableCell>
                  <TableCell className="text-right">{nft.assetType}</TableCell>
                  <TableCell className="text-right">
                    {nft.geolocation}
                  </TableCell>
                  <TableCell className="text-right">
                    {nft.priceChangePercentage.value}
                    <span className="text-green-600 flex items-center text-xs justify-end">
                      <span className="inline">
                        <ChevronUp size={15} />
                      </span>
                      {nft.priceChangePercentage.secondValue}
                    </span>
                  </TableCell>
                  <TableCell className="text-right space-x-2">
                    <Button variant="gray" size="sm">
                      Details
                    </Button>
                    <Button variant="green" size="sm">
                      Purchase
                    </Button>
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </div>
      </div>
    </>
  );
}
