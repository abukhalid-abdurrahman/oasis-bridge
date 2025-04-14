import type { Metadata } from "next";
import { DM_Sans } from "next/font/google";
import "./globals.css";
import TanstackProvider from "@/providers/TanstackProvider";
import bg from "@/../public/bg.png";

const dmSans = DM_Sans({
  weight: ["400", "600"],
  subsets: ["latin"],
});

export const metadata: Metadata = {
  title: "Quantum Street Bridge",
  description:
    "Quantum Street Bridge is a decentralized bridge that allows users to swap between different cryptocurrencies.",
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
        className={`${dmSans.className} antialiased sm:px-5 min-h-screen`}
      >
        <TanstackProvider>
          <main className="main max-w-[1200px] min-h-screen mx-auto w-full">
            {children}
          </main>
        </TanstackProvider>
      </body>
    </html>
  );
}
