'use client'

import { useUserStore } from '@/store/useUserStore'
import Link from 'next/link'

export default function CreateRwaLink() {
  const { user } = useUserStore()
  return (
    <Link href={`${user ? "/rwa/create" : "?signin=true"}`}>Create RWA</Link>
  )
}
