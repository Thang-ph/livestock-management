'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog';
import type React from 'react';
import { toast } from '@/components/ui/use-toast';
import { useDeleteStockFromOrder } from '@/queries/donmuale.query';

export const CellAction: React.FC<any> = ({ data }) => {
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const { mutateAsync: deleteStockFromOrder } = useDeleteStockFromOrder();
  const handleDelete = async () => {
    // Implement your delete logic here
    console.log('Deleting user:', data.id);
    const [err] = await deleteStockFromOrder(data.id);
    if (err) {
      toast({
        title: 'Xóa thất bại',
        description: err.data.data,
        variant: 'destructive'
      });
      return;
    } else {
      toast({
        title: 'Xóa thành công',
        description: `Đã xóa loại vật thành công!`,
        variant: 'default'
      });
    }

    setShowDeleteDialog(false);
  };

  return (
    <>
      <Button variant="destructive" onClick={() => setShowDeleteDialog(true)}>
        Xóa
      </Button>

      <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <DialogContent className="border-2 border-black p-0 sm:max-w-md">
          <DialogHeader className="border-b border-black p-4 text-center">
            <DialogTitle className="text-xl font-bold">
              Xác xóa loại vật
            </DialogTitle>
          </DialogHeader>
          <div className="p-6 text-center">
            <div className="mb-6 border-2 border-black p-4">
              <p className="mb-2">
                Mã kiểm dịch:{' '}
                <span className="font-bold">{data.inspectionCode}</span>
              </p>
              <p className="mb-2">
                Tên: <span className="font-bold">{data.specieName}</span>
              </p>
              <p className="font-medium">Xác nhận xóa?</p>
            </div>
            <div className="flex justify-center gap-6">
              <Button
                variant="outline"
                onClick={() => setShowDeleteDialog(false)}
                className="w-24 border-2 hover:bg-gray-100 hover:text-black"
              >
                Hủy
              </Button>
              <Button
                onClick={handleDelete}
                className="w-24  border-black bg-white text-black hover:bg-gray-100"
              >
                Xóa
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </>
  );
};
