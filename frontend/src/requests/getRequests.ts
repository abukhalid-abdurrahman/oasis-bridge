import axiosInstance from "@/lib/axiosInstance"
import { API } from "@/lib/constants"
import { useQuery } from "@tanstack/react-query"
import axios from "axios"

// Get for Exchange Rate
const getExchangeRate = async (fromToken: string, toToken: string) => {
  const res = await axios.get(`${API}/exchange-rate`, {
    headers: {
      "Content-Type": "application/json",
    },
    params: {
      fromToken: fromToken,
      toToken: toToken,
    }
  })
  return res.data
}

export const useExchangeRate = (fromToken: string, toToken: string) => {
  return useQuery({
    queryKey: [fromToken, toToken, 'exchange-rate'],
    queryFn: () => getExchangeRate(fromToken, toToken),
    refetchOnWindowFocus: false,
    refetchInterval: 300000,
    // enabled: !!token
  })
}

// Get for Transaction Status
const getTransactionStatus = async (transactionId: string) => {
  const res = await axios.get(`${API}/transaction-status`, {
    headers: {
      "Content-Type": "application/json",
    },
    params: {
      transactionId: transactionId
    }
  })
  return res.data
}

export const useTransactionStatus = (transactionId: string) => {
  return useQuery({
    queryKey: [transactionId, 'transaction-id'],
    queryFn: () => getTransactionStatus(transactionId),
    refetchOnWindowFocus: false,
    // initialData: true
    enabled: false
  })
}

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
  const res = await axiosInstance.get(`/accounts/list`)
  return res.data
}

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
  const res = await axiosInstance.get(`/orders/${orderId}/check-balance`)
  return res.data
}

export const useVirtualAccountBalance = (orderId: string, completed: string | boolean) => {
  return useQuery({
    queryKey: [orderId, 'virtual-account-balance'],
    queryFn: () => getVirtualAccountBalance(orderId),
    gcTime: 0,
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
    enabled: !!orderId && !!completed
  })
}


// Get Networks
const getNetworks = async () => {
  const res = await axios.get(`${API}/networks`, {
    headers: {
      "Content-Type": "application/json",
    },
  })
  return res.data
}

export const useNetworks = () => {
  return useQuery({
    queryKey: ['networks'],
    queryFn: () => getNetworks(),
    refetchOnWindowFocus: false,
    refetchOnReconnect: false,
    refetchOnMount: false,
  })
}