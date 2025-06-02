'use client';

import { useState } from 'react';
import {
  useAddPin,
  useGetListOverdue,
  useGetListPinned,
  useListNeardue,
  useRemovePin
} from '@/queries/pin.query';
import { X, Pin, Plus, Minus } from 'lucide-react';
import { toast } from '@/components/ui/use-toast';

export function PinPage() {
  const [nearDueCount, setNearDueCount] = useState(5);

  const { data: pinnedData } = useGetListPinned();
  const { data: overdueData } = useGetListOverdue();
  const { data: listNeardueData } = useListNeardue(nearDueCount);
  const { mutateAsync: addPin } = useAddPin();
  const { mutateAsync: removePin } = useRemovePin();

  // Extract items and totals from API responses
  const pinned = pinnedData?.items || [];
  const overdue = overdueData?.items || [];
  const listNeardue = listNeardueData?.items || [];

  const increaseNearDueCount = () => setNearDueCount((prev) => prev + 1);
  const decreaseNearDueCount = () =>
    setNearDueCount((prev) => (prev > 1 ? prev - 1 : 1));

  const formatDate = (dateString) => {
    if (!dateString) return '';
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric'
    });
  };

  const handleAddPin = async (batchImportId) => {
    try {
      const [err] = await addPin(batchImportId);
      if (err) {
        console.error('Error pinning batch import:', err);
        toast({
          title: 'Ghim thất bại',
          description: err.data.data,
          variant: 'destructive'
        });
      } else {
        toast({
          title: 'Ghim thành công',
          description: 'Lô nhập đã được ghim thành công',
          variant: 'success'
        });
      }
    } catch (error) {
      console.error('Error in pin operation:', error);
    }
  };

  const handleRemovePin = async (batchImportId) => {
    try {
      const [err] = await removePin(batchImportId);
      if (err) {
        toast({
          title: 'Ghim thất bại',
          description: err.data.data,
          variant: 'destructive'
        });
      } else {
        toast({
          title: 'Ghim thành công',
          description: 'Lô nhập đã được bỏ ghim thành công',
          variant: 'success'
        });
      }
    } catch (error) {
      console.error('Error in unpin operation:', error);
    }
  };

  return (
    <div className="bg-white p-4">
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        {/* Ghim Section */}
        <div className="border border-gray-300 p-4">
          <h2 className="mb-4 font-medium">Ghim ({pinnedData?.total || 0})</h2>
          <div className="grid gap-4">
            {pinned.length > 0 ? (
              pinned.map((item) => (
                <div key={item.id} className="relative bg-blue-200 p-4">
                  <div className="absolute right-2 top-2 flex space-x-1">
                    <button
                      className="rounded-full bg-white p-1"
                      onClick={() => handleRemovePin(item.id)}
                    >
                      <X size={16} />
                    </button>
                  </div>
                  <div className="text-xs">
                    Ngày dự kiến: {formatDate(item.batchImportCompletedDate)}
                  </div>
                  <div className="font-bold">{item.batchImportName}</div>
                </div>
              ))
            ) : (
              <div className="flex h-40 items-center justify-center text-gray-500">
                Không có lô nào được ghim
              </div>
            )}
          </div>
        </div>

        {/* Quá Hạn Section */}
        <div className="border border-gray-300 p-4">
          <h2 className="mb-4 font-medium">
            Quá Hạn ({overdueData?.total || 0})
          </h2>
          <div className="grid max-h-[500px] gap-4 overflow-y-auto">
            {overdue.length > 0 ? (
              overdue.map((item) => (
                <div
                  key={item.batchImportId}
                  className="relative bg-red-300 p-4"
                >
                  <div className="absolute right-2 top-2 flex space-x-1">
                    <button
                      className="rounded-full bg-white p-1"
                      onClick={() => handleAddPin(item.batchImportId)}
                    >
                      <Pin size={16} />
                    </button>
                  </div>
                  <div className="text-xs">
                    Ngày dự kiến: {formatDate(item.batchImportCompletedDate)}
                  </div>
                  <div className="font-bold">{item.batchImportName}</div>
                  <div className="text-sm">{item.dayOver}</div>
                </div>
              ))
            ) : (
              <div className="flex h-40 items-center justify-center text-gray-500">
                Không có lô nào quá hạn
              </div>
            )}
          </div>
        </div>

        {/* Đang Bị Thiếu Vật Section */}
        <div className="border border-gray-300 p-4">
          <div className="border border-gray-300 p-4">
            <div className="mb-4 flex items-center justify-between">
              <h2 className="font-medium">
                Sắp Quá Hạn ({listNeardueData?.total || 0})
              </h2>
              <div className="flex items-center">
                <button
                  className="border border-gray-300 px-2"
                  onClick={decreaseNearDueCount}
                >
                  <Minus size={16} />
                </button>
                <span className="px-2">{nearDueCount}</span>
                <button
                  className="border border-gray-300 px-2"
                  onClick={increaseNearDueCount}
                >
                  <Plus size={16} />
                </button>
              </div>
            </div>
            <div className="grid grid-cols-1 gap-4 md:grid-cols-2 lg:grid-cols-3">
              {listNeardue.length > 0 ? (
                listNeardue.map((item) => (
                  <div
                    key={item.batchImportId}
                    className="relative bg-yellow-200 p-4"
                  >
                    <div className="absolute right-2 top-2 flex space-x-1">
                      <button
                        className="rounded-full bg-white p-1"
                        onClick={() => handleAddPin(item.batchImportId)}
                      >
                        <Pin size={16} />
                      </button>
                    </div>
                    <div className="text-xs">
                      Ngày dự kiến: {formatDate(item.batchImportCompletedDate)}
                    </div>
                    <div className="font-bold">{item.batchImportName}</div>
                    <div className="text-sm">{item.dayleft}</div>
                  </div>
                ))
              ) : (
                <div className="col-span-3 flex h-40 items-center justify-center text-gray-500">
                  Không có lô nào sắp quá hạn
                </div>
              )}
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
