import "@/lib/styles/loading.css";

export default function LoadingAlt({ className }: { className?: string }) {
  return (
    <div className={`absolute top-0 left-0 right-0 bottom-0 z-40 bg-black bg-opacity-25 ${className}`}>
      <div className="loading-spinner-alt z-50"></div>
    </div>
  );
}
