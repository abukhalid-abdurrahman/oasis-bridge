import axiosInstance from "@/lib/axiosInstance"
import axiosInstanceForFiles from "@/lib/axiosInstanceForFiles"
import { API } from "@/lib/constants"
import { PostWallet } from "@/lib/types"
import { useMutation } from "@tanstack/react-query"
import axios from "axios"

// Pot for Register User
const postRegister = async (req: any) => {
  const res = await axios.post(`${API}/auth/register`, req, {
    headers: {
      "Content-Type": "application/json",
    }
  })
  return res.data
}

export const mutateRegister = () => {
  return useMutation({
    mutationFn: (req: any) => postRegister(req),
    onSuccess: () => console.log('Success'),
    onError: (error) => console.log('Error', error)
  })
}

// Post for Login User
const postLogin = async (req: any) => {
  const res = await axios.post(`${API}/auth/login`, req, {
    headers: {
      "Content-Type": "application/json",
    }
  })
  return res.data
}

export const mutateLogin = () => {
  return useMutation({
    mutationFn: (req: any) => postLogin(req),
    onSuccess: () => console.log('Success'),
    onError: (error) => console.log('Error', error)
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
    onSuccess: () => console.log('Success'),
    onError: (error) => console.log('Error', error)
  })
}


// Post for Logout
const postLogout = async (req: any) => {
  const res = await axios.post(`${API}/logout`, req, {
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${req.token}`
    }
  })
  return res.data
}

export const mutateLogout = () => {
  return useMutation({
    mutationFn: (req: any) => postLogout(req),
    onSuccess: () => console.log('Success'),
    onError: (error) => console.log('Error', error)
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
    onSuccess: () => console.log('Success'),
    onError: (error) => console.log('Error', error)
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
    onSuccess: () => console.log('Success'),
    onError: (error) => console.log('Error', error)
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
    onSuccess: () => console.log('success'),
    onError: (error) => console.log('Error', error)
  })
}