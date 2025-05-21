import { optional, z, ZodObject, ZodTypeAny } from "zod";
import { ASSET_TYPES, INSURANSE_STATUSES, PROPERTY_TYPES } from "../constants";
import { TokenizationField } from "../types";

export const tokenizationFieldsBase: TokenizationField[] = [
  {
    name: "image",
    placeholder: "Image",
    type: "string",
    validation: z.string().url({ message: "Please provide a valid image" }),
  },
  {
    name: "title",
    placeholder: "Title",
    type: "string",
    validation: z
      .string()
      .min(2, { message: "Title must be at least 2 characters" })
      .max(100, { message: "Title must be less than 100 characters" }),
  },
  {
    name: "assetDescription",
    placeholder: "Description",
    type: "string",
    validation: z
      .string()
      .min(10, { message: "Description must be at least 10 characters" })
      .max(1000, { message: "Description must be under 1000 characters" }),
  },
  {
    name: "proofOfOwnershipDocument",
    placeholder: "Proof of ownership document",
    type: "file",
    validation: z.string().url({
      message: "Please provide a valid image of the ownership document",
    }),
  },
  {
    name: "uniqueIdentifier",
    placeholder: "Unique identifier",
    type: "string",
    validation: z.string().regex(/^[a-zA-Z0-9-_]+$/, {
      message:
        "Identifier must contain only letters, numbers, dashes or underscores",
    }),
  },
  {
    name: "network",
    placeholder: "Network",
    type: "string",
    validation: z.enum(["Solana"], {
      message: "Only 'Solana' is supported for now",
    }),
  },
  {
    name: "royalty",
    placeholder: "Royalty",
    type: "number",
    validation: z.coerce
      .number({ invalid_type_error: "Royalty must be a number" })
      .min(1, { message: "Royalty must be at least 1%" })
      .max(100, { message: "Royalty must be no more than 100%" }),
  },
  {
    name: "price",
    placeholder: "Price",
    type: "number",
    validation: z.coerce
      .number({ invalid_type_error: "Price must be a number" })
      .min(0.0001, { message: "The price must be greater than 0.0001" }),
  },
  {
    name: "ownerContact",
    placeholder: "Owner contact",
    type: "string",
    validation: z
      .string()
      .min(5, { message: "Owner contact must be at least 5 characters" })
      .max(100, { message: "Owner contact must be under 100 characters" })
      .email({ message: "Must be a valid email" }),
  },
  {
    name: "assetType",
    placeholder: "AssetType",
    type: "string",
    validation: z.enum(
      ASSET_TYPES.map((asset) => asset.replace(/\s/g, "")) as [
        string,
        ...string[]
      ],
      { message: "Asset type is required" }
    ),
  },
];

export const tokenizationFieldsAutomobiles: TokenizationField[] = [
  {
    name: "serialNumber",
    placeholder: "VIN/Serial Number",
    type: "string",
    validation: z.string().regex(/^[A-HJ-NPR-Z0-9]{11,17}$/, {
      message: "VIN must be 11–17 characters (A-Z, 0–9), no I, O, Q",
    }),
    defaultValue: "",
  },
  {
    name: "geolocation",
    placeholder: "Geolocation (Latitude, Longitude)",
    type: "string",
    validation: z.string().min(1, { message: "Geolocation is requred" }),
    defaultValue: "",
  },
  {
    name: "manufactureYear",
    placeholder: "Manufacture Year",
    type: "number",
    validation: z.string().refine((val) => /^\d{4}$/.test(val), {
      message: "Manufacture Year must be a 4-digit year",
    }),
    defaultValue: "",
  },
  {
    name: "insuranceStatus",
    placeholder: "Insurance Status",
    type: "string",
    validation: z.enum([...INSURANSE_STATUSES] as [string, ...string[]], {
      message: "Please select a valid insurance status",
    }),
    defaultValue: "",
    HTMLType: "select",
    selectItems: [...INSURANSE_STATUSES],
  },
];

export const tokenizationFieldsRealEstate: TokenizationField[] = [
  {
    name: "geolocation",
    placeholder: "Asset Location (Geolocation / Country)",
    type: "object",
    validation: z.object({
      latitude: z
        .number({ invalid_type_error: "Latitude must be a number" })
        .min(-90, { message: "Latitude must be between -90 and 90" })
        .max(90, { message: "Latitude must be between -90 and 90" }),
      longitude: z
        .number({ invalid_type_error: "Longitude must be a number" })
        .min(-180, { message: "Longitude must be between -180 and 180" })
        .max(180, { message: "Longitude must be between -180 and 180" }),
    }),
    defaultValue: {
      latitude: "",
      longitude: "",
    },
  },
  {
    name: "valuationDate",
    placeholder: "Valuation Date",
    type: "string",
    HTMLType: "date",
    validation: z
      .string()
      .regex(/^\d{4}-\d{2}-\d{2}$/, {
        message: "Valuation Date must be in YYYY-MM-DD format",
      })
      .refine(
        (date) => {
          const inputDate = new Date(date);
          const today = new Date();
          return inputDate <= today;
        },
        {
          message: "Valuation Date cannot be in the future",
        }
      ),
    defaultValue: "",
  },
  {
    name: "area",
    placeholder: "Area (in square meters)",
    type: "number",
    validation: z.coerce
      .number()
      .min(0.1, { message: "Area must be greater than 0" }),
    defaultValue: "",
  },
  {
    name: "propertyType",
    placeholder: "Property Type",
    type: "string",
    HTMLType: "select",
    selectItems: [...PROPERTY_TYPES],
    validation: z.enum([...PROPERTY_TYPES] as [string, ...string[]], {
      message: "Please select a valid property type",
    }),
    defaultValue: "",
  },
  {
    name: "constructionYear",
    placeholder: "Construction Year",
    type: "number",
    validation: z.string().refine((val) => /^\d{4}$/.test(val), {
      message: "Construction Year must be a 4-digit year",
    }),
    defaultValue: "",
  },
  {
    name: "insuranceStatus",
    placeholder: "Insurance Status",
    type: "string",
    HTMLType: "select",
    selectItems: [...INSURANSE_STATUSES],
    validation: z.enum([...INSURANSE_STATUSES] as [string, ...string[]], {
      message: "Please select a valid insurance status",
    }),
    defaultValue: "",
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
