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
    accessorKey: 'status',
    header: 'Mã kiểm dịch',
    enableSorting: true
  },
  {
    accessorKey: 'quantity',
    header: 'Loài vật nuôi'
  },
  {
    accessorKey: 'quantity',
    header: 'Cân nặng'
  },

  {
    accessorKey: 'quantity',
    header: 'Trạng thái'
  },

  {
    accessorKey: 'quantity',
    header: 'Ngày chọn'
  },

  {
    accessorKey: 'quantity',
    header: 'Ngày xuất'
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
