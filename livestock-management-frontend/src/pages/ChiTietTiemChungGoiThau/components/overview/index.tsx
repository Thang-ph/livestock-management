import ListUser from '../../list-user';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useParams, useSearchParams } from 'react-router-dom';
// import { useSearchParams } from 'react-router-dom';
// import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useGetChiTietTiemChungGoiThau } from '@/queries/makiemdich.query';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  const pageLimit = Number(searchParams.get('limit') || 10);
  const { id } = useParams();
  const { data, isPending } = useGetChiTietTiemChungGoiThau(id as string);
  console.log('data', data);
  const listObjects = data?.diseaseRequiresForSpecie;
  const totalRecords = data?.total;
  const pageCount = Math.ceil(totalRecords / pageLimit);
  console.log('listObjects', listObjects);

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <div>
          <h1 className="text-center font-bold">
            DANH SÁCH MÃ KIỂM DỊCH THEO LOÀI
          </h1>
          <div>
            <p className="font-bold">Gói thầu: {data?.procurementName}</p>
            <p className="font-bold">Mã gói: {data?.procurementCode}</p>
            <p className="font-bold">Tổng con: {data?.livestockQuantity}</p>
          </div>
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
          <ListUser
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
