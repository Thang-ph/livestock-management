'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
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
import { PlusCircle, Info, Trash2 } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Separator } from '@/components/ui/separator';
import { useCreateDonMuaLe, useGetListLoaiVat } from '@/queries/donmuale.query';
import { useGetLoaiBenh } from '@/queries/benh.query';
import { DiseaseWarrantyDialog } from './disease-warranty-dialog';
import { toast } from '@/components/ui/use-toast';

interface SelectedDisease {
  diseaseId: string;
  insuranceDuration: number;
}

interface Requirement {
  id: number;
  animalType: string;
  weightMin: number;
  weightMax: number;
  quantity: number;
  otherRequirements: string;
  selectedDiseases: SelectedDisease[];
}

export default function Add() {
  const [requirements, setRequirements] = useState<Requirement[]>([
    {
      id: 1,
      animalType: '',
      weightMin: 100,
      weightMax: 200,
      quantity: 10,
      otherRequirements: '',
      selectedDiseases: []
    }
  ]);

  const [customerName, setCustomerName] = useState('');
  const [phone, setPhone] = useState('');
  const [address, setAddress] = useState('');
  const [email, setEmail] = useState('');

  const [dialogOpen, setDialogOpen] = useState(false);
  const [activeRequirementId, setActiveRequirementId] = useState<number | null>(
    null
  );

  const { mutateAsync: createDonMuaLe } = useCreateDonMuaLe();
  const { data: listLoaiBenh, isPending: isPendingBenh } = useGetLoaiBenh();
  const { data: listLoaiVat, isPending: isPendingVat } = useGetListLoaiVat();
  const addRequirement = () => {
    const newId =
      requirements.length > 0
        ? Math.max(...requirements.map((r) => r.id)) + 1
        : 1;
    setRequirements([
      ...requirements,
      {
        id: newId,
        animalType: '',
        weightMin: 100,
        weightMax: 200,
        quantity: 10,
        otherRequirements: '',
        selectedDiseases: []
      }
    ]);
  };

  const removeRequirement = (id: number) => {
    if (requirements.length > 1) {
      setRequirements(requirements.filter((req) => req.id !== id));
    }
  };

  const handleRequirementChange = (
    id: number,
    field: keyof Requirement,
    value: any
  ) => {
    setRequirements(
      requirements.map((req) =>
        req.id === id ? { ...req, [field]: value } : req
      )
    );
  };

  const openDiseaseDialog = (requirementId: number) => {
    setActiveRequirementId(requirementId);
    setDialogOpen(true);
  };

  const handleDiseaseConfirm = (selectedDiseases: SelectedDisease[]) => {
    if (activeRequirementId !== null) {
      handleRequirementChange(
        activeRequirementId,
        'selectedDiseases',
        selectedDiseases
      );
    }
  };

  const handleSubmit = async () => {
    const payload = {
      customerName,
      phone,
      addrress: address, // Note: there's a typo in the API field name
      email,
      details: requirements.map((req) => ({
        specieId: req.animalType,
        weightFrom: req.weightMin,
        weightTo: req.weightMax,
        total: req.quantity,
        description: req.otherRequirements,
        vaccintionRequirement: req.selectedDiseases
      })),
      requestedBy: 'user' // This should be replaced with the actual user ID
    };

    const [err] = await createDonMuaLe(payload);

    if (err) {
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi tạo đơn hàng. Vui lòng thử lại.',
        variant: 'destructive'
      });
    } else {
      toast({
        title: 'Thành công',
        description: 'Đơn hàng đã được tạo thành công.',
        variant: 'success'
      });
    }
  };

  console.log('listLoaiVat', listLoaiVat);

  if (isPendingBenh || isPendingVat) {
    return (
      <div className="flex h-screen items-center justify-center">
        <div className="h-32 w-32 animate-spin rounded-full border-b-2 border-gray-900"></div>
      </div>
    );
  }

  return (
    <main className="container mx-auto max-w-6xl p-4">
      <div className="mb-6">
        <h1 className="mb-2 text-2xl font-bold text-gray-800">
          Thêm yêu cầu mua hàng
        </h1>
        <p className="text-gray-500">
          Điền thông tin khách hàng và yêu cầu về loại vật
        </p>
        <Separator className="my-4" />
      </div>

      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {/* Customer Information Section */}
        <Card className="border border-gray-200 shadow-sm">
          <CardHeader className="border-b bg-gray-50">
            <CardTitle className="flex items-center gap-2 text-lg">
              <span className="rounded-full bg-green-100 p-1 text-green-800">
                <Info size={16} />
              </span>
              Thông tin khách hàng
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-6">
            <div className="grid gap-5">
              <div className="grid gap-2">
                <Label htmlFor="customer-name" className="font-medium">
                  Tên khách hàng <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="customer-name"
                  placeholder="Nhập tên khách hàng"
                  className="border border-gray-300 focus:border-green-500 focus:ring-1 focus:ring-green-500"
                  value={customerName}
                  onChange={(e) => setCustomerName(e.target.value)}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="phone" className="font-medium">
                  Số điện thoại <span className="text-red-500">*</span>
                </Label>
                <Input
                  id="phone"
                  placeholder="Nhập số điện thoại"
                  className="border border-gray-300 focus:border-green-500 focus:ring-1 focus:ring-green-500"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="address" className="font-medium">
                  Địa chỉ
                </Label>
                <Textarea
                  id="address"
                  placeholder="Nhập địa chỉ"
                  className="min-h-[100px] border border-gray-300 focus:border-green-500 focus:ring-1 focus:ring-green-500"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                />
              </div>

              <div className="grid gap-2">
                <Label htmlFor="email" className="font-medium">
                  Email
                </Label>
                <Input
                  id="email"
                  type="email"
                  placeholder="example@email.com"
                  className="border border-gray-300 focus:border-green-500 focus:ring-1 focus:ring-green-500"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Animal Requirements Section */}
        <div className="space-y-5">
          <div className="flex items-center rounded-lg border border-gray-200 bg-white p-4 shadow-sm">
            <h2 className="flex items-center gap-2 text-lg font-medium">
              <span className="rounded-full bg-green-100 p-1 text-green-800">
                <PlusCircle size={16} />
              </span>
              Yêu cầu về loại vật
            </h2>
          </div>

          {requirements.map((req, index) => (
            <Card
              key={req.id}
              className="overflow-hidden border border-gray-200 shadow-sm"
            >
              <CardHeader className="flex flex-row items-center justify-between space-y-0 border-b bg-gray-50 px-4 py-3">
                <CardTitle className="flex items-center gap-2 text-base font-medium">
                  <Badge
                    variant="outline"
                    className="border-green-200 bg-green-50 text-green-700"
                  >
                    Yêu cầu {index + 1}
                  </Badge>
                </CardTitle>
                {requirements.length > 1 && (
                  <Button
                    variant="ghost"
                    size="sm"
                    className="h-8 w-8 p-0 text-red-500 hover:bg-red-50 hover:text-red-700"
                    onClick={() => removeRequirement(req.id)}
                  >
                    <Trash2 size={16} />
                    <span className="sr-only">Xóa yêu cầu</span>
                  </Button>
                )}
              </CardHeader>
              <CardContent className="pt-5">
                <div className="grid gap-5">
                  <div className="grid gap-2">
                    <Label
                      htmlFor={`animal-type-${req.id}`}
                      className="font-medium"
                    >
                      Loại vật <span className="text-red-500">*</span>
                    </Label>
                    <Select
                      value={req.animalType}
                      onValueChange={(value) =>
                        handleRequirementChange(req.id, 'animalType', value)
                      }
                    >
                      <SelectTrigger
                        id={`animal-type-${req.id}`}
                        className="border border-gray-300 focus:border-green-500 focus:ring-1 focus:ring-green-500"
                      >
                        <SelectValue placeholder="Chọn loại vật" />
                      </SelectTrigger>
                      <SelectContent>
                        {listLoaiVat &&
                          listLoaiVat?.map((animal) => (
                            <SelectItem key={animal.id} value={animal.id}>
                              {animal.name}
                            </SelectItem>
                          ))}
                      </SelectContent>
                    </Select>
                  </div>

                  <div className="grid gap-2">
                    <Label className="font-medium">Cân nặng (kg)</Label>
                    <div className="flex items-center gap-3">
                      <div className="flex-1">
                        <Label
                          htmlFor={`weight-min-${req.id}`}
                          className="mb-1 block text-xs text-gray-500"
                        >
                          Tối thiểu
                        </Label>
                        <Input
                          id={`weight-min-${req.id}`}
                          type="number"
                          className="border border-gray-300 text-center focus:border-green-500 focus:ring-1 focus:ring-green-500"
                          value={req.weightMin}
                          onChange={(e) =>
                            handleRequirementChange(
                              req.id,
                              'weightMin',
                              Number.parseInt(e.target.value)
                            )
                          }
                        />
                      </div>
                      <span className="text-gray-400">-</span>
                      <div className="flex-1">
                        <Label
                          htmlFor={`weight-max-${req.id}`}
                          className="mb-1 block text-xs text-gray-500"
                        >
                          Tối đa
                        </Label>
                        <Input
                          id={`weight-max-${req.id}`}
                          type="number"
                          className="border border-gray-300 text-center focus:border-green-500 focus:ring-1 focus:ring-green-500"
                          value={req.weightMax}
                          onChange={(e) =>
                            handleRequirementChange(
                              req.id,
                              'weightMax',
                              Number.parseInt(e.target.value)
                            )
                          }
                        />
                      </div>
                    </div>
                  </div>

                  <div className="grid gap-2">
                    <Label
                      htmlFor={`quantity-${req.id}`}
                      className="font-medium"
                    >
                      Số lượng (con) <span className="text-red-500">*</span>
                    </Label>
                    <Input
                      id={`quantity-${req.id}`}
                      type="number"
                      className="border border-gray-300 focus:border-green-500 focus:ring-1 focus:ring-green-500"
                      value={req.quantity}
                      onChange={(e) =>
                        handleRequirementChange(
                          req.id,
                          'quantity',
                          Number.parseInt(e.target.value)
                        )
                      }
                    />
                  </div>

                  <div className="grid gap-2 rounded-lg border border-gray-200 bg-gray-50 p-3">
                    <div className="flex items-center justify-between">
                      <Label className="font-medium">
                        Bảo hành theo bệnh ({req.selectedDiseases.length}):
                      </Label>
                      <Button
                        variant="outline"
                        size="sm"
                        className="h-8 border border-gray-300 hover:bg-gray-100 hover:text-gray-900"
                        onClick={() => openDiseaseDialog(req.id)}
                      >
                        Chi tiết
                      </Button>
                    </div>
                    <div className="text-xs italic text-gray-500">
                      *tính từ thời điểm vật nuôi xuất
                    </div>
                  </div>

                  <div className="grid gap-2">
                    <Label
                      htmlFor={`other-req-${req.id}`}
                      className="font-medium"
                    >
                      Yêu cầu khác:
                    </Label>
                    <Textarea
                      id={`other-req-${req.id}`}
                      placeholder="Nhập các yêu cầu khác nếu có"
                      className="min-h-[80px] border border-gray-300 focus:border-green-500 focus:ring-1 focus:ring-green-500"
                      value={req.otherRequirements}
                      onChange={(e) =>
                        handleRequirementChange(
                          req.id,
                          'otherRequirements',
                          e.target.value
                        )
                      }
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          ))}
          <Button
            onClick={addRequirement}
            className="mt-3 flex w-full items-center justify-center gap-2 bg-green-600 text-white hover:bg-green-700"
          >
            <PlusCircle size={16} />
            Thêm yêu cầu
          </Button>
        </div>
      </div>

      <div className="mt-8 flex justify-end gap-3">
        <Button variant="outline" className="border-gray-300">
          Hủy
        </Button>
        <Button
          className="bg-green-600 text-white hover:bg-green-700"
          onClick={handleSubmit}
        >
          Lưu yêu cầu
        </Button>
      </div>

      {/* Disease Warranty Dialog */}
      {activeRequirementId !== null && (
        <DiseaseWarrantyDialog
          open={dialogOpen}
          onOpenChange={setDialogOpen}
          diseases={listLoaiBenh.items || []}
          selectedDiseases={
            requirements.find((r) => r.id === activeRequirementId)
              ?.selectedDiseases || []
          }
          onConfirm={handleDiseaseConfirm}
        />
      )}
    </main>
  );
}
