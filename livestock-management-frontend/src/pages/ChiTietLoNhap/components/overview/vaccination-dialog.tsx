'use client';

import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useToast } from '@/components/ui/use-toast';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table';
import { NotebookPen } from 'lucide-react';

interface LivestockInfo {
  livestockId: string;
  inspectionCode: string;
  specieName: string;
  color: string;
  vaccinationInfos: {
    medicineName: string;
    diseaseName: string;
    numberOfVaccination: number;
  }[];
}

interface VaccinationDialogProps {
  batchId: string;
  getLiveStockInfo: (tagCode: string) => Promise<LivestockInfo>;
  themVatNuoiVaoLoTiem: (data: {
    batchVaccinationId: string;
    livestockId: string;
    createdBy: string;
  }) => Promise<any>;
}

export function VaccinationDialog({
  batchId,
  getLiveStockInfo,
  themVatNuoiVaoLoTiem
}: VaccinationDialogProps) {
  const [isTagInputOpen, setIsTagInputOpen] = useState(false);
  const [isConfirmationOpen, setIsConfirmationOpen] = useState(false);
  const [tagCode, setTagCode] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [livestockInfo, setLivestockInfo] = useState<LivestockInfo | null>(
    null
  );
  const { toast } = useToast();

  const handleOpenTagInput = () => {
    setIsTagInputOpen(true);
  };

  const handleCloseTagInput = () => {
    setIsTagInputOpen(false);
    setTagCode('');
  };

  const handleTagSubmit = async () => {
    if (!tagCode.trim()) return;

    try {
      setIsLoading(true);
      const info = await getLiveStockInfo(tagCode);
      setLivestockInfo(info);
      setIsTagInputOpen(false);
      setIsConfirmationOpen(true);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể lấy thông tin vật nuôi. Vui lòng thử lại.',
        variant: 'destructive'
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleConfirm = async () => {
    if (!livestockInfo) return;

    try {
      setIsLoading(true);
      await themVatNuoiVaoLoTiem({
        batchVaccinationId: batchId,
        livestockId: livestockInfo.livestockId,
        createdBy: 'ADMIN'
      });

      toast({
        title: 'Thành công',
        description: 'Đã thêm vật nuôi vào lô tiêm thành công.',
        variant: 'success'
      });

      setIsConfirmationOpen(false);
      setLivestockInfo(null);
      setTagCode('');
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể thêm vật nuôi vào lô tiêm. Vui lòng thử lại.',
        variant: 'destructive'
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleCancelConfirmation = () => {
    setIsConfirmationOpen(false);
    setLivestockInfo(null);
  };

  return (
    <>
      <Button onClick={handleOpenTagInput} className="text-center font-bold">
        <NotebookPen size={16} className="mr-2" />
        Ghi nhận tiêm
      </Button>

      {/* Dialog nhập mã thẻ tai */}
      <Dialog open={isTagInputOpen} onOpenChange={setIsTagInputOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="text-center text-xl font-bold">
              Quét QR ghi nhận tiêm
            </DialogTitle>
          </DialogHeader>
          <div className="flex flex-col items-center space-y-4">
            <div className="flex h-48 w-64 items-center justify-center border">
              <div className="relative h-full w-full">
                {/* Placeholder for QR scanner */}
                <div className="absolute inset-0 flex items-center justify-center">
                  <div className="flex h-full w-full items-center justify-center border">
                    <div className="flex h-full w-full items-center justify-center border border-gray-300">
                      <div className="relative h-full w-full">
                        <div className="absolute left-0 top-0 h-0.5 w-full bg-gray-300" />
                        <div className="absolute bottom-0 left-0 h-0.5 w-full bg-gray-300" />
                        <div className="absolute left-0 top-0 h-full w-0.5 bg-gray-300" />
                        <div className="absolute right-0 top-0 h-full w-0.5 bg-gray-300" />
                        <div className="absolute inset-0 flex items-center justify-center">
                          <div className="h-3/4 w-3/4">
                            <div className="relative h-full w-full">
                              <div className="absolute bottom-0 left-0 right-0 top-0">
                                <div className="flex h-full w-full items-center justify-center">
                                  <div className="absolute h-0.5 w-full rotate-45 transform bg-gray-300" />
                                  <div className="absolute h-0.5 w-full -rotate-45 transform bg-gray-300" />
                                </div>
                              </div>
                            </div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            <p className="text-center text-sm">
              Nhập mã thẻ tai trên thẻ nhận vật nuôi
            </p>
            <Input
              value={tagCode}
              onChange={(e) => setTagCode(e.target.value)}
              placeholder="Nhập mã thẻ tai"
              className="w-full"
            />
          </div>
          <DialogFooter className="flex justify-between sm:justify-between">
            <Button variant="outline" onClick={handleCloseTagInput}>
              Hủy
            </Button>
            <Button
              onClick={handleTagSubmit}
              disabled={isLoading || !tagCode.trim()}
            >
              {isLoading ? 'Đang xử lý...' : 'Tiếp tục'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Dialog xác nhận tiêm vật nuôi */}
      <Dialog open={isConfirmationOpen} onOpenChange={setIsConfirmationOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle className="text-center text-xl font-bold">
              Xác nhận tiêm vật nuôi
            </DialogTitle>
          </DialogHeader>
          {livestockInfo && (
            <div className="space-y-4">
              <div className="space-y-1">
                <p>
                  <span className="font-medium">Mã thẻ tai:</span>{' '}
                  {livestockInfo.inspectionCode}
                </p>
                <p>
                  <span className="font-medium">Giống:</span>{' '}
                  {livestockInfo.specieName}
                </p>
                <p>
                  <span className="font-medium">Màu lông:</span>{' '}
                  {livestockInfo.color}
                </p>
              </div>

              <Table>
                <TableHeader>
                  <TableRow>
                    <TableHead className="bg-gray-400 text-center text-white">
                      Bệnh
                    </TableHead>
                    <TableHead className="bg-gray-400 text-center text-white">
                      Số mũi đã tiêm
                    </TableHead>
                  </TableRow>
                </TableHeader>
                <TableBody>
                  {livestockInfo.vaccinationInfos.map((info, index) => (
                    <TableRow
                      key={index}
                      className={
                        index % 2 === 0 ? 'bg-gray-100' : 'bg-gray-200'
                      }
                    >
                      <TableCell className="text-center">
                        {info.diseaseName}
                      </TableCell>
                      <TableCell className="text-center">
                        {info.numberOfVaccination}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </div>
          )}
          <DialogFooter className="flex justify-between sm:justify-between">
            <Button variant="outline" onClick={handleCancelConfirmation}>
              Hủy
            </Button>
            <Button onClick={handleConfirm} disabled={isLoading}>
              {isLoading ? 'Đang xử lý...' : 'Xác nhận'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
