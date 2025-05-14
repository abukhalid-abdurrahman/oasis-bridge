import axiosInstance from "@/lib/axiosInstance";
import { useMutation } from "@tanstack/react-query";

const putRwa = async (tokenId: string, req: any) => {
  const res = await axiosInstance.put(`/rwa/${tokenId}`, req);
  return res.data;
};

export const mutateRwaUpdate = (tokenId: string) => {
  return useMutation({
    mutationFn: (req: any) => putRwa(tokenId, req),
    onSuccess: () => console.log("Success"),
    onError: (error) => console.log("Error", error),
  });
};
