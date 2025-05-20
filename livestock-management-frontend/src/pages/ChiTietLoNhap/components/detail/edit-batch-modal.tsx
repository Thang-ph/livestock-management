'use client';

import type React from 'react';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Loader2 } from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { useGetTraiNhap, useUpdateLoNhap } from '@/queries/lo-nhap.query';

interface BatchInfo {
  id: string;
  name: string;
  estimatedQuantity: number;
  importedQuantity: number;
  originLocation: string;
  importToBarn: string;
  expectedCompletionDate: string;
  completionDate: string | null;
  status: string;
  createdBy: string;
  createdAt: string;
  createdBatchBy: string | null;
  createdBatchAt: string | null;
  listImportedLivestocks: {
    total: number;
    items: any[];
  };
}

interface TraiNhap {
  id: string;
  name: string;
  address: string;
}

interface EditBatchModalProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  batchInfo: BatchInfo;
  onUpdate: (updatedInfo: Partial<BatchInfo>) => void;
}

// Hàm định dạng ngày tháng cho input date
const formatDateForInput = (dateString: string | null) => {
  if (!dateString) return '';
  const date = new Date(dateString);
  return date.toISOString().split('T')[0]; // Lấy phần YYYY-MM-DD
};

export default function EditBatchModal({
  open,
  onOpenChange,
  batchInfo,
  onUpdate
}: EditBatchModalProps) {
  // Lấy danh sách trại nhập
  const { data: dataTraiNhap, isLoading: isLoadingTraiNhap } = useGetTraiNhap();
  const { mutate: updateLoNhap, isPending: isUpdatingLoNhap } =
    useUpdateLoNhap();

  // State cho form
  const [formData, setFormData] = useState({
    id: batchInfo.id, // Thêm id vào payload
    name: batchInfo.name,
    estimatedQuantity: batchInfo.estimatedQuantity,
    expectedCompletionDate: formatDateForInput(
      batchInfo.expectedCompletionDate
    ),
    originLocation: batchInfo.originLocation,
    barnId: '', // Sẽ được cập nhật sau khi có dữ liệu trại nhập
    updatedBy: 'User' // Giá trị mặc định
  });

  // Tìm barnId dựa trên tên trại nhập hiện tại
  useEffect(() => {
    if (dataTraiNhap && dataTraiNhap.length > 0 && batchInfo.importToBarn) {
      const barn = dataTraiNhap.find(
        (barn) => barn.name === batchInfo.importToBarn
      );
      if (barn) {
        setFormData((prev) => ({
          ...prev,
          barnId: barn.id
        }));
      } else {
        // Nếu không tìm thấy, chọn trại đầu tiên trong danh sách
        setFormData((prev) => ({
          ...prev,
          barnId: dataTraiNhap[0].id
        }));
      }
    }
  }, [dataTraiNhap, batchInfo.importToBarn]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value, type } = e.target;

    if (type === 'number') {
      setFormData((prev) => ({
        ...prev,
        [name]: Number.parseInt(value) || 0
      }));
    } else {
      setFormData((prev) => ({
        ...prev,
        [name]: value
      }));
    }
  };

  const handleSelectChange = (value: string) => {
    setFormData((prev) => ({
      ...prev,
      barnId: value
    }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    // Gọi API cập nhật sử dụng TanStack Query
    updateLoNhap(formData, {
      onSuccess: () => {
        // Tìm tên trại nhập dựa trên barnId
        const selectedBarn = dataTraiNhap?.find(
          (barn) => barn.id === formData.barnId
        );

        // Cập nhật UI
        onUpdate({
          name: formData.name,
          estimatedQuantity: formData.estimatedQuantity,
          expectedCompletionDate: formData.expectedCompletionDate,
          originLocation: formData.originLocation,
          importToBarn: selectedBarn ? selectedBarn.name : ''
        });

        // Đóng modal
        onOpenChange(false);
      },
      onError: (error) => {
        console.error('Lỗi khi cập nhật:', error);
      }
    });
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[600px]">
        <form onSubmit={handleSubmit}>
          <DialogHeader>
            <DialogTitle>Chỉnh sửa thông tin lô tiêm</DialogTitle>
            <DialogDescription>
              Cập nhật thông tin chi tiết của lô tiêm. Nhấn Lưu khi hoàn tất.
            </DialogDescription>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="name" className="text-right">
                Tên lô nhập
              </Label>
              <Input
                id="name"
                name="name"
                value={formData.name}
                onChange={handleChange}
                className="col-span-3"
                disabled={isUpdatingLoNhap}
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="estimatedQuantity" className="text-right">
                Số lượng dự kiến
              </Label>
              <Input
                id="estimatedQuantity"
                name="estimatedQuantity"
                type="number"
                value={formData.estimatedQuantity}
                onChange={handleChange}
                className="col-span-3"
                disabled={isUpdatingLoNhap}
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="originLocation" className="text-right">
                Nơi nhập
              </Label>
              <Input
                id="originLocation"
                name="originLocation"
                value={formData.originLocation}
                onChange={handleChange}
                className="col-span-3"
                disabled={isUpdatingLoNhap}
              />
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="barnId" className="text-right">
                Trại nhập về
              </Label>
              <div className="col-span-3">
                <Select
                  value={formData.barnId}
                  onValueChange={handleSelectChange}
                  disabled={isUpdatingLoNhap || isLoadingTraiNhap}
                >
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn trại nhập về" />
                  </SelectTrigger>
                  <SelectContent>
                    {isLoadingTraiNhap ? (
                      <div className="flex items-center justify-center p-2">
                        <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                        Đang tải...
                      </div>
                    ) : (
                      dataTraiNhap?.map((barn: TraiNhap) => (
                        <SelectItem key={barn.id} value={barn.id}>
                          {barn.name} - {barn.address}
                        </SelectItem>
                      ))
                    )}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="expectedCompletionDate" className="text-right">
                Ngày dự kiến hoàn thành
              </Label>
              <Input
                id="expectedCompletionDate"
                name="expectedCompletionDate"
                type="date"
                value={formData.expectedCompletionDate}
                onChange={handleChange}
                className="col-span-3"
                disabled={isUpdatingLoNhap}
              />
            </div>
          </div>
          <DialogFooter>
            <Button
              type="button"
              variant="outline"
              onClick={() => onOpenChange(false)}
              disabled={isUpdatingLoNhap}
            >
              Hủy
            </Button>
            <Button type="submit" disabled={isUpdatingLoNhap}>
              {isUpdatingLoNhap ? (
                <>
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Đang lưu...
                </>
              ) : (
                'Lưu thay đổi'
              )}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}
