'use client';

import { useState } from 'react';
import {
  Calendar,
  Save,
  Syringe,
  WormIcon as Virus,
  ClipboardList,
  User,
  AlertCircle
} from 'lucide-react';
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
import { Badge } from '@/components/ui/badge';
import { Alert, AlertDescription } from '@/components/ui/alert';
import {
  useGetListLoaiTiem,
  useGetLoaiDichBenh,
  useGetThuoc,
  useTaoChiTietLoTiemVacxin
} from '@/queries/vacxin.query';
import { toast } from '@/components/ui/use-toast';
import { useGetNguoiThucHien } from '@/queries/admin.query';

export default function AddForm() {
  const [date, setDate] = useState<Date | undefined>(new Date('2024-03-18'));
  const { data: loaiTiem } = useGetListLoaiTiem();
  const { data: loaiDichBenh } = useGetLoaiDichBenh();
  const { data: loaiThuoc } = useGetThuoc();
  const { mutateAsync: taoLoTiem } = useTaoChiTietLoTiemVacxin();
  const [medicines, setMedicines] = useState([{ id: 1, vaccineId: '' }]);
  const { mutateAsync: getNguoiThucHien } = useGetNguoiThucHien();
  const [availablePersonnel, setAvailablePersonnel] = useState([]);
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const [formData, setFormData] = useState({
    name: 'Tiêm phòng Lở mồm long móng',
    type: 0, // TIÊM_VACCINE
    diseaseId: '',
    dateSchedule: new Date(),
    conductedBy: '',
    description: '',
    status: 0,
    vaccineId: ''
  });

  const handleInputChange = (field, value) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));

    // Clear error for this field when user makes a change
    if (errors[field]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[field];
        return newErrors;
      });
    }
  };

  const handleMedicineChange = (index, vaccineId) => {
    const updatedMedicines = [...medicines];
    updatedMedicines[index] = { ...updatedMedicines[index], vaccineId };
    setMedicines(updatedMedicines);

    // Clear medicine error when user makes a change
    if (errors.vaccineId) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors.vaccineId;
        return newErrors;
      });
    }
  };

  const validateForm = () => {
    const newErrors: Record<string, string> = {};

    // Validate name
    if (!formData.name || formData.name.trim() === '') {
      newErrors.name = 'Vui lòng nhập tên lô tiêm';
    }

    // Validate diseaseId
    if (!formData.diseaseId) {
      newErrors.diseaseId = 'Vui lòng chọn loại dịch bệnh';
    }

    // Validate conductedBy
    if (
      !formData.conductedBy ||
      (formData.conductedBy === 'QUAN_TRAI' && availablePersonnel.length > 0)
    ) {
      newErrors.conductedBy = 'Vui lòng chọn người thực hiện';
    }

    // Validate medicine/vaccine
    if (!medicines[0].vaccineId) {
      newErrors.vaccineId = 'Vui lòng chọn loại thuốc';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setIsSubmitting(true);

    // Validate form before submission
    const isValid = validateForm();

    if (!isValid) {
      setIsSubmitting(false);
      // Scroll to the first error
      const firstErrorField = document.querySelector('[data-error="true"]');
      if (firstErrorField) {
        firstErrorField.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }
      return;
    }

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

      const [err] = await taoLoTiem(payload);
      if (!err) {
        toast({
          title: 'Thành công',
          description: 'Tạo lô tiêm thành công!',
          variant: 'success'
        });
      } else {
        toast({
          title: 'Thất bại',
          description: 'Có lỗi xảy ra khi tạo lô tiêm!',
          variant: 'destructive'
        });
      }
      // Reset form or redirect as needed
    } catch (error) {
      console.error('Error creating vaccination batch:', error);
      toast({
        title: 'Thất bại',
        description: 'Có lỗi xảy ra khi tạo lô tiêm!',
        variant: 'destructive'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleGetNguoiThucHien = async (date) => {
    const res = await getNguoiThucHien(date);
    setAvailablePersonnel(res.data);
    console.log('Available personnel:', res.data);
  };

  return (
    <div className="container mx-auto max-w-5xl py-8">
      {Object.keys(errors).length > 0 && (
        <Alert variant="destructive" className="mb-6">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>
            Vui lòng điền đầy đủ thông tin trước khi gửi
          </AlertDescription>
        </Alert>
      )}

      <Card className="overflow-hidden border-none shadow-lg">
        <CardHeader className="border-b bg-gradient-to-r from-emerald-50 to-teal-50 dark:from-emerald-950/20 dark:to-teal-950/20">
          <div className="flex items-center gap-3">
            <div className="rounded-full bg-emerald-100 p-2 text-emerald-600 dark:bg-emerald-900/30 dark:text-emerald-400">
              <Syringe className="h-5 w-5" />
            </div>
            <div>
              <CardTitle className="text-2xl font-semibold text-emerald-700 dark:text-emerald-400">
                Thông tin lô tiêm
              </CardTitle>
              <CardDescription className="text-emerald-600/80 dark:text-emerald-400/80">
                Nhập thông tin chi tiết về lô tiêm mới
              </CardDescription>
            </div>
          </div>
        </CardHeader>

        <CardContent className="pt-6">
          <form onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 gap-x-8 gap-y-6 md:grid-cols-2">
              <div className="space-y-5">
                <div className="space-y-2" data-error={!!errors.name}>
                  <Label
                    htmlFor="lotName"
                    className="flex items-center gap-2 text-sm font-medium"
                  >
                    <ClipboardList className="h-4 w-4" />
                    Tên lô tiêm <span className="text-red-500">*</span>
                  </Label>
                  <Textarea
                    id="lotName"
                    placeholder="Tiêm phòng Lở mồm long móng"
                    className={`min-h-[80px] resize-none ${errors.name ? 'border-red-500 focus-visible:ring-red-500' : ''}`}
                    value={formData.name}
                    onChange={(e) => handleInputChange('name', e.target.value)}
                  />
                  {errors.name && (
                    <p className="text-sm font-medium text-red-500">
                      {errors.name}
                    </p>
                  )}
                </div>

                <div className="space-y-2">
                  <Label
                    htmlFor="vaccinationType"
                    className="flex items-center gap-2 text-sm font-medium"
                  >
                    <Syringe className="h-4 w-4" />
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
                    <SelectTrigger>
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

                <div className="space-y-2" data-error={!!errors.diseaseId}>
                  <Label
                    htmlFor="diseaseType"
                    className="flex items-center gap-2 text-sm font-medium"
                  >
                    <Virus className="h-4 w-4" />
                    Loại dịch bệnh <span className="text-red-500">*</span>
                  </Label>
                  <Select
                    value={formData.diseaseId}
                    onValueChange={(value) =>
                      handleInputChange('diseaseId', value)
                    }
                  >
                    <SelectTrigger
                      className={
                        errors.diseaseId ? 'border-red-500 ring-red-500' : ''
                      }
                    >
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
                  {errors.diseaseId && (
                    <p className="text-sm font-medium text-red-500">
                      {errors.diseaseId}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label
                    htmlFor="notes"
                    className="flex items-center gap-2 text-sm font-medium"
                  >
                    <ClipboardList className="h-4 w-4" />
                    Ghi chú
                  </Label>
                  <Textarea
                    id="notes"
                    placeholder="Nhập ghi chú"
                    className="min-h-[80px] resize-none"
                    value={formData.description}
                    onChange={(e) =>
                      handleInputChange('description', e.target.value)
                    }
                  />
                </div>
              </div>

              {/* Right Column */}
              <div className="space-y-5">
                <div className="space-y-2">
                  <Label
                    htmlFor="implementationDate"
                    className="flex items-center gap-2 text-sm font-medium"
                  >
                    <Calendar className="h-4 w-4" />
                    Ngày dự kiến thực hiện{' '}
                    <span className="text-red-500">*</span>
                  </Label>
                  <Popover>
                    <PopoverTrigger asChild>
                      <Button
                        variant="outline"
                        className="w-full justify-start text-left font-normal"
                      >
                        {date ? format(date, 'dd-MM-yyyy') : 'Chọn ngày'}
                        <Calendar className="ml-auto h-4 w-4 opacity-70" />
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
                            const formattedDate = date.toISOString();
                            handleGetNguoiThucHien(formattedDate);
                          }
                        }}
                        locale={vi}
                      />
                    </PopoverContent>
                  </Popover>
                </div>
                <div className="space-y-2" data-error={!!errors.conductedBy}>
                  <Label
                    htmlFor="implementer"
                    className="flex items-center gap-2 text-sm font-medium"
                  >
                    <User className="h-4 w-4" />
                    Người thực hiện <span className="text-red-500">*</span>
                    <Badge
                      variant="outline"
                      className="ml-2 border-red-200 bg-red-50 text-xs font-normal text-red-600 dark:border-red-900/50 dark:bg-red-950/20 dark:text-red-400"
                    >
                      Chọn ngày trước
                    </Badge>
                  </Label>
                  <Select
                    value={formData.conductedBy}
                    onValueChange={(value) =>
                      handleInputChange('conductedBy', value)
                    }
                  >
                    <SelectTrigger
                      className={
                        errors.conductedBy ? 'border-red-500 ring-red-500' : ''
                      }
                    >
                      <SelectValue placeholder="Chọn người thực hiện" />
                    </SelectTrigger>
                    <SelectContent>
                      {availablePersonnel.length > 0 ? (
                        availablePersonnel.map((person: any) => (
                          <SelectItem key={person.id} value={person.id}>
                            {person.userName} - {person.phoneNumber}
                          </SelectItem>
                        ))
                      ) : (
                        <SelectItem value="empty" disabled>
                          Không có người thực hiện
                        </SelectItem>
                      )}
                    </SelectContent>
                  </Select>
                  {errors.conductedBy && (
                    <p className="text-sm font-medium text-red-500">
                      {errors.conductedBy}
                    </p>
                  )}
                </div>
                <div className="space-y-2">
                  <Label
                    htmlFor="disease"
                    className="flex items-center gap-2 text-sm font-medium"
                  >
                    <Virus className="h-4 w-4" />
                    Phòng bệnh
                  </Label>
                  <Input
                    id="disease"
                    value="Lở mồm long móng"
                    readOnly
                    className="bg-slate-50 dark:bg-slate-800/50"
                  />
                </div>
              </div>
            </div>

            {/* Medicine Section - Full Width */}
            <div className="mt-8">
              <div className="mb-4 flex items-center">
                <h3 className="text-lg font-medium">Thông tin thuốc</h3>
                <Separator className="ml-4 flex-1" />
              </div>

              <div className="grid gap-4 md:grid-cols-2">
                {medicines.map((medicine, index) => (
                  <div
                    key={medicine.id}
                    className={`rounded-lg border p-5 shadow-sm ${
                      errors.vaccineId && index === 0 ? 'border-red-500' : ''
                    }`}
                    data-error={!!errors.vaccineId && index === 0}
                  >
                    <div className="space-y-4">
                      <div className="flex items-center gap-2">
                        <Syringe className="h-5 w-5" />
                        <h4 className="font-medium">
                          Loại thuốc{' '}
                          {medicines.length > 1 ? `#${index + 1}` : ''}{' '}
                          <span className="text-red-500">*</span>
                        </h4>
                      </div>
                      <Select
                        value={medicine.vaccineId || loaiThuoc?.[0]?.id || ''}
                        onValueChange={(value) =>
                          handleMedicineChange(index, value)
                        }
                      >
                        <SelectTrigger
                          className={
                            errors.vaccineId && index === 0
                              ? 'border-red-500 ring-red-500'
                              : ''
                          }
                        >
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
                      {errors.vaccineId && index === 0 && (
                        <p className="text-sm font-medium text-red-500">
                          {errors.vaccineId}
                        </p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </form>
        </CardContent>

        <CardFooter className="flex justify-end gap-3 border-t bg-gradient-to-r from-emerald-50 to-teal-50 px-6 py-4 dark:from-emerald-950/20 dark:to-teal-950/20">
          <Button
            variant="outline"
            className="border-emerald-200 hover:bg-white hover:text-emerald-700 dark:border-emerald-800 dark:hover:bg-emerald-950/20"
            type="button"
          >
            Hủy
          </Button>
          <Button
            onClick={handleSubmit}
            className="bg-emerald-600 text-white hover:bg-emerald-700 dark:bg-emerald-700 dark:hover:bg-emerald-600"
            disabled={isSubmitting}
          >
            <Save className="mr-2 h-4 w-4" />
            {isSubmitting ? 'Đang lưu...' : 'Lưu thông tin'}
          </Button>
        </CardFooter>
      </Card>
    </div>
  );
}
