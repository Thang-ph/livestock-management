'use client';

import type { ColumnDef } from '@tanstack/react-table';
import __helpers from '@/helpers';
import { useSearchParams } from 'react-router-dom';
import { CellAction } from './cell-action';
import { Badge } from '@/components/ui/badge';
import { CalendarIcon, Package2Icon } from 'lucide-react';
import { Progress } from '@/components/ui/progress';
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger
} from '@/components/ui/tooltip';

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
    accessorKey: 'code',
    header: 'Mã gói thầu',
    enableSorting: true,
    cell: ({ row }) => {
      return <div className="font-medium">{row.original.code}</div>;
    }
  },

  {
    accessorKey: 'name',
    header: 'Tên gói thầu',
    enableSorting: true,
    cell: ({ row }) => {
      return (
        <div
          className="max-w-[200px] truncate font-medium"
          title={row.original.name}
        >
          {row.original.name}
        </div>
      );
    }
  },

  {
    accessorKey: 'successDate',
    header: 'Ngày trúng',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="flex items-center gap-2 text-muted-foreground">
        <CalendarIcon className="h-4 w-4" />
        <span>{__helpers.convertToDateDDMMYYYY(row.original.successDate)}</span>
      </div>
    )
  },

  {
    accessorKey: 'expirationDate',
    header: 'Ngày hết hạn',
    enableSorting: true,
    cell: ({ row }) => {
      const expirationDate = new Date(row.original.expirationDate);
      const today = new Date();
      const isExpiringSoon =
        expirationDate > today &&
        expirationDate < new Date(today.setDate(today.getDate() + 30));
      const isExpired = expirationDate < new Date();

      return (
        <div className="flex items-center gap-2">
          <CalendarIcon
            className={`h-4 w-4 ${isExpired ? 'text-destructive' : isExpiringSoon ? 'text-amber-500' : 'text-muted-foreground'}`}
          />
          <span
            className={
              isExpired
                ? 'font-medium text-destructive'
                : isExpiringSoon
                  ? 'font-medium text-amber-500'
                  : 'text-muted-foreground'
            }
          >
            {__helpers.convertToDateDDMMYYYY(row.original.expirationDate)}
          </span>
        </div>
      );
    }
  },
  {
    accessorKey: 'handover',
    header: 'Tiến độ bàn giao',
    enableSorting: true,
    cell: ({ row }) => {
      const completeCount = row.original.handoverinformation.completeCount;
      const totalCount = row.original.handoverinformation.totalCount;
      const percentage =
        totalCount > 0 ? Math.round((completeCount / totalCount) * 100) : 0;

      return (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <div className="w-full max-w-[180px] space-y-2">
                <div className="flex justify-between text-xs">
                  <span className="flex items-center gap-1 text-muted-foreground">
                    <Package2Icon className="h-3 w-3" />
                    {completeCount}/{totalCount}
                  </span>
                  <span className="font-medium">{percentage}%</span>
                </div>
                <Progress value={percentage} className="h-2" />
              </div>
            </TooltipTrigger>
            <TooltipContent>
              <p>
                Đã bàn giao: {completeCount} / Tổng cần bàn giao: {totalCount}
              </p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      );
    }
  },
  {
    accessorKey: 'handover',
    header: 'Tiến độ chọn',
    enableSorting: true,
    cell: ({ row }) => {
      const completeCount = row.original.handoverinformation.totalSelected;
      const totalCount = row.original.handoverinformation.totalCount;
      const percentage =
        totalCount > 0 ? Math.round((completeCount / totalCount) * 100) : 0;

      return (
        <TooltipProvider>
          <Tooltip>
            <TooltipTrigger asChild>
              <div className="w-full max-w-[180px] space-y-2">
                <div className="flex justify-between text-xs">
                  <span className="flex items-center gap-1 text-muted-foreground">
                    <Package2Icon className="h-3 w-3" />
                    {completeCount}/{totalCount}
                  </span>
                  <span className="font-medium">{percentage}%</span>
                </div>
                <Progress value={percentage} className="h-2" />
              </div>
            </TooltipTrigger>
            <TooltipContent>
              <p>
                Tổng chọn: {completeCount} / Tổng cần bàn giao: {totalCount}
              </p>
            </TooltipContent>
          </Tooltip>
        </TooltipProvider>
      );
    }
  },
  {
    accessorKey: 'status',
    header: 'Trạng thái',
    enableSorting: true,
    cell: ({ row }) => {
      const status = row.original.status;
      let badgeVariant = 'default';
      let displayText = status;

      // Map status codes to display text and colors
      switch (status) {
        case 'HOÀN_THÀNH':
          badgeVariant = 'success';
          displayText = 'Hoàn thành';
          break;
        case 'ĐANG_ĐẤU_THẦU':
          badgeVariant = 'default';
          displayText = 'Đang đấu thầu';
          break;
        case 'CHỜ_BÀN_GIAO':
          badgeVariant = 'warning';
          displayText = 'Chờ bàn giao';
          break;
        case 'ĐÃ_HỦY':
          badgeVariant = 'destructive';
          displayText = 'Đã hủy';
          break;
        case 'ĐANG_BÀN_GIAO':
          badgeVariant = 'pending';
          displayText = 'Đang bàn giao';
          break;
        default:
          badgeVariant = 'secondary';
      }

      return (
        <Badge variant={badgeVariant as any} className="font-normal">
          {displayText}
        </Badge>
      );
    }
  },
  {
    accessorKey: 'createdAt',
    header: 'Ngày tạo',
    enableSorting: true,
    cell: ({ row }) => (
      <div className="flex items-center gap-2 text-muted-foreground">
        <CalendarIcon className="h-4 w-4" />
        <span>{__helpers.convertToDateDDMMYYYY(row.original.createdAt)}</span>
      </div>
    )
  },
  {
    id: 'actions',
    cell: ({ row }) => <CellAction data={row.original} />
  }
];
