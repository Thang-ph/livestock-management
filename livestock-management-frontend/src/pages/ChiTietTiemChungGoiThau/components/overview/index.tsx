import { useGetAllUser } from '@/queries/user.query';
import ListUser from '../../list-user';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useParams, useSearchParams } from 'react-router-dom';
// import { useSearchParams } from 'react-router-dom';
// import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  useGetAllMaKiemDich,
  useGetAllMaKiemDichTheoLoai,
  useGetChiTietTiemChungGoiThau
} from '@/queries/makiemdich.query';

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
  const pageLimit = Number(searchParams.get('limit') || 10);
  const keyword = searchParams.get('keyword') || '';
  const { id } = useParams();
  const { data, isPending } = useGetChiTietTiemChungGoiThau(id as string);
  console.log('data', data);
  const listObjects = data?.diseaseRequiresForSpecie;
  const totalRecords = data?.total;
  const pageCount = Math.ceil(totalRecords / pageLimit);
  console.log('listObjects', listObjects);
  const cardGroups = [
    {
      title: 'Mã kiểm dịch sắp',
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
