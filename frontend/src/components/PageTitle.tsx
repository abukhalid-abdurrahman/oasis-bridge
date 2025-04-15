interface PageTitleProps {
  title: string,
  parentClassName?: string,
  className?: string,
}

export default function PageTitle({ title, className, parentClassName }: PageTitleProps) {
  return (
    <div className={`mb-8 ${parentClassName}`}>
      <h2 className={`text-white font-bold text-4xl ${className}`}>{title}</h2>
    </div>
  )
}
