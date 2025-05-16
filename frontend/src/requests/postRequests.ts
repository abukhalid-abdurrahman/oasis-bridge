import axiosInstance from "@/lib/axiosInstance"
import axiosInstanceForFiles from "@/lib/axiosInstanceForFiles"
import { PostWallet } from "@/lib/types"
import { useMutation } from "@tanstack/react-query"

// Pot for Register User
const postRegister = async (req: any) => {
  const res = await axiosInstance.post(`/auth/register`, req)
  return res.data
}

export const mutateRegister = () => {
  return useMutation({
    mutationFn: (req: any) => postRegister(req),
  })
}

// Post for Login User
const postLogin = async (req: any) => {
  const res = await axiosInstance.post(`/auth/login`, req)
  return res.data
}

export const mutateLogin = () => {
  return useMutation({
    mutationFn: (req: any) => postLogin(req),
  })
}


// Post for Change Password
const postChangePassword = async (req: any) => {
  const res = await axiosInstance.post(`/auth/change-password`, req)
  return res.data
}

export const mutateChangePassword = () => {
  return useMutation({
    mutationFn: (req: any) => postChangePassword(req),
  })
}


// Post for Logout
const postLogout = async (req: any) => {
  const res = await axiosInstance.post(`/logout`, req)
  return res.data
}

export const mutateLogout = () => {
  return useMutation({
    mutationFn: (req: any) => postLogout(req),
  })
}

// Post for Create Order
const postOrders = async (req: any) => {
  const res = await axiosInstance.post(`/orders`, req)
  return res.data
}

export const mutateOrders = () => {
  return useMutation({
    mutationFn: (req: any) => postOrders(req),
  })
}


// Post for a file uploading
export const postFiles = async (file: File) => {
  const formData = new FormData();
  formData.append("file", file);
  const res = await axiosInstanceForFiles.post(`/files/upload`, formData);
  return res.data;
};


// Post for linking wallet address
const postWallet = async (req: any) => {
  const res = await axiosInstance.post(`/linked-accounts`, req)
  return res.data
}

export const mutateWallet = () => {
  return useMutation({
    mutationFn: (req: PostWallet) => postWallet(req),
  })
}

// Post Rwa token to tokenize it
const postRwaToken = async(req: any) => {
  const res = await axiosInstance.post(`/rwa/tokenize`, req)
  return res.data
}

export const mutateRwaToken = () => {
  return useMutation({
    mutationFn: (req: any) => postRwaToken(req),
  })
}

// Post Rwa purchasing
const postRwaPurchase = async(req: string) => {
  const res = await axiosInstance.post(`/nft-purchase`, req)
  return res.data
}

export const mutateRwaPurchase = () => {
  return useMutation({
    mutationFn: (req: any) => postRwaPurchase(req),
  })
}

// Post Rwa Signed transaction
const postRwaTransaction = async(req: any) => {
  const res = await axiosInstance.post(`/nft-purchase/send`, req)
  return res.data
}

export const mutateRwaTransaction = () => {
  return useMutation({
    mutationFn: (req: any) => postRwaTransaction(req),
  })
}