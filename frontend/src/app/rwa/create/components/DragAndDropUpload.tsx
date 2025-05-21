import { useState } from "react";
import { FileRejection, useDropzone } from "react-dropzone";
import { Loader2 } from "lucide-react";
import {
  FormField,
  FormItem,
  FormControl,
  FormMessage,
} from "@/components/ui/form";
import { uploadFile } from "@/lib/scripts/script";
import { useFormContext } from "react-hook-form";

interface DragAndDropProps {
  control: any;
  name: string;
  label?: string;
}

export function DragAndDropUpload({ control, name }: DragAndDropProps) {
  const [preview, setPreview] = useState<string | null>(null);
  const [isUploading, setIsUploading] = useState(false);

  const { setError, clearErrors } = useFormContext();

  const onDrop = async (acceptedFiles: File[]) => {
    const file = acceptedFiles[0];
    if (!file) return;

    setPreview(URL.createObjectURL(file));
    setIsUploading(true);
    clearErrors(name);

    try {
      const uploadedUrl = await uploadFile(file);
      if (uploadedUrl.includes("http")) {
        formField.onChange(uploadedUrl);
      } else {
        throw new Error("Invalid upload URL");
      }
    } catch (err: any) {
      setError(name, {
        type: "manual",
        message: err.message || "Upload failed",
      });
    } finally {
      setIsUploading(false);
    }
  };

  const onDropRejected = (fileRejections: FileRejection[]) => {
    const rejection = fileRejections[0];
    const error = rejection.errors[0];

    if (error.code === "file-too-large") {
      setError(name, {
        type: "manual",
        message: "File must be smaller than 10MB",
      });
    } else {
      setError(name, { type: "manual", message: error.message });
    }
  };

  const { getRootProps, getInputProps, isDragActive } = useDropzone({
    onDrop,
    onDropRejected,
    maxSize: 10 * 1024 * 1024,
    accept: { "image/*": [] },
    multiple: false,
  });

  return (
    <FormField
      control={control}
      name={name}
      render={({ field }) => {
        formField = field;
        return (
          <FormItem className="w-1/2 md:w-full">
            <FormControl>
              <div className="aspect-square h-auto rounded-2xl bg-textGray">
                <div
                  {...getRootProps({
                    className: `flex justify-center items-center border-2 border-dashed border-gray p-4 rounded-md text-center cursor-pointer h-full transition-colors duration-150 ${
                      isUploading
                        ? "bg-gray-700 cursor-not-allowed opacity-70"
                        : "hover:bg-gray-50"
                    }`,
                  })}
                >
                  <input {...getInputProps()} />
                  {isUploading ? (
                    <div className="flex flex-col items-center gap-2 text-white">
                      <Loader2 className="animate-spin" size={32} />
                      <p className="p">Uploading...</p>
                    </div>
                  ) : preview || field.value ? (
                    <img
                      src={preview || field.value}
                      alt="Preview"
                      className="mx-auto max-h-64 rounded-md object-contain"
                    />
                  ) : (
                    <div className="flex flex-col gap-3 justify-center text-white">
                      <h2 className="h1">RWA Image</h2>
                      <p className="p">
                        {isDragActive
                          ? "Drop the file here..."
                          : "Drag & drop or click to upload"}
                      </p>
                    </div>
                  )}
                </div>
              </div>
            </FormControl>
            <FormMessage />
          </FormItem>
        );
      }}
    />
  );
}

let formField: any;
