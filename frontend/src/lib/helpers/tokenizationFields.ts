import { z } from "zod";
import { ACCEPTED_DOCUMENT_TYPES, ASSET_TYPES, MAX_FILE_SIZE } from "../constants";
import { TokenizationField } from "../types";

export const tokenizationFieldsBase: TokenizationField[] = [
  {
    name: "title",
    placeholder: "Title",
    type: "string",
    validation: z.string().min(1, { message: "Name is required" }),
  },
  {
    name: "assetDescription",
    placeholder: "Description",
    type: "string",
    validation: z.string().min(1, { message: "Description is required" }),
  },
  {
    name: "proofOfOwnershipDocumet",
    placeholder: "Proof of ownership document",
    type: "file",
    validation: z
      .any()
      // .refine((file) => file?.size <= MAX_FILE_SIZE, `Max image size is 5MB.`)
      // .refine(
      //   (file) => ACCEPTED_DOCUMENT_TYPES.includes(file?.type),
      //   "Only .jpg, .jpeg, .png and .webp .pdf formats are supported."
      // ),
  },
  {
    name: "uniqueIdentifier",
    placeholder: "Unique identifier",
    type: "string",
    validation: z.string().min(1, { message: "Invalid URL" }),
  },
  {
    name: "network",
    placeholder: "Network",
    type: "string",
    validation: z.enum(["Solana"]),
  },
  {
    name: "royalty",
    placeholder: "Royalty",
    type: "number",
    validation: z.coerce.number().min(1, { message: "Royalty is required" }),
  },
  {
    name: "price",
    placeholder: "Price",
    type: "number",
    validation: z.coerce.number().min(1, { message: "Royalty is required" }),
  },
  {
    name: "owner_contact",
    placeholder: "Owner contact",
    type: "string",
    validation: z.string().min(1, { message: "Royalty is required" }),
  },
  {
    name: "asset_type",
    placeholder: "AssetType",
    type: "string",
    validation: z.enum([...ASSET_TYPES]),
  },
];

export const tokenizationFieldsAutomobiles: TokenizationField[] = [
  {
    name: "serial_number",
    placeholder: "VIN/Serial Number",
    type: "string",
    validation: z.string().min(1, { message: "VIN/Serial Number is required" }),
  },
  {
    name: "geolocation",
    placeholder: "Geolocation",
    type: "string",
    validation: z.string().min(1, { message: "Geolocation is required" }),
  },
  {
    name: "manufacture_year",
    placeholder: "Manufacture Year",
    type: "string",
    validation: z.string().min(1, { message: "Manufacture Year is required" }),
  },
  {
    name: "insurance_status",
    placeholder: "Insurance Status",
    type: "string",
    validation: z.string().min(1, { message: "Insurance Status is required" }),
  },
]

export const tokenizationFieldsRealEstate: TokenizationField[] = [
  {
    name: "geolocation",
    placeholder: "Asset Location (Geolocation / Country)",
    type: "string",
    validation: z.string().min(1, { message: "Asset Location is required" }),
  },
  {
    name: "valuation_date",
    placeholder: "Valuation Date",
    type: "string",
    validation: z.string().min(1, { message: "Valuation Date is required" }),
  },
  {
    name: "area",
    placeholder: "Area",
    type: "string",
    validation: z.string().min(1, { message: "Area is required" }),
  },
  {
    name: "property_type",
    placeholder: "Property Type",
    type: "string",
    validation: z.string().min(1, { message: "Property Type is required" }),
  },
  {
    name: "construction_year",
    placeholder: "Construction Year",
    type: "string",
    validation: z.string().min(1, { message: "Construction Year is required" }),
  },
  {
    name: "insurance_status",
    placeholder: "Insurance Status",
    type: "string",
    validation: z.string().min(1, { message: "Insurance Status is required" }),
  },
]
