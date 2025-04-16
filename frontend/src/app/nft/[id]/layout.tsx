import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "NFT",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="max-w-[1200px] min-h-screen mx-auto w-full">
      {children}
    </div>
  );
}
