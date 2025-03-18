"use client";

import { mutateChangePassword } from "@/requests/postRequests";
import { useUserStore } from "@/store/useUserStore";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import Cookies from "js-cookie";
import { redirectOnUnauthorize, removeUser } from "@/lib/scripts/script";
import { AxiosError } from "axios";

export default function ChangePasswordForm() {
  const {
    register,
    handleSubmit,
    watch,
    formState: { errors, isSubmitting },
  } = useForm();
  const router = useRouter()
  const setUser = useUserStore(state => state.setUser)
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState('')

  const newPassword = watch("newPassword");

  const submit = mutateChangePassword()

  const onSubmit = (data: any) => {
    setIsLoading(true);
    setErrorMessage('')
    submit.mutate(data, {
      onSuccess: () => {
        removeUser(setUser, router)
      },
      onSettled: () => {
        setIsLoading(false)
      },
      onError: (error: any) => {
        setErrorMessage(error.response.data.error.message)
      }
    })
  };

  return (
    <div className="mt-16">
      <h2 className="h2 text-white mb-6">Change Password</h2>
      <form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-[10px]">
        {/* Old Password */}
        <input
          className={`input w-full ${errors.oldPassword ? "border border-red-500" : ""}`}
          type="password"
          placeholder="Old password"
          {...register("oldPassword", {
            required: "This field is required",
          })}
        />
        {errors.oldPassword && <p className="text-red-500 text-sm">{errors.oldPassword.message as any}</p>}

        {/* New Password */}
        <input
          className={`input w-full ${errors.newPassword ? "border border-red-500" : ""}`}
          type="password"
          placeholder="New password"
          {...register("newPassword", {
            required: "This field is required",
            minLength: {
              value: 6,
              message: "Password must be at least 6 characters long",
            },
          })}
        />
        {errors.newPassword && <p className="text-red-500 text-sm">{errors.newPassword.message as any}</p>}

        {/* Confirm Password */}
        <input
          className={`input w-full ${errors.confirmPassword ? "border border-red-500" : ""}`}
          type="password"
          placeholder="Repeat new password"
          {...register("confirmPassword", {
            required: "This field is required",
            validate: (value) =>
              value === newPassword || "Passwords do not match",
          })}
        />
        {errors.confirmPassword && (
          <p className="text-red-500 text-sm">{errors.confirmPassword.message as any}</p>
        )}

        {errorMessage && (
          <p className="p-sm text-red-500">{errorMessage}</p>
        )}

        {/* Submit Button */}
        <button 
          className="btn btn-lg" 
          type="submit" 
          disabled={isLoading || isSubmitting}
        >
          {isLoading ? "Changing..." : "Change"}
        </button>
      </form>
    </div>
  );
}
