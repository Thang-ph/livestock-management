import { ColumnDef } from '@tanstack/react-table';
import __helpers from '@/helpers';
import { useSearchParams } from 'react-router-dom';
export const columns: ColumnDef<any>[] = [
  {
    accessorKey: 'STT',
    header: 'STT',
    enableSorting: true,
    cell: ({ row }) => {
      const [searchParams] = useSearchParams();
      const pageLimit = Number(searchParams.get('limit') || 10);
      const page = Number(searchParams.get('page') || 1);
      const rowIndex = row.index;
      const serialNumber = (page - 1) * pageLimit + rowIndex + 1;
      return <span>{serialNumber}</span>;
    }
  },

  {
    accessorKey: 'customerName',
    header: 'Tên khách hàng',
    enableSorting: true
  },
  {
    accessorKey: 'customerPhone',
    header: 'Số điện thoại'
  },
  {
    accessorKey: 'customerAddress',
    header: 'Địa chỉ'
  },
  {
    accessorKey: 'customerNote',
    header: 'Ghi chú'
  },

  {
    accessorKey: 'total',
    header: 'Tổng bàn giao',
    enableSorting: true
  },

  {
    accessorKey: 'remaining',
    header: 'Chờ bàn giao',
    enableSorting: true
  }
];
