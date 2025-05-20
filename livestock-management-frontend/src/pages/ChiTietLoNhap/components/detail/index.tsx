'use client';

import { Skeleton } from '@/components/ui/skeleton';
import BatchInfoCard from './batch-info';
import { useParams } from 'react-router-dom';
import { useGetLoNhapById } from '@/queries/lo-nhap.query';

export default function ChiTiet() {
  const { id } = useParams<{ id: string }>();
  const { data, isLoading, error } = useGetLoNhapById(String(id));

  if (isLoading) {
    return <BatchInfoSkeleton />;
  }

  if (error) {
    return (
      <div className="rounded-lg border border-red-200 bg-red-50 p-6">
        <h3 className="text-lg font-medium text-red-800">Đã xảy ra lỗi</h3>
        <p className="text-red-600">{(error as Error).message}</p>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="rounded-lg border border-yellow-200 bg-yellow-50 p-6">
        <h3 className="text-lg font-medium text-yellow-800">
          Không tìm thấy dữ liệu
        </h3>
        <p className="text-yellow-600">
          Không thể tìm thấy thông tin lô nhập với ID: {id}
        </p>
      </div>
    );
  }

  return <BatchInfoCard batchInfo={data} />;
}

function BatchInfoSkeleton() {
  return (
    <div className="w-full rounded-lg border shadow-md">
      <div className="flex flex-row items-center justify-between border-b bg-gray-50 p-4">
        <Skeleton className="h-8 w-40" />
        <div className="flex gap-2">
          <Skeleton className="h-9 w-24" />
          <Skeleton className="h-9 w-28" />
        </div>
      </div>
      <div className="p-6">
        <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="space-y-2">
              <Skeleton className="h-4 w-32" />
              <Skeleton className="h-6 w-full" />
            </div>
          ))}
        </div>
      </div>
      <div className="flex justify-end border-t bg-gray-50 p-4">
        <Skeleton className="h-9 w-24" />
      </div>
    </div>
  );
}
