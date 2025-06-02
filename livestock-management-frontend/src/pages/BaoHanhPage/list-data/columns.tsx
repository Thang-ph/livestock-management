'use client';

import type { ColumnDef } from '@tanstack/react-table';
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
    header: 'Trạng thái',
    enableSorting: true,
    cell: ({ row }) => {
      return <StatusBadge status={row.original.status} />;
    }
  },
  {
    accessorKey: 'quantity',
    header: 'Số lượng'
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];

import { cn } from '@/lib/utils';

interface StatusBadgeProps {
  status: string;
  className?: string;
}

export const StatusBadge = ({ status, className }: StatusBadgeProps) => {
  // Define color mapping for each status
  const getStatusConfig = (status: string) => {
    switch (status) {
      case 'CHỜ_DUYỆT':
        return {
          color: 'bg-blue-100 text-blue-800',
          label: 'Chờ duyệt'
        };
      case 'ĐANG_CHUẨN_BỊ':
        return {
          color: 'bg-yellow-100 text-yellow-800',
          label: 'Đang chuẩn bị'
        };
      case 'CHỜ_BÀN_GIAO':
        return {
          color: 'bg-purple-100 text-purple-800',
          label: 'Chờ bàn giao'
        };
      case 'TỪ_CHỐI':
        return {
          color: 'bg-red-100 text-red-800',
          label: 'Từ chối'
        };
      case 'HOÀN_THÀNH':
        return {
          color: 'bg-green-100 text-green-800',
          label: 'Hoàn thành'
        };
      case 'ĐÃ_HỦY':
        return {
          color: 'bg-gray-100 text-gray-800',
          label: 'Đã hủy'
        };
      default:
        return {
          color: 'bg-gray-100 text-gray-800',
          label: status
        };
    }
  };

  const config = getStatusConfig(status);

  return (
    <span
      className={cn(
        'rounded-full px-2.5 py-0.5 text-xs font-medium',
        config.color,
        className
      )}
    >
      {config.label}
    </span>
  );
};
