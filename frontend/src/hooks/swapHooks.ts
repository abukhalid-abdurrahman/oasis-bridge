import { useEffect, useState } from "react";
import { CryptoOption, defaultSelectedFrom, defaultSelectedNetwork, defaultSelectedTo, SelectedCrypto } from "@/lib/cryptoOptions";

export function useSwap() {
  const [selectedNetwork, setSelectedNetwork] = useState();
  const [selectedFrom, setSelectedFrom] = useState<SelectedCrypto>(defaultSelectedFrom);
  const [selectedTo, setSelectedTo] = useState<SelectedCrypto>(defaultSelectedTo);
  const [isCryptoModalOpen, setIsCryptoModalOpen] = useState(false);
  const [isStatusModalOpen, setIsStatusModalOpen] = useState(false);
  const [statuses, setStatuses] = useState<string[]>([]);
  const [currentTarget, setCurrentTarget] = useState<"from" | "to" | null>(null);

  const openCryptoModal = (target: "from" | "to") => {
    setCurrentTarget(target);
    setIsCryptoModalOpen(true);
  };

  const closeCryptoModal = () => {
    setIsCryptoModalOpen(false);
    setCurrentTarget(null);
  };

  const selectNetwork = (crypto: any) => {
    setSelectedNetwork(crypto);
  }

  const selectCrypto = (crypto: SelectedCrypto) => {
    if (currentTarget === "from") {
      setSelectedFrom(crypto);
    } else if (currentTarget === "to") {
      setSelectedTo(crypto);
    }
    closeCryptoModal();
  };

  const simulateStatuses = () => {
    const steps = ["Connecting to network...", "Validating transaction...", "Processing swap...", "Swap completed!"];
    setStatuses([]);
    steps.forEach((step, index) => {
      setTimeout(() => {
        setStatuses((prev) => [...prev, step]);
      }, index * 1500);
    });
  };

  const closeStatusModal = () => {
    setIsStatusModalOpen(false);
    setStatuses([]);
  };

  return {
    selectNetwork,
    selectedNetwork,
    selectedFrom,
    setSelectedFrom,
    setSelectedTo,
    selectedTo,
    isCryptoModalOpen,
    isStatusModalOpen,
    statuses,
    openCryptoModal,
    closeCryptoModal,
    selectCrypto,
    simulateStatuses,
    closeStatusModal,
  };
}
