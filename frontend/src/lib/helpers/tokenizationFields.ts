import { z, ZodObject, ZodTypeAny } from "zod";
import {
  ASSET_TYPES,
  INSURANSE_STATUSES,
  PROPERTY_TYPES,
} from "../constants";
import { TokenizationField } from "../types";

export const tokenizationFieldsBase: TokenizationField[] = [
  {
    name: "image",
    placeholder: "Image",
    type: "string",
    validation: z.string().url({ message: "Image is required" }),
  },
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
    name: "proofOfOwnershipDocument",
    placeholder: "Proof of ownership document",
    type: "file",
    validation: z.string().url({ message: "Proof of ownership document is required" }),
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
    validation: z.coerce
      .number()
      .min(1, { message: "Royalty is required" })
      .max(100, { message: "Royalty must be no more than 100" }),
  },
  {
    name: "price",
    placeholder: "Price",
    type: "number",
    validation: z.coerce
      .number()
      .min(0.0001, { message: "The price must be above 0.0001" }),
  },
  {
    name: "ownerContact",
    placeholder: "Owner contact",
    type: "string",
    validation: z.string().min(1, { message: "Owner contact is required" }),
  },
  {
    name: "assetType",
    placeholder: "AssetType",
    type: "string",
    validation: z.enum(
      ASSET_TYPES.map((asset) => asset.replace(/\s/g, "")) as [
        string,
        ...string[]
      ]
    ),
  },
];

export const tokenizationFieldsAutomobiles: TokenizationField[] = [
  {
    name: "serial_number",
    placeholder: "VIN/Serial Number",
    type: "string",
    validation: z.string().min(1, { message: "VIN/Serial Number is required" }),
    defaultValue: "",
  },
  {
    name: "geolocation",
    placeholder: "Geolocation",
    type: "string",
    validation: z.any().refine(
      (val) => {
        return val !== null && val !== undefined && val !== "";
      },
      {
        message: "Geolocation is required",
      }
    ),
    defaultValue: "",
  },
  {
    name: "manufacture_year",
    placeholder: "Manufacture Year",
    type: "string",
    validation: z.string().min(1, { message: "Manufacture Year is required" }),
    defaultValue: "",
  },
  {
    name: "insurance_status",
    placeholder: "Insurance Status",
    type: "string",
    validation: z.string().min(1, { message: "Insurance Status is required" }),
    defaultValue: "",
  },
];

export const tokenizationFieldsRealEstate: TokenizationField[] = [
  {
    name: "geolocation",
    placeholder: "Asset Location (Geolocation / Country)",
    type: "string",
    validation: z.any().refine(
      (val) => {
        return val !== null && val !== undefined && val !== "";
      },
      {
        message: "Geolocation is required",
      }
    ),
    defaultValue: "",
  },
  {
    name: "valuationDate",
    placeholder: "Valuation Date",
    type: "string",
    validation: z.string().min(1, { message: "Valuation Date is required" }),
    defaultValue: "",
    HTMLType: "date",
  },
  {
    name: "area",
    placeholder: "Area",
    type: "number",
    validation: z.number().min(1, { message: "Area is required" }),
    defaultValue: "",
  },
  {
    name: "propertyType",
    placeholder: "Property Type",
    type: "string",
    validation: z.enum([...PROPERTY_TYPES] as [string, ...string[]]),
    defaultValue: "",
    HTMLType: "select",
    selectItems: [...PROPERTY_TYPES],
  },
  {
    name: "constructionYear",
    placeholder: "Construction Year",
    type: "number",
    validation: z
      .number()
      .min(1, { message: "Construction year is required" }),
    defaultValue: "",
  },
  {
    name: "insurance_status",
    placeholder: "Insurance Status",
    type: "string",
    validation: z.enum([...INSURANSE_STATUSES] as [string, ...string[]]),
    defaultValue: "",
    HTMLType: "select",
    selectItems: [...INSURANSE_STATUSES],
  },
];

export const getZodSchemaFromFields = (
  fields: TokenizationField[]
): ZodObject<any> => {
  const shape: Record<string, ZodTypeAny> = {};
  fields.forEach((field) => {
    shape[field.name] = field.validation;
  });
  return z.object(shape);
};

export const getDefaultValuesFromFields = (
  fields: TokenizationField[]
): Record<string, any> => {
  const values: Record<string, any> = {};
  fields.forEach((field) => {
    values[field.name] = "";
  });
  return values;
};
