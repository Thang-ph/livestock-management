'use client';

import type { ColumnDef } from '@tanstack/react-table';
import { useSearchParams } from 'react-router-dom';
import { Badge } from '@/components/ui/badge';
import { LockIcon, UnlockIcon } from 'lucide-react';
import { CellAction } from './cell-action';

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
      return <div className="text-center font-medium">{serialNumber}</div>;
    }
  },
  {
    accessorKey: 'userName',
    header: 'Nhân viên',
    enableSorting: true,
    cell: ({ row }) => {
      return <div className="font-medium">{row.original.userName}</div>;
    }
  },
  {
    accessorKey: 'email',
    header: 'Email',
    enableSorting: true,
    cell: ({ row }) => {
      return <div className="text-muted-foreground">{row.original.email}</div>;
    }
  },
  {
    accessorKey: 'phoneNumber',
    header: 'Số điện thoại',
    enableSorting: true,
    cell: ({ row }) => {
      return <div className="font-medium">{row.original.phoneNumber}</div>;
    }
  },
  {
    accessorKey: 'isLocked',
    header: 'Trạng thái tài khoản',
    enableSorting: true,
    cell: ({ row }) => {
      const isLocked = row.original.isLocked;
      return (
        <div className="flex items-center gap-2">
          {isLocked ? (
            <Badge variant="destructive" className="flex items-center gap-1">
              <LockIcon className="h-3 w-3" />
              <span>Đã khóa</span>
            </Badge>
          ) : (
            <Badge
              variant="outline"
              className="flex items-center gap-1 border-green-200 bg-green-50 text-green-700"
            >
              <UnlockIcon className="h-3 w-3" />
              <span>Hoạt động</span>
            </Badge>
          )}
        </div>
      );
    }
  },
  {
    accessorKey: 'roles',
    header: 'Vai trò',
    enableSorting: true,
    cell: ({ row }) => {
      const roles = row.original.roles;

      // Các màu sắc đơn giản theo thứ tự
      const roleColors = [
        { className: 'bg-blue-100 text-blue-800 hover:bg-blue-200' }, // Màu xanh cho vai trò đầu tiên
        { className: 'bg-yellow-100 text-yellow-800 hover:bg-yellow-200' }, // Màu vàng cho vai trò thứ hai
        { className: 'bg-purple-100 text-purple-800 hover:bg-purple-200' }, // Màu tím cho vai trò thứ ba
        { className: 'bg-red-100 text-red-800 hover:bg-red-200' }, // Màu đỏ cho vai trò thứ tư (nếu có)
        { className: 'bg-green-100 text-green-800 hover:bg-green-200' } // Màu xanh lá cho vai trò thứ năm (nếu có)
      ];

      return (
        <div className="flex flex-wrap gap-1">
          {roles.length > 0 ? (
            roles.map((role, index) => {
              const colorStyle = roleColors[index] || {
                className: 'bg-gray-100 text-gray-800 hover:bg-gray-200'
              };

              return (
                <Badge
                  key={index}
                  variant="outline"
                  className={`text-xs ${colorStyle.className}`}
                >
                  {role}
                </Badge>
              );
            })
          ) : (
            <span className="text-sm italic text-muted-foreground">
              Chưa phân quyền
            </span>
          )}
        </div>
      );
    }
  },
  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
