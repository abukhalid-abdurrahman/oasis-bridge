import { postFiles } from "@/requests/postRequests";
import { PublicKey } from "@solana/web3.js";
import Cookies from "js-cookie";
import { AppRouterInstance } from "next/dist/shared/lib/app-router-context.shared-runtime";

export const shortAddress = (address: string) => {
  if (address) {
    return `${address.slice(0, 5)}...${address.slice(-4)}`;
  }
};

export const shortDescription = (description: string) => {
  return `${description.slice(0, 30)}...`;
};

export const parseJwt = (token: string) => {
  if (!token) {
    return;
  }
  const base64Url = token.split(".")[1];
  const base64 = base64Url.replace("-", "+").replace("_", "/");
  return JSON.parse(window.atob(base64));
};

export const removeUser = (
  setUser: (user: null) => void,
  router: AppRouterInstance
) => {
  setUser(null);
  Cookies.remove("oasisToken");
  router.push("/?signin=true");
};

export const redirectOnUnauthorize = (
  err: any,
  setUser: (user: null) => void,
  router: AppRouterInstance
) => {
  if (err.response?.status === 401) {
    setUser(null);
    Cookies.remove("oasisToken");
    router.push("/?signin=true");
  }
};

export const uploadFile = async (file: File): Promise<string> => {
  try {
    const res = await postFiles(file);
    return res.data.data.fileUrl;
  } catch (error) {
    console.error("Upload failed", error);
    return "File upload failed";
  }
};

export const getVisiblePages = (
  current: number,
  total: number
): (number | "...")[] => {
  const delta = 1;
  const range: (number | "...")[] = [];
  const left = current - delta;
  const right = current + delta;

  let l: number;
  for (let i = 1; i <= total; i++) {
    if (i === 1 || i === total || (i >= left && i <= right)) {
      range.push(i);
    } else if (range[range.length - 1] !== "...") {
      range.push("...");
    }
  }
  return range;
};

export const calculatePercentageDifference = (
  oldPrice: number,
  newPrice: number
) => {
  if (!oldPrice || oldPrice === 0) return "0%";
  const difference = ((newPrice - oldPrice) / oldPrice) * 100;
  return `${Math.abs(difference).toFixed(2)}%`;
};
