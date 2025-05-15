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
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { Button, buttonVariants } from "@/components/ui/button";
import {
  getDefaultValuesFromFields,
  tokenizationFieldsBase,
} from "@/lib/helpers/tokenizationFields";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectLabel,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import PageTitle from "@/components/PageTitle";
import { useEffect, useMemo, useState } from "react";
import { ASSET_TYPES } from "@/lib/constants";
import { useRwa, useRwaMe } from "@/requests/getRequests";
import Loading from "@/components/Loading";
import Image from "next/image";
import { shortDescription, uploadFile } from "@/lib/scripts/script";
import { Loader2 } from "lucide-react";
import UpdatingModal from "./UpdatingModal";
import Link from "next/link";
import { useUserStore } from "@/store/useUserStore";
import { redirect } from "next/navigation";

interface ChangeRwaProps {
  params: any
}

export default function ChangeRwa({ params }: ChangeRwaProps) {
  const tokenId = JSON.parse(params.value).id
  const [netAmount, setNetAmount] = useState<number | string>("");
  const [existedNetAmount, setExistedNetAmount] = useState<number | string>("");
  const [isUpdated, setIsUpdated] = useState(false);
  const [formData, setFormData] = useState<z.infer<typeof FormSchema>>();
  const { user } = useUserStore()

  const { data, isFetching, isFetched } = useRwa(tokenId);

  const FormSchema = z.object(
    Object.fromEntries(
      tokenizationFieldsBase
        .filter((field) => field.name !== "image")
        .map((field) => [field.name, field.validation])
    )
  );

  const defaultValues = getDefaultValuesFromFields(tokenizationFieldsBase);

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues,
  });

  const price = form.watch("price");
  const royalty = form.watch("royalty");

  const onSubmit = async (data: z.infer<typeof FormSchema>) => {
    setFormData(data);
    setIsUpdated(true);
  };

  useEffect(() => {
    if (price && royalty) {
      setNetAmount(() => {
        return (royalty * price) / 100;
      });
    } else {
      setNetAmount("");
    }
  }, [price, royalty]);

  useEffect(() => {
    if (data) {
      if (data.data.price && data.data.royalty) {
        setExistedNetAmount(() => {
          return (data.data.royalty * data.data.price) / 100;
        });
      } else {
        setExistedNetAmount("");
      }
    }
  }, [data]);

  useEffect(() => {
    if (isFetched && data) {
      if (data.data.ownerUsername === user?.UserName) {
        return
      } else {
        redirect('/')
      }
    }
  }, [isFetched])

  if (isFetching || !isFetched) {
    return (
      <Loading
        className="flex justify-center mt-14"
        classNameLoading="!border-white !border-r-transparent !w-14 !h-14"
      />
    );
  }

  return (
    <>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="flex gap-10 items-start lg:gap-5 md:flex-col-reverse"
        >
          <div className="w-1/2 md:w-full">
            <PageTitle title="Update RWA" />
            <div className="flex flex-col gap-2 firstStep">
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
                      <Input placeholder="Unique Identifier" {...field} />
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
                    <FormItem className="w-1/3">
                      <FormControl>
                        <Input type="number" placeholder="Price in zBTC" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <FormField
                  control={form.control}
                  name="royalty"
                  render={({ field }) => (
                    <FormItem className="w-1/3">
                      <FormControl>
                        <Input type="number" placeholder="Royalty" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
                <div className="w-1/3">
                  <Input
                    type="number"
                    placeholder="Net amount"
                    disabled={true}
                    value={netAmount}
                  />
                </div>
              </div>

              <FormField
                control={form.control}
                name="ownerContact"
                render={({ field }) => (
                  <FormItem>
                    <FormControl>
                      <Input placeholder="Owner contact" {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="proofOfOwnershipDocument"
                render={({ field }) => {
                  const [isUploading, setIsUploading] = useState(false);

                  return (
                    <FormItem>
                      <FormLabel className="text-white">
                        Proof of Ownership Document
                      </FormLabel>
                      <FormControl>
                        <div className="relative">
                          <Input
                            type="file"
                            accept="image/*"
                            disabled={isUploading}
                            className={
                              isUploading ? "cursor-not-allowed opacity-50" : ""
                            }
                            onChange={async (e) => {
                              const file = e.target.files?.[0];
                              if (!file) return;

                              if (!file.type.startsWith("image/")) {
                                form.setError("proofOfOwnershipDocument", {
                                  type: "manual",
                                  message: "File must be an image",
                                });
                                return;
                              }

                              const maxSizeInBytes = 10 * 1024 * 1024;
                              if (file.size > maxSizeInBytes) {
                                form.setError("proofOfOwnershipDocument", {
                                  type: "manual",
                                  message: "File must be smaller than 10MB",
                                });
                                return;
                              }

                              try {
                                setIsUploading(true);
                                const uploadedUrl = await uploadFile(file);
                                field.onChange(uploadedUrl);
                              } catch (error) {
                                form.setError("proofOfOwnershipDocument", {
                                  type: "manual",
                                  message: "Upload failed. Try again.",
                                });
                              } finally {
                                setIsUploading(false);
                              }
                            }}
                          />
                          {isUploading && (
                            <div className="absolute right-3 top-1/2 -translate-y-1/2">
                              <Loader2
                                className="animate-spin text-white"
                                size={18}
                              />
                            </div>
                          )}
                        </div>
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  );
                }}
              />

              <FormField
                control={form.control}
                name="assetType"
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
                            <SelectItem
                              key={item}
                              value={item.replace(/\s/g, "")}
                            >
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
            </div>
            <Button
              variant="gray"
              type="submit"
              size="xl"
              className="w-full mt-2"
            >
              Update
            </Button>
          </div>
          <div className="w-1/2 aspect-[3/2] rounded-2xl md:w-full md:aspect-auto">
            <div className="relative aspect-[3/2] w-full max-w-full bg-neutral-700/50 rounded-2xl p-5 flex items-center justify-center overflow-hidden">
              <Image
                src={data.data.image}
                alt={data.data.title}
                width={350}
                height={350}
                className="object-contain !max-h-full !w-auto rounded-2xl"
              />
            </div>
            <div className="flex flex-col gap-2 mt-2">
              <div
                className={`${buttonVariants({
                  variant: "gray",
                  size: "lg",
                })} !px-5 !w-full flex justify-between flex-wrap`}
              >
                <span className="text-gray-500">Version:</span>
                {data.data.version}
              </div>
              <div
                className={`${buttonVariants({
                  variant: "gray",
                  size: "lg",
                })} !px-5 !w-full flex justify-between flex-wrap`}
              >
                <span className="text-gray-500">IPFS CID:</span>{" "}
                <Link href={data.data.image}>{shortDescription(data.data.image)}</Link>
              </div>
              <div className="flex gap-2 flex-wrap">
                <div
                  className={`${buttonVariants({
                    variant: "gray",
                    size: "lg",
                  })} !px-5 !w-full flex justify-between flex-wrap`}
                >
                  <span className="text-gray-500">Price:</span>
                  {data.data.price} zBTC
                </div>
                <div
                  className={`${buttonVariants({
                    variant: "gray",
                    size: "lg",
                  })} !px-5 !w-full flex justify-between flex-wrap`}
                >
                  <span className="text-gray-500">Royalty:</span>
                  {data.data.royalty}%
                </div>
                <div
                  className={`${buttonVariants({
                    variant: "gray",
                    size: "lg",
                  })} !px-5 !w-full flex justify-between flex-wrap`}
                >
                  <span className="text-gray-500">Net Amout:</span>
                  {existedNetAmount}
                </div>
              </div>
              <div
                className={`${buttonVariants({
                  variant: "gray",
                  size: "lg",
                })} !px-5 !w-full flex justify-between flex-wrap`}
              >
                <span className="text-gray-500">State:</span> Listed
              </div>
            </div>
          </div>
        </form>
      </Form>
      {isUpdated && formData && (
        <UpdatingModal
          formData={formData}
          setIsUpdated={setIsUpdated}
          form={form}
          tokenId={data.data.tokenId}
        />
      )}
    </>
  );
}
