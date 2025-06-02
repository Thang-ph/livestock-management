'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table';
import { Textarea } from '@/components/ui/textarea';
import { useParams } from 'react-router-dom';
import {
  useDuyetDonBaoHanh,
  useGetChiTietBaoHanh,
  useGetVacxinByInsureance,
  useTuChoiDonBaoHanh,
  useUpdateVatNuoiBaoHanh
} from '@/queries/baohanh.query';
import __helpers from '@/helpers';
import { useBanGiaoDonBaoHanh } from '@/queries/admin.query';
import { toast } from '@/components/ui/use-toast';
import { Badge } from '@/components/ui/badge';

// Confirm Dialog Component
function ConfirmDialog({ open, onOpenChange, title, description, onConfirm }) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          <DialogDescription>{description}</DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Hủy
          </Button>
          <Button onClick={onConfirm}>Xác nhận</Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// Reject Dialog Component with Reason Input
function RejectDialog({ open, onOpenChange, onConfirm }) {
  const [reason, setReason] = useState('');

  const handleConfirm = () => {
    if (reason.trim()) {
      onConfirm(reason);
      setReason(''); // Reset reason after confirm
    }
  };

  const handleClose = () => {
    setReason('');
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle>Xác nhận từ chối đơn</DialogTitle>
          <DialogDescription>
            Bạn có chắc chắn muốn từ chối đơn bảo hành này không? Vui lòng ghi
            rõ lý do từ chối.
          </DialogDescription>
        </DialogHeader>
        <div className="grid gap-4 py-4">
          <div className="grid grid-cols-4 items-center gap-4">
            <Label htmlFor="reason" className="text-right">
              Lý do từ chối:
            </Label>
            <Textarea
              id="reason"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Nhập lý do từ chối..."
              className="col-span-3 border border-black"
              rows={4}
            />
          </div>
        </div>
        <DialogFooter>
          <Button variant="outline" onClick={handleClose}>
            Hủy
          </Button>
          <Button
            onClick={handleConfirm}
            disabled={!reason.trim()}
            className="bg-red-500 hover:bg-red-600"
          >
            Xác nhận từ chối
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// Add/Update Animal Dialog Component
function AnimalDialog({ open, onOpenChange, mode, existingData, onConfirm }) {
  const [livestockId, setLivestockId] = useState(
    existingData?.inspectionCodeNew || '1231243'
  );
  const [weight, setWeight] = useState(existingData?.exportWeightReturn || 321);

  const handleConfirm = () => {
    onConfirm({
      livestockId,
      weight: Number(weight)
    });
    onOpenChange(false);
  };

  const handleClose = () => {
    setLivestockId(existingData?.inspectionCodeNew || '1231243');
    setWeight(existingData?.exportWeightReturn || 321);
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className="text-center text-lg">
            {mode === 'add' ? 'Thêm vật nuôi' : 'Cập nhật vật nuôi'}
          </DialogTitle>
        </DialogHeader>
        <div className="grid gap-6 py-4">
          <div className="grid grid-cols-3 items-center">
            <Label htmlFor="search-id" className="col-span-1">
              Mã kiểm dịch:
            </Label>
            <Input
              id="search-id"
              className="col-span-2 border border-black"
              value={livestockId}
              onChange={(e) => setLivestockId(e.target.value)}
            />
          </div>

          <div className="grid grid-cols-3 items-center">
            <Label htmlFor="animal-type" className="col-span-1">
              Loại vật nuôi:
            </Label>
            <div className="col-span-2 font-medium">BÒ 3B</div>
          </div>

          <div className="grid grid-cols-3 items-center">
            <Label htmlFor="weight" className="col-span-1">
              Trọng lượng:
            </Label>
            <Input
              id="weight"
              type="number"
              className="col-span-2 border border-black"
              value={weight}
              onChange={(e) => setWeight(e.target.value)}
            />
          </div>

          <div className="mt-4 flex justify-center">
            <Button
              className="w-64 border border-black bg-white text-black hover:bg-gray-100"
              onClick={handleConfirm}
              disabled={!livestockId.trim() || !weight}
            >
              Xác nhận {mode === 'add' ? 'thêm' : 'cập nhật'}
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}

// Delete Animal Dialog Component
function DeleteAnimalDialog({ open, onOpenChange, onConfirm }) {
  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[425px]">
        <DialogHeader>
          <DialogTitle>Xác nhận xóa vật nuôi</DialogTitle>
          <DialogDescription>
            Bạn có chắc chắn muốn xóa vật nuôi trả bảo hành này không?
          </DialogDescription>
        </DialogHeader>
        <DialogFooter>
          <Button variant="outline" onClick={() => onOpenChange(false)}>
            Hủy
          </Button>
          <Button onClick={onConfirm} className="bg-red-500 hover:bg-red-600">
            Xác nhận xóa
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
}

// Warranty Request Details Component
function WarrantyRequestDetails({ chiTietBaoHanh }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-center text-lg">
          Chi tiết yêu cầu bảo hành
        </CardTitle>
      </CardHeader>
      <CardContent>
        Trạng thái{' '}
        <Badge
          className="text-center"
          variant={
            chiTietBaoHanh?.status === 'CHỜ_DUYỆT'
              ? 'secondary'
              : chiTietBaoHanh?.status === 'TỪ_CHỐI'
                ? 'destructive'
                : chiTietBaoHanh?.status === 'HOÀN_THÀNH'
                  ? 'success'
                  : 'default'
          }
        >
          {chiTietBaoHanh?.status || 'CHỜ_DUYỆT'}
        </Badge>
      </CardContent>
    </Card>
  );
}

// Animal Warranty Request Component
function AnimalWarrantyRequest({ chiTietBaoHanh }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-lg">Vật nuôi yêu cầu bảo hành</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4">
          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="search-id">Mã kiểm dịch:</Label>
            <div className="font-medium">
              {chiTietBaoHanh?.inspectionCodeRequest || '23231'}
            </div>
          </div>

          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="animal-type">Loại vật nuôi:</Label>
            <div className="font-medium">
              {chiTietBaoHanh?.species || 'BÒ 3B'}
            </div>
          </div>

          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="weight">Trọng lượng xuất:</Label>
            <div className="font-medium">
              {chiTietBaoHanh?.exportWeight
                ? `${chiTietBaoHanh.exportWeight}kg`
                : '321kg'}
            </div>
          </div>

          <div className="my-2 border-t border-dashed"></div>

          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="disease">Loại bệnh đang mắc:</Label>
            <div className="font-medium">
              {chiTietBaoHanh?.diseaseName || 'Lở mồm long móng'}
            </div>
          </div>

          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="image">Hình ảnh:</Label>
            <img
              src={
                chiTietBaoHanh?.imageUris || 'https://via.placeholder.com/150'
              }
              alt="Hình ảnh vật nuôi"
              className="h-20 w-20 rounded-md border border-black object-cover"
            />
          </div>

          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="reason">Lý do khác:</Label>
            <div className="font-medium">
              {chiTietBaoHanh?.otherReason || 'N/A'}
            </div>
          </div>

          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="return">Thu hồi vật nuôi này:</Label>
            <RadioGroup
              defaultValue={chiTietBaoHanh?.isLivestockReturn ? 'yes' : 'no'}
              className="flex gap-4"
            >
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="yes" id="yes" />
                <Label htmlFor="yes">Có</Label>
              </div>
              <div className="flex items-center space-x-2">
                <RadioGroupItem value="no" id="no" />
                <Label htmlFor="no">Không</Label>
              </div>
            </RadioGroup>
          </div>

          <div className="grid grid-cols-2 items-center">
            <Label htmlFor="notes">Ghi chú:</Label>
            <Textarea
              id="notes"
              className="border border-black"
              defaultValue={chiTietBaoHanh?.note || ''}
            />
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

// Animal Warranty Return Component
function AnimalWarrantyReturn({ chiTietBaoHanh, onUpdateAnimal }) {
  const [showAnimalDialog, setShowAnimalDialog] = useState(false);
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);

  // Kiểm tra điều kiện hiển thị component
  const shouldShowComponent = !['CHỜ_DUYỆT', 'TỪ_CHỐI', 'ĐÃ_HỦY'].includes(
    chiTietBaoHanh?.status
  );

  if (!shouldShowComponent) {
    return null;
  }

  // Kiểm tra xem đã có vật nuôi trả hay chưa
  const hasReturnAnimal = chiTietBaoHanh?.inspectionCodeNew;

  // Kiểm tra xem có hiển thị buttons hay không (ẩn khi status = HOÀN_THÀNH)
  const showButtons = chiTietBaoHanh?.status !== 'HOÀN_THÀNH';

  const handleAnimalAction = (data) => {
    onUpdateAnimal(data);
    setShowAnimalDialog(false);
  };

  const handleDelete = () => {
    // Gọi API xóa vật nuôi ở đây
    onUpdateAnimal({ livestockId: null, weight: null });
    setShowDeleteDialog(false);
  };

  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-lg">
          Vật nuôi trả bảo hành(đổi lại cho khách)
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4">
          {hasReturnAnimal ? (
            <>
              <div className="grid grid-cols-2 items-center">
                <Label htmlFor="search-id">Mã kiểm dịch:</Label>
                <div className="font-medium text-red-500">
                  {chiTietBaoHanh?.inspectionCodeNew}
                </div>
              </div>

              <div className="grid grid-cols-2 items-center">
                <Label htmlFor="animal-type">Loại vật nuôi:</Label>
                <div className="font-medium">
                  {chiTietBaoHanh?.species || 'BÒ 3B'}
                </div>
              </div>

              <div className="grid grid-cols-2 items-center">
                <Label htmlFor="weight">Trọng lượng:</Label>
                <div className="font-medium text-red-500">
                  {chiTietBaoHanh?.exportWeightReturn
                    ? `${chiTietBaoHanh.exportWeightReturn}kg`
                    : 'N/A'}
                </div>
              </div>

              {showButtons && (
                <div className="flex justify-end gap-2">
                  <Button
                    className="border border-blue-500 bg-white text-blue-500 hover:bg-blue-50"
                    onClick={() => setShowAnimalDialog(true)}
                  >
                    Cập nhật vật nuôi
                  </Button>
                  <Button
                    className="border border-red-500 bg-white text-red-500 hover:bg-red-50"
                    onClick={() => setShowDeleteDialog(true)}
                  >
                    Xóa vật nuôi
                  </Button>
                </div>
              )}
            </>
          ) : (
            <>
              <div className="py-8 text-center text-gray-500">
                Chưa có vật nuôi trả bảo hành
              </div>

              {showButtons && (
                <div className="flex justify-end">
                  <Button
                    className="border border-red-500 bg-white text-red-500 hover:bg-red-50"
                    onClick={() => setShowAnimalDialog(true)}
                  >
                    Thêm vật nuôi
                  </Button>
                </div>
              )}
            </>
          )}
        </div>
      </CardContent>

      <AnimalDialog
        open={showAnimalDialog}
        onOpenChange={setShowAnimalDialog}
        mode={hasReturnAnimal ? 'update' : 'add'}
        existingData={chiTietBaoHanh}
        onConfirm={handleAnimalAction}
      />

      <DeleteAnimalDialog
        open={showDeleteDialog}
        onOpenChange={setShowDeleteDialog}
        onConfirm={handleDelete}
      />
    </Card>
  );
}

// Warranty Contract Component
function WarrantyContract({ vacXinByBaoHanh }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-center text-lg">
          Hợp đồng đơn bảo hành
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4">
          <div className="grid grid-cols-1 gap-2">
            <Label htmlFor="contract-id">Mã gói thầu/ đơn mua:</Label>
            <div className="font-medium">{vacXinByBaoHanh?.id || ''}</div>
          </div>

          <div className="grid grid-cols-1 gap-2">
            <Label htmlFor="contract-name">Tên gói thầu/ đơn mua:</Label>
            <div className="font-medium">{vacXinByBaoHanh?.name || ''}</div>
          </div>

          <div className="grid grid-cols-1 gap-2">
            <Label htmlFor="disease-list">Danh sách bệnh bảo hành:</Label>
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="bg-gray-400 text-center">
                    Loại bệnh
                  </TableHead>
                  <TableHead className="bg-gray-400 text-center">
                    Thời gian bảo hành
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {vacXinByBaoHanh?.vaccinationRequirements?.map(
                  (requirement, index) => (
                    <TableRow key={requirement.id}>
                      <TableCell
                        className={
                          index % 2 === 0
                            ? 'bg-gray-200 text-center'
                            : 'text-center'
                        }
                      >
                        {requirement.name}
                      </TableCell>
                      <TableCell
                        className={
                          index % 2 === 0
                            ? 'bg-gray-200 text-center'
                            : 'text-center'
                        }
                      >
                        {requirement.dateOfInsurance} ngày
                      </TableCell>
                    </TableRow>
                  )
                ) || (
                  <>
                    <TableRow>
                      <TableCell className="bg-gray-200 text-center">
                        Lở mồm long móng
                      </TableCell>
                      <TableCell className="bg-gray-200 text-center">
                        12 ngày
                      </TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell className="text-center">
                        Tụ huyết trùng
                      </TableCell>
                      <TableCell className="text-center">21 ngày</TableCell>
                    </TableRow>
                    <TableRow>
                      <TableCell className="bg-gray-200 text-center">
                        Ký sinh trùng
                      </TableCell>
                      <TableCell className="bg-gray-200 text-center">
                        10 ngày
                      </TableCell>
                    </TableRow>
                  </>
                )}
              </TableBody>
            </Table>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

// Customer Information Component
function CustomerInformation({ vacXinByBaoHanh }) {
  return (
    <Card>
      <CardHeader className="pb-2">
        <CardTitle className="text-center text-lg">
          Thông tin khách hàng
        </CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid gap-4">
          <div className="grid grid-cols-1 gap-2">
            <Label htmlFor="customer-name">Tên khách hàng:</Label>
            <div className="font-medium">
              {vacXinByBaoHanh?.customerName || ''}
            </div>
          </div>

          <div className="grid grid-cols-1 gap-2">
            <Label htmlFor="address">Địa chỉ:</Label>
            <div className="font-medium">
              {vacXinByBaoHanh?.customerAddress || ''}
            </div>
          </div>

          <div className="grid grid-cols-1 gap-2">
            <Label htmlFor="phone">Số điện thoại:</Label>
            <div className="font-medium">
              {vacXinByBaoHanh?.customerPhone || ''}
            </div>
          </div>
        </div>
      </CardContent>
    </Card>
  );
}

// Main Page Component
export default function OverviewTab() {
  const [showAcceptDialog, setShowAcceptDialog] = useState(false);
  const [showRejectDialog, setShowRejectDialog] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const { id } = useParams();
  const { data: chiTietBaoHanh } = useGetChiTietBaoHanh(id);
  const { data: vacXinByBaoHanh } = useGetVacxinByInsureance(id);
  const { mutateAsync: duyetDonBaoHanh } = useDuyetDonBaoHanh();
  const { mutateAsync: tuChoiDonBaoHanh } = useTuChoiDonBaoHanh();
  const { mutateAsync: updateVatNuoi } = useUpdateVatNuoiBaoHanh();
  const { mutateAsync: banGiaoDon } = useBanGiaoDonBaoHanh();
  // Mẫu dữ liệu cho trường hợp API chưa trả về
  const sampleChiTietBaoHanh = {
    requestLivestockId: '6ukrvnzt9bovhedglwjq8hbv5',
    diseaseId: 'd3a8a180eb1ab3a0fd52fe57722634be',
    otherReason: 'N/A',
    imageUris: 'hieu',
    status: 'CHỜ_DUYỆT',
    approvedAt: null,
    rejectedAt: null,
    processingAt: null,
    completedAt: null,
    cancelledAt: null,
    newLivestockId: null,
    rejectReason: null,
    note: null,
    procurementDetailId: null,
    orderRequirementId: '74boadi94hfi1nelc11o91c8q',
    requestLivestockStatus: 'KHÔNG_THU_HỒI',
    isLivestockReturn: false,
    species: 'Bò lai Mỹ',
    exportWeight: 430,
    exportWeightReturn: null,
    inspectionCodeRequest: '100023',
    inspectionCodeNew: null,
    diseaseName: 'Nở mồm long móng',
    id: '6gzwls1krlcs3egqj83kocqf9',
    createdAt: '2025-05-28T21:00:20.1722521',
    updatedAt: '0001-01-01T00:00:00',
    createdBy: 'HieuNT',
    updatedBy: null
  };

  const sampleVacXinByBaoHanh = {
    id: 'OD-06',
    name: 'Đơn mua lẻ mã OD-06',
    customerName: 'Nguyen Trung Hieu',
    customerAddress: '81 Dong Cua, Le Loi, Bac Ninh',
    customerPhone: '0926760876',
    vaccinationRequirements: [
      {
        id: 'd3a8a180eb1ab3a0fd52fe57722634be',
        name: 'Nở mồm long móng',
        dateOfInsurance: 21
      }
    ]
  };

  // Sử dụng dữ liệu mẫu nếu API chưa trả về
  const displayChiTietBaoHanh = chiTietBaoHanh || sampleChiTietBaoHanh;
  const displayVacXinByBaoHanh = vacXinByBaoHanh || sampleVacXinByBaoHanh;

  // Handle approve warranty request
  const handleApprove = async () => {
    try {
      setIsLoading(true);
      await duyetDonBaoHanh({
        updatedBy: __helpers.getUserId(), // Thay thế bằng user hiện tại
        id: id
      });
      setShowAcceptDialog(false);
      // Có thể thêm toast notification hoặc redirect
      console.log('Duyệt đơn thành công');
    } catch (error) {
      console.error('Lỗi khi duyệt đơn:', error);
      // Xử lý lỗi - có thể hiển thị toast error
    } finally {
      setIsLoading(false);
    }
  };

  // Handle reject warranty request
  const handleReject = async (reason) => {
    try {
      setIsLoading(true);
      await tuChoiDonBaoHanh({
        updatedBy: __helpers.getUserId(), // Thay thế bằng user hiện tại
        id: id,
        reasonReject: reason
      });
      setShowRejectDialog(false);
      // Có thể thêm toast notification hoặc redirect
      console.log('Từ chối đơn thành công');
    } catch (error) {
      console.error('Lỗi khi từ chối đơn:', error);
      // Xử lý lỗi - có thể hiển thị toast error
    } finally {
      setIsLoading(false);
    }
  };

  // Handle update animal
  const handleUpdateAnimal = async (data) => {
    try {
      setIsLoading(true);
      await updateVatNuoi({
        weight: data.weight,
        livestockId: data.livestockId,
        updatedBy: __helpers.getUserId(),
        id: id
      });
      // Có thể thêm toast notification hoặc refetch data
      console.log('Cập nhật vật nuôi thành công');
    } catch (error) {
      console.error('Lỗi khi cập nhật vật nuôi:', error);
      // Xử lý lỗi - có thể hiển thị toast error
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <main className="container mx-auto p-4">
      <div className="mb-4 flex justify-end gap-2">
        {chiTietBaoHanh?.newLivestockId !== null ? (
          <Button
            variant="outline"
            disabled={chiTietBaoHanh?.status === 'HOÀN_THÀNH'}
            onClick={async () => {
              const model = {
                id: id,
                updatedBy: __helpers.getUserId() // Thay thế bằng user hiện tại
              };
              const [err] = await banGiaoDon(model);
              if (err) {
                toast({
                  title: 'Lỗi',
                  description:
                    err.data?.data || 'Không thể bàn giao đơn bảo hành',
                  variant: 'destructive'
                });
                return;
              }
              toast({
                title: 'Thành công',
                description: 'Đã bàn giao đơn bảo hành thành công',
                variant: 'success'
              });
            }}
            className="bg-green-500 text-white hover:bg-green-600"
          >
            Bàn giao
          </Button>
        ) : (
          <Button
            variant="outline"
            onClick={() => setShowAcceptDialog(true)}
            disabled={isLoading}
            className="bg-green-500 text-white hover:bg-green-600"
          >
            {isLoading ? 'Đang xử lý...' : 'Đồng ý đơn'}
          </Button>
        )}

        <Button
          variant="outline"
          onClick={() => setShowRejectDialog(true)}
          disabled={isLoading}
          className="bg-red-500 text-white hover:bg-red-600"
        >
          {isLoading ? 'Đang xử lý...' : 'Từ chối đơn'}
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <div className="space-y-4">
          <WarrantyRequestDetails chiTietBaoHanh={displayChiTietBaoHanh} />
          <AnimalWarrantyRequest chiTietBaoHanh={displayChiTietBaoHanh} />
          <AnimalWarrantyReturn
            chiTietBaoHanh={displayChiTietBaoHanh}
            onUpdateAnimal={handleUpdateAnimal}
          />
        </div>
        <div className="space-y-4">
          <WarrantyContract vacXinByBaoHanh={displayVacXinByBaoHanh} />
          <CustomerInformation vacXinByBaoHanh={displayVacXinByBaoHanh} />
        </div>
      </div>

      <ConfirmDialog
        open={showAcceptDialog}
        onOpenChange={setShowAcceptDialog}
        title="Xác nhận đồng ý đơn"
        description="Bạn có chắc chắn muốn đồng ý đơn bảo hành này không?"
        onConfirm={handleApprove}
      />

      <RejectDialog
        open={showRejectDialog}
        onOpenChange={setShowRejectDialog}
        onConfirm={handleReject}
      />
    </main>
  );
}
