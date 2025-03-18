import { API } from "@/lib/constants"
import axios from "axios"

export const getUserVirtualAccountsOnServer = async (token: string) => {
  const res = await axios.get(`${API}/accounts/list`, {
    headers: {
      "Content-Type": "application/json",
      "Authorization": `Bearer ${token}`
    }
  })
  return res
}