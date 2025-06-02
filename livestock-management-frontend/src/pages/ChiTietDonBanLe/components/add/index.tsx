'use client';

import { useState } from 'react';
import { ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog';

export default function Add() {
  const [showWarrantyDetails, setShowWarrantyDetails] = useState(false);

  return (
    <div className="container mx-auto max-w-6xl p-4">
      <h1 className="mb-6 border-b pb-2 text-xl font-semibold">
        Trạng thái đơn
      </h1>

      {/* Order Status Flow */}
      <div className="mb-10 flex flex-wrap items-center justify-between rounded-lg bg-gray-50 p-6 shadow-sm">
        <StatusStep title="Mới" isActive={true} date="5/5/22" />
        <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
        <StatusStep title="Đang chuẩn bị" isActive={false} />
        <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
        <StatusStep title="Chờ bàn giao" isActive={false} />
        <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
        <StatusStep title="Đang bàn giao" isActive={false} />
        <ArrowRight className="mx-1 hidden h-8 w-8 text-gray-400 sm:block" />
        <StatusStep title="Hoàn Thành" isActive={false} />
      </div>

      {/* Main Content */}
      <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
        {/* Customer Information */}
        <div className="overflow-hidden rounded-lg border border-gray-200 shadow-sm">
          <div className="border-b border-gray-200 bg-gray-50 p-3 font-medium text-gray-700">
            Thông tin khách hàng
          </div>
          <div className="p-5">
            <div className="grid grid-cols-2 gap-y-5">
              <div className="text-sm text-gray-600">Tên khách hàng:</div>
              <div className="text-sm font-medium">Nguyen Van B</div>

              <div className="text-sm text-gray-600">Số điện thoại:</div>
              <div className="text-sm font-medium">0987654321</div>

              <div className="text-sm text-gray-600">Địa chỉ:</div>
              <div className="text-sm font-medium">
                63, Nguyễn Trãi Hai Bà Trưng Hà Nội
              </div>

              <div className="text-sm text-gray-600">Email:</div>
              <div className="text-sm font-medium">N/A</div>

              <div className="text-sm text-gray-600">
                Tổng số lượng vật nuôi:
              </div>
              <div className="text-sm font-medium">100</div>

              <div className="text-sm text-gray-600">Đã nhận:</div>
              <div className="text-sm font-medium">10</div>
            </div>
          </div>
        </div>

        {/* Requirements */}
        <div className="overflow-hidden rounded-lg border border-gray-200 shadow-sm">
          <div className="border-b border-gray-200 bg-gray-50 p-3 font-medium text-gray-700">
            Yêu cầu về loại vật
          </div>
          <div className="p-5">
            <div className="mb-4">
              <div className="mb-3 border-b pb-2 font-medium text-gray-800">
                Yêu cầu 1
              </div>
              <div className="grid grid-cols-2 gap-y-5">
                <div className="text-sm text-gray-600">Loại vật:</div>
                <div className="text-sm font-medium">Bò lại Sind</div>

                <div className="text-sm text-gray-600">Cân nặng:</div>
                <div className="text-sm font-medium">100 - 200 kg</div>

                <div className="text-sm text-gray-600">Số lượng:</div>
                <div className="text-sm font-medium">10 con</div>

                <div className="text-sm text-gray-600">
                  Bảo hành theo bệnh (0):
                </div>
                <div className="text-sm italic text-gray-500">
                  *tính từ thời điểm vật nuôi xuất chuồng
                </div>

                <div className="text-sm text-gray-600">Yêu cầu khác:</div>
                <div className="text-sm font-medium">N/A</div>
              </div>
              <div className="mt-6 flex justify-end">
                <Button
                  variant="outline"
                  className="rounded border-gray-800 px-6 py-2 text-gray-800 hover:bg-gray-100"
                  onClick={() => setShowWarrantyDetails(true)}
                >
                  Chi Tiết
                </Button>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Warranty Details Modal */}
      <WarrantyDetailsModal
        open={showWarrantyDetails}
        onClose={() => setShowWarrantyDetails(false)}
      />
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
        className={`flex h-20 w-20 items-center justify-center rounded-full text-sm font-medium transition-all
          ${isActive ? 'bg-green-400 text-white shadow-md' : 'border-2 border-gray-300 text-gray-500'}`}
      >
        {title}
      </div>
      {date && <div className="mt-2 text-xs text-gray-600">{date}</div>}
      {!date && <div className="invisible mt-2 text-xs">Date</div>}
    </div>
  );
}

function WarrantyDetailsModal({
  open,
  onClose
}: {
  open: boolean;
  onClose: () => void;
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
                  Tên Bệnh
                </th>
                <th className="whitespace-nowrap border border-gray-800 bg-gray-100 p-2 text-center">
                  Thời gian bảo hành
                  <br />
                  (ngày)
                </th>
              </tr>
            </thead>
            <tbody>
              <tr className="border border-gray-800">
                <td className="border-r border-gray-800 p-3">
                  Lở mồm long móng
                </td>
                <td className="p-2 text-center">21</td>
              </tr>
              <tr className="border border-gray-800">
                <td className="border-r border-gray-800 p-3">
                  Viêm đa nội cực
                </td>
                <td className="p-2 text-center">21</td>
              </tr>
              <tr className="border border-gray-800">
                <td className="border-r border-gray-800 p-3">
                  Nhiễm ký sinh trùng
                </td>
                <td className="p-2 text-center">21</td>
              </tr>
            </tbody>
          </table>

          <div className="mt-8 flex justify-end">
            <Button
              className="bg-gray-800 px-6 text-white hover:bg-gray-700"
              onClick={onClose}
            >
              Xác nhận
            </Button>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
