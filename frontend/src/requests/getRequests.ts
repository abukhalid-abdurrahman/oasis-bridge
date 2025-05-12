import axiosInstance from "@/lib/axiosInstance";
import { API } from "@/lib/constants";
import { RwasReq } from "@/lib/types";
import { useQueries, useQuery } from "@tanstack/react-query";
import axios from "axios";

// Get for Exchange Rate
const getExchangeRate = async (fromToken: string, toToken: string) => {
  const res = await axios.get(`${API}/exchange-rate`, {
    headers: {
      "Content-Type": "application/json",
    },
    params: {
      fromToken: fromToken,
      toToken: toToken,
    },
  });
  return res.data;
};

export const useExchangeRate = (fromToken: string, toToken: string) => {
  return useQuery({
    queryKey: [fromToken, toToken, "exchange-rate"],
    queryFn: () => getExchangeRate(fromToken, toToken),
    refetchOnWindowFocus: false,
    refetchInterval: 300000,
    // enabled: !!token
  });
};

// Get for Transaction Status
const getTransactionStatus = async (transactionId: string) => {
  const res = await axios.get(`${API}/transaction-status`, {
    headers: {
      "Content-Type": "application/json",
    },
    params: {
      transactionId: transactionId,
    },
  });
  return res.data;
};

export const useTransactionStatus = (transactionId: string) => {
  return useQuery({
    queryKey: [transactionId, "transaction-id"],
    queryFn: () => getTransactionStatus(transactionId),
    refetchOnWindowFocus: false,
    // initialData: true
    enabled: false,
  });
};

// Get for Virtual Account
// const getVirtualAccount = async (fromNetwork: string, toNetwork: string) => {
//   const res = await axios.get(`${API}/virtual-account`, {
//     headers: {
//       "Content-Type": "application/json",
//     },
//     params: {
//       From: fromNetwork,
//       To: toNetwork
//     }
//   })
//   return res.data
// }

// export const useVirtualAccount = (fromNetwork: string, toNetwork: string) => {
//   return useQuery({
//     queryKey: [fromNetwork, toNetwork, 'virtual-account'],
//     queryFn: () => getVirtualAccount(fromNetwork, toNetwork),
//     refetchOnWindowFocus: false,
//     refetchOnReconnect: false,
//     refetchOnMount: false,
//     enabled: false
//   })
// }

// Get for User virtual Accounts
const getUserVirtualAccounts = async () => {
  const res = await axiosInstance.get(`/accounts/list`);
  return res.data;
};

export const useUserVirtualAccounts = (token: string) => {
  return useQuery({
    queryKey: [token, "user-accounts"],
    queryFn: () => getUserVirtualAccounts(),
    gcTime: 0,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

// Get for Chaecking Virtual Accaount Balance
const getVirtualAccountBalance = async (orderId: string) => {
  const res = await axiosInstance.get(`/orders/${orderId}/check-balance`);
  return res.data;
};

export const useVirtualAccountBalance = (
  orderId: string,
  completed: string | boolean
) => {
  return useQuery({
    queryKey: [orderId, "virtual-account-balance"],
    queryFn: () => getVirtualAccountBalance(orderId),
    gcTime: 0,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
    enabled: !!orderId && !!completed,
  });
};

// Get Networks
const getNetworks = async () => {
  const res = await axios.get(`${API}/networks`, {
    headers: {
      "Content-Type": "application/json",
    },
  });
  return res.data;
};

export const useNetworks = () => {
  return useQuery({
    queryKey: ["networks"],
    queryFn: () => getNetworks(),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

// Get all NFTs
const getNfts = async (reqParams: RwasReq) => {
  const res = await axiosInstance.get("/rwa", {
    params: {
      AssetType: reqParams.assetType,
      PriceMin: reqParams.priceMin,
      PriceMax: reqParams.priceMax,
      SortBy: reqParams.sortBy,
      SortOrder: reqParams.sortOrder,
      PageSize: reqParams.pageSize,
      PageNumber: reqParams.pageNumber,
    },
  });
  return res.data;
};

export const useNfts = (reqParams: any) => {
  return useQuery({
    queryKey: ["rwas", reqParams],
    queryFn: () => getNfts(reqParams),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

// Get a specific NFT
const getNft = async (tokenId: string) => {
  const res = await axiosInstance.get(`/rwa/${tokenId}`);
  return res.data;
};

export const useNft = (tokenId: string) => {
  return useQuery({
    queryKey: ["nft", tokenId],
    queryFn: () => getNft(tokenId),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

// Example with multiple request
export const useNftMultiple = (tokenIds: string[]) => {
  return useQueries({
    queries: tokenIds.map((id) => ({
      queryKey: ["nft", "multiple", id],
      queryFn: () => getNft(id),
      refetchOnWindowFocus: false,
      refetchOnReconnect: false,
      refetchOnMount: false,
      enabled: !!tokenIds,
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data),
        isFetching: results.map((result) => result.isFetching),
      };
    },
  });
};

// Get Sell/Buy history and price change history
const getNftChanges = async (tokenId: string) => {
  const res = await axiosInstance.get(`/rwa/${tokenId}/history`);
  return res.data;
};

export const useNftChanges = (tokenId: string) => {
  return useQuery({
    queryKey: ["nft-changes", tokenId],
    queryFn: () => getNftChanges(tokenId),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

export const useNftChangesMultiple = (tokenIds: string[]) => {
  return useQueries({
    queries: tokenIds.map((id) => ({
      queryKey: ["nft-changes", "multiple", id],
      queryFn: () => getNftChanges(id),
      refetchOnWindowFocus: false,
      refetchOnReconnect: false,
      refetchOnMount: false,
      enabled: !!tokenIds,
    })),
    combine: (results) => {
      return {
        data: results.map((result) => result.data),
        isFetching: results.map((result) => result.isFetching),
      };
    },
  });
};

// Get linked wallets
const getLinkedWallets = async () => {
  const res = await axiosInstance.get("/linked-accounts/me");
  return res.data;
}

export const useLinkedWallets = (token: string) => {
  return useQuery({
    queryKey: [token, "linked-wallets"],
    queryFn: () => getLinkedWallets(),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};
