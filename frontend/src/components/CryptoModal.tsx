"use client";

import { CryptoOption, SelectedCrypto } from "@/lib/cryptoOptions";
import Modal from "./Modal";
import CryptoItem from "./CryptoItem";
import { useForm } from "react-hook-form";
import Image from "next/image";
import { useNetworks } from "@/requests/getRequests";
import { useEffect } from "react";
import { useUserStore } from "@/store/useUserStore";

interface CryptoModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSelect: (crypto: SelectedCrypto) => void;
  selectedFrom: SelectedCrypto;
  selectedTo: SelectedCrypto;
  selectedNetwork: CryptoOption;
  selectNetwork: (crypto: any) => void;
  setSelectedTo: any,
  setSelectedFrom: any
}

export default function CryptoModal({
  isOpen,
  onClose,
  onSelect,
  selectedNetwork,
  selectNetwork,
}: CryptoModalProps) {
  const { register } = useForm();
  const user = useUserStore((state) => state.user)
  const { data } = useNetworks();

  useEffect(() => {
    if (data) {
      selectNetwork(data.data.data[1])
    }
  }, [data])

  if (!isOpen) return null;

  return (
    <Modal isNonUrlModal={true} onCloseFunc={onClose}>
      <div className="flex flex-col justify-center h-full">
        <ul className="flex space-x-4">
          {data?.data?.data.map((network: any) => (
            <li key={network.id}>
              <CryptoItem
                image={
                  network.name === "Solana"
                    ? "/SOL.png"
                    : network.name === "Radix"
                    ? "/XRD.png"
                    : ""
                }
                crypto={network.name}
                className="cursor-pointer hover:bg-darkGray transition-all"
                onClick={() => selectNetwork(network)}
              />
            </li>
          ))}
        </ul>
        {/* <form className="mt-6 flex items-center bg-gray rounded-xl pl-5">
          <label htmlFor="search" className="block">
            <Image src="/search.svg" alt="search" width={20} height={20} />
          </label>
          <input
            className="input w-full"
            type="text"
            placeholder="Search"
            {...register("search", {
              required: "This field is required",
            })}
          />
        </form> */}
        <div className="rounded-xl bg-gray mt-[10px] p-5">
          <ul>
            {data?.data?.data
              .find(({ name }: { name: string }) => name === selectedNetwork.name)
              ?.tokens.map((token: string, i: number) => (
                <li
                  className="flex gap-3 items-center text-sm cursor-pointer text-textGray"
                  key={i}
                  onClick={() => onSelect({
                    network: selectedNetwork.name,
                    token: token
                  })}
                >
                  <Image
                    src={`/${token}.png`}
                    alt={token}
                    width={20}
                    height={20}
                  />
                  <p className="p-sm">{token}</p>
                </li>
              )) || <p className="p-sm text-textGray">No tokens available</p>}
          </ul>
        </div>
      </div>
    </Modal>
  );
}
