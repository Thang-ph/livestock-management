'use client';

import type { ColumnDef } from '@tanstack/react-table';
import { useSearchParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { useRouter } from '@/routes/hooks';
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
    // cell: ({ row }) => {
    //   const quantity = 20;
    //   const total = row.original.totalQuantity || 100;

    //   const percentage = Math.min(100, Math.round((quantity / total) * 100));

    //   return (
    //     <div className="w-full">
    //       <div className="flex items-center gap-2">
    //         <div className="h-2.5 w-full rounded-full bg-gray-200 dark:bg-gray-700">
    //           <div
    //             className="h-2.5 rounded-full bg-green-600"
    //             style={{ width: `${percentage}%` }}
    //             aria-valuenow={quantity}
    //             aria-valuemin={0}
    //             aria-valuemax={total}
    //             role="progressbar"
    //           ></div>
    //         </div>
    //         <span className="text-xs font-medium">
    //           {quantity}/{total}
    //         </span>
    //       </div>
    //     </div>
    //   );
    // }
  },
  {
    id: 'actions',
    cell: ({ row }) => {
      const router = useRouter();
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
