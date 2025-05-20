'use client';

import { useState } from 'react';
import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { Button } from '@/components/ui/button';
import { Plus, Calendar } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/dialog';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { useToast } from '@/components/ui/use-toast';
import { useParams } from 'react-router-dom';
import { useGetLoNhapById } from '@/queries/lo-nhap.query';
import {
  useAddLiveStockToBatch,
  useGetSpeciceName,
  useGetSpecieType
} from '@/queries/admin.query';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Popover,
  PopoverContent,
  PopoverTrigger
} from '@/components/ui/popover';
import { format } from 'date-fns';
import { Calendar as CalendarComponent } from '@/components/ui/calendar';

const STATUS = {
  'Chờ Nhập': 0,
  'Chờ Định Danh': 1,
  'Khỏe Mạnh': 2,
  Ốm: 3,
  'Chờ Xuất': 4,
  'Đã Xuất': 5,
  Chết: 6
};

const GENDER = {
  Đực: 0,
  Cái: 1
};

export function OverViewTab() {
  const { toast } = useToast();
  const pageLimit = 10;
  const params = useParams();
  const id = params?.id as string;

  const { data, isPending } = useGetLoNhapById(id);
  const { data: loaiVatNuoi, isLoading: isLoadingLoaiVatNuoi } =
    useGetSpecieType();
  const [animalType, setAnimalType] = useState<string>('BÒ');

  const { data: listTenVatNuoi } = useGetSpeciceName(animalType);

  const listObjects = data?.listImportedLivestocks?.items || [];
  const totalRecords = listObjects.length || 0;

  const pageCount = Math.ceil(totalRecords / pageLimit);
  const { mutateAsync: addLiveStockToBatch } = useAddLiveStockToBatch();

  // State cho form thêm mới
  const [showAddForm, setShowAddForm] = useState(false);
  const [formData, setFormData] = useState({
    inspectionCode: '',
    specieType: 0,
    specieName: '',
    livestockStatus: 2, // Default to "Khỏe Mạnh"
    gender: 0, // Default to "Đực"
    color: '',
    weight: 0.1,
    dob: new Date(),
    requestedBy: 'admin'
  });

  const handleOpenAddForm = () => {
    setShowAddForm(true);
    // Set default animal type based on first item in loaiVatNuoi if available
    if (loaiVatNuoi && loaiVatNuoi.length > 0) {
      setAnimalType(loaiVatNuoi[0]);
      // Map animal type string to numeric index
      const typeIndex = loaiVatNuoi.findIndex(
        (type) => type === loaiVatNuoi[0]
      );
      setFormData((prev) => ({
        ...prev,
        specieType: typeIndex >= 0 ? typeIndex : 0
      }));
    }
  };

  // Đóng form thêm mới
  const handleCloseAddForm = () => {
    setShowAddForm(false);
    // Reset form data
    setFormData({
      inspectionCode: '',
      specieType: 0,
      specieName: '',
      livestockStatus: 2,
      gender: 0,
      color: '',
      weight: 0.1,
      dob: new Date(),
      requestedBy: 'admin'
    });
  };

  // Handle form field changes
  const handleFormChange = (field: string, value: any) => {
    setFormData((prev) => ({
      ...prev,
      [field]: value
    }));
  };

  // Handle animal type change
  const handleAnimalTypeChange = (value: string) => {
    setAnimalType(value);
    // Map animal type string to numeric index
    const typeIndex = loaiVatNuoi?.findIndex((type) => type === value) || 0;
    setFormData((prev) => ({
      ...prev,
      specieType: typeIndex >= 0 ? typeIndex : 0,
      specieName: '' // Reset species name when animal type changes
    }));
  };

  // Xử lý khi submit form
  const handleSubmitForm = async () => {
    try {
      const model = {
        id: id,
        item: {
          ...formData,
          weight: Number.parseFloat(formData.weight.toString())
        }
      };
      // Gọi API để thêm vật nuôi
      const [err] = await addLiveStockToBatch(model);
      if (err) {
        toast({
          title: 'Lỗi',
          description: err.data.data,
          variant: 'destructive'
        });
      } else {
        toast({
          title: 'Thành công',
          description: 'Đã thêm vật nuôi mới',
          variant: 'success'
        });
        handleCloseAddForm();
      }
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể thêm vật nuôi. Vui lòng thử lại',
        variant: 'destructive'
      });
    }
  };

  console.log('listTenVatNuoi', listTenVatNuoi);

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0">
        <h1 className="text-center font-bold">DANH SÁCH VẬT NUÔI</h1>
        <div className="flex justify-end gap-4">
          <Button
            onClick={handleOpenAddForm}
            className="bg-green-600 hover:bg-green-700"
          >
            <Plus className="mr-2 h-4 w-4" />
            Thêm mới
          </Button>
        </div>

        {isPending ? (
          <div className="p-5">
            <DataTableSkeleton
              columnCount={10}
              filterableColumnCount={2}
              searchableColumnCount={1}
            />
          </div>
        ) : (
          <ListData
            data={listObjects}
            page={pageLimit}
            totalUsers={totalRecords}
            pageCount={pageCount}
          />
        )}
      </div>

      {/* Dialog thêm mới vật nuôi */}
      <Dialog open={showAddForm} onOpenChange={setShowAddForm}>
        <DialogContent className="sm:max-w-[500px]">
          <DialogHeader>
            <DialogTitle>Thêm mới vật nuôi</DialogTitle>
          </DialogHeader>
          <div className="grid gap-4 py-4">
            {/* Mã kiểm định */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="inspectionCode" className="text-right">
                Mã kiểm định
              </Label>
              <Input
                id="inspectionCode"
                value={formData.inspectionCode}
                onChange={(e) =>
                  handleFormChange('inspectionCode', e.target.value)
                }
                className="col-span-3"
              />
            </div>

            {/* Loại vật nuôi */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="animalType" className="text-right">
                Loại vật nuôi
              </Label>
              <Select value={animalType} onValueChange={handleAnimalTypeChange}>
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Chọn loại vật nuôi" />
                </SelectTrigger>
                <SelectContent>
                  {loaiVatNuoi?.map((type, index) => (
                    <SelectItem key={index} value={type}>
                      {type}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Loài vật */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="specieName" className="text-right">
                Loài vật
              </Label>
              <Select
                value={formData.specieName}
                onValueChange={(value) => handleFormChange('specieName', value)}
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Chọn loài vật" />
                </SelectTrigger>
                <SelectContent>
                  {listTenVatNuoi?.map((species) => (
                    <SelectItem key={species.name} value={species.name}>
                      {species.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Trạng thái */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="livestockStatus" className="text-right">
                Trạng thái
              </Label>
              <Select
                value={formData.livestockStatus.toString()}
                onValueChange={(value) =>
                  handleFormChange('livestockStatus', Number.parseInt(value))
                }
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Chọn trạng thái" />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(STATUS).map(([label, value]) => (
                    <SelectItem key={value} value={value.toString()}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Giới tính */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="gender" className="text-right">
                Giới tính
              </Label>
              <Select
                value={formData.gender.toString()}
                onValueChange={(value) =>
                  handleFormChange('gender', Number.parseInt(value))
                }
              >
                <SelectTrigger className="col-span-3">
                  <SelectValue placeholder="Chọn giới tính" />
                </SelectTrigger>
                <SelectContent>
                  {Object.entries(GENDER).map(([label, value]) => (
                    <SelectItem key={value} value={value.toString()}>
                      {label}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>

            {/* Màu sắc */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="color" className="text-right">
                Màu sắc
              </Label>
              <Input
                id="color"
                value={formData.color}
                onChange={(e) => handleFormChange('color', e.target.value)}
                className="col-span-3"
              />
            </div>

            {/* Cân nặng */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="weight" className="text-right">
                Cân nặng (kg)
              </Label>
              <Input
                id="weight"
                type="number"
                step="0.1"
                min="0.1"
                value={formData.weight}
                onChange={(e) => handleFormChange('weight', e.target.value)}
                className="col-span-3"
              />
            </div>

            {/* Ngày sinh */}
            <div className="grid grid-cols-4 items-center gap-4">
              <Label htmlFor="dob" className="text-right">
                Ngày sinh
              </Label>
              <div className="col-span-3">
                <Popover>
                  <PopoverTrigger asChild>
                    <Button
                      variant="outline"
                      className="w-full justify-start text-left font-normal"
                    >
                      <Calendar className="mr-2 h-4 w-4" />
                      {formData.dob ? (
                        format(formData.dob, 'dd/MM/yyyy')
                      ) : (
                        <span>Chọn ngày</span>
                      )}
                    </Button>
                  </PopoverTrigger>
                  <PopoverContent className="w-auto p-0">
                    <CalendarComponent
                      mode="single"
                      selected={formData.dob}
                      onSelect={(date) => handleFormChange('dob', date)}
                      initialFocus
                    />
                  </PopoverContent>
                </Popover>
              </div>
            </div>
          </div>
          <DialogFooter>
            <Button variant="outline" onClick={handleCloseAddForm}>
              Hủy
            </Button>
            <Button onClick={handleSubmitForm}>Lưu</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </>
  );
}
