import { Dispatch, SetStateAction, useMemo, useState } from "react";
import { Table, TableBody, TableCell, TableRow } from "./ui/table";
import {
  tokenizationFieldsAutomobiles,
  tokenizationFieldsBase,
  tokenizationFieldsRealEstate,
} from "@/lib/helpers/tokenizationFields";
import Link from "next/link";
import { Button } from "./ui/button";
import Image from "next/image";
import {
  handleCopyAlt,
  shortAddress,
  shortDescription,
} from "@/lib/scripts/script";

const fieldsMeta = [
  ...tokenizationFieldsBase,
  ...tokenizationFieldsAutomobiles,
  ...tokenizationFieldsRealEstate,
  { name: "insuranceStatus", placeholder: "Insurance Status" },
  { name: "mintAccount", placeholder: "Mint Account" },
  { name: "transactionHash", placeholder: "Transaction Hash" },
  { name: "metadata", placeholder: "Metadata" },
];

interface AllRwaDataProps {
  data: any;
  setIsOpen: Dispatch<SetStateAction<boolean>>;
}
export default function AllRwaData({ data, setIsOpen }: AllRwaDataProps) {
  const [copiedMap, setCopiedMap] = useState<Record<string, boolean>>({});
  const sortedData = useMemo(() => {
    if (!data) return;

    const excludedKeys = [
      "tokenId",
      "ownerEmail",
      "ownerUsername",
      "ownerContact",
    ];

    return Object.fromEntries(
      Object.entries(data).filter(([key]) => !excludedKeys.includes(key))
    );
  }, [data]);

  const getPlaceholder = (key: string) => {
    const field = fieldsMeta.find((item) => item.name === key);
    return field?.placeholder || key;
  };

  return (
    <div className="fixed inset-0 z-[1000000] bg-backgroundWebsite">
      <div className="w-full h-full overflow-y-auto p-10 md:p-5">
        <div className="max-w-[1200px] mx-auto">
          <div className="flex justify-between items-center mb-10 md:mb-5">
            <h2 className="h2 text-white">All RWA information</h2>
            <Button
              variant="empty"
              size="icon"
              onClick={() => setIsOpen(false)}
            >
              <Image src="/close.svg" alt="Close" width={12} height={12} />
            </Button>
          </div>

          <Table className="text-white">
            <TableBody>
              {sortedData &&
                Object.entries(sortedData).map(
                  ([key, value]: [string, any]) => (
                    <TableRow
                      key={key}
                      className="hover:bg-transparent border-textGray relative md:flex md:gap-2 md:justify-between"
                    >
                      <TableCell className="px-0">
                        {getPlaceholder(key)}
                      </TableCell>
                      {key === "geolocation" ? (
                        <TableCell className="px-0 text-right">
                          latitude: {value.latitude} longitude:{" "}
                          {value.longitude}
                        </TableCell>
                      ) : (
                        <>
                          {typeof value === "string" &&
                          value.includes("http") ? (
                            <TableCell className="px-0 text-right">
                              <div>
                                <Link
                                  target="_blank"
                                  className="px-0 text-right"
                                  href={value}
                                >
                                  {shortDescription(value, 25)}
                                </Link>
                              </div>
                            </TableCell>
                          ) : (
                            <>
                              {key === "mintAccount" ||
                              key === "transactionHash" ? (
                                <TableCell
                                  onClick={() =>
                                    handleCopyAlt(key, value, setCopiedMap)
                                  }
                                  className="px-0 text-right cursor-pointer block"
                                >
                                  {shortAddress(value)}
                                  {copiedMap[key] && (
                                    <span className="absolute right-0 -top-5 bg-white text-black text-xs px-2 py-1 rounded-md opacity-90 transition">
                                      Copied
                                    </span>
                                  )}
                                </TableCell>
                              ) : (
                                <TableCell className="px-0 text-right">
                                  {value}
                                </TableCell>
                              )}
                            </>
                          )}
                        </>
                      )}
                    </TableRow>
                  )
                )}
            </TableBody>
          </Table>
        </div>
      </div>
    </div>
  );
}
