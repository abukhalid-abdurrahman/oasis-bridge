import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "NFT Market",
};

export default function NftMarketLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="w-full min-h-screen bg-backgroundWebsite">
      <div className="max-w-[1200px] min-h-screen mx-auto w-full md:!px-5">
        {children}
      </div>
    </div>
  );
}
