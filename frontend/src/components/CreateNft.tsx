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
import {
  getDefaultValuesFromFields,
  tokenizationFieldsAutomobiles,
  tokenizationFieldsBase,
  tokenizationFieldsRealEstate,
} from "@/lib/helpers/tokenizationFields";
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
import { useCallback, useEffect, useMemo, useState } from "react";
import { ASSET_TYPES } from "@/lib/constants";
import { TokenizationField } from "@/lib/types";

export default function CreateNft() {
  const [preview, setPreview] = useState<string | null>(null);
  const [isSecondStep, setIsSecondStep] = useState(false);
  const [selectedAssetType, setSelectedAssetType] = useState<string>("");

  const onDrop = useCallback((acceptedFiles: File[]) => {
    const file = acceptedFiles[0];
    const url = URL.createObjectURL(file);
    setPreview(url);
  }, []);

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    accept: {
      "image/*": [],
    },
    multiple: false,
  });

  const getFieldsByAssetType = (type: string): TokenizationField[] => {
    switch (type) {
      case "Automobiles":
        return tokenizationFieldsAutomobiles;
      case "Real Estate":
        return tokenizationFieldsRealEstate;
      default:
        return [];
    }
  };

  const allFields: any[] = [
    ...tokenizationFieldsBase,
    ...getFieldsByAssetType(selectedAssetType),
  ];

  const FormSchema = z.object(
    Object.fromEntries(allFields.map((field) => [field.name, field.validation]))
  );

  const defaultValues = getDefaultValuesFromFields(allFields);

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues,
  });

  const assetType = form.watch("asset_type");

  useEffect(() => {
    if (assetType) {
      setSelectedAssetType(assetType);
    }
  }, [assetType]);

  return (
    <>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit((data) => {
            console.log(data);
          })}
          className="flex gap-20 items-start lg:gap-5 md:flex-col"
        >
          <div className="w-1/2 aspect-square h-auto rounded-2xl bg-textGray md:w-full">
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
          <div className="w-1/2 md:w-full">
            <PageTitle title="Create your own NFT" />
            <div
              className={`flex flex-col gap-2 firstStep ${
                isSecondStep ? "hidden" : "block"
              }`}
            >
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
                    <FormItem className="w-1/3">
                      <FormControl>
                        <Input type="number" placeholder="Price" {...field} />
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
                  />
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
                name="proofOfOwnershipDocument"
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
                    <Select onValueChange={field.onChange} value={field.value}>
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
            </div>
            <div
              className={`flex flex-col gap-2 isSecondStep ${
                isSecondStep ? "block" : "hidden"
              }`}
            >
              <h2 className="h2 mb-2 text-white border-b border-textGray pb-2">
                Additional fields for {assetType}
              </h2>
              {getFieldsByAssetType(selectedAssetType).map((item) => (
                <FormField
                  key={item.name}
                  control={form.control}
                  name={item.name}
                  defaultValue={item.defaultValue}
                  render={({ field }) => (
                    <FormItem>
                      <FormControl>
                        <Input placeholder={item.placeholder} {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              ))}
            </div>

            <div className="flex gap-2 mt-2">
              <Button
                onClick={() => {
                  setIsSecondStep(false);
                }}
                variant="gray"
                type="button"
                size="xl"
                className={`w-full ${isSecondStep ? "block" : "hidden"}`}
              >
                Prev Step
              </Button>
              <Button
                onClick={async () => {
                  const isValid = await form.trigger([
                    "title",
                    "assetDescription",
                    "uniqueIdentifier",
                    "network",
                    "price",
                    "royalty",
                    "ownerContact",
                  ]);
                  if (isValid) {
                    setIsSecondStep(true);
                  }
                }}
                variant="gray"
                type="button"
                size="xl"
                className={`w-full ${isSecondStep ? "hidden" : "block"}`}
              >
                Next Step
              </Button>
              <Button
                variant="gray"
                type="submit"
                size="xl"
                className={`w-full ${isSecondStep ? "block" : "hidden"}`}
              >
                Tokenize
              </Button>
            </div>
          </div>
        </form>
      </Form>
    </>
  );
}
