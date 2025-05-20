'use client';

import { Badge } from '@/components/ui/badge';
import {
  useChinhSuaGoiThau,
  useGetThongTinGoiThau
} from '@/queries/admin.query';
import { useParams } from 'react-router-dom';
import { format } from 'date-fns';
import { Skeleton } from '@/components/ui/skeleton';
import { BiddingEditSimple } from './binding-edit-simple';
import { toast } from '@/components/ui/use-toast';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import {
  Clipboard,
  Calendar,
  User,
  FileText,
  Package,
  Settings,
  Info,
  Clock
} from 'lucide-react';
import { Separator } from '@/components/ui/separator';

export function BiddingView() {
  const { id } = useParams();
  const {
    data: serverData,
    isPending,
    refetch
  } = useGetThongTinGoiThau(String(id));
  const { mutateAsync: updateGoiThau } = useChinhSuaGoiThau();

  const formatDate = (dateString: string | null) => {
    if (!dateString) return 'N/A';
    try {
      return format(new Date(dateString), 'dd-MM-yyyy');
    } catch (error) {
      return 'Invalid date';
    }
  };

  const getStatusColor = (status: string) => {
    switch (status) {
      case 'ĐANG_ĐẤU_THẦU':
        return 'bg-blue-100 text-blue-800 border-blue-200';
      case 'ĐÃ_ĐÓNG_THẦU':
        return 'bg-gray-100 text-gray-800 border-gray-200';
      case 'ĐÃ_HỦY':
        return 'bg-red-100 text-red-800 border-red-200';
      case 'ĐANG_XÉT_DUYỆT':
        return 'bg-yellow-100 text-yellow-800 border-yellow-200';
      case 'ĐÃ_HOÀN_THÀNH':
        return 'bg-green-100 text-green-800 border-green-200';
      default:
        return 'bg-blue-100 text-blue-800 border-blue-200';
    }
  };

  const formatStatus = (status: string) => {
    return status.replace(/_/g, ' ');
  };

  const handleUpdateBidding = async (formData: any) => {
    try {
      console.log('Submitting data:', {
        id: String(id),
        ...formData
      });

      await updateGoiThau({
        id: String(id),
        ...formData
      });

      toast({
        title: 'Cập nhật thành công',
        description: 'Thông tin gói thầu đã được cập nhật',
        variant: 'success'
      });
      refetch();
    } catch (error) {
      console.error('Error updating bidding:', error);
      toast({
        title: 'Cập nhật thất bại',
        description: 'Đã xảy ra lỗi khi cập nhật gói thầu',
        variant: 'destructive'
      });
    }
  };

  if (isPending) {
    return (
      <Card className="max-w-8xl mx-auto w-full">
        <CardHeader className="pb-0">
          <Skeleton className="h-8 w-64" />
        </CardHeader>
        <CardContent className="pt-6">
          <div className="grid gap-6 md:grid-cols-2">
            <div className="space-y-4">
              <Skeleton className="h-6 w-1/3" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-2/3" />
              <Skeleton className="h-4 w-1/2" />
              <Skeleton className="h-4 w-2/3" />
            </div>
            <div className="space-y-4">
              <Skeleton className="h-6 w-1/3" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-full" />
              <Skeleton className="h-4 w-2/3" />
              <Skeleton className="h-4 w-1/2" />
              <Skeleton className="h-4 w-2/3" />
            </div>
          </div>
        </CardContent>
      </Card>
    );
  }

  if (!serverData || !serverData.data) {
    return (
      <Card className="max-w-8xl mx-auto w-full">
        <CardContent className="pt-6">
          <div className="flex flex-col items-center justify-center py-12 text-center">
            <Info className="h-12 w-12 text-gray-300" />
            <h3 className="mt-4 text-lg font-medium text-gray-900">
              Không tìm thấy thông tin
            </h3>
            <p className="mt-2 text-sm text-gray-500">
              Không tìm thấy thông tin gói thầu với ID đã cung cấp
            </p>
          </div>
        </CardContent>
      </Card>
    );
  }

  const data = serverData.data;
  const hasDetails = data.details && data.details.length > 0;

  return (
    <Card className="max-w-8xl mx-auto w-full shadow-md">
      <CardHeader className="flex flex-row items-center justify-between bg-gradient-to-r from-purple-50 to-indigo-50 px-6 py-4">
        <div className="flex items-center gap-2">
          <FileText className="h-5 w-5 text-indigo-600" />
          <h1 className="text-xl font-semibold text-indigo-900">
            Chi tiết gói thầu
          </h1>
        </div>
        <BiddingEditSimple biddingData={data} onSubmit={handleUpdateBidding} />
      </CardHeader>

      <CardContent className="p-0">
        <div className="grid md:grid-cols-2">
          {/* Left Column - Package Information */}
          <div className="border-b border-gray-200 p-6 md:border-b-0 md:border-r">
            <div className="mb-6 flex items-center gap-2">
              <Package className="h-5 w-5 text-indigo-600" />
              <h2 className="text-lg font-medium text-indigo-900">
                Thông tin gói thầu
              </h2>
            </div>

            <div className="rounded-lg border border-gray-100 bg-white p-5 shadow-sm">
              <div className="space-y-4">
                <div className="grid grid-cols-1 gap-1">
                  <div className="text-sm font-medium text-gray-500">
                    Tên gói thầu
                  </div>
                  <div className="text-base font-medium text-gray-900">
                    {data.name}
                  </div>
                </div>

                <Separator className="my-2" />

                <div className="grid grid-cols-1 gap-1">
                  <div className="text-sm font-medium text-gray-500">
                    Bên mời thầu
                  </div>
                  <div className="text-base text-gray-900">{data.owner}</div>
                </div>

                <Separator className="my-2" />

                <div className="grid grid-cols-1 gap-1">
                  <div className="text-sm font-medium text-gray-500">
                    Mã gói thầu
                  </div>
                  <div className="flex items-center gap-2 text-base text-gray-900">
                    <span>{data.code}</span>
                    <button className="rounded-full p-1 text-gray-400 hover:bg-gray-100 hover:text-gray-600">
                      <Clipboard className="h-4 w-4" />
                    </button>
                  </div>
                </div>

                <Separator className="my-2" />

                <div className="grid grid-cols-2 gap-4">
                  <div className="grid grid-cols-1 gap-1">
                    <div className="text-sm font-medium text-gray-500">
                      Thời gian thực hiện
                    </div>
                    <div className="flex items-center gap-1 text-base text-gray-900">
                      <span>{data.expiredDuration}</span>
                      <span className="text-sm text-gray-500">ngày</span>
                    </div>
                  </div>

                  <div className="grid grid-cols-1 gap-1">
                    <div className="text-sm font-medium text-gray-500">
                      Trạng thái
                    </div>
                    <div>
                      <Badge
                        className={`px-3 py-1 font-medium ${getStatusColor(data.status)}`}
                      >
                        {formatStatus(data.status)}
                      </Badge>
                    </div>
                  </div>
                </div>

                <Separator className="my-2" />

                <div className="grid grid-cols-2 gap-4">
                  <div className="grid grid-cols-1 gap-1">
                    <div className="flex items-center gap-1 text-sm font-medium text-gray-500">
                      <Calendar className="h-3.5 w-3.5" />
                      <span>Ngày tạo</span>
                    </div>
                    <div className="text-base text-gray-900">
                      {formatDate(data.createdAt)}
                    </div>
                  </div>

                  <div className="grid grid-cols-1 gap-1">
                    <div className="flex items-center gap-1 text-sm font-medium text-gray-500">
                      <Clock className="h-3.5 w-3.5" />
                      <span>Ngày hết hạn</span>
                    </div>
                    <div className="text-base text-gray-900">
                      {formatDate(data.expirationDate)}
                    </div>
                  </div>

                  <div className="grid grid-cols-1 gap-1">
                    <div className="flex items-center gap-1 text-sm font-medium text-gray-500">
                      <Clock className="h-3.5 w-3.5" />
                      <span>Ngày hoàn thành</span>
                    </div>
                    <div className="text-base text-gray-900">
                      {formatDate(data.completionDate)}
                    </div>
                  </div>
                </div>

                <Separator className="my-2" />

                <div className="grid grid-cols-2 gap-4">
                  <div className="grid grid-cols-1 gap-1">
                    <div className="flex items-center gap-1 text-sm font-medium text-gray-500">
                      <span>Tổng lượng xuất</span>
                    </div>
                    <div className="text-base text-gray-900">
                      {data.totalExported}
                    </div>
                  </div>

                  <div className="grid grid-cols-1 gap-1">
                    <div className="flex items-center gap-1 text-sm font-medium text-gray-500">
                      <span>Tổng yêu cầu</span>
                    </div>
                    <div className="text-base text-gray-900">
                      {data.totalRequired}
                    </div>
                  </div>
                </div>

                <Separator className="my-2" />

                <div className="grid grid-cols-1 gap-1">
                  <div className="flex items-center gap-1 text-sm font-medium text-gray-500">
                    <User className="h-3.5 w-3.5" />
                    <span>Người tạo</span>
                  </div>
                  <div className="text-base text-gray-900">
                    {data.createdBy}
                  </div>
                </div>

                <Separator className="my-2" />

                <div className="grid grid-cols-1 gap-1">
                  <div className="text-sm font-medium text-gray-500">Mô tả</div>
                  <div className="text-base text-gray-900">
                    {data.description || 'Không có mô tả'}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Right Column - Technical Requirements */}
          <div className="p-6">
            <div className="mb-6 flex items-center gap-2">
              <Settings className="h-5 w-5 text-indigo-600" />
              <h2 className="text-lg font-medium text-indigo-900">
                Yêu cầu kỹ thuật
              </h2>
            </div>

            {hasDetails ? (
              <div className="space-y-6">
                {data.details.map((detail, index) => (
                  <div
                    key={index}
                    className="rounded-lg border border-gray-100 bg-white p-5 shadow-sm transition-all duration-200 hover:shadow-md"
                  >
                    <div className="mb-4 flex items-center justify-between">
                      <Badge
                        variant="outline"
                        className="border-indigo-200 bg-indigo-50 px-3 py-1 text-indigo-700"
                      >
                        Yêu cầu #{index + 1}
                      </Badge>
                    </div>

                    <div className="space-y-4">
                      <div className="grid grid-cols-1 gap-1">
                        <div className="text-sm font-medium text-gray-500">
                          Loài vật
                        </div>
                        <div className="text-base font-medium text-gray-900">
                          {detail.speciesName}
                        </div>
                      </div>

                      <div className="grid grid-cols-2 gap-4">
                        <div className="grid grid-cols-1 gap-1">
                          <div className="text-sm font-medium text-gray-500">
                            Biểu cân (kg)
                          </div>
                          <div className="text-base text-gray-900">
                            {detail.requiredWeightMin} -{' '}
                            {detail.requiredWeightMax}
                          </div>
                        </div>

                        <div className="grid grid-cols-1 gap-1">
                          <div className="text-sm font-medium text-gray-500">
                            Số lượng (con)
                          </div>
                          <div className="text-base text-gray-900">
                            {detail.requiredQuantity}
                          </div>
                        </div>
                      </div>

                      <div className="grid grid-cols-2 gap-4">
                        <div className="grid grid-cols-1 gap-1">
                          <div className="text-sm font-medium text-gray-500">
                            Tuổi (tháng)
                          </div>
                          <div className="text-base text-gray-900">
                            {detail.requiredAgeMin} - {detail.requiredAgeMax}
                          </div>
                        </div>

                        <div className="grid grid-cols-1 gap-1">
                          <div className="text-sm font-medium text-gray-500">
                            Thời gian bảo hành (ngày)
                          </div>
                          <div className="text-base text-gray-900">
                            {detail.requiredInsurance}
                          </div>
                        </div>
                      </div>

                      <div className="grid grid-cols-1 gap-1">
                        <div className="text-sm font-medium text-gray-500">
                          Yêu cầu khác
                        </div>
                        <div className="text-base text-gray-900">
                          {detail.description || 'Không có yêu cầu khác'}
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center rounded-lg border border-dashed border-gray-200 bg-gray-50 py-12 text-center">
                <Info className="h-10 w-10 text-gray-300" />
                <p className="mt-2 text-sm text-gray-500">
                  Không có thông tin chi tiết
                </p>
              </div>
            )}
          </div>
        </div>
      </CardContent>
    </Card>
  );
}
