import { ColumnDef } from '@tanstack/react-table';
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
    accessorKey: 'inspectionCode',
    header: 'Mã kiểm dịch',
    enableSorting: true
  },

  {
    accessorKey: 'specieName',
    header: 'Loài',
    enableSorting: true
  },

  {
    accessorKey: 'createdAt',
    header: 'Ngày chọn',
    enableSorting: true,
    cell: ({ row }) => {
      const date = new Date(row.original.createdAt);
      return __helpers.convertToDate(date);
    }
  },
  {
    accessorKey: 'importedDate',
    header: 'Ngày nhập',
    enableSorting: true,
    cell: ({ row }) => {
      const date = new Date(row.original.importedDate);
      return __helpers.convertToDate(date);
    }
  },

  {
    accessorKey: 'status',
    header: 'Trạng thái'
  },

  {
    id: 'Hành động',
    header: 'Hành động',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
