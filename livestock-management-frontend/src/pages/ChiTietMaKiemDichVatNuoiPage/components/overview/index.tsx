import ListUser from '../../list-user';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useParams, useSearchParams } from 'react-router-dom';
import { useGetListCodeRangeBySpecies } from '@/queries/makiemdich.query';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  const pageLimit = Number(searchParams.get('limit') || 10);
  const { speciesName } = useParams();
  const { data, isPending } = useGetListCodeRangeBySpecies(
    speciesName as string
  );
  console.log('data', data);
  const listObjects = data?.items;
  const totalRecords = data?.total;
  const pageCount = Math.ceil(totalRecords / pageLimit);
  console.log('listObjects', listObjects);

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <h1 className="font text-center font-semibold">
          DANH SÁCH MÃ KIỂM DỊCH CỦA LOÀI{' '}
          <span className="font-bold ">{speciesName}</span>
        </h1>
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
