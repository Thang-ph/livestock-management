import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useSearchParams } from 'react-router-dom';
import { useGetGoiThau } from '@/queries/admin.query';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  const page = Number(searchParams.get('page') || 1); // Bỏ comment để lấy page từ URL
  const pageLimit = Number(searchParams.get('limit') || 10);
  const keyword = searchParams.get('keyword') || '';

  // API get all data
  const { data, isPending } = useGetGoiThau(keyword);

  const allItems = data?.data.items || [];
  const totalRecords = allItems.length;
  const pageCount = Math.ceil(totalRecords / pageLimit);

  const startIndex = (page - 1) * pageLimit;
  const endIndex = startIndex + pageLimit;
  const listObjects = allItems.slice(startIndex, endIndex);

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
            page={page} // Truyền page thay vì pageLimit
            totalUsers={totalRecords}
            pageCount={pageCount}
          />
        )}
      </div>
    </>
  );
}
