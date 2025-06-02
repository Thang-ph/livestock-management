'use client';

import type { ColumnDef } from '@tanstack/react-table';
import { useSearchParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { CellAction } from './cell-action';

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
      return <span>{serialNumber}</span>;
    }
  },

  {
    accessorKey: 'name',
    header: 'Tên',
    enableSorting: true
  },

  {
    accessorKey: 'description',
    header: 'Mô tả chức năng',
    enableSorting: true
  },

  {
    accessorKey: 'type',
    header: 'Loại thuốc',
    enableSorting: true,
    cell: ({ row }) => {
      const type = row.original.type;

      if (type === 'VACCINE') {
        return (
          <Badge className="bg-green-500 hover:bg-green-600">Vaccine</Badge>
        );
      } else if (type === 'THUỐC_CHỮA_BỆNH') {
        return (
          <Badge className="bg-blue-500 hover:bg-blue-600">
            Thuốc chữa bệnh
          </Badge>
        );
      } else if (type === 'KHÁNG_SINH') {
        return (
          <Badge className="bg-purple-500 hover:bg-purple-600">
            Kháng sinh
          </Badge>
        );
      }

      return <span>{type}</span>;
    }
  },

  {
    id: 'actions',
    header: 'Hành động',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
