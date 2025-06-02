'use client';

import type { ColumnDef } from '@tanstack/react-table';
import { useParams, useSearchParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { useRouter } from '@/routes/hooks';
import __helpers from '@/helpers';
// import { CellAction } from './cell-action';

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
      return <div className="font-medium">{serialNumber}</div>;
    }
  },
  {
    accessorKey: 'diseaseName',
    header: 'Bệnh',
    enableSorting: true
  },
  {
    accessorKey: 'medicineName',
    header: 'Thuốc',
    enableSorting: true
  },
  {
    accessorKey: 'specieName',
    header: 'Loài',
    enableSorting: true
  },
  {
    accessorKey: 'hasDone',
    header: 'Số lượng đã tiêm',
    enableSorting: true
  },
  {
    accessorKey: 'vaccinationStatus',
    header: 'Tình hình tiêm chủng',
    enableSorting: true,
    cell: ({ row }) => {
      const totalRecord = row.original.hasDone || 0;
      const totalQuantity = row.original.totalQuantity || 0;

      // Tính toán phần trăm hoàn thành
      const percentage =
        totalQuantity > 0
          ? Math.min(100, Math.round((totalRecord / totalQuantity) * 100))
          : 0;

      // Xác định màu sắc dựa trên phần trăm
      const getProgressColor = (percent: number) => {
        if (percent >= 80) return 'bg-green-600';
        if (percent >= 50) return 'bg-yellow-500';
        return 'bg-red-500';
      };

      const getStatusText = (percent: number) => {
        if (percent >= 80) return 'Hoàn thành tốt';
        if (percent >= 50) return 'Đang thực hiện';
        return 'Cần chú ý';
      };

      return (
        <div className="w-full space-y-1">
          <div className="flex items-center gap-2">
            <div className="h-2.5 w-full rounded-full bg-gray-200 dark:bg-gray-700">
              <div
                className={`h-2.5 rounded-full ${getProgressColor(percentage)}`}
                style={{ width: `${percentage}%` }}
                aria-valuenow={totalRecord}
                aria-valuemin={0}
                aria-valuemax={totalQuantity}
                role="progressbar"
              ></div>
            </div>
            <span className="whitespace-nowrap text-xs font-medium">
              {totalRecord}/{totalQuantity}
            </span>
          </div>
          <div className="flex items-center justify-between">
            <span
              className={`rounded-full px-2 py-1 text-xs ${
                percentage >= 80
                  ? 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200'
                  : percentage >= 50
                    ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200'
                    : 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-200'
              }`}
            >
              {getStatusText(percentage)}
            </span>
            <span className="text-xs font-medium text-gray-500">
              {percentage}%
            </span>
          </div>
        </div>
      );
    }
  },
  {
    id: 'actions',
    header: 'Hành động',
    cell: ({ row }) => {
      const router = useRouter();
      const { id } = useParams();

      const isCreated = row.original.isCreated;
      return isCreated == 0 ? (
        <Button
          onClick={() => {
            router.push(`/chi-tiet-lo-tiem/${row.original.batchVaccinationId}`);
          }}
        >
          Chi tiết lô tiêm
        </Button>
      ) : isCreated == 1 ? (
        <Button
          onClick={() => {
            __helpers.localStorage_set('diseaseName', row.original.diseaseName);
            __helpers.localStorage_set('specieName', row.original.specieName);
            __helpers.localStorage_set(
              'medicineName',
              row.original.medicineName
            );
            __helpers.localStorage_set('diseaseId', row.original.diseaseId);
            __helpers.localStorage_set('specieId', row.original.specieId);
            __helpers.localStorage_set('procurementId', id as string);
            localStorage.setItem('goi-thau-tab', 'add');
            router.push(`/lo-tiem`);
          }}
        >
          Tạo lô tiêm
        </Button>
      ) : null;
    }
  }
];
