'use client';

import type React from 'react';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription
} from '@/components/ui/dialog';
import { format } from 'date-fns';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

import {
  Popover,
  PopoverContent,
  PopoverTrigger
} from '@/components/ui/popover';
import { Calendar } from '@/components/ui/calendar';
import { CalendarIcon } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';
import {
  useDanhDauVatNuoIDaChet,
  useDeleteVatNuoiLoNhap,
  useUpdateLiveStockInBatchImport
} from '@/queries/admin.query';
import __helpers from '@/helpers';

interface LivestockData {
  id: string;
  livestockId: string;
  inspectionCode: string;
  specieId: string;
  specieName: string;
  specieType: string;
  createdAt: string;
  importedBy: string;
  importedDate: string | null;
  weightImport: number;
  status: string;
}

interface UserCellActionProps {
  data: LivestockData;
}

const STATUS_MAP: Record<string, string> = {
  CHỜ_NHẬP: 'Chờ Nhập',
  CHỜ_ĐỊNH_DANH: 'Chờ Định Danh',
  KHỎE_MẠNH: 'Khỏe Mạnh',
  ỐM: 'Ốm',
  CHỜ_XUẤT: 'Chờ Xuất',
  ĐÃ_XUẤT: 'Đã Xuất',
  CHẾT: 'Chết'
};

export const CellAction: React.FC<UserCellActionProps> = ({ data }) => {
  const { toast } = useToast();
  const [showDetailsDialog, setShowDetailsDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showDeadDialog, setShowDeadDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  const { mutateAsync: deleteVatNuoi } = useDeleteVatNuoiLoNhap();
  const { mutateAsync: danhDauVatNuoiDaChet } = useDanhDauVatNuoIDaChet();
  const { mutateAsync: updateLiveStock } = useUpdateLiveStockInBatchImport();
  const [editData, setEditData] = useState({
    inspectionCode: data.inspectionCode,
    specieName: data.specieName,
    specieId: data.specieId,
    status: data.status,
    gender: 'ĐỰC', // Default value, would need to be fetched from API
    color: 'Nâu', // Default value, would need to be fetched from API
    weight: data.weightImport,
    dob: new Date(),
    requestedBy: __helpers.getUserId() // Assuming you have a helper to get the user ID
  });

  const handleOpenEdit = () => {
    setEditData({
      inspectionCode: data.inspectionCode,
      specieName: data.specieName,
      specieId: data.specieId,
      status: data.status,
      gender: 'ĐỰC', // Default value, would need to be fetched from API
      color: 'Nâu', // Default value, would need to be fetched from API
      weight: data.weightImport,
      dob: new Date(),
      requestedBy: __helpers.getUserId() // Assuming you have a helper to get the user ID
    });
    setShowEditDialog(true);
    setShowDetailsDialog(false);
  };

  const handleEditChange = (field: string, value: any) => {
    setEditData((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSaveEdit = async () => {
    try {
      const model = {
        id: data.id,
        item: editData
      };

      const [err] = await updateLiveStock(model);

      if (err) {
        toast({
          title: 'Lỗi',
          description: 'Không thể cập nhật thông tin. Vui lòng thử lại',
          variant: 'destructive'
        });
        return;
      }

      toast({
        title: 'Thành công',
        description: 'Đã cập nhật thông tin vật nuôi',
        variant: 'success'
      });
      setShowEditDialog(false);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể cập nhật thông tin. Vui lòng thử lại',
        variant: 'destructive'
      });
    }
  };

  const handleDelete = async () => {
    try {
      // API call would go here
      const [err] = await deleteVatNuoi(data.id as any);

      if (err) {
        toast({
          title: 'Lỗi',
          description: 'Không thể xóa vật nuôi. Vui lòng thử lại',
          variant: 'destructive'
        });
        return;
      }
      toast({
        title: 'Thành công',
        description: 'Đã xóa vật nuôi',
        variant: 'success'
      });
      setShowDeleteDialog(false);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể xóa vật nuôi. Vui lòng thử lại',
        variant: 'destructive'
      });
    }
  };

  const handleMarkAsDead = async () => {
    try {
      const [err] = await danhDauVatNuoiDaChet(data.id as any);
      if (err) {
        toast({
          title: 'Lỗi',
          description: 'Không thể đánh dấu vật nuôi đã chết. Vui lòng thử lại',
          variant: 'destructive'
        });
        return;
      }

      toast({
        title: 'Thành công',
        description: 'Đã đánh dấu vật nuôi đã chết',
        variant: 'success'
      });
      setShowDeadDialog(false);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể cập nhật trạng thái. Vui lòng thử lại',
        variant: 'destructive'
      });
    }
  };

  // Format date for display
  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'N/A';
    try {
      return format(new Date(dateString), 'dd/MM/yyyy');
    } catch (error) {
      return 'N/A';
    }
  };

  return (
    <>
      <div className="flex items-center space-x-2">
        <Button variant="destructive" onClick={() => setShowDeleteDialog(true)}>
          Xóa
        </Button>
        <Button onClick={() => setShowDetailsDialog(true)}>Chi tiết</Button>
      </div>

      {/* Chi tiết Dialog */}
      <Dialog open={showDetailsDialog} onOpenChange={setShowDetailsDialog}>
        <DialogContent className="sm:max-w-[550px]">
          <DialogHeader>
            <DialogTitle>Thông tin loại vật</DialogTitle>
          </DialogHeader>
          <div className="mt-4 rounded-md border p-6">
            <div className="grid grid-cols-2 gap-y-4">
              <div>
                <p className="text-gray-500">Mã kiểm dịch:</p>
                <p className="font-medium">{data.inspectionCode}</p>
              </div>

              <div>
                <p className="text-gray-500">Loại {data.specieType}:</p>
                <p className="font-medium">{data.specieName}</p>
              </div>

              <div>
                <p className="text-gray-500">Trạng thái:</p>
                <p className="font-medium">
                  {STATUS_MAP[data.status] || data.status}
                </p>
              </div>

              <div>
                <p className="text-gray-500">Cân nặng:</p>
                <p className="font-medium">{data.weightImport} (Kg)</p>
              </div>

              <div>
                <p className="text-gray-500">Ngày chọn:</p>
                <p className="font-medium">{formatDate(data.createdAt)}</p>
              </div>

              <div>
                <p className="text-gray-500">Ngày nhập:</p>
                <p className="font-medium">{formatDate(data.importedDate)}</p>
              </div>
            </div>
          </div>
          <DialogFooter className="flex justify-between sm:justify-between">
            <Button
              variant="outline"
              onClick={() => setShowDetailsDialog(false)}
            >
              Quay lại
            </Button>
            <div className="flex space-x-2">
              <Button variant="outline" onClick={handleOpenEdit}>
                Chỉnh sửa
              </Button>
              <Button
                variant="destructive"
                onClick={() => {
                  setShowDeadDialog(true);
                  setShowDetailsDialog(false);
                }}
              >
                Đã chết
              </Button>
            </div>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Xóa Dialog */}
      <Dialog open={showDeleteDialog} onOpenChange={setShowDeleteDialog}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Xác nhận xóa</DialogTitle>
            <DialogDescription>
              Bạn có chắc chắn muốn xóa vật nuôi này? Hành động này không thể
              hoàn tác.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setShowDeleteDialog(false)}
            >
              Hủy
            </Button>
            <Button variant="destructive" onClick={handleDelete}>
              Xóa
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Đã chết Dialog */}
      <Dialog open={showDeadDialog} onOpenChange={setShowDeadDialog}>
        <DialogContent className="sm:max-w-[425px]">
          <DialogHeader>
            <DialogTitle>Xác nhận đã chết</DialogTitle>
            <DialogDescription>
              Bạn có chắc chắn muốn đánh dấu vật nuôi này đã chết? Hành động này
              không thể hoàn tác.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowDeadDialog(false)}>
              Hủy
            </Button>
            <Button variant="destructive" onClick={handleMarkAsDead}>
              Xác nhận
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Chỉnh sửa Dialog */}
      <Dialog open={showEditDialog} onOpenChange={setShowEditDialog}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>Chỉnh sửa thông tin vật nuôi</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            {/* Mã kiểm định */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="inspectionCode" className="text-right">
                Mã kiểm định
              </Label>
              <p>{data.inspectionCode}</p>
            </div>

            {/* Loài vật */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="specieName" className="text-right">
                Loài vật
              </Label>
              <p>
                {data.specieName} ({data.specieType})
              </p>
            </div>

            {/* Trạng thái */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="status" className="text-right">
                Trạng thái
              </Label>
              <p>{data.status}</p>
            </div>

            {/* Giới tính */}

            {/* Màu sắc */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="color" className="text-right">
                Màu lông
              </Label>
              <Input
                id="color"
                value={editData.color}
                onChange={(e) => handleEditChange('color', e.target.value)}
                className="col-span-3"
              />
            </div>

            {/* Cân nặng */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="weight" className="text-right">
                Cân nặng (kg)
              </Label>
              <Input
                id="weight"
                type="number"
                step="0.1"
                min="0.1"
                value={editData.weight}
                onChange={(e) =>
                  handleEditChange('weight', Number.parseFloat(e.target.value))
                }
                className="col-span-3"
              />
            </div>

            {/* Ngày sinh */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="dob" className="text-right">
                Ngày sinh
              </Label>
              <div className="col-span-3">
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className="w-full justify-start text-left font-normal"
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {editData.dob ? (
                        format(editData.dob, 'dd/MM/yyyy')
                      ) : (
                        <span>Chọn ngày</span>
                      )}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <Calendar
                      mode="single"
                      selected={editData.dob}
                      onSelect={(date) => handleEditChange('dob', date)}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={() => setShowEditDialog(false)}>
              Hủy
            </Button>
            <Button onClick={handleSaveEdit}>Lưu</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
};
