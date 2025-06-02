import { useMemo } from 'react';
import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useSearchParams } from 'react-router-dom';
import { useGetBatchVaccinList } from '@/queries/admin.query';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  const page = Number(searchParams.get('page') || 1);
  const pageLimit = Number(searchParams.get('limit') || 10);
  const keyword = searchParams.get('keyword') || '';

  const { data, isPending, error } = useGetBatchVaccinList(keyword);

  // Xử lý và phân trang dữ liệu
  const paginatedData = useMemo(() => {
    if (!data?.data?.items) {
      return {
        items: [],
        totalRecords: 0,
        pageCount: 0,
        currentPage: page,
        hasData: false
      };
    }

    const allItems = data.data.items;
    const totalRecords = allItems.length;
    const pageCount = Math.ceil(totalRecords / pageLimit);

    // Tính toán index cho trang hiện tại
    const startIndex = (page - 1) * pageLimit;
    const endIndex = startIndex + pageLimit;

    // Lấy dữ liệu cho trang hiện tại
    const pageItems = allItems.slice(startIndex, endIndex);

    return {
      items: pageItems,
      totalRecords,
      pageCount,
      currentPage: page,
      hasData: totalRecords > 0
    };
  }, [data?.data?.items, page, pageLimit]);

  // Loading state
  if (isPending) {
    return (
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <h1 className="text-center font-bold">DANH SÁCH LÔ TIÊM</h1>
        <div className="p-5">
          <DataTableSkeleton
            columnCount={10}
            filterableColumnCount={2}
            searchableColumnCount={1}
          />
        </div>
      </div>
    );
  }

  // Error state
  if (error) {
    return (
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <h1 className="text-center font-bold">DANH SÁCH LÔ TIÊM</h1>
        <div className="p-5 text-center">
          <p className="mb-2 text-red-500">Có lỗi xảy ra khi tải dữ liệu</p>
          <p className="text-sm text-gray-500">
            {error?.message || 'Vui lòng thử lại sau'}
          </p>
        </div>
      </div>
    );
  }

  // Empty state với keyword
  if (!paginatedData.hasData && keyword) {
    return (
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <h1 className="text-center font-bold">DANH SÁCH LÔ TIÊM</h1>
        <div className="p-5 text-center">
          <p className="mb-2 text-gray-500">
            Không tìm thấy kết quả cho từ khóa "{keyword}"
          </p>
          <p className="text-sm text-gray-400">Thử tìm kiếm với từ khóa khác</p>
        </div>
      </div>
    );
  }

  // Empty state không có keyword
  if (!paginatedData.hasData && !keyword) {
    return (
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <h1 className="text-center font-bold">DANH SÁCH LÔ TIÊM</h1>
        <div className="p-5 text-center">
          <p className="mb-2 text-gray-500">Chưa có dữ liệu lô tiêm</p>
          <p className="text-sm text-gray-400">Thêm lô tiêm mới để bắt đầu</p>
        </div>
      </div>
    );
  }

  // Success state với dữ liệu
  return (
    <div className="grid gap-6 rounded-md p-4 pt-0">
      <div className="flex items-center justify-between">
        <h1 className="flex-1 text-center font-bold">DANH SÁCH LÔ TIÊM</h1>

        {/* Hiển thị thông tin tìm kiếm nếu có */}
        {keyword && (
          <div className="text-sm text-gray-500">
            Tìm kiếm: "{keyword}" • {paginatedData.totalRecords} kết quả
          </div>
        )}
      </div>

      <ListData
        data={paginatedData.items}
        page={paginatedData.currentPage}
        totalUsers={paginatedData.totalRecords}
        pageCount={paginatedData.pageCount}
      />
    </div>
  );
}
