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
    accessorKey: 'name',
    header: 'Tên',
    enableSorting: true
  },

  {
    accessorKey: 'estimatedQuantity',
    header: 'Tổng dự kiến nhập',
    enableSorting: true
  },

  {
    accessorKey: 'importedQuantity',
    header: 'Đã nhập',
    enableSorting: true
  },

  {
    accessorKey: 'expectedCompletionDate',
    header: 'Ngày dự kiến hoàn thành',
    enableSorting: true,
    cell: ({ row }) =>
      __helpers.convertToDateDDMMYYYY(row.original.expectedCompletionDate)
  },

  {
    accessorKey: 'status',
    header: 'Trạng thái'
  },

  {
    accessorKey: 'completionDate',
    header: 'Ngày hoàn thành',
    cell: ({ row }) => {
      return (
        <span>
          {row.original.completionDate != null
            ? __helpers.convertToDateDDMMYYYY(row.original.completionDate)
            : 'Chưa thực hiện'}
        </span>
      );
    }
  },
  {
    accessorKey: 'createdBy',
    header: 'Người tạo',
    enableSorting: true
  },

  {
    accessorKey: 'createdAt',
    header: 'Ngày tạo',
    enableSorting: true,
    cell: ({ row }) => __helpers.convertToDateDDMMYYYY(row.original.createdAt)
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
