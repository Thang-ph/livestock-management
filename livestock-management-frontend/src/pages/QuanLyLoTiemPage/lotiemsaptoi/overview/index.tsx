'use client';

import ListData from '../../listlotiemsaptoi';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useSearchParams } from 'react-router-dom';
import { useGetListLoTiemSapToi } from '@/queries/admin.query';
import { Button } from '@/components/ui/button';
import { useState } from 'react';
import { Eye } from 'lucide-react';
import { useRouter } from '@/routes/hooks';

export function LoTiemSapToiTab() {
  const [searchParams] = useSearchParams();
  const [viewAll] = useState(false);

  // Default page limit from URL or 10
  const defaultPageLimit = Number(searchParams.get('limit') || 10);
  // Use a very high number when "View All" is active
  const pageLimit = viewAll ? 1000 : defaultPageLimit;

  const { data, isPending } = useGetListLoTiemSapToi();
  console.log('data', data);
  const listObjects = data;
  const totalRecords = data?.length || 0;
  const pageCount = Math.ceil(totalRecords / pageLimit);
  const router = useRouter();

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <div className="">
          <h1 className="text-center font-bold">DANH SÁCH LÔ TIÊM SẮP TỚI</h1>
        </div>
        <div>
          <Button
            variant={viewAll ? 'default' : 'outline'}
            size="sm"
            onClick={() => {
              router.push('/lo-tiem');
            }}
            className="flex items-center gap-2"
          >
            <Eye className="h-4 w-4" />
            Xem tất cả
          </Button>
        </div>

        {isPending ? (
          <div className="p-5">
            <DataTableSkeleton
              columnCount={10}
              filterableColumnCount={2}
              searchableColumnCount={1}
            />
          </div>
        ) : (
          <ListData
            data={listObjects}
            page={pageLimit}
            totalUsers={totalRecords}
            pageCount={pageCount}
          />
        )}
      </div>
    </>
  );
}
