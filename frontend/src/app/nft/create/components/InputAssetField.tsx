import {
  FormControl,
  FormField,
  FormItem,
  FormMessage,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import { TokenizationField } from "@/lib/types";
import { Dispatch, SetStateAction } from "react";
import { UseFormReturn } from "react-hook-form";

interface InputAssetField {
  item: TokenizationField;
  form: UseFormReturn<
    {
      [x: string]: any;
    },
    any,
    {
      [x: string]: any;
    }
  >;
  setIsMapOpen: Dispatch<SetStateAction<boolean>>;
  coords: {
    latitude: number;
    longitude: number;
  } | null;
}

export default function InputAssetField({
  item,
  form,
  setIsMapOpen,
  coords,
}: InputAssetField) {
  return (
    <FormField
      control={form.control}
      name={item.name}
      defaultValue={item.defaultValue}
      render={({ field }) => (
        <FormItem>
          <FormControl>
            {item.name === "geolocation" ? (
              <Input
                onClick={() => {
                  setIsMapOpen(true);
                }}
                placeholder={item.placeholder}
                value={coords ? coords.latitude + " " + coords.longitude : ""}
                onChange={() => {}}
                className="cursor-pointer"
              />
            ) : (
              <Input placeholder={item.placeholder} {...field} />
            )}
          </FormControl>
          <FormMessage />
        </FormItem>
      )}
    />
  );
}
