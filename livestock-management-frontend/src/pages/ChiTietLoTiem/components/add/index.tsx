'use client';

import { useState } from 'react';
import { Calendar, Plus, Save } from 'lucide-react';
import { Button } from '@/components/ui/button';
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
import {
  Popover,
  PopoverContent,
  PopoverTrigger
} from '@/components/ui/popover';
import { Calendar as CalendarComponent } from '@/components/ui/calendar';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import {
  Card,
  CardContent,
  CardDescription,
  CardFooter,
  CardHeader,
  CardTitle
} from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import {
  useGetListLoaiTiem,
  useGetLoaiDichBenh,
  useGetThuoc,
  useTaoChiTietLoTiemVacxin
} from '@/queries/vacxin.query';

export default function AddForm() {
  const [date, setDate] = useState<Date | undefined>(new Date('2024-03-18'));
  const { data: loaiTiem } = useGetListLoaiTiem();
  const { data: loaiDichBenh } = useGetLoaiDichBenh();
  const { data: loaiThuoc } = useGetThuoc();
  const { mutateAsync: taoLoTiem } = useTaoChiTietLoTiemVacxin();
  const [medicines, setMedicines] = useState([{ id: 1, vaccineId: '' }]);

  const [formData, setFormData] = useState({
    name: 'Tiêm phòng Lở mồm long móng',
    type: 0, // TIÊM_VACCINE
    diseaseId: '',
    dateSchedule: new Date(),
    conductedBy: 'QUAN_TRAI',
    description: '',
    status: 0,
    vaccineId: ''
  });

  const handleInputChange = (field, value) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleMedicineChange = (index, vaccineId) => {
    const updatedMedicines = [...medicines];
    updatedMedicines[index] = { ...updatedMedicines[index], vaccineId };
    setMedicines(updatedMedicines);
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    try {
      const payload = {
        dateSchedule: formData.dateSchedule.toISOString(),
        name: formData.name,
        conductedBy: formData.conductedBy,
        vaccineId: medicines[0].vaccineId, // Using the first medicine for now
        description: formData.description,
        type: formData.type,
        status: formData.status,
        createdBy: 'USER' // You might want to get this from user context
      };

      await taoLoTiem(payload);
      alert('Lô tiêm đã được tạo thành công!');
      // Reset form or redirect as needed
    } catch (error) {
      console.error('Error creating vaccination batch:', error);
      alert('Có lỗi xảy ra khi tạo lô tiêm!');
    }
  };

  const addMedicine = () => {
    setMedicines([
      ...medicines,
      { id: medicines.length + 1, vaccineId: loaiThuoc?.[0]?.id || '' }
    ]);
  };

  return (
    <div className="container mx-auto max-w-5xl py-8">
      <Card className="shadow-md">
        <CardHeader className="border-b bg-muted/30">
          <CardTitle className="text-2xl font-semibold text-primary">
            Thông tin lô tiêm
          </CardTitle>
          <CardDescription>
            Nhập thông tin chi tiết về lô tiêm mới
          </CardDescription>
        </CardHeader>

        <CardContent className="pt-6">
          <form onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 gap-x-8 gap-y-6 md:grid-cols-2">
              <div className="space-y-4">
                <div>
                  <Label htmlFor="lotName" className="text-sm font-medium">
                    Tên lô tiêm
                  </Label>
                  <Textarea
                    id="lotName"
                    placeholder="Tiêm phòng Lở mồm long móng"
                    className="mt-1.5 min-h-[80px] resize-none"
                    value={formData.name}
                    onChange={(e) => handleInputChange('name', e.target.value)}
                  />
                </div>

                <div>
                  <Label
                    htmlFor="vaccinationType"
                    className="text-sm font-medium"
                  >
                    Loại tiêm
                  </Label>
                  <Select
                    value={loaiTiem?.[formData.type] || ''}
                    onValueChange={(value) => {
                      const typeIndex =
                        loaiTiem?.findIndex((type) => type === value) || 0;
                      handleInputChange('type', typeIndex);
                    }}
                  >
                    <SelectTrigger className="mt-1.5">
                      <SelectValue placeholder="Chọn loại tiêm" />
                    </SelectTrigger>
                    <SelectContent>
                      {loaiTiem?.map((type) => (
                        <SelectItem key={type} value={type}>
                          {type}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label htmlFor="diseaseType" className="text-sm font-medium">
                    Loại dịch bệnh
                  </Label>
                  <Select
                    value={formData.diseaseId}
                    onValueChange={(value) =>
                      handleInputChange('diseaseId', value)
                    }
                  >
                    <SelectTrigger className="mt-1.5">
                      <SelectValue placeholder="Chọn loại dịch bệnh" />
                    </SelectTrigger>
                    <SelectContent>
                      {loaiDichBenh?.items?.map((disease) => (
                        <SelectItem key={disease.id} value={disease.id}>
                          {disease.name}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label
                    htmlFor="implementationDate"
                    className="text-sm font-medium"
                  >
                    Ngày dự kiến thực hiện
                  </Label>
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        className="mt-1.5 w-full justify-start text-left font-normal"
                      >
                        {date ? format(date, 'dd-MM-yyyy') : 'Chọn ngày'}
                        <Calendar className="ml-auto h-4 w-4 opacity-50" />
                      </Button>
                    </PopoverTrigger>
                    <PopoverContent className="w-auto p-0" align="start">
                      <CalendarComponent
                        mode="single"
                        selected={formData.dateSchedule}
                        onSelect={(date) => {
                          if (date) {
                            handleInputChange('dateSchedule', date);
                            setDate(date);
                          }
                        }}
                        locale={vi}
                      />
                    </PopoverContent>
                  </Popover>
                </div>
              </div>

              {/* Right Column */}
              <div className="space-y-4">
                <div>
                  <Label htmlFor="disease" className="text-sm font-medium">
                    Phòng bệnh
                  </Label>
                  <Input
                    id="disease"
                    value="Lở mồm long móng"
                    readOnly
                    className="mt-1.5 bg-muted/30"
                  />
                </div>

                <div>
                  <Label htmlFor="implementer" className="text-sm font-medium">
                    Người thực hiện
                  </Label>
                  <Select
                    value={formData.conductedBy}
                    onValueChange={(value) =>
                      handleInputChange('conductedBy', value)
                    }
                  >
                    <SelectTrigger className="mt-1.5">
                      <SelectValue placeholder="Chọn người thực hiện" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="QUAN_TRAI">Quản trại (Đội)</SelectItem>
                      <SelectItem value="BAC_SI">Bác sĩ</SelectItem>
                      <SelectItem value="KY_THUAT_VIEN">
                        Kỹ thuật viên
                      </SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <Label htmlFor="notes" className="text-sm font-medium">
                    Ghi chú
                  </Label>
                  <Textarea
                    id="notes"
                    placeholder="Nhập ghi chú"
                    className="mt-1.5 min-h-[80px] resize-none"
                    value={formData.description}
                    onChange={(e) =>
                      handleInputChange('description', e.target.value)
                    }
                  />
                </div>
              </div>
            </div>

            <Separator className="my-6" />

            {/* Medicine Section - Full Width */}
            <div className="space-y-4">
              <div className="flex items-center justify-between">
                <h3 className="text-base font-medium">Thông tin thuốc</h3>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={addMedicine}
                  type="button"
                  className="h-8"
                >
                  <Plus className="mr-1 h-3.5 w-3.5" />
                  Thêm một thuốc
                </Button>
              </div>

              <div className="grid gap-4 md:grid-cols-2">
                {medicines.map((medicine, index) => (
                  <div key={medicine.id} className="rounded-md border p-4">
                    <div className="space-y-4">
                      <div>
                        <Label
                          htmlFor={`medicineType-${index}`}
                          className="text-sm font-medium"
                        >
                          Loại thuốc{' '}
                          {medicines.length > 1 ? `#${index + 1}` : ''}
                        </Label>
                        <Select
                          value={medicine.vaccineId || loaiThuoc?.[0]?.id || ''}
                          onValueChange={(value) =>
                            handleMedicineChange(index, value)
                          }
                        >
                          <SelectTrigger className="mt-1.5">
                            <SelectValue placeholder="Chọn loại thuốc" />
                          </SelectTrigger>
                          <SelectContent>
                            {loaiThuoc?.map((med) => (
                              <SelectItem key={med.id} value={med.id}>
                                {med.name}
                              </SelectItem>
                            ))}
                          </SelectContent>
                        </Select>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </form>
        </CardContent>

        <CardFooter className="flex justify-end gap-3 border-t bg-muted/30 px-6 py-4">
          <Button variant="outline">Hủy</Button>
          <Button
            onClick={handleSubmit}
            className="hover:bg-primary-dark flex items-center justify-center bg-primary text-white"
          >
            <Save className="mr-2 h-4 w-4" />
            Lưu thông tin
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
