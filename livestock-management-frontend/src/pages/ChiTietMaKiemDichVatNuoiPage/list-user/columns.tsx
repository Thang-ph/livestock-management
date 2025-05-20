'use client';

import type { ColumnDef } from '@tanstack/react-table';
import { useSearchParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { LockIcon, UnlockIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import __helpers from '@/helpers';
import { CellAction } from './cell-action';
import { InspectionCodeRangeStatus } from '@/constants/data';

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
      return <div className="text-center font-medium">{serialNumber}</div>;
    }
  },
  {
    accessorKey: 'specieTypeList',
    header: 'Vật nuôi',
    enableSorting: true
  },
  {
    accessorKey: 'startCode',
    header: 'Số bắt đầu',
    enableSorting: true
  },
  {
    accessorKey: 'endCode',
    header: 'Số kết thúc',
    enableSorting: true
  },
  {
    accessorKey: 'status',
    header: 'Trạng thái',
    enableSorting: true,
    cell: ({ row }) => {
      const status = row.getValue(
        'status'
      ) as keyof typeof InspectionCodeRangeStatus;
      return (
        <Badge className="w-fit">{InspectionCodeRangeStatus[status]}</Badge>
      );
    }
  },
  {
    accessorKey: 'quantity',
    header: 'Số lượng',
    enableSorting: true
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
