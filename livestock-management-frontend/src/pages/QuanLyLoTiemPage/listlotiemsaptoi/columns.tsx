'use client';

import type { ColumnDef } from '@tanstack/react-table';
import __helpers from '@/helpers';
import { useSearchParams } from 'react-router-dom';

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
    accessorKey: 'conductName',
    header: 'Người thực hiện',
    enableSorting: true
  },

  {
    accessorKey: 'medicineName',
    header: 'Loại thuốc'
  },

  {
    accessorKey: 'diseaseName',
    header: 'Bệnh',
    enableSorting: true
  },

  {
    accessorKey: 'schedulteTime',
    header: 'Ngày dự kiến hoàn thành',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="font-medium">
        {__helpers.convertToDateDDMMYYYY(row.original.schedulteTime)}
      </div>
    )
  }
];
