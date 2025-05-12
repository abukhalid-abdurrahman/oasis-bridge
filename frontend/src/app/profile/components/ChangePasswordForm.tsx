"use client";

import { mutateChangePassword } from "@/requests/postRequests";
import { useUserStore } from "@/store/useUserStore";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { removeUser } from "@/lib/scripts/script";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Form, FormControl, FormField, FormItem, FormMessage } from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";

export default function ChangePasswordForm() {
  const router = useRouter();
  const setUser = useUserStore((state) => state.setUser);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const FormSchema = z
    .object({
      oldPassword: z.string().min(6, {
        message: "Password must be at least 6 characters long",
      }),
      newPassword: z.string().min(6, {
        message: "Password must be at least 6 characters long",
      }),
      confirmPassword: z.string(),
    })
    .refine((data) => data.newPassword === data.confirmPassword, {
      message: "Passwords do not match",
      path: ["confirmPassword"],
    });

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      oldPassword: "",
      newPassword: "",
      confirmPassword: "",
    },
  });

  const submit = mutateChangePassword();

  const onSubmit = (data: any) => {
    setIsLoading(true);
    setErrorMessage("");
    submit.mutate(data, {
      onSuccess: () => {
        removeUser(setUser, router);
      },
      onSettled: () => {
        setIsLoading(false);
      },
      onError: (error: any) => {
        setErrorMessage(error.response.data.error.message);
      },
    });
  };

  return (
    <div className="mt-16">
      <h2 className="h2 text-white mb-6">Change Password</h2>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="flex flex-col gap-[10px]"
        >
          <FormField
            control={form.control}
            name="oldPassword"
            render={({ field }) => (
              <FormItem>
                <FormControl>
                  <Input
                    type="password"
                    placeholder="Old password"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="newPassword"
            render={({ field }) => (
              <FormItem>
                <FormControl>
                  <Input
                    type="password"
                    placeholder="New password"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
          <FormField
            control={form.control}
            name="confirmPassword"
            render={({ field }) => (
              <FormItem>
                <FormControl>
                  <Input
                    type="password"
                    placeholder="Repeat new password"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          {errorMessage && <p className="p-sm text-red-500">{errorMessage}</p>}

          <Button variant="gray" type="submit" size="xl" className="w-full">
            {isLoading ? "Changing..." : "Change"}
          </Button>
        </form>
      </Form>
    </div>
  );
}
