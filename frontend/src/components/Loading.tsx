import "@/lib/styles/loading.css";

export default function Loading({ className }: { className?: string }) {
  return (
    <div className={className}>
      <div className="loading-spinner"></div>
    </div>
  );
}
