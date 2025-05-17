"use client";

import { useForm } from "react-hook-form";
import Link from "next/link";
import { mutateRegister } from "@/requests/postRequests";
import { useRouter } from "next/navigation";
import { useState } from "react";
import LoadingAlt from "./LoadingAlt";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { Form, FormControl, FormField, FormItem, FormMessage } from "./ui/form";
import { Input } from "./ui/input";
import { Button } from "./ui/button";

export default function SignUpForm() {
  const router = useRouter();
  const [signUpMessage, setSignUpMessage] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  const FormSchema = z
    .object({
      userName: z.string().min(4, {
        message: "Username must be at least 4 characters long",
      }),
      emailAddress: z.string().email({
        message: "Invalid email address",
      }),
      password: z.string().min(6, {
        message: "Password must be at least 6 characters long",
      }),
      confirmPassword: z.string(),
    })
    .refine((data) => data.password === data.confirmPassword, {
      message: "Passwords do not match",
      path: ["confirmPassword"],
    });

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      userName: "",
      emailAddress: "",
      password: "",
      confirmPassword: "",
    },
  });

  const submit = mutateRegister();

  const onSubmit = (data: z.infer<typeof FormSchema>) => {
    setIsLoading(true);
    setErrorMessage("");
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
        setErrorMessage(error.response?.data?.error?.message || "An error occurred");
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
          name="userName"
          render={({ field }) => (
            <>
              <FormItem className="w-full">
                <FormControl>
                  <Input placeholder="Username" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            </>
          )}
        />
        <FormField
          control={form.control}
          name="emailAddress"
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
                  <Input type='password' placeholder="Password" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            </>
          )}
        />
        <FormField
          control={form.control}
          name="confirmPassword"
          render={({ field }) => (
            <>
              <FormItem className="w-full">
                <FormControl>
                  <Input type='password' placeholder="Confirm password" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            </>
          )}
        />

        {errorMessage && (
          <p className="p-sm text-red-500 text-center">{errorMessage}</p>
        )}

        {signUpMessage && (
          <p className="p-sm text-center text-black">
            Your account has been successfully created
          </p>
        )}

        <Button variant="gray" type="submit" size="xl" className="w-full">
          {isLoading ? "Signing up..." : "Sign up"}
        </Button>

        <Link href="?signin=true" className="text-sm text-blue-600">
          Already have an account?
        </Link>

        {isLoading && <LoadingAlt />}
      </form>
    </Form>
  );
}
