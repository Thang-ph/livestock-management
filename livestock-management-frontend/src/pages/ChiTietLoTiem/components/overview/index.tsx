'use client';
import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useParams, useSearchParams } from 'react-router-dom';
import {
  useGetDanhSachVatNuoi,
  useGetLiveStockInfo,
  useGetLiveStockInfoById,
  useThemVatNuoiVaoLoTiem
} from '@/queries/vacxin.query';
import { VaccinationDialog } from './vaccination-dialog';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  const pageLimit = Number(searchParams.get('limit') || 10);
  const { mutateAsync: themVatNuoiVaoLoTiem } = useThemVatNuoiVaoLoTiem();
  const { id } = useParams();
  const { data, isPending } = useGetDanhSachVatNuoi(String(id));
  const { mutateAsync: getLiveStockInfo } = useGetLiveStockInfo();
  const { mutateAsync: getLiveStockInfoById } = useGetLiveStockInfoById();

  const listObjects = data?.items || [];
  const totalRecords = data?.items?.length || 0;
  const pageCount = Math.ceil(totalRecords / pageLimit);

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0 ">
        <h1 className="text-center font-bold">
          DANH SÁCH VẬT NUÔI CỦA LÔ TIÊM
        </h1>
        <div className="flex justify-end gap-4">
          <VaccinationDialog
            batchId={String(id)}
            getLiveStockInfo={getLiveStockInfo}
            themVatNuoiVaoLoTiem={themVatNuoiVaoLoTiem}
            getLiveStockInfoById={getLiveStockInfoById}
          />
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
