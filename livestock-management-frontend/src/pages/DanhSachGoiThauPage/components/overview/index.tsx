import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useSearchParams } from 'react-router-dom';
import { useGetGoiThau } from '@/queries/admin.query';
// import { useSearchParams } from 'react-router-dom';
// import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  // const page = Number(searchParams.get('page') || 1);
  const pageLimit = Number(searchParams.get('limit') || 10);
  const keyword = searchParams.get('keyword') || '';
  const { data, isPending } = useGetGoiThau(keyword);

  const listObjects = data?.data.items;
  const totalRecords = data?.data.items.length;
  const pageCount = Math.ceil(totalRecords / pageLimit);

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0 ">
        <h1 className="text-center font-bold">DANH SÁCH GÓI THẦU</h1>
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
