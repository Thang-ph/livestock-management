'use client';

import type React from 'react';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
} from '@/components/ui/dialog';
import { Tabs, TabsContent } from '@/components/ui/tabs';
import { Input } from '@/components/ui/input';
import { useToast } from '@/components/ui/use-toast';
import { useGetListVatNuoiKhachHang } from '@/queries/admin.query';

interface UserCellActionProps {
  data: {
    id: number;
    name: string;
    email: string;
    role: string;
  };
}

export const CellAction: React.FC<UserCellActionProps> = ({ data }) => {
  const { toast } = useToast();
  const [open, setOpen] = useState(false);
  const [activeTab, setActiveTab] = useState('details');
  const [qrCode, setQrCode] = useState('');
  const { data: dataServer } = useGetListVatNuoiKhachHang(data.id);

  // Use server data if available, otherwise use fallback
  const customerInfo = dataServer
    ? {
        name: dataServer.customerName,
        quantity: dataServer.totalLivestocks,
        address: dataServer.customerAddress,
        note: dataServer.customerNote,
        received: dataServer.received,
        status: dataServer.status
      }
    : {
        name: 'Ngô Đình Thiềm',
        quantity: 2,
        address: 'Khu 1',
        note: 'Thành viên',
        received: 2,
        status: 'ĐANG_BẢN_GIAO'
      };

  // Map server items to animal data format
  const animalData = dataServer?.items?.map((item) => ({
    id: item.inspectionCode,
    category: 'Bò lai Sind', // This could be fetched from another API based on livestockId if needed
    weight: item.weightExport ? `${item.weightExport} kg` : 'N/A',
    receiveDate: item.handoverDate
      ? new Date(item.handoverDate).toLocaleDateString('vi-VN')
      : 'N/A',
    sellDate: item.exportDate
      ? new Date(item.exportDate).toLocaleDateString('vi-VN')
      : 'N/A',
    warrantyDate: item.expiredInsuranceDate
      ? new Date(item.expiredInsuranceDate).toLocaleDateString('vi-VN')
      : 'N/A',
    actions: '[v]',
    status: item.status
  })) || [
    {
      id: '5346',
      category: 'Bò lai Sind',
      weight: '200 kg',
      receiveDate: '01/02/2025',
      sellDate: '02/01/2025',
      warrantyDate: '01/02/2025',
      actions: '[v]',
      status: 'CHỜ_BÀN_GIAO'
    },
    {
      id: '5347',
      category: 'Bò lai Sind',
      weight: 'N/A',
      receiveDate: '01/02/2025',
      sellDate: 'N/A',
      warrantyDate: 'N/A',
      actions: '[v]',
      status: 'CHỜ_BÀN_GIAO'
    }
  ];

  // Default selected animal (first one in the list or fallback)
  const [selectedAnimal, setSelectedAnimal] = useState(
    animalData.length > 0
      ? {
          id: animalData[0].id,
          category: animalData[0].category,
          weight: animalData[0].weight.replace(' kg', ''),
          color: 'Vàng cánh gián',
          type: 'Đực'
        }
      : {
          id: '5346',
          category: 'Bò lai Sind',
          weight: '200',
          color: 'Vàng cánh gián',
          type: 'Đực'
        }
  );

  const handleNext = () => {
    if (activeTab === 'details') {
      setActiveTab('scan');
    } else if (activeTab === 'scan') {
      // Find the animal based on QR code if it exists
      const foundAnimal = animalData.find((animal) => animal.id === qrCode);
      if (foundAnimal) {
        setSelectedAnimal({
          id: foundAnimal.id,
          category: foundAnimal.category,
          weight: foundAnimal.weight.replace(' kg', ''),
          color: 'Vàng cánh gián', // This could be fetched from another API
          type: 'Đực' // This could be fetched from another API
        });
      }
      setActiveTab('confirm');
    }
  };

  const handleBack = () => {
    if (activeTab === 'confirm') {
      setActiveTab('scan');
    } else if (activeTab === 'scan') {
      setActiveTab('details');
    }
  };

  const handleConfirm = () => {
    toast({
      title: 'Xác nhận thành công',
      description: 'Vật nuôi đã được xác nhận'
    });
    setOpen(false);
    setActiveTab('details');
  };

  return (
    <>
      <Button onClick={() => setOpen(true)}>Xem chi tiết</Button>

      <Dialog open={open} onOpenChange={setOpen}>
        <DialogContent className="sm:max-w-[700px]">
          <Tabs
            value={activeTab}
            onValueChange={setActiveTab}
            className="w-full"
          >
            <TabsContent value="details">
              <DialogHeader>
                <DialogTitle>Thông tin khách hàng</DialogTitle>
              </DialogHeader>

              <div className="grid grid-cols-2 gap-4 py-4">
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="font-medium">Tên khách hàng:</span>
                    <span>{customerInfo.name}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="font-medium">Địa chỉ:</span>
                    <span>{customerInfo.address}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="font-medium">Ghi chú:</span>
                    <span>{customerInfo.note}</span>
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="font-medium">Số lượng:</span>
                    <span>{customerInfo.quantity}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="font-medium">Đã nhận:</span>
                    <span>{customerInfo.received}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="font-medium">Trạng thái:</span>
                    <span>{customerInfo.status}</span>
                  </div>
                </div>
              </div>

              <div className="mt-4">
                <h3 className="mb-2 font-medium">Vật nuôi đã nhận</h3>
                <div className="overflow-hidden rounded-md border">
                  <table className="w-full">
                    <thead className="bg-muted">
                      <tr>
                        <th className="p-2 text-left">Mã thẻ tai</th>
                        <th className="p-2 text-left">Danh mục hàng hóa</th>
                        <th className="p-2 text-left">Trọng lượng xuất</th>
                        <th className="p-2 text-left">Ngày chọn</th>
                        <th className="p-2 text-left">Ngày bán giao</th>
                        <th className="p-2 text-left">Ngày hết bảo hành</th>
                        <th className="p-2 text-left">Đối</th>
                      </tr>
                    </thead>
                    <tbody>
                      {animalData.map((animal) => (
                        <tr key={animal.id} className="border-t">
                          <td className="p-2">{animal.id}</td>
                          <td className="p-2">{animal.category}</td>
                          <td className="p-2">{animal.weight}</td>
                          <td className="p-2">{animal.receiveDate}</td>
                          <td className="p-2">{animal.sellDate}</td>
                          <td className="p-2">{animal.warrantyDate}</td>
                          <td className="p-2">{animal.actions}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>

              <DialogFooter className="mt-6">
                <Button onClick={handleNext}>Thêm mới</Button>
              </DialogFooter>
            </TabsContent>

            <TabsContent value="scan">
              <DialogHeader>
                <DialogTitle>Quét QR nhận vật nuôi</DialogTitle>
              </DialogHeader>

              <div className="flex flex-col items-center space-y-4 py-6">
                <div className="flex h-[200px] w-[200px] items-center justify-center border">
                  <div className="text-4xl text-gray-300">
                    <img src="https://upload.wikimedia.org/wikipedia/commons/thumb/d/d0/QR_code_for_mobile_English_Wikipedia.svg/800px-QR_code_for_mobile_English_Wikipedia.svg.png" />
                  </div>
                </div>
                <p className="text-center text-sm">
                  Nhập mã thẻ tai hoặc thẻ nhận vật nuôi
                </p>
                <Input
                  placeholder="Nhập mã thẻ tai"
                  value={qrCode}
                  onChange={(e) => setQrCode(e.target.value)}
                  className="max-w-[300px]"
                />
              </div>

              <DialogFooter className="flex justify-between">
                <Button variant="outline" onClick={handleBack}>
                  Hủy
                </Button>
                <Button onClick={handleNext}>Tiếp tục</Button>
              </DialogFooter>
            </TabsContent>

            <TabsContent value="confirm">
              <DialogHeader>
                <DialogTitle>Xác nhận chọn vật nuôi</DialogTitle>
              </DialogHeader>

              <div className="grid grid-cols-2 gap-4 py-6">
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="font-medium">Mã thẻ tai:</span>
                    <span>{selectedAnimal.id}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="font-medium">Giống:</span>
                    <span>{selectedAnimal.category}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="font-medium">Cân nặng (kg):</span>
                    <span>{selectedAnimal.weight}</span>
                  </div>
                </div>
                <div className="space-y-2">
                  <div className="flex justify-between">
                    <span className="font-medium">Màu lông:</span>
                    <span>{selectedAnimal.color}</span>
                  </div>
                  <div className="flex justify-between">
                    <span className="font-medium">Giới tính:</span>
                    <span>{selectedAnimal.type}</span>
                  </div>
                </div>
              </div>

              <DialogFooter className="flex justify-between">
                <Button variant="outline" onClick={handleBack}>
                  Hủy
                </Button>
                <Button onClick={handleConfirm}>Xác nhận</Button>
              </DialogFooter>
            </TabsContent>
          </Tabs>
        </DialogContent>
      </Dialog>
    </>
  );
};
