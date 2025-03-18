import type { Metadata } from "next";
import { DM_Sans } from "next/font/google";
import "./globals.css";
import TanstackProvider from "@/providers/TanstackProvider";
import bg from "@/../public/bg.png";

const dmSans = DM_Sans({
  weight: ["400", "700"],
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "OASIS Celestial Bridge",
  description:
    "OASIS Celestial Bridge is a decentralized bridge that allows users to swap between different cryptocurrencies.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en">
      <body
        style={{ backgroundImage: `url(/bg3.png)` }}
        className={`${dmSans.className} antialiased sm:px-5 h-screen`}
      >
        <TanstackProvider>
          <main className="main max-w-[512px] mx-auto py-[35px] h-full">
            {children}
          </main>
        </TanstackProvider>
      </body>
    </html>
  );
}
