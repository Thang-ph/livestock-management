'use client';

import { useState } from 'react';
import { Calendar, Plus, Save, CheckCircle2 } from 'lucide-react';
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
import { toast } from '@/components/ui/use-toast';

export default function AddForm() {
  const [date, setDate] = useState<Date | undefined>(new Date('2024-03-18'));
  const { data: loaiTiem } = useGetListLoaiTiem();
  const { data: loaiDichBenh } = useGetLoaiDichBenh();
  const { data: loaiThuoc } = useGetThuoc();
  const { mutateAsync: taoLoTiem } = useTaoChiTietLoTiemVacxin();
  const [medicines, setMedicines] = useState([{ id: 1, vaccineId: '' }]);
  console.log('loaiDichBenh', loaiDichBenh);

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
        vaccineId: medicines[0].vaccineId,
        description: formData.description,
        type: formData.type,
        status: formData.status,
        createdBy: 'USER'
      };

      await taoLoTiem(payload);
      const [err, data] = await taoLoTiem(payload);
      if (err) {
        toast({
          title: 'Tạo lô tiêm thất bại',
          description: 'Đã xảy ra lỗi khi tạo lô tiêm. Vui lòng thử lại.',
          variant: 'destructive'
        });
      } else if (!err) {
        toast({
          title: 'Tạo lô tiêm thành công',
          description: `Đã tạo lô tiêm ${data.name} thành công.`,
          variant: 'success'
        });
      }
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
      <Card className="overflow-hidden border-t-4 border-t-primary shadow-lg">
        <CardHeader className="border-b bg-gradient-to-r from-primary/10 to-transparent">
          <div className="flex items-center gap-2">
            <CheckCircle2 className="h-6 w-6 text-primary" />
            <CardTitle className="text-2xl font-semibold text-primary">
              Thông tin vật nuôi
            </CardTitle>
          </div>
          <CardDescription className="mt-1 text-muted-foreground">
            Nhập thông tin chi tiết về lô tiêm mới
          </CardDescription>
        </CardHeader>

        <CardContent className="px-8 pt-8">
          <form onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 gap-x-10 gap-y-6 md:grid-cols-2">
              <div className="space-y-5">
                <div className="group">
                  <Label
                    htmlFor="lotName"
                    className="inline-flex items-center gap-1.5 text-sm font-medium"
                  >
                    Tên lô tiêm <span className="text-red-500">*</span>
                  </Label>
                  <Textarea
                    id="lotName"
                    placeholder="Tiêm phòng Lở mồm long móng"
                    className="mt-1.5 min-h-[80px] resize-none border-muted-foreground/30 transition-all focus:border-primary"
                    value={formData.name}
                    onChange={(e) => handleInputChange('name', e.target.value)}
                  />
                </div>

                <div className="group">
                  <Label
                    htmlFor="vaccinationType"
                    className="inline-flex items-center gap-1.5 text-sm font-medium"
                  >
                    Loại tiêm <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={loaiTiem?.[formData.type] || ''}
                    onValueChange={(value) => {
                      const typeIndex =
                        loaiTiem?.findIndex((type) => type === value) || 0;
                      handleInputChange('type', typeIndex);
                    }}
                  >
                    <SelectTrigger className="mt-1.5 border-muted-foreground/30 focus:border-primary">
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

                <div className="group">
                  <Label
                    htmlFor="diseaseType"
                    className="inline-flex items-center gap-1.5 text-sm font-medium"
                  >
                    Loại dịch bệnh <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={formData.diseaseId}
                    onValueChange={(value) =>
                      handleInputChange('diseaseId', value)
                    }
                  >
                    <SelectTrigger className="mt-1.5 border-muted-foreground/30 focus:border-primary">
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

                <div className="group">
                  <Label
                    htmlFor="implementationDate"
                    className="inline-flex items-center gap-1.5 text-sm font-medium"
                  >
                    Ngày dự kiến thực hiện{' '}
                    <span className="text-red-500">*</span>
                  </Label>
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        className="mt-1.5 w-full justify-start border-muted-foreground/30 text-left font-normal hover:border-primary/50 hover:bg-primary/5"
                      >
                        {date ? format(date, 'dd-MM-yyyy') : 'Chọn ngày'}
                        <Calendar className="ml-auto h-4 w-4 text-primary/70" />
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
                        className="rounded-md border"
                      />
                    </PopoverContent>
                  </Popover>
                </div>
              </div>

              {/* Right Column */}
              <div className="space-y-5">
                <div className="group">
                  <Label htmlFor="disease" className="text-sm font-medium">
                    Phòng bệnh
                  </Label>
                  <Input
                    id="disease"
                    value="Lở mồm long móng"
                    readOnly
                    className="mt-1.5 border-muted-foreground/20 bg-muted/30 text-muted-foreground"
                  />
                </div>

                <div className="group">
                  <Label
                    htmlFor="implementer"
                    className="inline-flex items-center gap-1.5 text-sm font-medium"
                  >
                    Người thực hiện <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={formData.conductedBy}
                    onValueChange={(value) =>
                      handleInputChange('conductedBy', value)
                    }
                  >
                    <SelectTrigger className="mt-1.5 border-muted-foreground/30 focus:border-primary">
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

                <div className="group">
                  <Label htmlFor="notes" className="text-sm font-medium">
                    Ghi chú
                  </Label>
                  <Textarea
                    id="notes"
                    placeholder="Nhập ghi chú"
                    className="mt-1.5 min-h-[80px] resize-none border-muted-foreground/30 focus:border-primary"
                    value={formData.description}
                    onChange={(e) =>
                      handleInputChange('description', e.target.value)
                    }
                  />
                </div>
              </div>
            </div>

            <Separator className="my-8" />

            {/* Medicine Section - Full Width */}
            <div className="space-y-5">
              <div className="flex items-center justify-between">
                <h3 className="flex items-center gap-2 text-base font-medium">
                  <span className="h-5 w-1 rounded-full bg-primary"></span>
                  Thông tin thuốc
                </h3>
                <Button
                  variant="outline"
                  size="sm"
                  onClick={addMedicine}
                  type="button"
                  className="h-9 border-primary/30 text-primary transition-colors hover:bg-primary/10 hover:text-primary"
                >
                  <Plus className="mr-1.5 h-4 w-4" />
                  Thêm một thuốc
                </Button>
              </div>

              <div className="grid gap-5 md:grid-cols-2">
                {medicines.map((medicine, index) => (
                  <div
                    key={medicine.id}
                    className="rounded-md border border-muted-foreground/20 bg-muted/5 p-5 transition-shadow hover:shadow-sm"
                  >
                    <div className="space-y-4">
                      <div className="flex items-center justify-between">
                        <span className="rounded-full bg-muted/50 px-2 py-0.5 text-xs font-medium text-muted-foreground">
                          Thuốc {medicines.length > 1 ? `#${index + 1}` : ''}
                        </span>
                      </div>
                      <div>
                        <Label
                          htmlFor={`medicineType-${index}`}
                          className="inline-flex items-center gap-1.5 text-sm font-medium"
                        >
                          Loại thuốc <span className="text-red-500">*</span>
                        </Label>
                        <Select
                          value={medicine.vaccineId || loaiThuoc?.[0]?.id || ''}
                          onValueChange={(value) =>
                            handleMedicineChange(index, value)
                          }
                        >
                          <SelectTrigger className="mt-1.5 border-muted-foreground/30 focus:border-primary">
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

        <CardFooter className="flex justify-end gap-3 border-t bg-muted/10 px-8 py-5">
          <Button
            variant="outline"
            className="border-muted-foreground/30 hover:bg-muted/20"
          >
            Hủy
          </Button>
          <Button
            onClick={handleSubmit}
            className="flex items-center justify-center gap-2 bg-primary px-6 text-white transition-colors hover:bg-primary/90"
          >
            <Save className="h-4 w-4" />
            Lưu thông tin
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
