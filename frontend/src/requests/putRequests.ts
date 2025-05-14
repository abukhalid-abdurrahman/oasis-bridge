import axiosInstance from "@/lib/axiosInstance";
import { useMutation } from "@tanstack/react-query";

const putRwa = async (tokenId: string) => {
  const res = await axiosInstance.put(`/rwa${tokenId}`);
  return res.data;
};

export const mutateRwaUpdate = () => {
  return useMutation({
    mutationFn: (tokenId: string) => putRwa(tokenId),
    onSuccess: () => console.log("Success"),
    onError: (error) => console.log("Error", error),
  });
};
