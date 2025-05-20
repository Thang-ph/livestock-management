'use client';

import type { ColumnDef } from '@tanstack/react-table';
import __helpers from '@/helpers';
import { useSearchParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { useDeleteVatNuoiKhoiLoTiem } from '@/queries/vacxin.query';
import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog';
import { useToast } from '@/components/ui/use-toast';
import { Trash2 } from 'lucide-react';

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
    accessorKey: 'inspectionCode',
    header: 'Mã thẻ tai',
    enableSorting: true
  },

  {
    accessorKey: 'species',
    header: 'Loài',
    enableSorting: true
  },

  {
    accessorKey: 'color',
    header: 'Màu lông',
    enableSorting: true
  },

  {
    accessorKey: 'status',
    header: 'Trạng thái',
    enableSorting: true
  },

  {
    accessorKey: 'injections_count',
    header: 'Số mũi đã tiêm'
  },

  {
    accessorKey: 'createdAt',
    header: 'Thời gian tiêm',
    enableSorting: true,
    cell: ({ row }) => __helpers.convertToDateDDMMYYYY(row.original.dateConduct)
  },

  {
    id: 'Hành động',
    header: 'Hành động',
    cell: ({ row }) => {
      const { mutateAsync: deleteVatNuoiKhoiLoTiem, isPending } =
        useDeleteVatNuoiKhoiLoTiem();
      const [isDialogOpen, setIsDialogOpen] = useState(false);
      const { toast } = useToast();

      const handleDelete = async () => {
        try {
          await deleteVatNuoiKhoiLoTiem({
            livestockVaccinationId: row.original.id
          });

          toast({
            title: 'Thành công',
            description: 'Đã xóa vật nuôi khỏi lô tiêm thành công.'
          });

          setIsDialogOpen(false);
        } catch (error) {
          toast({
            title: 'Lỗi',
            description:
              'Không thể xóa vật nuôi khỏi lô tiêm. Vui lòng thử lại.',
            variant: 'destructive'
          });
        }
      };

      return (
        <>
          <Button
            variant="outline"
            className="flex items-center gap-2"
            onClick={() => setIsDialogOpen(true)}
          >
            <Trash2 className="h-4 w-4" />
            Xóa
          </Button>

          <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
            <DialogContent className="sm:max-w-md">
              <DialogHeader>
                <DialogTitle>Xác nhận xóa</DialogTitle>
                <DialogDescription>
                  Bạn có chắc chắn muốn xóa vật nuôi này khỏi lô tiêm không?
                  Hành động này không thể hoàn tác.
                </DialogDescription>
              </DialogHeader>
              <DialogFooter>
                <Button
                  variant="outline"
                  onClick={() => setIsDialogOpen(false)}
                >
                  Hủy
                </Button>
                <Button
                  variant="destructive"
                  onClick={handleDelete}
                  disabled={isPending}
                >
                  {isPending ? 'Đang xử lý...' : 'Xác nhận xóa'}
                </Button>
              </DialogFooter>
            </DialogContent>
          </Dialog>
        </>
      );
    }
  }
];
