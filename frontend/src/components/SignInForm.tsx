"use client";

import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import Button from "./Button";
import Link from "next/link";
import { mutateLogin } from "@/requests/postRequests";
import LoadingAlt from "./LoadingAlt";
import { useRouter } from "next/navigation";
import { useUserStore } from "@/store/useUserStore";
import { parseJwt } from "@/lib/scripts/script";
import Cookies from "js-cookie";

export default function SignInForm() {
  const router = useRouter()
  const [isLoading, setIsLoading] = useState(false);
  const setUser = useUserStore((state) => state.setUser);
  const [errorMessage, setErrorMessage] = useState('')
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm();

  const submit = mutateLogin();

  const onSubmit = (data: any) => {
    setIsLoading(true);
    setErrorMessage('')
    submit.mutate(data, {
      onSuccess: (response) => {
        const { token, expiresAt, startTime } = response.data;
        const expiresDate = new Date(expiresAt);
        const { Id, UserName, Email } = parseJwt(token);
        
        setUser({ token, expiresAt, startTime, Id, UserName, Email });
  
        Cookies.set("oasisToken", token, {
          expires: expiresDate,
        });
  
        router.push("/");
      },
      onError: (error: any) => {
        setErrorMessage(error.response.data.error.message);
      },
      onSettled: () => {
        setIsLoading(false);
      },
    });
  };

  return (
    <form 
      onSubmit={handleSubmit(onSubmit)}
      className="flex flex-col items-center gap-[10px] w-full"
    >
      <div className="w-full">
        <input
          className={`input w-full ${errors.email ? "border border-red-500" : ""}`}
          type="email"
          placeholder="Email"
          {...register("email", {
            required: "This field is required",
            pattern: {
              value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/,
              message: "Invalid email address",
            },
          })}
        />
        {errors.email && (
          <p className="text-red-500 text-sm mt-1">{errors.email.message as any}</p>
        )}
      </div>

      <div className="w-full">
        <input
          className={`input w-full ${errors.password ? "border border-red-500" : ""}`}
          type="password"
          placeholder="Password"
          {...register("password", {
            required: "This field is required",
            minLength: {
              value: 6,
              message: "Password must be at least 6 characters long",
            },
          })}
        />
        {errors.password && (
          <p className="text-red-500 text-sm mt-1">{errors.password.message as any}</p>
        )}
      </div>

      {errorMessage && (
        <p className="p-sm text-red-500 text-center py-2">{errorMessage}</p>
      )}

      <Button className="btn-lg w-full" type="submit" disabled={isLoading}>
        {isLoading ? "Signing in..." : "Sign In"}
      </Button>

      <Link href="?signup=true" className="text-sm text-blue-600">
        Create account
      </Link>

      {/* Показываем индикатор загрузки только когда идет запрос */}
      {isLoading && <LoadingAlt />}
    </form>
  );
}
