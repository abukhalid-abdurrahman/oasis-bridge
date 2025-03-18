"use client";

import { useForm } from "react-hook-form";
import Button from "./Button";
import Link from "next/link";
import { mutateRegister } from "@/requests/postRequests";
import { useRouter } from "next/navigation";
import { useState } from "react";
import LoadingAlt from "./LoadingAlt";

export default function SignUpForm() {
  const router = useRouter();
  const [isLoading, setIsLoading] = useState(false);
  const [signUpMessage, setSignUpMessage] = useState(false);
  const [errorMessage, setErrorMessage] = useState('')
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm();

  const submit = mutateRegister();

  const onSubmit = (data: any) => {
    setIsLoading(true);
    setErrorMessage('')
    submit.mutate(data, {
      onSuccess: (response) => {
        setSignUpMessage(!!response.data.userId);
        setTimeout(() => {
          router.push("/?signin=true");
        }, 3000);
      },
      onSettled: () => {
        setIsLoading(false);
      },
      onError: (error: any) => {
        setIsLoading(false);
        setErrorMessage(error.response.data.error.message);
      }
    });
  };

  const password = watch("password");

  return (
    <form
      onSubmit={handleSubmit(onSubmit)}
      className="flex flex-col items-center gap-[10px] w-full"
    >
      <div className="w-full">
        <input
          className={`input w-full ${
            errors.userName ? "border border-red-500" : ""
          }`}
          type="text"
          placeholder="Username"
          {...register("userName", {
            required: "This field is required",
            minLength: {
              value: 4,
              message: "Username must be at least 4 characters long",
            },
          })}
        />
        {errors.userName && (
          <p className="text-red-500 text-sm mt-1">
            {errors.userName.message as any}
          </p>
        )}
      </div>

      <div className="w-full">
        <input
          className={`input w-full ${
            errors.emailAddress ? "border border-red-500" : ""
          }`}
          type="email"
          placeholder="Email"
          {...register("emailAddress", {
            required: "This field is required",
            pattern: {
              value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,4}$/,
              message: "Invalid email address",
            },
          })}
        />
        {errors.emailAddress && (
          <p className="text-red-500 text-sm mt-1">
            {errors.emailAddress.message as any}
          </p>
        )}
      </div>

      <div className="w-full">
        <input
          className={`input w-full ${
            errors.password ? "border border-red-500" : ""
          }`}
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
          <p className="text-red-500 text-sm mt-1">
            {errors.password.message as any}
          </p>
        )}
      </div>

      <div className="w-full">
        <input
          className={`input w-full ${
            errors.confirmPassword ? "border border-red-500" : ""
          }`}
          type="password"
          placeholder="Confirm Password"
          {...register("confirmPassword", {
            required: "This field is required",
            minLength: {
              value: 6,
              message: "Password must be at least 6 characters long",
            },
            validate: (value) => value === password || "Passwords do not match",
          })}
        />
        {errors.confirmPassword && (
          <p className="text-red-500 text-sm mt-1">
            {errors.confirmPassword.message as any}
          </p>
        )}
      </div>

      {errorMessage && (
        <p className="p-sm text-red-500 text-center py-2">{errorMessage}</p>
      )}

      {signUpMessage && (
        <p className="p-sm text-center py-2">Your account has been successfully created</p>
      )}

      <Button className="btn-lg w-full" type="submit" disabled={isLoading}>
        {isLoading ? "Signing up..." : "Sign up"}
      </Button>

      <Link href="?signin=true" className="text-sm text-blue-600">
        Already have an account?
      </Link>
      {isLoading && <LoadingAlt />}
    </form>
  );
}
