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
import { Button } from "@/components/ui/button";
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
} from "@/components/ui/select";
import PageTitle from "@/components/PageTitle";
import { useEffect, useState } from "react";
import { ASSET_TYPES } from "@/lib/constants";
import { TokenizationField } from "@/lib/types";
import dynamic from "next/dynamic";
import { uploadFile } from "@/lib/scripts/script";
import { DragAndDropUpload } from "@/app/nft/create/components/DragAndDropUpload";
import InputAssetField from "@/app/nft/create/components/InputAssetField";
import SelectAssetField from "@/app/nft/create/components/SelectAssetField";
import TokenizationModal from "./TokenizationModal";
import DateAssetField from "./DateAssetField";
import { Loader2 } from "lucide-react";
import { mutateRwaToken } from "@/requests/postRequests";

const LocationPickerModal = dynamic(
  () => import("@/components/LocationPickerModal"),
  {
    ssr: false,
  }
);

export default function CreateNft() {
  const [isSecondStep, setIsSecondStep] = useState(false);
  const [selectedAssetType, setSelectedAssetType] = useState<string>("");
  const [coords, setCoords] = useState<{
    latitude: number;
    longitude: number;
  } | null>(null);
  const [isMapOpen, setIsMapOpen] = useState(false);
  const [isTokenized, setIsTokenized] = useState(false);
  const [netAmount, setNetAmount] = useState<number | string>("");
  const [isSuccessfullyDone, setIsSuccessfullyDone] = useState(false);
  const [isError, setIsError] = useState(false);
  const [tokenId, setTokenId] = useState("");

  const submit = mutateRwaToken();

  const getFieldsByAssetType = (type: string): TokenizationField[] => {
    switch (type) {
      case "Automobiles":
        return tokenizationFieldsAutomobiles;
      case "RealEstate":
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

  const assetType = form.watch("assetType");
  const price = form.watch("price");
  const royalty = form.watch("royalty");

  const onSubmit = (data: z.infer<typeof FormSchema>) => {
    setIsTokenized(true);
    submit.mutate(data, {
      onSuccess: (res) => {
        setIsSuccessfullyDone(true);
        setTokenId(res.data.tokenId);
      },
      onError: () => {
        setIsError(true);
      },
    });
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
    if (coords) {
      form.setValue("geolocation", {
        latitude: coords.latitude,
        longitude: coords.longitude,
      });
    }
  }, [coords]);

  useEffect(() => {
    if (assetType) {
      setSelectedAssetType(assetType);
    }
  }, [assetType]);

  return (
    <>
      <Form {...form}>
        <form
          onSubmit={form.handleSubmit(onSubmit)}
          className="flex gap-20 items-start lg:gap-5 md:flex-col"
        >
          <DragAndDropUpload control={form.control} name="image" />
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
                      <Input placeholder="Unique identifier" {...field} />
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
            <div
              className={`flex flex-col gap-2 isSecondStep ${
                isSecondStep ? "block" : "hidden"
              }`}
            >
              <h2 className="h2 mb-2 text-white border-b border-textGray pb-2">
                Additional fields for {assetType}
              </h2>
              {getFieldsByAssetType(selectedAssetType).map((item) => (
                <div key={item.name}>
                  {item?.HTMLType === "select" && (
                    <SelectAssetField item={item} form={form} />
                  )}
                  {!item?.HTMLType && (
                    <InputAssetField
                      item={item}
                      form={form}
                      setIsMapOpen={setIsMapOpen}
                      coords={coords}
                    />
                  )}
                  {item?.HTMLType === "date" && (
                    <DateAssetField item={item} form={form} />
                  )}
                </div>
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
                    "image",
                    "proofOfOwnershipDocument",
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
                onClick={() => setIsTokenized(true)}
              >
                Tokenize
              </Button>
            </div>
          </div>
        </form>
      </Form>
      {isMapOpen && (
        <LocationPickerModal
          onSelect={(newCoords) => {
            setCoords(newCoords);
            // setIsMapOpen(false);
          }}
          setIsOpen={setIsMapOpen}
        />
      )}
      {isTokenized && (
        <TokenizationModal
          isError={isError}
          isSuccessfullyDone={isSuccessfullyDone}
          setIsSuccessfullyDone={setIsSuccessfullyDone}
          tokenId={tokenId}
          setIsTokenized={setIsTokenized}
          setIsSecondStep={setIsSecondStep}
          form={form}
        />
      )}
    </>
  );
}
