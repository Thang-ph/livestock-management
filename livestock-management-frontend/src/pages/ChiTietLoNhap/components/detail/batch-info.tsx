'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle
} from '@/components/ui/card';
import { Edit, Check, X } from 'lucide-react';
import ConfirmDialog from './confirm-dialog';
import EditBatchModal from './edit-batch-modal';
import {
  useUpdateLoNhap,
  useUpdateSuccessLoNhap,
  useUpdateCancelLoNhap
} from '@/queries/lo-nhap.query';
import { useQueryClient } from '@tanstack/react-query';
import { toast } from '@/components/ui/use-toast';

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

interface BatchInfoCardProps {
  batchInfo: BatchInfo;
}

export default function BatchInfoCard({ batchInfo }: BatchInfoCardProps) {
  // State cho các dialog và modal
  const [showCancelDialog, setShowCancelDialog] = useState(false);
  const [showCompleteDialog, setShowCompleteDialog] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);

  // State để lưu trữ dữ liệu hiện tại (để cập nhật UI ngay lập tức)
  const [currentBatchInfo, setCurrentBatchInfo] =
    useState<BatchInfo>(batchInfo);

  // Sử dụng TanStack Query
  const { mutate: updateLoNhap, isPending: isUpdating } = useUpdateLoNhap();
  const { mutate: updateCancelLoNhap, isPending: isCancelling } =
    useUpdateCancelLoNhap();
  const { mutate: updateSuccessLoNhap, isPending: isCompleting } =
    useUpdateSuccessLoNhap();
  const queryClient = useQueryClient();

  // Trạng thái đang xử lý tổng hợp
  const isProcessing = isUpdating || isCancelling || isCompleting;

  // Hàm định dạng ngày tháng
  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'N/A';
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    }).format(date);
  };

  const handleCancel = () => {
    const payload = {
      id: currentBatchInfo.id,
      requestedBy: 'SYS'
    };

    updateCancelLoNhap(payload, {
      onSuccess: () => {
        setShowCancelDialog(false);
        // Cập nhật UI ngay lập tức
        setCurrentBatchInfo((prev) => ({
          ...prev,
          status: 'ĐÃ_HỦY'
        }));
        // Làm mới dữ liệu từ server
        queryClient.invalidateQueries({
          queryKey: ['get-lo-nhap-by-id', currentBatchInfo.id]
        });
      },
      onError: (error) => {
        console.error('Lỗi khi hủy bỏ:', error);
      }
    });
  };

  // Xử lý khi hoàn thành
  const handleComplete = () => {
    const payload = {
      id: currentBatchInfo.id,
      requestedBy: 'SYS'
    };

    updateSuccessLoNhap(payload, {
      onSuccess: () => {
        setShowCompleteDialog(false);
        // Cập nhật UI ngay lập tức
        setCurrentBatchInfo((prev) => ({
          ...prev,
          status: 'ĐÃ_HOÀN_THÀNH'
        }));
        // Làm mới dữ liệu từ server
        queryClient.invalidateQueries({
          queryKey: ['get-lo-nhap-by-id', currentBatchInfo.id]
        });
      },
      onError: (error: any) => {
        console.error('Lỗi khi hoàn thành:', error);
        toast({
          title: 'Lỗi',
          description: error.data.data,
          variant: 'destructive'
        });

        console.error('Lỗi khi hoàn thành:', error);
      }
    });
  };

  // Xử lý khi cập nhật thông tin
  const handleUpdateInfo = (updatedInfo: Partial<BatchInfo>) => {
    // Cập nhật UI ngay lập tức
    setCurrentBatchInfo((prev) => ({
      ...prev,
      ...updatedInfo
    }));
  };

  return (
    <>
      <Card className="w-full shadow-md">
        <CardHeader className="flex flex-row items-center justify-between border-b bg-gray-50">
          <CardTitle className="text-xl">Thông tin lô tiêm</CardTitle>
          <div className="flex gap-2">
            <Button
              variant="outline"
              size="sm"
              className="gap-1"
              onClick={() => setShowEditModal(true)}
              disabled={currentBatchInfo.status !== 'CHỜ_NHẬP' || isProcessing}
            >
              <Edit className="h-4 w-4" />
              Chỉnh sửa
            </Button>
            <Button
              variant="default"
              size="sm"
              className="gap-1"
              onClick={() => setShowCompleteDialog(true)}
              disabled={currentBatchInfo.status !== 'CHỜ_NHẬP' || isProcessing}
            >
              <Check className="h-4 w-4" />
              Hoàn thành
            </Button>
          </div>
        </CardHeader>
        <CardContent className="pt-6">
          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="space-y-1 border-b pb-2 md:border-b-0 md:pb-0">
              <p className="text-sm font-medium text-gray-500">Tên lô nhập:</p>
              <p>{currentBatchInfo.name}</p>
            </div>
            <div className="space-y-1 border-b pb-2 md:border-b-0 md:pb-0">
              <p className="text-sm font-medium text-gray-500">
                Số lượng dự kiến:
              </p>
              <p>{currentBatchInfo.estimatedQuantity} (con)</p>
            </div>
            <div className="space-y-1 border-b pb-2 md:border-b-0 md:pb-0">
              <p className="text-sm font-medium text-gray-500">Nơi nhập:</p>
              <p>{currentBatchInfo.originLocation}</p>
            </div>
            <div className="space-y-1 border-b pb-2 md:border-b-0 md:pb-0">
              <p className="text-sm font-medium text-gray-500">Trại nhập về:</p>
              <p>{currentBatchInfo.importToBarn}</p>
            </div>
            <div className="space-y-1 border-b pb-2 md:border-b-0 md:pb-0">
              <p className="text-sm font-medium text-gray-500">
                Ngày dự kiến hoàn thành:
              </p>
              <p>{formatDate(currentBatchInfo.expectedCompletionDate)}</p>
            </div>
            <div className="space-y-1 border-b pb-2 md:border-b-0 md:pb-0">
              <p className="text-sm font-medium text-gray-500">Trạng thái:</p>
              <p className="font-medium">
                <span
                  className={`rounded-full px-2 py-1 text-xs ${
                    currentBatchInfo.status === 'CHỜ_NHẬP'
                      ? 'bg-yellow-100 text-yellow-800'
                      : currentBatchInfo.status === 'ĐÃ_HOÀN_THÀNH'
                        ? 'bg-green-100 text-green-800'
                        : 'bg-red-100 text-red-800'
                  }`}
                >
                  {currentBatchInfo.status}
                </span>
              </p>
            </div>
            <div className="space-y-1 border-b pb-2 md:border-b-0 md:pb-0">
              <p className="text-sm font-medium text-gray-500">Người tạo:</p>
              <p>{currentBatchInfo.createdBy}</p>
            </div>
            <div className="space-y-1">
              <p className="text-sm font-medium text-gray-500">Ngày tạo:</p>
              <p>{formatDate(currentBatchInfo.createdAt)}</p>
            </div>
          </div>
        </CardContent>
        <CardFooter className="flex justify-end gap-2 border-t bg-gray-50 pt-2">
          <Button
            variant="destructive"
            size="sm"
            className="gap-1"
            onClick={() => setShowCancelDialog(true)}
            disabled={currentBatchInfo.status !== 'CHỜ_NHẬP' || isProcessing}
          >
            <X className="h-4 w-4" />
            Hủy bỏ
          </Button>
        </CardFooter>
      </Card>

      {/* Dialog xác nhận hủy bỏ */}
      <ConfirmDialog
        open={showCancelDialog}
        onOpenChange={setShowCancelDialog}
        title="Xác nhận hủy bỏ"
        description="Bạn có chắc chắn muốn hủy bỏ lô tiêm này không? Hành động này không thể hoàn tác."
        confirmLabel="Hủy bỏ"
        cancelLabel="Đóng"
        onConfirm={handleCancel}
        variant="destructive"
        isLoading={isCancelling}
      />

      {/* Dialog xác nhận hoàn thành */}
      <ConfirmDialog
        open={showCompleteDialog}
        onOpenChange={setShowCompleteDialog}
        title="Xác nhận hoàn thành"
        description="Bạn có chắc chắn muốn đánh dấu lô tiêm này là đã hoàn thành không?"
        confirmLabel="Hoàn thành"
        cancelLabel="Đóng"
        onConfirm={handleComplete}
        variant="default"
        isLoading={isCompleting}
      />

      {/* Modal chỉnh sửa thông tin */}
      <EditBatchModal
        open={showEditModal}
        onOpenChange={setShowEditModal}
        batchInfo={currentBatchInfo}
        onUpdate={handleUpdateInfo}
      />
    </>
  );
}
