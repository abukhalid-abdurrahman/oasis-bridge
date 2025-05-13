"use client";

import { Button } from "@/components/ui/button";
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
} from "@/components/ui/form";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectGroup,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { ASSET_TYPES } from "@/lib/constants";
import { zodResolver } from "@hookform/resolvers/zod";
import { Dispatch, SetStateAction, useMemo, useState } from "react";
import { useForm } from "react-hook-form";
import { z } from "zod";

interface FiltersFormProps {
  reqParams: any;
  setReqParams: Dispatch<SetStateAction<any>>;
  setIsFiltersOpen?: Dispatch<SetStateAction<boolean>>;
}

export default function FiltersForm({
  reqParams,
  setReqParams,
  setIsFiltersOpen,
}: FiltersFormProps) {
  const [inputClasses] = useState(
    "px-2 py-1 bg-transparent border-textGray text-white rounded-sm text-sm w-full lg:text-base lg:text-black"
  );

  const FormSchema = z.object({
    assetType: z.any(),
    priceMin: z.any(),
    priceMax: z.any(),
    sortBy: z.any(),
    sortOrder: z.any(),
  });

  const form = useForm<z.infer<typeof FormSchema>>({
    resolver: zodResolver(FormSchema),
    defaultValues: {
      assetType: "",
      priceMin: "",
      priceMax: "",
      sortBy: "",
      sortOrder: "",
    },
  });

  const onSubmit = (data: z.infer<typeof FormSchema>) => {
    setReqParams((prevState: any) => {
      return {
        ...prevState,
        ...data,
      };
    });
  };

  return (
    <Form {...form}>
      <form
        className="flex gap-3 text-sm text-nowrap lg:flex-col lg:text-base"
        onSubmit={form.handleSubmit(onSubmit)}
      >
        <div className="flex items-center gap-3 lg:justify-between">
          <p className="">Price</p>
          <div className="flex gap-1 items-center max-w-36 lg:max-w-40">
            <FormField
              control={form.control}
              name="priceMin"
              render={({ field }) => (
                <>
                  <FormItem className="w-full">
                    <FormControl>
                      <Input
                        type="number"
                        className={`${inputClasses} text-right`}
                        placeholder="min"
                        {...field}
                      />
                    </FormControl>
                  </FormItem>
                </>
              )}
            />
            <FormField
              control={form.control}
              name="priceMax"
              render={({ field }) => (
                <>
                  <FormItem className="w-full">
                    <FormControl>
                      <Input
                        type="number"
                        className={`${inputClasses} text-right`}
                        placeholder="max"
                        {...field}
                      />
                    </FormControl>
                  </FormItem>
                </>
              )}
            />
          </div>
        </div>
        <FormField
          control={form.control}
          name="assetType"
          render={({ field }) => (
            <FormItem className="flex gap-2 items-center lg:justify-between">
              <FormLabel className="lg:text-base">Asset Type</FormLabel>
              <Select
                onValueChange={field.onChange}
                value={field.value}
                defaultValue={field.value}
              >
                <FormControl>
                  <SelectTrigger
                    className={`${inputClasses} !mt-0 lg:max-w-[300px] sm:!max-w-[160px]`}
                  >
                    <SelectValue placeholder="AssetType" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectGroup>
                    {ASSET_TYPES.map((type, i) => (
                      <SelectItem key={i} value={type.replace(/\s/g, "")}>
                        {type}
                      </SelectItem>
                    ))}
                  </SelectGroup>
                </SelectContent>
              </Select>
              {/* <FormMessage /> */}
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="sortBy"
          render={({ field }) => (
            <FormItem className="flex gap-2 items-center lg:justify-between">
              <FormLabel className="lg:text-base">Sort by</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl>
                  <SelectTrigger
                    className={`${inputClasses} !mt-0 lg:max-w-[300px] sm:!max-w-[160px]`}
                  >
                    <SelectValue placeholder="Sort by" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectGroup>
                    <SelectItem value="Price">Price</SelectItem>
                    <SelectItem value="CreatedAt">Date of Creation</SelectItem>
                  </SelectGroup>
                </SelectContent>
              </Select>
              {/* <FormMessage /> */}
            </FormItem>
          )}
        />
        <FormField
          control={form.control}
          name="sortOrder"
          render={({ field }) => (
            <FormItem className="flex gap-2 items-center lg:justify-between">
              <FormLabel className="lg:text-base">Sort order</FormLabel>
              <Select onValueChange={field.onChange} defaultValue={field.value}>
                <FormControl>
                  <SelectTrigger
                    className={`${inputClasses} !mt-0 lg:max-w-[300px] sm:!max-w-[160px]`}
                  >
                    <SelectValue placeholder="Sort order" />
                  </SelectTrigger>
                </FormControl>
                <SelectContent>
                  <SelectGroup>
                    <SelectItem value="Asc">Asc</SelectItem>
                    <SelectItem value="Desc">Desc</SelectItem>
                  </SelectGroup>
                </SelectContent>
              </Select>
              {/* <FormMessage /> */}
            </FormItem>
          )}
        />
        <div className="flex gap-3 lg:hidden">
          <Button variant="gray" size="sm" type="submit">
            Apply filters
          </Button>
          <Button
            onClick={() => {
              form.reset();
              setIsFiltersOpen ? setIsFiltersOpen(false) : null;
            }}
            variant="gray"
            size="sm"
            type="submit"
          >
            Clear filters
          </Button>
        </div>
        <div className="hidden lg:flex lg:flex-col lg:gap-3">
          <Button
            onClick={() => (setIsFiltersOpen ? setIsFiltersOpen(false) : null)}
            variant="gray"
            size="default"
            type="submit"
            className="hidden lg:flex lg:mt-3"
          >
            Apply filters
          </Button>
          <Button
            onClick={() => {
              form.reset();
              setIsFiltersOpen ? setIsFiltersOpen(false) : null;
            }}
            variant="gray"
            size="default"
            type="submit"
          >
            Clear filters
          </Button>
        </div>
      </form>
    </Form>
  );
}
