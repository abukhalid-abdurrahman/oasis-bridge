'use client'

import { useUserStore } from '@/store/useUserStore'
import Link from 'next/link'

export default function MyRwaLink() {
  const { user } = useUserStore()
  return (
    <Link href={`${user ? "/rwa/me" : "?signin=true"}`}>My RWAs</Link>
  )
}
