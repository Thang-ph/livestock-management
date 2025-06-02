import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useSearchParams } from 'react-router-dom';
import { useGetListMuaLe } from '@/queries/donmuale.query';
import { useMemo } from 'react';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  const page = Number(searchParams.get('page') || 1);
  const pageLimit = Number(searchParams.get('limit') || 10);

  const { data, isPending } = useGetListMuaLe();
  console.log('data', data);

  // Phân trang dữ liệu từ client-side
  const paginatedData = useMemo(() => {
    const allItems = data?.items || [];
    const totalRecords = allItems.length;
    const pageCount = Math.ceil(totalRecords / pageLimit);

    // Tính toán index bắt đầu và kết thúc cho trang hiện tại
    const startIndex = (page - 1) * pageLimit;
    const endIndex = startIndex + pageLimit;

    // Lấy dữ liệu cho trang hiện tại
    const pageItems = allItems.slice(startIndex, endIndex);

    return {
      items: pageItems,
      totalRecords,
      pageCount,
      currentPage: page
    };
  }, [data?.items, page, pageLimit]);

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0 ">
        <h1 className="text-center font-bold">DANH SÁCH ĐƠN MUA LẺ</h1>
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
            data={paginatedData.items}
            page={paginatedData.currentPage}
            totalUsers={paginatedData.totalRecords}
            pageCount={paginatedData.pageCount}
          />
        )}
      </div>
    </>
  );
}
