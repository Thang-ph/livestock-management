'use client';

import type { ColumnDef } from '@tanstack/react-table';
import { useSearchParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import __helpers from '@/helpers';
import { useRouter } from '@/routes/hooks';
// import { CellAction } from './cell-action';

export const columns: ColumnDef<any>[] = [
  {
    accessorKey: 'STT',
    header: 'STT',
    enableSorting: true,
    cell: ({ row }) => {
      const [searchParams] = useSearchParams();
      const pageLimit = Number(searchParams.get('limit') || 10);
      const page = Number(searchParams.get('page') || 1);
      const rowIndex = row.index;
      const serialNumber = (page - 1) * pageLimit + rowIndex + 1;
      return <div className="font-medium">{serialNumber}</div>;
    }
  },
  {
    accessorKey: 'specieType',
    header: 'Vật nuôi',
    enableSorting: true
  },
  {
    accessorKey: 'quantity',
    header: 'Số lượng',
    enableSorting: true
  },

  {
    id: 'actions',
    cell: ({ row }) => {
      const router = useRouter();
      return (
        <Button
          onClick={() => {
            router.push(`/chi-tiet-ma-kiem-dich/${row.original.specieType}`);
          }}
        >
          Chi tiết
        </Button>
      );
    }
  }
];
