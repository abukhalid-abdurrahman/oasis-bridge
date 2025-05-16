"use client";

import Loading from "@/components/Loading";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { shortAddress } from "@/lib/scripts/script";
import { useRwaPurchaseHistory } from "@/requests/getRequests";
import { format } from "date-fns";

interface SellBuyDataProps {
  tokenId: string;
}

export default function SellBuyData({ tokenId }: SellBuyDataProps) {
  const { data, isFetching } = useRwaPurchaseHistory(tokenId);

  return (
    <div className="mt-16 text-white">
      <h3 className="h3">Sell / Buy History</h3>
      <Table className="mt-5">
        <TableHeader>
          <TableRow className="hover:bg-transparent border-textGray">
            <TableHead className="uppercase tracking-wider text-sm px-0">
              Type
            </TableHead>
            <TableHead className="uppercase tracking-wider text-sm px-0">
              Price
            </TableHead>
            <TableHead className="uppercase tracking-wider text-sm px-0">
              Token
            </TableHead>
            <TableHead className="uppercase tracking-wider text-sm px-0">
              Buyer
            </TableHead>
            <TableHead className="uppercase tracking-wider text-sm px-0 text-right">
              Date
            </TableHead>
          </TableRow>
        </TableHeader>
        <TableBody>
          {!data?.data.length ? (
            <TableRow className="hover:bg-transparent">
              <TableCell colSpan={5} className="text-center py-8 px-0 text-textGray">
                No purchases or sales yet.
              </TableCell>
            </TableRow>
          ) : (
            data.data.map((row: any, i: number) => (
              <TableRow key={i} className="hover:bg-transparent">
                <TableCell
                  className={`px-0 ${
                    row.type === "Buy"
                      ? "text-rose-400"
                      : "text-emerald-400"
                  }`}
                >
                  Buy
                </TableCell>
                <TableCell className=" py-5 px-0">
                  {row.price}
                </TableCell>
                <TableCell className=" py-5 px-0">zBTC</TableCell>
                <TableCell className=" py-5 px-0">{shortAddress(row.buyerPublicKey)}</TableCell>
                <TableCell className="text-zinc-500 py-5 px-0 text-right">
                  {format(new Date(row.transactionDate), "yyyy-MM-dd")}
                </TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </div>
  );
}
