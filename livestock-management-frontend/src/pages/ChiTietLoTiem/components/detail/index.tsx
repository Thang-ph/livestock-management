'use client';

import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import {
  Card,
  CardContent,
  CardFooter,
  CardHeader,
  CardTitle
} from '@/components/ui/card';
import { useParams } from 'react-router-dom';
import {
  useGetLoTiemById,
  useHuyBoToLiem,
  useUpdateInfoLoTiem,
  useXacNhanHoanThanhLoTiem
} from '@/queries/vacxin.query';
import { EditLoTiemDialog } from './edit-lo-tiem-dialog';
import { Button } from '@/components/ui/button';
import { useState } from 'react';
import { ConfirmationDialog } from './confirmation-dialog';
import { CheckCircle, XCircle } from 'lucide-react';
import { toast } from '@/components/ui/use-toast';

export const ChiTiet = () => {
  const { id } = useParams();
  const { data, isPending, refetch } = useGetLoTiemById(String(id));
  const { mutateAsync: capNhapLoTiem } = useUpdateInfoLoTiem();
  const { mutateAsync: xacNhanHoanThanhLoTiem } = useXacNhanHoanThanhLoTiem();
  const { mutateAsync: huyBoLoTiem } = useHuyBoToLiem();
  const [showConfirmDialog, setShowConfirmDialog] = useState(false);
  const [showCancelDialog, setShowCancelDialog] = useState(false);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const renderField = (label: string, value: string) => (
    <div className="grid grid-cols-[200px_1fr] py-2.5">
      <div className="text-sm font-medium text-gray-700">{label}:</div>
      <div className="text-sm text-gray-900">{value}</div>
    </div>
  );

  const handleUpdateSuccess = () => {
    refetch();
  };

  const handleConfirmBatch = async () => {
    try {
      setIsSubmitting(true);
      const model = {
        vaccinationBatchId: String(id),
        requestedBy: 'currentUser'
      };
      await xacNhanHoanThanhLoTiem(model);
      setShowConfirmDialog(false);
      refetch();
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi xác nhận lô tiêm.',
        variant: 'destructive'
      });
      console.error('Error confirming batch:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleCancelBatch = async () => {
    try {
      setIsSubmitting(true);
      const model = {
        vaccinationBatchId: String(id),
        requestedBy: 'currentUser'
      };
      await huyBoLoTiem(model);
      setShowCancelDialog(false);
      refetch();
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi hủy bỏ lô tiêm.',
        variant: 'destructive'
      });
      console.error('Error canceling batch:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isPending) {
    return <div>Loading...</div>;
  }
  if (!data) {
    return <div>Không tìm thấy thông tin lô tiêm.</div>;
  }

  return (
    <div className="container mx-auto max-w-4xl py-8">
      <Card className="relative border shadow-sm">
        <CardHeader className="border-b bg-muted/20 pb-4">
          <CardTitle className="text-xl font-medium text-gray-900">
            Thông tin lô tiêm
          </CardTitle>
          <EditLoTiemDialog
            loTiemData={{
              id: String(id),
              name: data.name,
              vaccinationType: data.vaccinationType,
              medcicalType: data.medcicalType,
              symptom: data.symptom,
              dateSchedule: data.dateSchedule,
              status: data.status,
              conductedBy: data.conductedBy,
              note: data.note,
              vaccineId: data.vaccineId || '',
              diseaseId: data.diseaseId || ''
            }}
            capNhapLoTiem={async (updateData) => {
              const [err] = await capNhapLoTiem(updateData);
              if (err) {
                toast({
                  title: 'Lỗi',
                  description: 'Có lỗi xảy ra khi cập nhật lô tiêm.',
                  variant: 'destructive'
                });
                return;
              }
              handleUpdateSuccess();
            }}
          />
        </CardHeader>
        <CardContent className="p-6">
          {renderField('Tên lô tiêm', data.name)}
          {renderField('Loại tiêm', data.vaccinationType)}
          {renderField('Loại thuốc', data.medicineName)}
          {renderField('Phòng bệnh', data.symptom)}
          {renderField(
            'Ngày dự kiến thực hiện',
            format(data.dateSchedule, 'dd-MM-yyyy', { locale: vi })
          )}
          {renderField('Trạng thái', data.status)}
          {renderField('Người thực hiện', data.conductedBy)}
          {renderField('Ghi chú', data.note)}
        </CardContent>
        <CardFooter className="flex justify-end gap-4 border-t p-4">
          {data.status !== 'ĐÃ_HỦY' && data.status !== 'HOÀN_THÀNH' && (
            <>
              {data.status !== 'ĐANG_THỰC_HIỆN' && (
                <Button
                  variant="outline"
                  className="flex items-center gap-2"
                  onClick={() => setShowCancelDialog(true)}
                  disabled={
                    isSubmitting ||
                    data.status === 'ĐÃ_HỦY' ||
                    data.status === 'HOÀN_THÀNH'
                  }
                >
                  <XCircle className="h-4 w-4" />
                  Hủy lô tiêm
                </Button>
              )}
              {data.status !== 'CHỜ_THỰC_HIỆN' && (
                <Button
                  className="flex items-center gap-2"
                  onClick={() => setShowConfirmDialog(true)}
                  disabled={
                    isSubmitting ||
                    data.status === 'ĐÃ_HỦY' ||
                    data.status === 'HOÀN_THÀNH'
                  }
                >
                  <CheckCircle className="h-4 w-4" />
                  Xác nhận lô tiêm
                </Button>
              )}
            </>
          )}
        </CardFooter>
      </Card>

      {/* Confirmation Dialog */}

      <ConfirmationDialog
        isOpen={showConfirmDialog}
        onClose={() => setShowConfirmDialog(false)}
        onConfirm={handleConfirmBatch}
        title="Xác nhận lô tiêm"
        description="Bạn có chắc chắn muốn xác nhận hoàn thành lô tiêm này không?"
        confirmText="Xác nhận"
      />

      {/* Cancel Dialog */}
      <ConfirmationDialog
        isOpen={showCancelDialog}
        onClose={() => setShowCancelDialog(false)}
        onConfirm={handleCancelBatch}
        title="Hủy lô tiêm"
        description="Bạn có chắc chắn muốn hủy lô tiêm này không?"
        confirmText="Hủy lô tiêm"
      />
    </div>
  );
};

export default ChiTiet;
