'use client'

import { useUserStore } from '@/store/useUserStore'
import Link from 'next/link'

export default function RwaLink() {
  const { user } = useUserStore()
  return (
    <Link href={`${user ? "/rwa" : "?signin=true"}`}>RWA Market</Link>
  )
}
