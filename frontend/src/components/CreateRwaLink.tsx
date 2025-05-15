'use client'

import { useUserStore } from '@/store/useUserStore'
import Link from 'next/link'
import React from 'react'

export default function CreateRwaLink() {
  const { user } = useUserStore()
  if (user) {
    return <Link href={`${user ? "/rwa/create" : "?signin=true"}`}>Create RWA</Link>
  }
}
