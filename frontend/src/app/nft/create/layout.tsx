import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Create NFT",
};

export default function CreateNftLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <div className="">{children}</div>;
}
