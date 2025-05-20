import { useGetAllUser } from '@/queries/user.query';
import ListUser from '../../list-user';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useSearchParams } from 'react-router-dom';
// import { useSearchParams } from 'react-router-dom';
// import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Detail } from '../detail';

function InfoCard({ title, value, bgColor = 'bg-secondary' }) {
  return (
    <Card className={`${bgColor} `}>
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium text-current">
          {title}
        </CardTitle>
      </CardHeader>
      <CardContent className="text-lg font-bold text-current">
        {value}
      </CardContent>
    </Card>
  );
}

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  // const page = Number(searchParams.get('page') || 1);
  const pageLimit = Number(searchParams.get('limit') || 10);
  const keyword = searchParams.get('keyword') || '';
  const { data, isPending } = useGetAllUser(keyword);

  const listObjects = data?.data;
  const totalRecords = data?.data.length;
  const pageCount = Math.ceil(totalRecords / pageLimit);

  const cardGroups = [
    {
      title: 'Tổng  người dùng hiện có',
      value: 122,
      bgColor: 'bg-blue-500/60'
    },
    {
      title: 'Tổng nhân viên đang hoạt động',
      value: 100,
      bgColor: 'bg-green-500/60'
    },
    {
      title: 'Số nhân viên hiện có',
      value: 66,
      bgColor: 'bg-green-500/60'
    },
    {
      title: 'Số quản lý hiện có',
      value: 100,
      bgColor: 'bg-red-500/60'
    }
  ];
  return (
    <>
      <div className="grid gap-6 rounded-md  p-4 ">
        <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4 ">
          {cardGroups.map((group, index) => (
            <InfoCard
              key={index}
              title={group.title}
              value={group.value}
              bgColor={group.bgColor}
            />
          ))}
        </div>
        <Detail />
      </div>
      <div className="grid gap-6 rounded-md p-4 pt-5 ">
        <h1 className="text-center font-bold">DANH SÁCH NGƯỜI DÙNG</h1>
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
