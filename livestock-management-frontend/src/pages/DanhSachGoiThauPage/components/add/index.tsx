'use client';

import { useState } from 'react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useToast } from '@/components/ui/use-toast';
import { useCreateGoiThau, useGetSpecie } from '@/queries/admin.query';
import {
  Plus,
  Trash2,
  Save,
  FileText,
  Settings,
  Package,
  Shield
} from 'lucide-react';
import __helpers from '@/helpers';
import { Card } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { useGetLoaiBenh } from '@/queries/benh.query';

type VaccinationRequirement = {
  diseaseId: string;
  insuranceDuration: number;
};

type TechnicalRequirement = {
  speciesId: string;
  requiredWeightMin: number;
  requiredWeightMax: number;
  requiredAgeMin: number;
  requiredAgeMax: number;
  requiredInsuranceDuration: number;
  requiredQuantity: number;
  description: string;
  vaccinationRequireProcurementDetailCreates: VaccinationRequirement[];
};

type FormData = {
  code: string;
  name: string;
  owner: string;
  expiredDuration: number;
  description: string;
  details: TechnicalRequirement[];
  requestedBy: string;
};

export default function AddForm() {
  const [formData, setFormData] = useState<FormData>({
    code: '',
    name: '',
    owner: '',
    expiredDuration: 5,
    description: '',
    details: [
      {
        speciesId: '',
        requiredWeightMin: 190,
        requiredWeightMax: 210,
        requiredAgeMin: 12,
        requiredAgeMax: 36,
        requiredInsuranceDuration: 21,
        requiredQuantity: 100,
        description: '',
        vaccinationRequireProcurementDetailCreates: []
      }
    ],
    requestedBy: __helpers.getUserEmail()
  });

  const { toast } = useToast();
  const { data } = useGetSpecie();
  const { mutateAsync: createGoiThau } = useCreateGoiThau();
  const { data: resListLoaiBenh } = useGetLoaiBenh();
  const listLoaiBenh = resListLoaiBenh?.items || [];
  console.log('listLoaiBenh', listLoaiBenh);
  const listDanhMuc = data?.data || [];

  const handleInputChange = (field, value) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleDetailChange = (index, field, value) => {
    setFormData((prev) => ({
      ...prev,
      details: prev.details.map((detail, i) =>
        i === index ? { ...detail, [field]: value } : detail
      )
    }));
  };

  const addTechnicalRequirement = () => {
    setFormData((prev) => ({
      ...prev,
      details: [
        ...prev.details,
        {
          speciesId: '',
          requiredWeightMin: 190,
          requiredWeightMax: 210,
          requiredAgeMin: 12,
          requiredAgeMax: 36,
          requiredInsuranceDuration: 21,
          requiredQuantity: 100,
          description: '',
          vaccinationRequireProcurementDetailCreates: []
        }
      ]
    }));
  };

  const removeTechnicalRequirement = (index) => {
    if (formData.details.length <= 1) return; // Prevent removing the last item

    setFormData((prev) => ({
      ...prev,
      details: prev.details.filter((_, i) => i !== index)
    }));
  };

  const addVaccinationRequirement = (detailIndex) => {
    setFormData((prev: any) => ({
      ...prev,
      details: prev.details.map((detail, i) =>
        i === detailIndex
          ? {
              ...detail,
              vaccinationRequireProcurementDetailCreates: [
                ...detail.vaccinationRequireProcurementDetailCreates,
                {
                  diseaseId: '',
                  insuranceDuration: 0
                }
              ]
            }
          : detail
      )
    }));
  };

  const removeVaccinationRequirement = (detailIndex, vaccinationIndex) => {
    setFormData((prev) => ({
      ...prev,
      details: prev.details.map((detail, i) =>
        i === detailIndex
          ? {
              ...detail,
              vaccinationRequireProcurementDetailCreates:
                detail.vaccinationRequireProcurementDetailCreates.filter(
                  (_, j) => j !== vaccinationIndex
                )
            }
          : detail
      )
    }));
  };

  const handleVaccinationChange = (
    detailIndex,
    vaccinationIndex,
    field,
    value
  ) => {
    setFormData((prev: any) => ({
      ...prev,
      details: prev.details.map((detail, i) =>
        i === detailIndex
          ? {
              ...detail,
              vaccinationRequireProcurementDetailCreates:
                detail.vaccinationRequireProcurementDetailCreates.map(
                  (vaccination, j) =>
                    j === vaccinationIndex
                      ? { ...vaccination, [field]: value }
                      : vaccination
                )
            }
          : detail
      )
    }));
  };

  const handleDiseaseChange = (detailIndex, vaccinationIndex, diseaseId) => {
    const selectedDisease = listLoaiBenh.find(
      (disease) => disease.id === diseaseId
    );
    const defaultInsuranceDuration =
      selectedDisease?.defaultInsuranceDuration || 0;

    setFormData((prev: any) => ({
      ...prev,
      details: prev.details.map((detail, i) =>
        i === detailIndex
          ? {
              ...detail,
              vaccinationRequireProcurementDetailCreates:
                detail.vaccinationRequireProcurementDetailCreates.map(
                  (vaccination, j) =>
                    j === vaccinationIndex
                      ? {
                          ...vaccination,
                          diseaseId: diseaseId,
                          insuranceDuration: defaultInsuranceDuration
                        }
                      : vaccination
                )
            }
          : detail
      )
    }));
  };

  const handleSave = async () => {
    try {
      // Prepare the payload according to the required structure
      const payload = {
        ...formData,
        // Ensure numeric values are actually numbers
        expiredDuration: Number(formData.expiredDuration),
        details: formData.details.map((detail) => ({
          ...detail,
          requiredWeightMin: Number(detail.requiredWeightMin),
          requiredWeightMax: Number(detail.requiredWeightMax),
          requiredAgeMin: Number(detail.requiredAgeMin),
          requiredAgeMax: Number(detail.requiredAgeMax),
          requiredInsuranceDuration: Number(detail.requiredInsuranceDuration),
          requiredQuantity: Number(detail.requiredQuantity),
          vaccinationRequireProcurementDetailCreates:
            detail.vaccinationRequireProcurementDetailCreates.map(
              (vaccination: any) => ({
                ...vaccination,
                insuranceDuration: Number(vaccination.insuranceDuration)
              })
            )
        }))
      };

      console.log('Payload:', JSON.stringify(payload, null, 2));

      // Submit the data
      const [err] = await createGoiThau(payload);

      if (err) {
        console.error('Error creating package:', err);
        toast({
          title: 'Lỗi',
          description: err.data?.data || 'Không thể tạo gói thầu',
          variant: 'destructive',
          duration: 3000
        });
        return;
      }

      toast({
        title: 'Đã lưu thành công',
        description: 'Thông tin gói thầu đã được lưu lại',
        variant: 'success',
        duration: 3000
      });
    } catch (error: any) {
      console.error('Error submitting form:', error);
      toast({
        title: 'Lỗi',
        description: error?.data?.data || 'Không thể lưu gói thầu',
        variant: 'destructive',
        duration: 3000
      });
    }
  };

  return (
    <div className="mx-auto w-full max-w-7xl">
      <Card className="overflow-hidden border-0 bg-white shadow-md">
        <div className="bg-gradient-to-r from-teal-500 to-emerald-600 p-6 text-white">
          <div className="flex items-center gap-3">
            <FileText className="h-6 w-6" />
            <h1 className="text-xl font-semibold">Tạo gói thầu mới</h1>
          </div>
          <p className="mt-2 text-sm text-teal-50">
            Điền đầy đủ thông tin để tạo gói thầu mới
          </p>
        </div>

        <div className="grid gap-8 p-6 md:grid-cols-2">
          {/* Left Column - Package Information */}
          <div className="space-y-6">
            <div className="flex items-center gap-2 border-b border-teal-100 pb-2">
              <Package className="h-5 w-5 text-teal-600" />
              <h2 className="text-lg font-medium text-teal-700">
                Thông tin gói thầu
              </h2>
            </div>

            <div className="rounded-lg border border-gray-100 bg-white p-5 shadow-sm">
              <div className="space-y-5">
                <div className="grid gap-2">
                  <label className="text-sm font-medium text-gray-700">
                    Tên gói thầu
                  </label>
                  <Input
                    className="rounded-md border-gray-200 bg-white shadow-sm focus-visible:ring-teal-500"
                    value={formData.name}
                    onChange={(e) => handleInputChange('name', e.target.value)}
                    placeholder="Nhập tên gói thầu"
                  />
                </div>

                <div className="grid gap-2">
                  <label className="text-sm font-medium text-gray-700">
                    Bên mời thầu
                  </label>
                  <Textarea
                    className="min-h-[60px] rounded-md border-gray-200 bg-white shadow-sm focus-visible:ring-teal-500"
                    value={formData.owner}
                    onChange={(e) => handleInputChange('owner', e.target.value)}
                    placeholder="Nhập tên bên mời thầu"
                  />
                </div>

                <div className="grid gap-2">
                  <label className="text-sm font-medium text-gray-700">
                    Mã gói thầu
                  </label>
                  <Input
                    className="rounded-md border-gray-200 bg-white shadow-sm focus-visible:ring-teal-500"
                    value={formData.code}
                    onChange={(e) => handleInputChange('code', e.target.value)}
                    placeholder="Nhập mã gói thầu"
                  />
                </div>

                <div className="grid gap-2">
                  <label className="text-sm font-medium text-gray-700">
                    Thời gian thực hiện
                  </label>
                  <div className="flex items-center gap-2">
                    <Input
                      type="number"
                      className="w-24 rounded-md border-gray-200 bg-white text-center shadow-sm focus-visible:ring-teal-500"
                      value={formData.expiredDuration}
                      onChange={(e) =>
                        handleInputChange('expiredDuration', e.target.value)
                      }
                    />
                    <div className="rounded-md bg-gray-50 px-3 py-2 text-sm text-gray-500">
                      ngày
                    </div>
                  </div>
                </div>

                <div className="grid gap-2">
                  <label className="text-sm font-medium text-gray-700">
                    Mô tả
                  </label>
                  <Textarea
                    className="min-h-[120px] rounded-md border-gray-200 bg-white shadow-sm focus-visible:ring-teal-500"
                    value={formData.description}
                    onChange={(e) =>
                      handleInputChange('description', e.target.value)
                    }
                    placeholder="Nhập mô tả gói thầu"
                  />
                </div>
              </div>
            </div>
          </div>

          {/* Right Column - Technical Requirements */}
          <div className="space-y-6">
            <div className="flex items-center justify-between border-b border-teal-100 pb-2">
              <div className="flex items-center gap-2">
                <Settings className="h-5 w-5 text-teal-600" />
                <h2 className="text-lg font-medium text-teal-700">
                  Yêu cầu kỹ thuật
                </h2>
              </div>
              <Button
                onClick={addTechnicalRequirement}
                variant="outline"
                size="sm"
                className="border-teal-200 text-teal-700 hover:bg-teal-50 hover:text-teal-800"
              >
                <Plus className="mr-1 h-4 w-4" />
                Thêm yêu cầu
              </Button>
            </div>

            <div className="space-y-6">
              {formData.details.map((detail, index) => (
                <div
                  key={index}
                  className="relative rounded-lg border border-gray-100 bg-white p-5 shadow-sm"
                >
                  <div className="mb-4 flex items-center justify-between">
                    <Badge
                      variant="outline"
                      className="border-teal-200 bg-teal-50 text-teal-700"
                    >
                      Yêu cầu #{index + 1}
                    </Badge>

                    {index > 0 && (
                      <Button
                        onClick={() => removeTechnicalRequirement(index)}
                        variant="ghost"
                        size="sm"
                        className="h-8 w-8 rounded-full p-0 text-red-500 hover:bg-red-50 hover:text-red-700"
                      >
                        <Trash2 className="h-4 w-4" />
                        <span className="sr-only">Xóa yêu cầu</span>
                      </Button>
                    )}
                  </div>

                  <div className="space-y-4">
                    <div className="grid gap-2">
                      <label className="text-sm font-medium text-gray-700">
                        Danh mục loài vật
                      </label>
                      <Select
                        value={detail.speciesId}
                        onValueChange={(value) =>
                          handleDetailChange(index, 'speciesId', value)
                        }
                      >
                        <SelectTrigger className="rounded-md border-gray-200 bg-white shadow-sm focus:ring-teal-500">
                          <SelectValue placeholder="Chọn loài vật" />
                        </SelectTrigger>
                        <SelectContent>
                          {listDanhMuc?.map((item) => (
                            <SelectItem key={item.id} value={item.id}>
                              {item.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div className="grid gap-2">
                        <label className="text-sm font-medium text-gray-700">
                          Biểu cân (kg)
                        </label>
                        <div className="flex items-center gap-2">
                          <Input
                            type="number"
                            className="w-full rounded-md border-gray-200 bg-white text-center shadow-sm focus-visible:ring-teal-500"
                            value={detail.requiredWeightMin}
                            onChange={(e) =>
                              handleDetailChange(
                                index,
                                'requiredWeightMin',
                                e.target.value
                              )
                            }
                          />
                          <span className="text-gray-500">-</span>
                          <Input
                            type="number"
                            className="w-full rounded-md border-gray-200 bg-white text-center shadow-sm focus-visible:ring-teal-500"
                            value={detail.requiredWeightMax}
                            onChange={(e) =>
                              handleDetailChange(
                                index,
                                'requiredWeightMax',
                                e.target.value
                              )
                            }
                          />
                        </div>
                      </div>

                      <div className="grid gap-2">
                        <label className="text-sm font-medium text-gray-700">
                          Số lượng (con)
                        </label>
                        <Input
                          type="number"
                          className="rounded-md border-gray-200 bg-white text-center shadow-sm focus-visible:ring-teal-500"
                          value={detail.requiredQuantity}
                          onChange={(e) =>
                            handleDetailChange(
                              index,
                              'requiredQuantity',
                              e.target.value
                            )
                          }
                        />
                      </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4">
                      <div className="grid gap-2">
                        <label className="text-sm font-medium text-gray-700">
                          Tuổi (tháng)
                        </label>
                        <div className="flex items-center gap-2">
                          <Input
                            type="number"
                            className="w-full rounded-md border-gray-200 bg-white text-center shadow-sm focus-visible:ring-teal-500"
                            value={detail.requiredAgeMin}
                            onChange={(e) =>
                              handleDetailChange(
                                index,
                                'requiredAgeMin',
                                e.target.value
                              )
                            }
                          />
                          <span className="text-gray-500">-</span>
                          <Input
                            type="number"
                            className="w-full rounded-md border-gray-200 bg-white text-center shadow-sm focus-visible:ring-teal-500"
                            value={detail.requiredAgeMax}
                            onChange={(e) =>
                              handleDetailChange(
                                index,
                                'requiredAgeMax',
                                e.target.value
                              )
                            }
                          />
                        </div>
                      </div>

                      <div className="grid gap-2">
                        <label className="text-sm font-medium text-gray-700">
                          Thời gian bảo hành (ngày)
                        </label>
                        <Input
                          type="number"
                          className="rounded-md border-gray-200 bg-white text-center shadow-sm focus-visible:ring-teal-500"
                          value={detail.requiredInsuranceDuration}
                          onChange={(e) =>
                            handleDetailChange(
                              index,
                              'requiredInsuranceDuration',
                              e.target.value
                            )
                          }
                        />
                      </div>
                    </div>

                    <div className="grid gap-2">
                      <label className="text-sm font-medium text-gray-700">
                        Yêu cầu khác
                      </label>
                      <Textarea
                        className="min-h-[80px] rounded-md border-gray-200 bg-white shadow-sm focus-visible:ring-teal-500"
                        value={detail.description}
                        onChange={(e) =>
                          handleDetailChange(
                            index,
                            'description',
                            e.target.value
                          )
                        }
                        placeholder="Nhập các điều kiện và yêu cầu khác"
                      />
                    </div>

                    {/* Vaccination Requirements Section */}
                    <div className="border-t border-gray-100 pt-4">
                      <div className="mb-3 flex items-center justify-between">
                        <div className="flex items-center gap-2">
                          <Shield className="h-4 w-4 text-blue-600" />
                          <label className="text-sm font-medium text-gray-700">
                            Yêu cầu tiêm phòng bệnh
                          </label>
                        </div>
                        <Button
                          onClick={() => addVaccinationRequirement(index)}
                          variant="outline"
                          size="sm"
                          className="border-blue-200 text-blue-700 hover:bg-blue-50 hover:text-blue-800"
                        >
                          <Plus className="mr-1 h-4 w-4" />
                          Thêm bệnh
                        </Button>
                      </div>

                      <div className="space-y-3">
                        {detail.vaccinationRequireProcurementDetailCreates.map(
                          (vaccination, vaccinationIndex) => (
                            <div
                              key={vaccinationIndex}
                              className="rounded-lg border border-blue-100 bg-blue-50/30 p-3"
                            >
                              <div className="mb-2 flex items-center justify-between">
                                <Badge
                                  variant="outline"
                                  className="border-blue-200 bg-blue-50 text-blue-700"
                                >
                                  Bệnh #{vaccinationIndex + 1}
                                </Badge>
                                <Button
                                  onClick={() =>
                                    removeVaccinationRequirement(
                                      index,
                                      vaccinationIndex
                                    )
                                  }
                                  variant="ghost"
                                  size="sm"
                                  className="h-6 w-6 rounded-full p-0 text-red-500 hover:bg-red-50 hover:text-red-700"
                                >
                                  <Trash2 className="h-3 w-3" />
                                </Button>
                              </div>

                              <div className="grid grid-cols-2 gap-3">
                                <div className="grid gap-1">
                                  <label className="text-xs font-medium text-gray-600">
                                    Loại bệnh
                                  </label>
                                  <Select
                                    value={vaccination.diseaseId}
                                    onValueChange={(value) =>
                                      handleDiseaseChange(
                                        index,
                                        vaccinationIndex,
                                        value
                                      )
                                    }
                                  >
                                    <SelectTrigger className="h-8 rounded-md border-gray-200 bg-white text-xs shadow-sm focus:ring-blue-500">
                                      <SelectValue placeholder="Chọn bệnh" />
                                    </SelectTrigger>
                                    <SelectContent>
                                      {listLoaiBenh?.map((disease) => (
                                        <SelectItem
                                          key={disease.id}
                                          value={disease.id}
                                        >
                                          <div className="flex flex-col">
                                            <span className="font-medium">
                                              {disease.name}
                                            </span>
                                            <span className="text-xs text-gray-500">
                                              Mặc định:{' '}
                                              {disease.defaultInsuranceDuration}{' '}
                                              ngày
                                            </span>
                                          </div>
                                        </SelectItem>
                                      ))}
                                    </SelectContent>
                                  </Select>
                                </div>

                                <div className="grid gap-1">
                                  <label className="text-xs font-medium text-gray-600">
                                    Thời gian bảo hành (ngày)
                                  </label>
                                  <Input
                                    type="number"
                                    className="h-8 rounded-md border-gray-200 bg-white text-center text-xs shadow-sm focus-visible:ring-blue-500"
                                    value={vaccination.insuranceDuration}
                                    onChange={(e) =>
                                      handleVaccinationChange(
                                        index,
                                        vaccinationIndex,
                                        'insuranceDuration',
                                        e.target.value
                                      )
                                    }
                                  />
                                </div>
                              </div>
                            </div>
                          )
                        )}

                        {detail.vaccinationRequireProcurementDetailCreates
                          .length === 0 && (
                          <div className="rounded-lg border-2 border-dashed border-gray-200 p-4 text-center text-sm text-gray-500">
                            Chưa có yêu cầu tiêm phòng bệnh nào
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Footer with save button */}
        <div className="flex items-center justify-end gap-4 border-t border-gray-100 bg-gray-50 p-6">
          <Button
            variant="outline"
            className="border-gray-200 text-gray-600 hover:bg-gray-100 hover:text-gray-800"
          >
            Hủy bỏ
          </Button>
          <Button
            onClick={handleSave}
            className="bg-gradient-to-r from-teal-500 to-emerald-600 text-white hover:from-teal-600 hover:to-emerald-700"
          >
            <Save className="mr-2 h-4 w-4" />
            Lưu lại
          </Button>
        </div>
      </Card>
    </div>
  );
}
