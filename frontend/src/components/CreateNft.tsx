"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useForm } from "react-hook-form";
import { z } from "zod";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage,
} from "./ui/form";
import { Input } from "./ui/input";
import { Button } from "./ui/button";
import { tokenizationFieldsBase } from "@/lib/helpers/tokenizationFields";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import PageTitle from "./PageTitle";
import { useDropzone } from "react-dropzone";
import { useCallback, useState } from "react";
import { ASSET_TYPES } from "@/lib/constants";

export default function CreateNft() {
  const [preview, setPreview] = useState<string | null>(null);

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const file = acceptedFiles[0];
    const url = URL.createObjectURL(file);
    setPreview(url);
    // Здесь можно также сохранить файл в состояние формы
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "image/*": [],
    },
    multiple: false,
  });

  const FormSchema = z.object(
    Object.fromEntries(
      tokenizationFieldsBase.map((field) => [field.name, field.validation])
    )
  );

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
  });

  return (
    <>
      <PageTitle title="Create your own NFT" />
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit((data) => {
            console.log(data);
          })}
          className="flex gap-10 items-stretch  "
        >
          <div className="space-y-2 w-1/2">
            <FormField
              control={form.control}
              name="title"
              render={({ field }) => (
                <FormItem>
                  <FormControl>
                    <Input placeholder="Title" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="assetDescription"
              render={({ field }) => (
                <FormItem>
                  <FormControl>
                    <Input placeholder="Description" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="uniqueIdentifier"
              render={({ field }) => (
                <FormItem>
                  <FormControl>
                    <Input placeholder="uniqueIdentifier" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="network"
              render={({ field }) => (
                <FormItem>
                  <Select
                    onValueChange={field.onChange}
                    defaultValue={field.value}
                  >
                    <FormControl>
                      <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Network" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectGroup>
                        <SelectLabel>Networks</SelectLabel>
                        <SelectItem value="Solana">Solana</SelectItem>
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <div className="flex justify-between gap-2">
              <FormField
                control={form.control}
                name="price"
                render={({ field }) => (
                  <FormItem>
                    <FormControl>
                      <Input type="number" placeholder="Price" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="Royalty"
                render={({ field }) => (
                  <FormItem>
                    <FormControl>
                      <Input type="number" placeholder="Royalty" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <div className="">
                <Input type="number" placeholder="Net amount" disabled={true} />
              </div>
            </div>

            <FormField
              control={form.control}
              name="owner_contact"
              render={({ field }) => (
                <FormItem>
                  <FormControl>
                    <Input placeholder="owner_contact" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="proofOfOwnershipDocumet"
              render={({ field }) => (
                <FormItem>
                  <FormLabel className="text-white">
                    Proof of Ownership Document
                  </FormLabel>
                  <FormControl>
                    <Input id="picture" type="file" {...field} />
                  </FormControl>
                  <FormMessage />
                </FormItem>
              )}
            />

            <FormField
              control={form.control}
              name="asset_type"
              render={({ field }) => (
                <FormItem>
                  <Select
                    onValueChange={field.onChange}
                    defaultValue={field.value}
                  >
                    <FormControl>
                      <SelectTrigger className="w-[180px]">
                        <SelectValue placeholder="Asset type" />
                      </SelectTrigger>
                    </FormControl>
                    <SelectContent>
                      <SelectGroup>
                        <SelectLabel>Asset Types</SelectLabel>
                        {ASSET_TYPES.map((item) => (
                          <SelectItem key={item} value={item}>
                            {item}
                          </SelectItem>
                        ))}
                      </SelectGroup>
                    </SelectContent>
                  </Select>
                  <FormMessage />
                </FormItem>
              )}
            />

            <Button variant="gray" type="submit" size="xl" className="w-full">
              Tokenize
            </Button>
          </div>
          <div className="w-1/2 rounded-2xl bg-textGray">
            <div
              {...getRootProps()}
              className="flex justify-center items-center border-2 border-dashed border-gray   p-4 rounded-md text-center cursor-pointer hover:bg-gray-50 text-white h-full"
            >
              <input {...getInputProps()} />
              {preview ? (
                <img
                  src={preview}
                  alt="Preview"
                  className="mx-auto max-h-64 rounded-md object-contain"
                />
              ) : (
                <div className="flex flex-col gap-3 justify-center">
                  <h2 className="h1">NFT Image</h2>
                  {isDragActive ? (
                    <p className="text-gray-500">Drop the files here ...</p>
                  ) : (
                    <p>Drag & drop an image here, or click to select one</p>
                  )}
                </div>
              )}
            </div>
          </div>
        </form>
      </Form>
    </>
  );
}
