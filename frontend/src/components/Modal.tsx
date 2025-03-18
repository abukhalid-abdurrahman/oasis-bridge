import "@/lib/styles/modal.css";
import Image from "next/image";
import Link from "next/link";
import { usePathname } from "next/navigation";

interface ModalProps {
  children: React.ReactNode;
  isNonUrlModal?: boolean;
  onCloseFunc?: () => void;
  isNonClosable?: boolean;
  className?: string;
}

export default function Modal({ children, isNonUrlModal, onCloseFunc, isNonClosable, className }: ModalProps) {
  return (
    <div className="modal-backdrop sm:px-5 relative">
      <div className={`modal-content ${className}`}>
        {!isNonUrlModal && !isNonClosable && (
          <Link
            href="/"
            className="modal-close !bg-gray rounded-full p-2 sm:!top-3 sm:!right-3"
          >
            <Image src="/close.svg" alt="Close" width={12} height={12} />
          </Link>
        )}
        {isNonUrlModal && !isNonClosable && (
          <button
            type="button"
            className="modal-close !bg-gray rounded-full p-2 sm:!top-3 sm:!right-3"
            onClick={onCloseFunc}
          >
            <Image src="/close.svg" alt="Close" width={12} height={12} />
          </button>
        )}
        {children}
      </div>
    </div>
  );
}
