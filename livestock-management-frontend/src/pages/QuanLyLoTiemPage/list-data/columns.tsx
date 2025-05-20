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
      return <span>{serialNumber}</span>;
    }
  },

  {
    accessorKey: 'batchVaccinationName',
    header: 'Tên lô tiêm',
    enableSorting: true
  },

  {
    accessorKey: 'lastDate',
    header: 'Ngày tiêm cuối',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="font-medium">
        {__helpers.convertToDateDDMMYYYY(row.original.lastDate)}
      </div>
    )
  },

  {
    accessorKey: 'livestockQuantity',
    header: 'Số lượng',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="text-gray-700">{row.original.livestockQuantity}</div>
    )
  },

  {
    accessorKey: 'medicineName',
    header: 'Loại thuốc',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="text-gray-700">{row.original.medicineName}</div>
    )
  },

  {
    accessorKey: 'diseasaName',
    header: 'Bệnh',
    enableSorting: true
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
