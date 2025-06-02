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
    header: 'Loài vật nuôi',
    enableSorting: true
  },
  {
    accessorKey: 'weight',
    header: 'Cân nặng',
    enableSorting: true
  },
  {
    accessorKey: 'status',
    header: 'Trạng thái',
    enableSorting: true
  },

  {
    accessorKey: 'createdAt',
    header: 'Ngày chọn',
    cell: ({ row }) => {
      const date = row.getValue('createdAt');
      return (
        <div className="flex items-center gap-2">
          <span>{__helpers.convertToDateDDMMYYYY(date)}</span>
        </div>
      );
    }
  },
  {
    accessorKey: 'exportedDate',
    header: 'Ngày xuất',
    cell: ({ row }) => {
      const date = row.getValue('exportedDate');
      return (
        <div className="flex items-center gap-2">
          <span>{__helpers.convertToDateDDMMYYYY(date)}</span>
        </div>
      );
    }
  },
  {
    id: 'actions',
    header: 'Hành động',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
