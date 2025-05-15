import axiosInstance from "@/lib/axiosInstance";
import { RwasReq } from "@/lib/types";
import { useQueries, useQuery } from "@tanstack/react-query";

// Get for Exchange Rate
const getExchangeRate = async (fromToken: string, toToken: string) => {
  const res = await axiosInstance.get(`/exchange-rate`, {
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
  const res = await axiosInstance.get(`/transaction-status`, {
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

// Get for User virtual Accounts
const getUserVirtualAccounts = async () => {
  const res = await axiosInstance.get(`/accounts/list`);
  return res.data;
};

export const useUserVirtualAccounts = (isEnabled: boolean, token: string) => {
  return useQuery({
    queryKey: [token, 'user-accounts'],
    queryFn: () => getUserVirtualAccounts(),
    gcTime: 0,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
    enabled: isEnabled
  })
}

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
  const res = await axiosInstance.get(`/networks`, {
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

// Get all RWAs
const getRwas = async (reqParams: RwasReq) => {
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

export const useRwas = (reqParams: any) => {
  return useQuery({
    queryKey: ["rwas", reqParams],
    queryFn: () => getRwas(reqParams),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

// Get a specific RWA
const getRwa = async (tokenId: string) => {
  const res = await axiosInstance.get(`/rwa/${tokenId}`);
  return res.data;
};

export const useRwa = (tokenId: string) => {
  return useQuery({
    queryKey: ["rwa", tokenId],
    queryFn: () => getRwa(tokenId),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

// Example with multiple request
export const useRwaMultiple = (tokenIds: string[]) => {
  return useQueries({
    queries: tokenIds.map((id) => ({
      queryKey: ["rwa", "multiple", id],
      queryFn: () => getRwa(id),
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
const getRwaChanges = async (tokenId: string) => {
  const res = await axiosInstance.get(`/rwa-price-histories/${tokenId}`);
  return res.data;
};

export const useRwaChanges = (tokenId: string) => {
  return useQuery({
    queryKey: ["rwa-changes", tokenId],
    queryFn: () => getRwaChanges(tokenId),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

export const useRwaChangesMultiple = (tokenIds: string[]) => {
  return useQueries({
    queries: tokenIds.map((id) => ({
      queryKey: ["rwa-changes", "multiple", id],
      queryFn: () => getRwaChanges(id),
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

// Get RWAs by user
const getRwaMe = async (reqParams: any) => {
  const res = await axiosInstance.get(`/rwa/me`, {
    params: {
      RwaId: reqParams.rwaId,
      PageSize: reqParams.pageSize,
      PageNumber: reqParams.pageNumber,
    },
  });
  return res.data;
};

export const useRwaMe = (token: string, reqParams: any) => {
  return useQuery({
    queryKey: ["rwa-me", token],
    queryFn: () => getRwaMe(reqParams),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};

// Get linked wallets
const getLinkedWallets = async () => {
  const res = await axiosInstance.get("/linked-accounts/me");
  return res.data;
};

export const useLinkedWallets = (token: string) => {
  return useQuery({
    queryKey: [token, "linked-wallets"],
    queryFn: () => getLinkedWallets(),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  });
};
