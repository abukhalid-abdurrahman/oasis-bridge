"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { Form, FormControl, FormField, FormItem, FormMessage } from "./ui/form";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { tokenizationFieldsBase } from "@/lib/helpers/tokenizationFieldsBase";

export default function CreateNft() {
  const FormSchema = z.object(
    Object.fromEntries(
      tokenizationFieldsBase.map((field) => [field.name, field.validation])
    )
  );

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
  });

  return (
    <Form {...form}>
      <form
        onSubmit={form.handleSubmit((data) => {
          console.log(data);
        })}
        className="space-y-4"
      >
        {tokenizationFieldsBase.map((field) => (
          <FormField
            key={field.name}
            control={form.control}
            name={field.name as any}
            render={({ field: f }) => (
              <FormItem>
                <FormControl>
                  <Input placeholder={field.placeholder} {...f} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />
        ))}

        <Button variant="gray" type="submit" size="xl" className="w-full">
          Create NFT
        </Button>
      </form>
    </Form>
  );
}
