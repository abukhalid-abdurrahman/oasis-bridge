"use client";

import { removeUser } from "@/lib/scripts/script";
import { useUserStore } from "@/store/useUserStore";
import {
  MutationCache,
  QueryCache,
  QueryClient,
  QueryClientProvider,
} from "@tanstack/react-query";
import { useRouter } from "next/navigation";

export default function TanstackProvider({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  const setUser = useUserStore((state) => state.setUser);
  const router = useRouter();
  const queryClient = new QueryClient({
    defaultOptions: {
      queries: {
        retry: false,
        refetchOnWindowFocus: false,
        refetchOnReconnect: false,
        refetchOnMount: true,
      },
    },
    queryCache: new QueryCache({
      onError: (err: any) => {
        if (err?.response.status === 401) {
          removeUser(setUser, router);
        }
      },
    }),
    mutationCache: new MutationCache({
      onError: (err: any) => {
        if (err.response.status === 401) {
          removeUser(setUser, router);
        }
      },
    }),
  });
  return (
    <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>
  );
}
