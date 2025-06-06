import DataTable from '@/components/shared/data-table';
import { columns } from './columns';
type TTableProps = {
  data: any;
  page: number;
  totalUsers: number;
  pageCount: number;
};

export default function ListUser({ data, pageCount }: TTableProps) {
  return (
    <>
      {data && (
        <DataTable
          columns={columns}
          data={data}
          pageCount={pageCount}
          showAdd={false}
          heightTable="50dvh"
          placeHolderInputSearch="Tìm kiếm..."
          showSearch={true}
        />
      )}
    </>
  );
}
