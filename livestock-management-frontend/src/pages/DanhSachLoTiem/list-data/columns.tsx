'use client';

import type { ColumnDef } from '@tanstack/react-table';
import __helpers from '@/helpers';
import { useSearchParams } from 'react-router-dom';
import { CellAction } from './cell-action';
import { Badge } from '@/components/ui/badge';
import { Calendar, CheckCircle, Loader2 } from 'lucide-react';

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
    enableSorting: true,
    cell: ({ row }) => <div className="font-medium">{row.original.name}</div>
  },

  {
    accessorKey: 'medcicalType',
    header: 'Loại thuốc',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="text-gray-700">{row.original.medcicalType}</div>
    )
  },

  {
    accessorKey: 'symptom',
    header: 'Phòng bệnh',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="text-gray-700">{row.original.symptom}</div>
    )
  },

  {
    accessorKey: 'dateSchedule',
    header: 'Ngày dự kiến thực hiện',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="flex items-center gap-2 text-gray-700">
        <Calendar className="h-4 w-4 text-gray-500" />
        {__helpers.convertToDateDDMMYYYY(row.original.dateSchedule)}
      </div>
    )
  },

  {
    accessorKey: 'dateConduct',
    header: 'Ngày hoàn thành',
    cell: ({ row }) => {
      return (
        <div className="flex items-center gap-2 text-gray-700">
          {row.original.dateConduct != null ? (
            <>
              <CheckCircle className="h-4 w-4 text-green-500" />
              {__helpers.convertToDateDDMMYYYY(row.original.dateConduct)}
            </>
          ) : (
            <span className="italic text-gray-500">Chưa thực hiện</span>
          )}
        </div>
      );
    }
  },

  {
    accessorKey: 'status',
    header: 'Trạng thái',
    enableSorting: true,
    cell: ({ row }) => {
      const status = row.original.status;

      if (status === 'HOÀN_THÀNH') {
        return (
          <Badge className="border-green-200 bg-green-100 text-green-800 hover:bg-green-200">
            Hoàn thành
          </Badge>
        );
      } else if (status === 'CHỜ_THỰC_HIỆN') {
        return (
          <Badge className="border-amber-200 bg-amber-100 text-amber-800 hover:bg-amber-200">
            Chờ thực hiện
          </Badge>
        );
      } else if (status === 'ĐANG_THỰC_HIỆN') {
        return (
          <Badge className="border-blue-200 bg-blue-100 text-blue-800 hover:bg-blue-200">
            <Loader2 className="mr-1 h-3.5 w-3.5 animate-spin" />
            Đang thực hiện
          </Badge>
        );
      } else if (status === 'ĐÃ_HỦY') {
        return (
          <Badge className="border-red-200 bg-red-100 text-red-800 hover:bg-red-200">
            Đã hủy
          </Badge>
        );
      }

      return <span>{status}</span>;
    }
  },

  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
