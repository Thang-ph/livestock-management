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
    accessorKey: 'description',
    header: 'Mô tả',
    enableSorting: true
  },

  {
    accessorKey: 'growthRate',
    header: 'Tốc độ sinh trưởng',
    enableSorting: true,
    cell: ({ row }) => {
      const growthRate = row.original.growthRate;
      return (
        <span>
          {growthRate != null ? `${growthRate} cm` : 'Chưa thực hiện'}
        </span>
      );
    }
  },

  {
    accessorKey: 'dressingPercentage',
    header: 'Tỷ lệ dress',
    enableSorting: true,
    cell: ({ row }) => {
      const dressingPercentage = row.original.dressingPercentage;
      return (
        <span>
          {dressingPercentage != null
            ? `${dressingPercentage} %`
            : 'Chưa thực hiện'}
        </span>
      );
    }
  },

  {
    accessorKey: 'createdAt',
    header: 'Ngày tạo',
    enableSorting: true,
    cell: ({ row }) => __helpers.convertToDateDDMMYYYY(row.original.createdAt)
  },
  {
    accessorKey: 'updatedAt',
    header: 'Ngày cập nhật',
    enableSorting: true,
    cell: ({ row }) => __helpers.convertToDateDDMMYYYY(row.original.updatedAt)
  },
  {
    accessorKey: 'createdBy',
    header: 'Người tạo',
    enableSorting: true
  },
  {
    accessorKey: 'updatedBy',
    header: 'Người cập nhật',
    enableSorting: true
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
