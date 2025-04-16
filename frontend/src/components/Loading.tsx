import "@/lib/styles/loading.css";

export default function Loading({
  className,
  classNameLoading,
  style,
}: {
  className?: string;
  classNameLoading?: string;
  style?: React.CSSProperties;
}) {
  return (
    <div className={className}>
      <div
        style={style}
        className={`loading-spinner ${classNameLoading}`}
      ></div>
    </div>
  );
}
