'use client';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { useToast } from '@/components/ui/use-toast';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { CalendarIcon, Pencil } from 'lucide-react';
import { format } from 'date-fns';
import { vi } from 'date-fns/locale';
import { Calendar } from '@/components/ui/calendar';
import {
  Popover,
  PopoverContent,
  PopoverTrigger
} from '@/components/ui/popover';
import { cn } from '@/lib/utils';
import { useGetListLoaiTiem, useGetLoaiDichBenh } from '@/queries/vacxin.query';
import __helpers from '@/helpers';
import {
  useGetNguoiThucHien,
  useGetThuocTheoLoaiBenh
} from '@/queries/admin.query';

interface LoTiemData {
  id: string;
  name: string;
  vaccinationType: string;
  medcicalType: string;
  symptom: string;
  dateSchedule: string | Date;
  status: string;
  conductedBy: string;
  note: string;
  vaccineId?: string;
  diseaseId?: string;
}

interface EditLoTiemDialogProps {
  loTiemData: LoTiemData;
  capNhapLoTiem: (data: any) => Promise<any>;
}

export function EditLoTiemDialog({
  loTiemData,
  capNhapLoTiem
}: EditLoTiemDialogProps) {
  const [isOpen, setIsOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const { toast } = useToast();

  // Fetch data for dropdowns
  const { data: loaiTiem } = useGetListLoaiTiem();
  const { data: loaiDichBenh } = useGetLoaiDichBenh();
  const { mutateAsync: getNguoiThucHienByDate } = useGetNguoiThucHien();
  const [loaiBenh, setLoaiBenh] = useState('');
  const { data: dataLoaiThuoc } = useGetThuocTheoLoaiBenh(loaiBenh);
  const [formData, setFormData] = useState({
    name: loTiemData.name,
    dateSchedule: new Date(loTiemData.dateSchedule),
    conductedBy: loTiemData.conductedBy,
    vaccineId: loTiemData.vaccineId || '',
    diseaseId: loTiemData.diseaseId || '',
    description: loTiemData.note,
    type: loTiemData.vaccinationType === 'Tiêm phòng' ? 0 : 1,
    updatedBy: __helpers.getUserId()
  });

  // Find the type index based on the vaccination type name
  const getTypeIndex = () => {
    if (!loaiTiem) return 0;
    const index = loaiTiem.findIndex(
      (type) => type === loTiemData.vaccinationType
    );
    return index >= 0 ? index : 0;
  };

  // Update type when loaiTiem data is loaded
  useEffect(() => {
    if (loaiTiem) {
      setFormData((prev) => ({
        ...prev,
        type: getTypeIndex()
      }));
    }
  }, [loaiTiem]);

  const [conductors, setConductors] = useState<any[]>([]);

  useEffect(() => {
    if (formData.dateSchedule) {
      const fetchConductors = async () => {
        try {
          const dateString = format(formData.dateSchedule, 'yyyy-MM-dd');
          const response = await getNguoiThucHienByDate(dateString);
          setConductors(response.data || []);
        } catch (error) {
          console.error('Error fetching conductors:', error);
          toast({
            title: 'Lỗi',
            description:
              'Không thể lấy danh sách người thực hiện. Vui lòng thử lại.',
            variant: 'destructive'
          });
        }
      };

      fetchConductors();
    }
  }, [formData.dateSchedule, getNguoiThucHienByDate, toast]);

  const handleInputChange = (field: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  const handleSubmit = async () => {
    try {
      setIsLoading(true);

      await capNhapLoTiem({
        ...formData,
        dateSchedule: formData.dateSchedule.toISOString(),
        batchVaccinationId: loTiemData.id
      });

      toast({
        title: 'Thành công',
        description: 'Đã cập nhật thông tin lô tiêm thành công.',
        variant: 'success'
      });

      setIsOpen(false);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể cập nhật thông tin lô tiêm. Vui lòng thử lại.',
        variant: 'destructive'
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      <Button
        variant="outline"
        size="sm"
        className="absolute right-6 top-6"
        onClick={() => setIsOpen(true)}
        disabled={
          loTiemData.status === 'ĐÃ_HỦY' || loTiemData.status === 'HOÀN_THÀNH'
        }
      >
        <Pencil className="mr-2 h-4 w-4" />
        Chỉnh sửa
      </Button>

      <Dialog open={isOpen} onOpenChange={setIsOpen}>
        <DialogContent className="sm:max-w-xl">
          <DialogHeader>
            <DialogTitle>Chỉnh sửa thông tin lô tiêm</DialogTitle>
          </DialogHeader>

          <div className="grid gap-4 py-4">
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="name" className="text-right">
                Tên lô tiêm
              </Label>
              <Textarea
                id="name"
                value={formData.name}
                onChange={(e) => handleInputChange('name', e.target.value)}
                className="col-span-3 min-h-[80px] resize-none"
              />
            </div>

            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="type" className="text-right">
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
                <SelectTrigger className="col-span-3">
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

            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="diseaseId" className="text-right">
                Loại dịch bệnh
              </Label>
              <Select
                value={formData.diseaseId}
                onValueChange={(value) => {
                  handleInputChange('diseaseId', value);
                  setLoaiBenh(value);
                }}
              >
                <SelectTrigger className="col-span-3">
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

            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="vaccineId" className="text-right">
                Loại thuốc{' '}
                <p className="text-red-500">(* chọn loại dịch bệnh trước)</p>
              </Label>
              <Select
                value={formData.vaccineId}
                onValueChange={(value) => handleInputChange('vaccineId', value)}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Chọn loại thuốc" />
                </SelectTrigger>
                <SelectContent>
                  {dataLoaiThuoc?.map((med) => (
                    <SelectItem key={med.id} value={med.id}>
                      {med.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="dateSchedule" className="text-right">
                Ngày dự kiến
              </Label>
              <div className="col-span-3">
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant={'outline'}
                      className={cn(
                        'w-full justify-start text-left font-normal',
                        !formData.dateSchedule && 'text-muted-foreground'
                      )}
                    >
                      <CalendarIcon className="mr-2 h-4 w-4" />
                      {formData.dateSchedule ? (
                        format(formData.dateSchedule, 'dd-MM-yyyy', {
                          locale: vi
                        })
                      ) : (
                        <span>Chọn ngày</span>
                      )}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <Calendar
                      mode="single"
                      selected={formData.dateSchedule}
                      onSelect={(date) => {
                        if (date) {
                          handleInputChange('dateSchedule', date);
                        }
                      }}
                      initialFocus
                      locale={vi}
                    />
                  </PopoverContent>
                </Popover>
              </div>
            </div>

            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="conductedBy" className="text-right">
                Người thực hiện
              </Label>
              <Select
                value={formData.conductedBy}
                onValueChange={(value) =>
                  handleInputChange('conductedBy', value)
                }
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Chọn người thực hiện" />
                </SelectTrigger>
                <SelectContent>
                  {conductors.length > 0 ? (
                    conductors.map((conductor) => (
                      <SelectItem key={conductor.id} value={conductor.id}>
                        {conductor.userName} ({conductor.roles.join(', ')})
                      </SelectItem>
                    ))
                  ) : (
                    <SelectItem value="" disabled>
                      Không có người thực hiện cho ngày này
                    </SelectItem>
                  )}
                </SelectContent>
              </Select>
            </div>

            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="description" className="text-right">
                Ghi chú
              </Label>
              <Textarea
                id="description"
                value={formData.description}
                onChange={(e) =>
                  handleInputChange('description', e.target.value)
                }
                className="col-span-3 min-h-[80px] resize-none"
              />
            </div>
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsOpen(false)}>
              Hủy
            </Button>
            <Button onClick={handleSubmit} disabled={isLoading}>
              {isLoading ? 'Đang xử lý...' : 'Cập nhật'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
