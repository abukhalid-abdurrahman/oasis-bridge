'use client'

import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from '@/components/ui/table'

export default function SellBuyData({ data }: { data: any[] }) {
  const isEmpty = !data || data.length === 0;

  return (
    <div className="mt-10 p-6 bg-zinc-950 rounded-2xl shadow-lg border border-zinc-800">
      <h2 className="text-2xl font-semibold mb-6 text-white">
        Sell / Buy History
      </h2>
      <div className="rounded-xl overflow-hidden border border-zinc-800">
        <Table>
          <TableHeader className="bg-zinc-900">
            <TableRow className='hover:bg-transparent'>
              <TableHead className="text-zinc-400 uppercase tracking-wider text-sm">Type</TableHead>
              <TableHead className="text-zinc-400 uppercase tracking-wider text-sm">Price</TableHead>
              <TableHead className="text-zinc-400 uppercase tracking-wider text-sm">Amount</TableHead>
              <TableHead className="text-zinc-400 uppercase tracking-wider text-sm">Date</TableHead>
            </TableRow>
          </TableHeader>
          <TableBody>
            {isEmpty ? (
              <TableRow className='hover:bg-transparent'>
                <TableCell colSpan={4} className="text-center text-zinc-500 py-8">
                  No purchases or sales yet.
                </TableCell>
              </TableRow>
            ) : (
              data.map((row, i) => (
                <TableRow
                  key={i}
                  className="hover:bg-zinc-900 transition-colors"
                >
                  <TableCell className={row.type === 'Buy' ? 'text-emerald-400 font-medium' : 'text-rose-400 font-medium'}>
                    {row.type}
                  </TableCell>
                  <TableCell className="text-zinc-200 py-5">{row.asset}</TableCell>
                  <TableCell className="text-zinc-200 py-5">{row.amount}</TableCell>
                  <TableCell className="text-zinc-500 py-5">{row.date}</TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </div>
    </div>
  )
}
