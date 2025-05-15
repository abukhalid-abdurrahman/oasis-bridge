"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import Link from "next/link";
import { mutateLogin } from "@/requests/postRequests";
import LoadingAlt from "./LoadingAlt";
import { useRouter } from "next/navigation";
import { useUserStore } from "@/store/useUserStore";
import { parseJwt } from "@/lib/scripts/script";
import Cookies from "js-cookie";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Form, FormControl, FormField, FormItem, FormMessage } from "./ui/form";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { AxiosError } from "axios";

export default function SignInForm() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const setUser = useUserStore((state) => state.setUser);
  const [errorMessage, setErrorMessage] = useState("");

  const FormSchema = z.object({
    email: z.string().email({
      message: "Invalid email address",
    }),
    password: z.string().min(6, {
      message: "Password must be at least 6 characters long",
    }),
  });

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      email: "",
      password: "",
    },
  });

  const submit = mutateLogin();

  const onSubmit = (data: z.infer<typeof FormSchema>) => {
    setIsLoading(true);
    setErrorMessage("");

    submit.mutate(data, {
      onSuccess: (response) => {
        const { token, expiresAt, startTime } = response.data;
        const expiresDate = new Date(expiresAt);
        const { Id, UserName, Email } = parseJwt(token);

        setUser({ token, expiresAt, startTime, Id, UserName, Email });

        Cookies.set("oasisToken", token, {
          expires: expiresDate,
        });

        const referrer = document.referrer;
        const currentOrigin = window.location.origin;

        if (!referrer || !referrer.startsWith(currentOrigin)) {
          router.push("/");
        } else {
          router.back();
        }
      },
      onError: (error: any) => {
        setErrorMessage(error.response?.data.message || "An error occurred");
      },
      onSettled: () => {
        setIsLoading(false);
      },
    });
  };

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit(onSubmit)}
        className="flex flex-col items-center gap-[10px] w-full"
      >
        <FormField
          control={form.control}
          name="email"
          render={({ field }) => (
            <>
              <FormItem className="w-full">
                <FormControl>
                  <Input placeholder="Email" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            </>
          )}
        />
        <FormField
          control={form.control}
          name="password"
          render={({ field }) => (
            <>
              <FormItem className="w-full">
                <FormControl>
                  <Input placeholder="Password" type='password' {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            </>
          )}
        />
        {errorMessage && <p className="p-sm text-red-500">{errorMessage}</p>}
        <Button variant="gray" type="submit" size="xl" className="w-full">
          {isLoading ? "Signing in..." : "Sign In"}
        </Button>
        <Link href="?signup=true" className="text-sm text-blue-600">
          Create account
        </Link>

        {isLoading && <LoadingAlt />}
      </form>
    </Form>
  );
}
