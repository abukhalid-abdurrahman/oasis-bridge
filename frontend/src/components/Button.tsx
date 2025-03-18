interface Button {
  children: React.ReactNode,
  onClick?: () => void,
  className?: string,
  type?: "submit" | "reset" | "button" | undefined,
  disabled?: boolean
}

export default function Button({ children, onClick, className, type, disabled }: Button) {
  return (
    <button onClick={ onClick } className={`btn ${className}`} type={ type } disabled={disabled}>
      { children }
    </button>
  )
}
