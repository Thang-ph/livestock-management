'use client';

import { useState } from 'react';
import { ArrowRight, AlertCircle, Plus, X } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { useParams } from 'react-router-dom';
import {
  useCancelDonMuaLe,
  useCompleteDonMuaLe,
  useGetChiTietDonMuaLe,
  useTaoBaoCaoDonMuaLe,
  useUpdateDonMuaLe
} from '@/queries/donmuale.query';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from '@/components/ui/alert-dialog';
import { toast } from '@/components/ui/use-toast';
import { useGetLoaiBenh } from '@/queries/benh.query';

export default function OverviewTab() {
  const [showWarrantyDetails, setShowWarrantyDetails] = useState(false);
  const [selectedRequirementIndex, setSelectedRequirementIndex] = useState(0);
  const [showCancelDialog, setShowCancelDialog] = useState(false);
  const [showCompleteDialog, setShowCompleteDialog] = useState(false);
  const [showEditDialog, setShowEditDialog] = useState(false);
  interface VaccinationRequirementForm {
    vaccinationRequirementId: string;
    diseaseId: string;
    diseaseName: string;
    insuranceDuration: number;
  }

  interface DetailForm {
    orderRequirementId: string;
    specieId: string;
    specieName: string;
    weightFrom: number;
    weightTo: number;
    total: number;
    description: string;
    vaccintionRequirement: VaccinationRequirementForm[];
  }

  interface EditFormData {
    customerName: string;
    phoneNumber: string;
    address: string;
    email: string;
    details: DetailForm[];
  }

  const [editFormData, setEditFormData] = useState<EditFormData | null>(null);

  const { mutateAsync: cancleDonMuaLe } = useCancelDonMuaLe();
  const { mutateAsync: completeDonMuaLe } = useCompleteDonMuaLe();
  const { data: resLoaiBenh } = useGetLoaiBenh();
  const listLoaiBenh = resLoaiBenh?.items || [];
  console.log('listLoaiBenh', listLoaiBenh);
  const { mutateAsync: updateDonMuaLe } = useUpdateDonMuaLe();
  const { id } = useParams();
  const {
    data: chiTietDon,
    isLoading,
    isPending,
    refetch
  } = useGetChiTietDonMuaLe(id as string);

  const { mutateAsync: taoBaoCao } = useTaoBaoCaoDonMuaLe();

  const handleShowWarrantyDetails = (index: number) => {
    setSelectedRequirementIndex(index);
    setShowWarrantyDetails(true);
  };

  const handleEditClick = () => {
    if (chiTietDon) {
      setEditFormData({
        customerName: chiTietDon.customerName || '',
        phoneNumber: chiTietDon.phoneNumber || '',
        address: chiTietDon.address || '',
        email: chiTietDon.email || '',
        details:
          chiTietDon.details?.map((detail) => ({
            orderRequirementId: detail.orderRequirementId,
            specieId: detail.specieId,
            specieName: detail.specieName,
            weightFrom: detail.weightFrom,
            weightTo: detail.weightTo,
            total: detail.total,
            description: detail.description || '',
            vaccintionRequirement:
              detail.vaccintionRequirement?.map((vr) => ({
                vaccinationRequirementId: vr.vaccinationRequirementId,
                diseaseId: vr.diseaseId,
                diseaseName: vr.diseaseName,
                insuranceDuration: vr.insuranceDuration
              })) || []
          })) || []
      });
      setShowEditDialog(true);
    }
  };

  const handleEditFormChange = (
    field,
    value,
    detailIndex = null as any,
    vaccineIndex = null as any
  ) => {
    setEditFormData((prev: any) => {
      const newData = { ...prev };

      if (detailIndex !== null && vaccineIndex !== null) {
        // Update vaccination requirement
        newData.details[detailIndex].vaccintionRequirement[vaccineIndex][
          field
        ] = value;
      } else if (detailIndex !== null) {
        // Update detail field
        newData.details[detailIndex][field] = value;
      } else {
        // Update main field
        newData[field] = value;
      }

      return newData;
    });
  };

  const addVaccinationRequirement = (detailIndex) => {
    setEditFormData((prev: any) => {
      const newData = { ...prev };
      newData.details[detailIndex].vaccintionRequirement.push({
        vaccinationRequirementId: '',
        diseaseId: '',
        diseaseName: '',
        insuranceDuration: 21
      });
      return newData;
    });
  };

  const removeVaccinationRequirement = (detailIndex, vaccineIndex) => {
    setEditFormData((prev: any) => {
      const newData = { ...prev };
      newData.details[detailIndex].vaccintionRequirement.splice(
        vaccineIndex,
        1
      );
      return newData;
    });
  };

  const handleDiseaseChange = (diseaseId, detailIndex, vaccineIndex) => {
    const selectedDisease = listLoaiBenh.find(
      (disease) => disease.id === diseaseId
    );
    if (selectedDisease) {
      handleEditFormChange('diseaseId', diseaseId, detailIndex, vaccineIndex);
      handleEditFormChange(
        'diseaseName',
        selectedDisease.name,
        detailIndex,
        vaccineIndex
      );
      handleEditFormChange(
        'insuranceDuration',
        selectedDisease.defaultInsuranceDuration,
        detailIndex,
        vaccineIndex
      );
    }
  };

  const handleSaveEdit = async () => {
    try {
      if (!editFormData) {
        toast({
          title: 'Lỗi',
          description: 'Dữ liệu chỉnh sửa không hợp lệ.',
          variant: 'destructive'
        });
        return;
      }
      // Format payload theo yêu cầu API
      const payload = {
        customerName: editFormData.customerName,
        phone: editFormData.phoneNumber,
        addrress: editFormData.address, // Note: typo in API - "addrress" not "address"
        email: editFormData.email,
        details: editFormData.details.map((detail) => ({
          id: detail.orderRequirementId,
          specieId: detail.specieId,
          weightFrom: detail.weightFrom,
          weightTo: detail.weightTo,
          total: detail.total,
          description: detail.description,
          vaccintionRequirement: detail.vaccintionRequirement.map((vr) => ({
            id: vr.vaccinationRequirementId,
            diseaseId: vr.diseaseId,
            insuranceDuration: vr.insuranceDuration
          }))
        })),
        requestedBy: 'user'
      };

      const [err] = await updateDonMuaLe({
        id: id as string,
        data: payload
      });

      if (err) {
        toast({
          title: 'Cập nhật đơn hàng',
          description: err.data.data || 'Không thể cập nhật đơn hàng.',
          variant: 'destructive'
        });
        return;
      }

      toast({
        title: 'Cập nhật đơn hàng',
        description: 'Đơn hàng đã được cập nhật thành công.',
        variant: 'success'
      });

      refetch();
      setShowEditDialog(false);
    } catch (error) {
      console.error('Error updating order:', error);
    }
  };

  async function handleCancelOrder() {
    try {
      const [err] = await cancleDonMuaLe({
        id: id as string,
        requestBy: 'user'
      });
      if (err) {
        toast({
          title: 'Hủy đơn hàng',
          description: err.data.data || 'Đơn hàng không thể hủy.',
          variant: 'destructive'
        });
        return;
      }
      toast({
        title: 'Hủy đơn hàng',
        description: 'Đơn hàng đã được hủy thành công.',
        variant: 'success'
      });
    } catch (error) {
      console.error('Error cancelling order:', error);
    }
  }

  async function handleCompleteOrder() {
    try {
      const [err] = await completeDonMuaLe({
        id: id as string,
        requestBy: 'user'
      });
      console.log('err', err);
      if (err) {
        toast({
          title: 'Hoàn thành đơn hàng',
          description: err.data.data || 'Đơn hàng không thể hoàn thành.',
          variant: 'destructive'
        });
        return;
      } else {
        toast({
          title: 'Hoàn thành đơn hàng',
          description: 'Đơn hàng đã được hoàn thành thành công.',
          variant: 'success'
        });
      }

      refetch();
      setShowCompleteDialog(false);
    } catch (error) {
      console.error('Error completing order:', error);
    }
  }

  if (isLoading || !chiTietDon) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="h-32 w-32 animate-spin rounded-full border-b-2 border-gray-900"></div>
      </div>
    );
  }

  const getStatusIndex = (status: string) => {
    const statusMap: Record<string, number> = {
      MỚI: 0,
      ĐANG_CHUẨN_BỊ: 1,
      ĐANG: 2,
      CHỜ_BÀN_GIAO: 2,
      ĐANG_BÀN_GIAO: 3,
      HOÀN_THÀNH: 4,
      ĐÃ_HỦY: -1
    };
    return statusMap[status] ?? 0;
  };

  const statusIndex = getStatusIndex(chiTietDon.status);
  const isCancelled = chiTietDon.status === 'ĐÃ_HỦY';
  const isCompleted = chiTietDon.status === 'HOÀN_THÀNH';
  const canModifyStatus = !isCancelled && !isCompleted;

  if (isPending) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="h-32 w-32 animate-spin rounded-full border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <div className="container mx-auto max-w-6xl p-4">
      <div className="mb-6 flex items-center justify-between border-b pb-2">
        <h1 className="text-xl font-semibold">
          Trạng thái đơn {chiTietDon.code}
        </h1>
        <div className="flex items-center gap-3">
          <Button className="bg-blue-500" onClick={handleEditClick}>
            Chỉnh sửa
          </Button>
          <div>
            <Button
              variant="outline"
              className="border-blue-500 text-blue-500 hover:bg-blue-50 hover:text-blue-600"
              onClick={async () => {
                const [err, res] = await taoBaoCao(id as any);
                if (err) {
                  toast({
                    title: 'Lỗi xuất báo cáo',
                    description:
                      err?.data?.message || 'Không thể xuất báo cáo.',
                    variant: 'destructive'
                  });
                } else {
                  const downloadLink = document.createElement('a');
                  console.log('res', res);
                  downloadLink.href = res.data;
                  downloadLink.download = `bao_cao_don_mua_le_${chiTietDon.code}.xlsx`;
                  downloadLink.target = '_blank';
                  document.body.appendChild(downloadLink);
                  downloadLink.click();
                  document.body.removeChild(downloadLink);

                  toast({
                    title: 'Xuất báo cáo thành công',
                    description: 'Báo cáo đã được xuất thành công.',
                    variant: 'success'
                  });
                }
              }}
            >
              Xuất báo cáo
            </Button>
          </div>
          {canModifyStatus && (
            <div className="flex gap-3">
              <Button
                variant="outline"
                className="border-red-500 text-red-500 hover:bg-red-50 hover:text-red-600"
                onClick={() => setShowCancelDialog(true)}
              >
                Hủy đơn
              </Button>
              <Button
                className="bg-green-600 text-white hover:bg-green-700"
                onClick={() => setShowCompleteDialog(true)}
              >
                Hoàn thành
              </Button>
            </div>
          )}
        </div>
      </div>

      {/* Status Flow */}
      <div className="mb-10 flex flex-wrap items-center justify-between rounded-lg bg-gray-50 p-6 shadow-sm">
        {isCancelled ? (
          <div className="flex w-full items-center justify-center gap-2 rounded-md bg-red-100 p-4 text-red-800">
            <AlertCircle size={20} />
            <span className="font-medium">Đơn hàng đã bị hủy</span>
          </div>
        ) : (
          <>
            <StatusStep title="Mới" isActive={statusIndex >= 0} />
            <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
            <StatusStep title="Đang chuẩn bị" isActive={statusIndex >= 1} />
            <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
            <StatusStep title="Chờ bàn giao" isActive={statusIndex >= 2} />
            <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
            <StatusStep title="Đang bàn giao" isActive={statusIndex >= 3} />
            <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
            <StatusStep title="Hoàn Thành" isActive={statusIndex >= 4} />
          </>
        )}
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {/* Customer Info */}
        <div className="overflow-hidden rounded-lg border border-gray-200 shadow-sm">
          <div className="border-b border-gray-200 bg-gray-50 p-3 font-medium text-gray-700">
            Thông tin khách hàng
          </div>
          <div className="grid grid-cols-2 gap-y-5 p-5">
            <div className="text-sm text-gray-600">Tên khách hàng:</div>
            <div className="text-sm font-medium">{chiTietDon.customerName}</div>
            <div className="text-sm text-gray-600">Số điện thoại:</div>
            <div className="text-sm font-medium">{chiTietDon.phoneNumber}</div>
            <div className="text-sm text-gray-600">Địa chỉ:</div>
            <div className="text-sm font-medium">
              {chiTietDon.address || 'N/A'}
            </div>
            <div className="text-sm text-gray-600">Email:</div>
            <div className="text-sm font-medium">
              {chiTietDon.email || 'N/A'}
            </div>
            <div className="text-sm text-gray-600">Tổng số lượng vật nuôi:</div>
            <div className="text-sm font-medium">{chiTietDon.total}</div>
            <div className="text-sm text-gray-600">Đã nhận:</div>
            <div className="text-sm font-medium">{chiTietDon.imported}</div>
          </div>
        </div>

        {/* Requirements */}
        <div className="overflow-hidden rounded-lg border border-gray-200 shadow-sm">
          <div className="border-b border-gray-200 bg-gray-50 p-3 font-medium text-gray-700">
            Yêu cầu về loại vật
          </div>
          <div className="p-5">
            {Array.isArray(chiTietDon.details) &&
            chiTietDon.details.length > 0 ? (
              chiTietDon.details.map((detail, index) => (
                <div
                  key={detail.orderRequirementId}
                  className={`${index > 0 ? 'mt-8 border-t pt-6' : ''} mb-4`}
                >
                  <div className="mb-3 border-b pb-2 font-medium text-gray-800">
                    Yêu cầu {index + 1}
                  </div>
                  <div className="grid grid-cols-2 gap-y-5">
                    <div className="text-sm text-gray-600">Loại vật:</div>
                    <div className="text-sm font-medium">
                      {detail.specieName}
                    </div>
                    <div className="text-sm text-gray-600">Cân nặng:</div>
                    <div className="text-sm font-medium">
                      {detail.weightFrom} - {detail.weightTo} kg
                    </div>
                    <div className="text-sm text-gray-600">Số lượng:</div>
                    <div className="text-sm font-medium">
                      {detail.total} con
                    </div>
                    <div className="text-sm text-gray-600">
                      <p>
                        Bảo hành theo bệnh (
                        {detail.vaccintionRequirement?.length || 0}):
                      </p>
                      <p>*tính từ thời điểm vật nuôi xuất chuồng</p>
                    </div>
                    <div className="text-sm italic text-gray-500">
                      {detail.vaccintionRequirement?.length > 0 && (
                        <div className="">
                          <Button
                            variant="outline"
                            className="rounded border-gray-800 px-6 py-2 text-gray-800 hover:bg-gray-100"
                            onClick={() => handleShowWarrantyDetails(index)}
                          >
                            Chi Tiết
                          </Button>
                        </div>
                      )}
                    </div>
                    <div className="text-sm text-gray-600">Yêu cầu khác:</div>
                    <div className="text-sm font-medium">
                      {detail.description || 'N/A'}
                    </div>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center text-gray-500">
                Không có yêu cầu nào.
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Edit Dialog */}
      <Dialog open={showEditDialog} onOpenChange={setShowEditDialog}>
        <DialogContent className="max-h-[90vh] overflow-y-auto sm:max-w-6xl">
          <DialogHeader>
            <DialogTitle className="text-center text-lg font-bold">
              Chỉnh sửa đơn hàng
            </DialogTitle>
          </DialogHeader>

          {editFormData && (
            <div className="space-y-6">
              {/* Customer Information */}
              <div className="rounded-lg border p-4">
                <h3 className="mb-4 font-semibold text-gray-800">
                  Thông tin khách hàng
                </h3>
                <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                  <div>
                    <Label htmlFor="customerName">Tên khách hàng</Label>
                    <Input
                      id="customerName"
                      value={editFormData.customerName}
                      onChange={(e) =>
                        handleEditFormChange('customerName', e.target.value)
                      }
                    />
                  </div>
                  <div>
                    <Label htmlFor="phoneNumber">Số điện thoại</Label>
                    <Input
                      id="phoneNumber"
                      value={editFormData.phoneNumber}
                      onChange={(e) =>
                        handleEditFormChange('phoneNumber', e.target.value)
                      }
                    />
                  </div>
                  <div>
                    <Label htmlFor="address">Địa chỉ</Label>
                    <Input
                      id="address"
                      value={editFormData.address}
                      onChange={(e) =>
                        handleEditFormChange('address', e.target.value)
                      }
                    />
                  </div>
                  <div>
                    <Label htmlFor="email">Email</Label>
                    <Input
                      id="email"
                      type="email"
                      value={editFormData.email}
                      onChange={(e) =>
                        handleEditFormChange('email', e.target.value)
                      }
                    />
                  </div>
                </div>
              </div>

              {/* Details */}
              <div className="rounded-lg border p-4">
                <h3 className="mb-4 font-semibold text-gray-800">
                  Yêu cầu về loại vật
                </h3>
                {editFormData.details.map((detail, detailIndex) => (
                  <div
                    key={detail.orderRequirementId}
                    className="mb-6 rounded border p-4"
                  >
                    <h4 className="mb-3 font-medium">
                      Yêu cầu {detailIndex + 1}
                    </h4>

                    <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
                      <div>
                        <Label>Loại vật</Label>
                        <Input
                          value={detail.specieName}
                          onChange={(e) =>
                            handleEditFormChange(
                              'specieName',
                              e.target.value,
                              detailIndex
                            )
                          }
                        />
                      </div>
                      <div>
                        <Label>Cân nặng từ (kg)</Label>
                        <Input
                          type="number"
                          value={detail.weightFrom}
                          onChange={(e) =>
                            handleEditFormChange(
                              'weightFrom',
                              parseFloat(e.target.value),
                              detailIndex
                            )
                          }
                        />
                      </div>
                      <div>
                        <Label>Cân nặng đến (kg)</Label>
                        <Input
                          type="number"
                          value={detail.weightTo}
                          onChange={(e) =>
                            handleEditFormChange(
                              'weightTo',
                              parseFloat(e.target.value),
                              detailIndex
                            )
                          }
                        />
                      </div>
                      <div>
                        <Label>Số lượng</Label>
                        <Input
                          type="number"
                          value={detail.total}
                          onChange={(e) =>
                            handleEditFormChange(
                              'total',
                              parseInt(e.target.value),
                              detailIndex
                            )
                          }
                        />
                      </div>
                      <div className="md:col-span-2">
                        <Label>Yêu cầu khác</Label>
                        <Textarea
                          value={detail.description}
                          onChange={(e) =>
                            handleEditFormChange(
                              'description',
                              e.target.value,
                              detailIndex
                            )
                          }
                        />
                      </div>
                    </div>

                    {/* Vaccination Requirements */}
                    <div className="mt-4">
                      <div className="mb-2 flex items-center justify-between">
                        <Label className="text-base font-medium">
                          Bảo hành theo bệnh
                        </Label>
                        <Button
                          type="button"
                          variant="outline"
                          size="sm"
                          onClick={() => addVaccinationRequirement(detailIndex)}
                          className="flex items-center gap-1"
                        >
                          <Plus size={16} />
                          Thêm bệnh
                        </Button>
                      </div>

                      {detail.vaccintionRequirement.map(
                        (vaccine, vaccineIndex) => (
                          <div
                            key={vaccineIndex}
                            className="mb-3 flex items-end gap-2 rounded border p-3"
                          >
                            <div className="flex-1">
                              <Label>Loại bệnh</Label>
                              <Select
                                value={vaccine.diseaseId}
                                onValueChange={(value) =>
                                  handleDiseaseChange(
                                    value,
                                    detailIndex,
                                    vaccineIndex
                                  )
                                }
                              >
                                <SelectTrigger>
                                  <SelectValue placeholder="Chọn loại bệnh" />
                                </SelectTrigger>
                                <SelectContent>
                                  {listLoaiBenh.map((disease) => (
                                    <SelectItem
                                      key={disease.id}
                                      value={disease.id}
                                    >
                                      {disease.name}
                                    </SelectItem>
                                  ))}
                                </SelectContent>
                              </Select>
                            </div>
                            <div className="w-32">
                              <Label>Thời gian bảo hành (ngày)</Label>
                              <Input
                                type="number"
                                value={vaccine.insuranceDuration}
                                onChange={(e) =>
                                  handleEditFormChange(
                                    'insuranceDuration',
                                    parseInt(e.target.value),
                                    detailIndex,
                                    vaccineIndex
                                  )
                                }
                              />
                            </div>
                            <Button
                              type="button"
                              variant="outline"
                              size="sm"
                              onClick={() =>
                                removeVaccinationRequirement(
                                  detailIndex,
                                  vaccineIndex
                                )
                              }
                              className="text-red-500 hover:bg-red-50"
                            >
                              <X size={16} />
                            </Button>
                          </div>
                        )
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setShowEditDialog(false)}>
              Hủy
            </Button>
            <Button
              onClick={handleSaveEdit}
              className="bg-blue-500 hover:bg-blue-600"
            >
              Lưu thay đổi
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Warranty Details Modal */}
      {chiTietDon &&
        Array.isArray(chiTietDon.details) &&
        chiTietDon.details[selectedRequirementIndex]?.vaccintionRequirement && (
          <WarrantyDetailsModal
            open={showWarrantyDetails}
            onClose={() => setShowWarrantyDetails(false)}
            warranties={
              chiTietDon.details[selectedRequirementIndex].vaccintionRequirement
            }
          />
        )}

      {/* Cancel Confirmation */}
      <AlertDialog open={showCancelDialog} onOpenChange={setShowCancelDialog}>
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Xác nhận hủy đơn</AlertDialogTitle>
            <AlertDialogDescription>
              Bạn có chắc chắn muốn hủy đơn hàng này? Hành động này không thể
              hoàn tác.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Không</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleCancelOrder}
              className="bg-red-500 text-white hover:bg-red-600"
            >
              Xác nhận hủy
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>

      {/* Complete Confirmation */}
      <AlertDialog
        open={showCompleteDialog}
        onOpenChange={setShowCompleteDialog}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>Xác nhận hoàn thành</AlertDialogTitle>
            <AlertDialogDescription>
              Bạn có chắc chắn muốn đánh dấu đơn hàng này là hoàn thành?
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel>Không</AlertDialogCancel>
            <AlertDialogAction
              onClick={handleCompleteOrder}
              className="bg-green-500 text-white hover:bg-green-600"
            >
              Xác nhận hoàn thành
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
    </div>
  );
}

function StatusStep({
  title,
  isActive,
  date
}: {
  title: string;
  isActive: boolean;
  date?: string;
}) {
  return (
    <div className="mb-4 flex flex-col items-center sm:mb-0">
      <div
        className={`flex h-20 w-20 items-center justify-center rounded-full text-center text-sm font-medium transition-all
        ${isActive ? 'bg-green-400 text-white shadow-md' : 'border-2 border-gray-300 text-gray-500'}`}
      >
        {title}
      </div>
      {date ? (
        <div className="mt-2 text-xs text-gray-600">{date}</div>
      ) : (
        <div className="invisible mt-2 text-xs">Date</div>
      )}
    </div>
  );
}

interface Warranty {
  vaccinationRequirementId: string;
  diseaseId: string;
  diseaseName: string;
  insuranceDuration: number;
}

function WarrantyDetailsModal({
  open,
  onClose,
  warranties
}: {
  open: boolean;
  onClose: () => void;
  warranties: Warranty[];
}) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-4xl">
        <DialogHeader>
          <DialogTitle className="text-center text-lg font-bold">
            Bảo Hành Theo Bệnh
          </DialogTitle>
        </DialogHeader>
        <div className="rounded border-2 border-gray-800 p-4">
          <table className="w-full border-collapse">
            <thead>
              <tr>
                <th className="border border-gray-800 bg-gray-100 p-2 text-center">
                  Thời gian bảo hành (ngày)
                </th>
              </tr>
            </thead>
            <tbody>
              {warranties.map((warranty) => (
                <tr
                  key={warranty.vaccinationRequirementId}
                  className="border border-gray-800"
                >
                  <td className="border-r border-gray-800 p-3">
                    {warranty.diseaseName}
                  </td>
                  <td className="p-2 text-center">
                    {warranty.insuranceDuration}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
          <DialogFooter className="mt-8">
            <Button
              className="bg-gray-800 px-6 text-white hover:bg-gray-700"
              onClick={onClose}
            >
              Xác nhận
            </Button>
          </DialogFooter>
        </div>
      </DialogContent>
    </Dialog>
  );
}
