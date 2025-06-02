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
    header: 'Tên bệnh',
    enableSorting: true
  },

  {
    accessorKey: 'symptom',
    header: 'Triệu chứng',
    enableSorting: true,
    cell: ({ getValue }) => {
      const symptom = getValue() as string;
      return (
        <div className="max-w-xs truncate" title={symptom}>
          {symptom}
        </div>
      );
    }
  },

  {
    accessorKey: 'description',
    header: 'Mô tả',
    enableSorting: true,
    cell: ({ getValue }) => {
      const description = getValue() as string;
      return (
        <div className="max-w-xs truncate" title={description}>
          {description}
        </div>
      );
    }
  },

  {
    accessorKey: 'defaultInsuranceDuration',
    header: 'Thời gian bảo hiểm mặc định (ngày)',
    enableSorting: true,
    cell: ({ getValue }) => {
      const duration = getValue() as number;
      return <span>{duration}</span>;
    }
  },

  {
    accessorKey: 'type',
    header: 'Loại bệnh',
    enableSorting: true,
    cell: ({ getValue }) => {
      const type = getValue() as string;
      // Format type để hiển thị đẹp hơn
      const formattedType = type?.replace(/_/g, ' ');
      return (
        <span className="rounded-full bg-red-100 px-2 py-1 text-xs font-medium text-red-800">
          {formattedType}
        </span>
      );
    }
  },

  {
    accessorKey: 'createdAt',
    header: 'Ngày tạo',
    enableSorting: true,
    cell: ({ getValue }) => {
      const date = getValue() as string;
      const formattedDate = new Date(date).toLocaleDateString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
      });
      return <span>{formattedDate}</span>;
    }
  },

  {
    id: 'actions',
    header: 'Hành động',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
