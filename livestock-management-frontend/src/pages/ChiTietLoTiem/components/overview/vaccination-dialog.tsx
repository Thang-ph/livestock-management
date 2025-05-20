'use client';

import { useEffect, useRef, useState } from 'react';
import { Html5Qrcode } from 'html5-qrcode';
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
import { useGetSpecieType } from '@/queries/admin.query';

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
  getLiveStockInfo: any;
  getLiveStockInfoById: any;
  themVatNuoiVaoLoTiem: (data: {
    batchVaccinationId: string;
    livestockId: string;
    createdBy: string;
  }) => Promise<any>;
}

export function VaccinationDialog({
  batchId,
  getLiveStockInfo,
  getLiveStockInfoById,
  themVatNuoiVaoLoTiem
}: VaccinationDialogProps) {
  const [isTagInputOpen, setIsTagInputOpen] = useState(false);
  const [isConfirmationOpen, setIsConfirmationOpen] = useState(false);
  const [tagCode, setTagCode] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [livestockInfo, setLivestockInfo] = useState<LivestockInfo | null>(
    null
  );
  const [selectedSpecieType, setSelectedSpecieType] = useState<string>('TRÂU');
  const { data: listSpecieType } = useGetSpecieType();
  const { toast } = useToast();

  const [isCameraOpen, setIsCameraOpen] = useState(false);
  const qrScannerRef = useRef<Html5Qrcode | null>(null);
  const qrRegionId = 'qr-reader';

  const handleOpenTagInput = () => {
    setIsTagInputOpen(true);
  };

  const handleCloseTagInput = async () => {
    await stopCamera();
    setIsTagInputOpen(false);
    setTagCode('');
    setSelectedSpecieType('TRÂU');
  };

  const startCamera = () => {
    setIsCameraOpen(true);
  };

  const stopCamera = async () => {
    if (qrScannerRef.current) {
      await qrScannerRef.current.stop();
      qrScannerRef.current.clear();
      qrScannerRef.current = null;
    }
    setIsCameraOpen(false);
  };

  const getLivestockIdFromUrl = (text: string): string | null => {
    const segments = text.split('/');
    const id = segments[segments.length - 1];
    return id || null;
  };

  useEffect(() => {
    if (isCameraOpen) {
      const html5QrCode = new Html5Qrcode(qrRegionId);
      qrScannerRef.current = html5QrCode;

      Html5Qrcode.getCameras().then((devices) => {
        if (devices && devices.length) {
          html5QrCode
            .start(
              devices[0].id,
              { fps: 10, qrbox: 250 },
              async (decodedText) => {
                await stopCamera();
                const livestockId = getLivestockIdFromUrl(decodedText);
                if (!livestockId) return;

                try {
                  setIsLoading(true);
                  const info = await getLiveStockInfoById({
                    id: livestockId
                  });
                  setLivestockInfo(info);
                  setIsTagInputOpen(false);
                  setIsConfirmationOpen(true);
                } catch (error) {
                  toast({
                    title: 'Lỗi',
                    description:
                      'Không thể lấy thông tin vật nuôi. Vui lòng thử lại.',
                    variant: 'destructive'
                  });
                } finally {
                  setIsLoading(false);
                }
              },
              () => {}
            )
            .catch((err) => {
              console.error('Camera start failed:', err);
              toast({
                title: 'Lỗi',
                description: 'Không thể khởi động camera.',
                variant: 'destructive'
              });
              setIsCameraOpen(false);
            });
        }
      });
    }

    return () => {
      if (isCameraOpen) stopCamera();
    };
  }, [isCameraOpen, selectedSpecieType]);

  const handleTagSubmit = async () => {
    if (!tagCode.trim()) return;

    try {
      setIsLoading(true);
      const model = {
        inspectionCode: tagCode,
        specieType: selectedSpecieType
      };
      const info = await getLiveStockInfo(model);
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
            <Button onClick={startCamera}>Mở camera quét mã</Button>

            {isCameraOpen && (
              <div className="w-full">
                <div id={qrRegionId} className="mt-2 h-64 w-full border" />
                <Button variant="outline" onClick={stopCamera} className="mt-2">
                  Tắt camera
                </Button>
              </div>
            )}

            <p className="mt-2 text-center text-sm">
              Hoặc nhập mã thẻ tai trên thẻ nhận vật nuôi
            </p>
            <Input
              value={tagCode}
              onChange={(e) => setTagCode(e.target.value)}
              placeholder="Nhập mã thẻ tai"
              className="w-full"
            />
            <div className="w-full">
              <label
                htmlFor="specieType"
                className="mb-1 block text-sm font-medium"
              >
                Loại vật nuôi
              </label>
              <select
                id="specieType"
                value={selectedSpecieType}
                onChange={(e) => setSelectedSpecieType(e.target.value)}
                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2"
              >
                {listSpecieType?.map((type, index) => (
                  <option key={index} value={type}>
                    {type}
                  </option>
                ))}
              </select>
            </div>
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
