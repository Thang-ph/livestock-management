'use client';

import type { ColumnDef } from '@tanstack/react-table';
import __helpers from '@/helpers';
import { useSearchParams } from 'react-router-dom';
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
      return <div className="text-center font-medium">{serialNumber}</div>;
    }
  },

  {
    accessorKey: 'code',
    header: 'Mã đơn',
    enableSorting: true
  },

  {
    accessorKey: 'customerName',
    header: 'Tên khách hàng',
    enableSorting: true
  },

  {
    accessorKey: 'phoneNumber',
    header: 'Số điện thoại',
    enableSorting: true
  },

  {
    accessorKey: 'total',
    header: 'Tổng số con',
    enableSorting: true
  },

  {
    accessorKey: 'received',
    header: 'Đã nhận',
    enableSorting: true
  },

  {
    accessorKey: 'status',
    header: 'Trạng thái',
    enableSorting: true
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
