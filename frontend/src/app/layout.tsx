import type { Metadata } from "next";
import { DM_Sans } from "next/font/google";
import "./globals.css";
import TanstackProvider from "@/providers/TanstackProvider";

const dmSans = DM_Sans({
  weight: ["400", "600", '700'],
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
        style={{ backgroundImage: `url(/bg3.png)`, backgroundSize: "cover" }}
        className={`${dmSans.className} antialiased min-h-screen`}
      >
        <TanstackProvider>
          <main className="main w-full">
            {children}
          </main>
        </TanstackProvider>
      </body>
    </html>
  );
}
