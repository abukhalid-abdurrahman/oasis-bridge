import { useEffect, useState } from "react";
import QRCode from "qrcode";
import Image from "next/image";

export default function QRCodeDisplay({ text }: { text: string }) {
  const [qrCode, setQrCode] = useState("");

  useEffect(() => {
    if (text) {
      const generateQRCode = async () => {
        try {
          const url = await QRCode.toDataURL(text);
          setQrCode(url);
        } catch (err) {
          console.error("Ошибка генерации QR-кода:", err);
        }
      };

      generateQRCode();
    }
  }, [text]);

  return (
    <div className="flex flex-col items-center">
      {qrCode && (
        <Image
          src={qrCode}
          alt="Your virtual account"
          width={150}
          height={150}
        />
      )}
    </div>
  );
}
