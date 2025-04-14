import { z } from "zod";

export const tokenizationFieldsBase = [
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
    type: "url",
    validation: z.string().url({ message: "Invalid URL" }),
  },
  {
    name: "royalty",
    placeholder: "Royalty",
    type: "number",
    validation: z.coerce.number().min(1, { message: "Royalty is required" }),
  },
  {
    name: "network",
    placeholder: "Network",
    type: "string",
    validation: z.enum(["SOL"]),
  },
];
