import axiosInstance from "@/lib/axiosInstance"
import { API } from "@/lib/constants"
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