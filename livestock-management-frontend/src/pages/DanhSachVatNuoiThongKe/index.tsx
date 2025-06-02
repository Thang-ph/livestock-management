'use client';

import { useState, useRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

import { Checkbox } from '@/components/ui/checkbox';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table';
import {
  AlertTriangle,
  Download,
  Upload,
  X,
  Loader2,
  ChevronLeft,
  ChevronRight
} from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import BasePages from '@/components/shared/base-pages';
import {
  useCreateLiveStockRecord,
  useDownloadQRCode,
  useGetAllLiveStock,
  useGetListLiveStockSumary,
  useGetTotalEmptyRecords,
  useMarkLiveStockAsDead,
  useMarkLiveStockRecovered,
  useTaiMauGhiNhanTrangThai,
  useExportLivestockReport
} from '@/queries/admin.query';
import { toast } from '@/components/ui/use-toast';
import __helpers from '@/helpers';
import axios from 'axios';

// API base URL
const API_BASE_URL =
  'https://api.hoptacxaluavang.site/api/livestock-management';

// File Upload Hook
const useFileUpload = () => {
  const [isUploading, setIsUploading] = useState(false);

  const uploadStatusFile = async (file) => {
    setIsUploading(true);
    try {
      const formData = new FormData();
      formData.append('requestedBy', __helpers.getUserId());
      formData.append('file', file);

      const response = await axios.post(
        `${API_BASE_URL}/import-record-livestock-status-file`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        }
      );

      return [null, response.data];
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message || error.message || 'Upload failed';
      return [{ message: errorMessage }, null];
    } finally {
      setIsUploading(false);
    }
  };

  const uploadInformationFile = async (file) => {
    setIsUploading(true);
    try {
      const formData = new FormData();
      formData.append('requestedBy', __helpers.getUserId());
      formData.append('file', file);

      const response = await axios.post(
        `${API_BASE_URL}/import-record-livestock-information-file`,
        formData,
        {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        }
      );

      return [null, response.data];
    } catch (error: any) {
      const errorMessage =
        error.response?.data?.message || error.message || 'Upload failed';
      return [{ message: errorMessage }, null];
    } finally {
      setIsUploading(false);
    }
  };

  return {
    uploadStatusFile,
    uploadInformationFile,
    isUploading
  };
};

// Loading Skeleton Components
const TableRowSkeleton = () => (
  <TableRow>
    <TableCell>
      <Skeleton className="h-4 w-4" />
    </TableCell>
    <TableCell>
      <Skeleton className="mx-auto h-4 w-20" />
    </TableCell>
    <TableCell>
      <Skeleton className="mx-auto h-4 w-16" />
    </TableCell>
    <TableCell>
      <Skeleton className="mx-auto h-4 w-12" />
    </TableCell>
    <TableCell>
      <Skeleton className="mx-auto h-4 w-12" />
    </TableCell>
    <TableCell>
      <Skeleton className="mx-auto h-4 w-16" />
    </TableCell>
    <TableCell>
      <Skeleton className="mx-auto h-4 w-20" />
    </TableCell>
    <TableCell>
      <Skeleton className="mx-auto h-4 w-16" />
    </TableCell>
    <TableCell>
      <div className="flex justify-center gap-1">
        <Skeleton className="h-6 w-12" />
        <Skeleton className="h-6 w-16" />
        <Skeleton className="h-6 w-14" />
      </div>
    </TableCell>
  </TableRow>
);

const StatsSkeleton = () => (
  <Card className="w-80">
    <CardHeader className="pb-3">
      <CardTitle className="border-b pb-2 text-sm font-medium">
        <Skeleton className="h-4 w-48" />
      </CardTitle>
    </CardHeader>
    <CardContent className="space-y-2">
      {[...Array(4)].map((_, i) => (
        <div key={i} className="flex items-center justify-between">
          <Skeleton className="h-4 w-20" />
          <Skeleton className="h-4 w-8" />
          <Skeleton className="h-4 w-12" />
        </div>
      ))}
      <div className="flex items-center justify-between border-t pt-2 font-medium">
        <Skeleton className="h-4 w-12" />
        <Skeleton className="h-4 w-8" />
        <Skeleton className="h-4 w-12" />
      </div>
      <div className="flex gap-2 pt-2">
        <Skeleton className="h-8 w-40" />
        <Skeleton className="h-8 w-24" />
      </div>
    </CardContent>
  </Card>
);

// Pagination Component
const Pagination = ({
  currentPage,
  totalPages,
  onPageChange,
  totalItems,
  itemsPerPage
}) => {
  const startItem = (currentPage - 1) * itemsPerPage + 1;
  const endItem = Math.min(currentPage * itemsPerPage, totalItems);

  return (
    <div className="flex items-center justify-between px-2 py-4">
      <div className="text-sm text-gray-700">
        Hiển thị {startItem} đến {endItem} của {totalItems} kết quả
      </div>
      <div className="flex items-center space-x-2">
        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(currentPage - 1)}
          disabled={currentPage <= 1}
        >
          <ChevronLeft className="h-4 w-4" />
          Trước
        </Button>

        {/* Page numbers */}
        {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
          let pageNum;
          if (totalPages <= 5) {
            pageNum = i + 1;
          } else if (currentPage <= 3) {
            pageNum = i + 1;
          } else if (currentPage >= totalPages - 2) {
            pageNum = totalPages - 4 + i;
          } else {
            pageNum = currentPage - 2 + i;
          }

          return (
            <Button
              key={pageNum}
              variant={currentPage === pageNum ? 'default' : 'outline'}
              size="sm"
              onClick={() => onPageChange(pageNum)}
            >
              {pageNum}
            </Button>
          );
        })}

        <Button
          variant="outline"
          size="sm"
          onClick={() => onPageChange(currentPage + 1)}
          disabled={currentPage >= totalPages}
        >
          Sau
          <ChevronRight className="h-4 w-4" />
        </Button>
      </div>
    </div>
  );
};

// QR Code Modal Component with Loading
const QRCodeModal = ({ isOpen, onClose }) => {
  const [quantity, setQuantity] = useState(5);
  const [isCreating, setIsCreating] = useState(false);
  const [isDownloading, setIsDownloading] = useState(false);

  const { data: totalEmptyRecords, isLoading: isLoadingEmptyRecords } =
    useGetTotalEmptyRecords();
  const { mutateAsync: downloadQR } = useDownloadQRCode();
  const { mutateAsync: createLiveStockRecord } = useCreateLiveStockRecord();

  if (!isOpen) return null;

  const handleCancel = () => {
    if (!isCreating && !isDownloading) {
      onClose();
    }
  };

  const handleDownloadQR = async () => {
    setIsDownloading(true);
    try {
      const [err, data] = await downloadQR();
      if (err) {
        toast({
          title: 'Lỗi tải mã QR',
          description: err.message || 'Không thể tải mã QR',
          variant: 'destructive'
        });
        return;
      }
      if (data) {
        const link = document.createElement('a');
        link.href = data.data;
        link.target = '_blank';
        link.download = `qr_codes_${new Date().toISOString()}.zip`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
        toast({
          title: 'Tải mã QR thành công',
          description: 'Mã QR đã được tải xuống thành công.',
          variant: 'success'
        });
      }
    } finally {
      setIsDownloading(false);
    }
  };

  const handleCreateNew = async () => {
    if (quantity <= 0) {
      toast({
        title: 'Số lượng không hợp lệ',
        description: 'Vui lòng nhập số lượng mã QR lớn hơn 0.',
        variant: 'destructive'
      });
      return;
    }

    setIsCreating(true);
    try {
      const model = {
        quantity: quantity,
        requestedBy: __helpers.getUserId()
      };
      const [err, data] = await createLiveStockRecord(model);
      if (err) {
        toast({
          title: 'Lỗi tạo mã QR',
          description: err.message || 'Không thể tạo mã QR mới',
          variant: 'destructive'
        });
        return;
      }
      if (data) {
        toast({
          title: 'Tạo mã QR thành công',
          description: `Đã tạo ${quantity} mã QR mới.`,
          variant: 'success'
        });
        onClose();
      }
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50">
      <div className="mx-4 w-96 max-w-md rounded-lg bg-white p-6">
        <div className="mb-4 flex items-center justify-between">
          <h2 className="text-lg font-semibold">Tạo mới mã QR vật nuôi</h2>
          <Button
            variant="ghost"
            size="sm"
            onClick={onClose}
            className="h-6 w-6 p-0"
            disabled={isCreating || isDownloading}
          >
            <X className="h-4 w-4" />
          </Button>
        </div>

        <div className="space-y-4">
          <div>
            <Label className="text-sm">
              Số lượng mã còn tồn:{' '}
              {isLoadingEmptyRecords ? (
                <Skeleton className="inline-block h-4 w-8" />
              ) : (
                totalEmptyRecords || 0
              )}
            </Label>
          </div>

          <div className="space-y-2">
            <Label htmlFor="quantity" className="text-sm font-medium">
              Nhập số lượng mã cần tạo:
            </Label>
            <Input
              id="quantity"
              type="number"
              value={quantity}
              onChange={(e) => setQuantity(parseInt(e.target.value) || 0)}
              className="w-20"
              min="1"
              disabled={isCreating || isDownloading}
            />
          </div>

          <div className="flex gap-2 pt-4">
            <Button
              variant="outline"
              onClick={handleCancel}
              className="flex-1"
              disabled={isCreating || isDownloading}
            >
              Hủy bỏ
            </Button>
            <Button
              variant="outline"
              onClick={handleDownloadQR}
              className="flex-1"
              disabled={isCreating || isDownloading}
            >
              {isDownloading && (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              )}
              Tải file mã QR
            </Button>
            <Button
              onClick={handleCreateNew}
              className="flex-1 bg-blue-600 hover:bg-blue-700"
              disabled={isCreating || isDownloading}
            >
              {isCreating && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
              Tạo mới
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default function LivestockManagement() {
  const [selectedItems, setSelectedItems] = useState<string[]>([]);
  const [searchCode] = useState('');
  const [isQRModalOpen, setIsQRModalOpen] = useState(false);
  const [isDownloadingTemplate, setIsDownloadingTemplate] = useState(false);
  const [isExportingReport, setIsExportingReport] = useState(false);
  const [processingItems, setProcessingItems] = useState(new Set());
  const [processingBulkAction, setProcessingBulkAction] = useState(false);

  // File upload refs
  const statusFileInputRef = useRef<HTMLInputElement>(null);
  const informationFileInputRef = useRef<HTMLInputElement>(null);

  // Pagination states
  const [currentPage, setCurrentPage] = useState(1);
  const [itemsPerPage] = useState(10);

  // Hooks
  const { uploadStatusFile, uploadInformationFile, isUploading } =
    useFileUpload();
  const { data: livestockSumary, isLoading: isLoadingSummary } =
    useGetListLiveStockSumary();
  const { data: listLiveStock, isLoading: isLoadingLivestock } =
    useGetAllLiveStock();
  const { mutateAsync: taiMauGhiNhan } = useTaiMauGhiNhanTrangThai();
  const { mutateAsync: markLiveStockDead } = useMarkLiveStockAsDead();
  const { mutateAsync: markLiveStockRecovered } = useMarkLiveStockRecovered();
  const { mutateAsync: exportReport } = useExportLivestockReport();

  // Extract data from API responses
  const livestockData = listLiveStock?.items || [];
  const totalQuantity = livestockSumary?.totalLivestockQuantity || 0;
  const summaryItems = livestockSumary?.summaryByStatus?.items || [];

  // Filter livestock based on search
  const filteredLivestock = livestockData.filter(
    (item) =>
      searchCode === '' ||
      item.inspectionCode.toLowerCase().includes(searchCode.toLowerCase())
  );

  // Pagination calculations
  const totalPages = Math.ceil(filteredLivestock.length / itemsPerPage);
  const startIndex = (currentPage - 1) * itemsPerPage;
  const endIndex = startIndex + itemsPerPage;
  const currentPageData = filteredLivestock.slice(startIndex, endIndex);

  // File upload handlers
  const handleStatusFileUpload = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
      toast({
        title: 'File không hợp lệ',
        description: 'Vui lòng chọn file Excel (.xlsx hoặc .xls)',
        variant: 'destructive'
      });
      return;
    }

    const [err] = await uploadStatusFile(file);
    if (err) {
      toast({
        title: 'Lỗi tải file',
        description: err.message,
        variant: 'destructive'
      });
    } else {
      toast({
        title: 'Tải file thành công',
        description: 'File trạng thái đã được tải lên và xử lý thành công',
        variant: 'success'
      });
      // Refresh data after successful upload
      // You might want to call refetch functions here
    }

    // Reset file input
    event.target.value = '';
  };

  const handleInformationFileUpload = async (event) => {
    const file = event.target.files[0];
    if (!file) return;

    // Validate file type
    if (!file.name.endsWith('.xlsx') && !file.name.endsWith('.xls')) {
      toast({
        title: 'File không hợp lệ',
        description: 'Vui lòng chọn file Excel (.xlsx hoặc .xls)',
        variant: 'destructive'
      });
      return;
    }

    const [err] = await uploadInformationFile(file);
    if (err) {
      toast({
        title: 'Lỗi tải file',
        description: err.message,
        variant: 'destructive'
      });
    } else {
      toast({
        title: 'Tải file thành công',
        description: 'File thông tin đã được tải lên và xử lý thành công',
        variant: 'success'
      });
      // Refresh data after successful upload
      // You might want to call refetch functions here
    }

    // Reset file input
    event.target.value = '';
  };

  const handleSelectAll = (checked) => {
    if (checked) {
      setSelectedItems(currentPageData.map((item) => item.id));
    } else {
      setSelectedItems([]);
    }
  };

  const handleSelectItem = (itemId, checked) => {
    if (checked) {
      setSelectedItems([...selectedItems, itemId]);
    } else {
      setSelectedItems(selectedItems.filter((id) => id !== itemId));
    }
  };

  const handlePageChange = (page) => {
    setCurrentPage(page);
    setSelectedItems([]); // Clear selections when changing page
  };

  const handleDownloadTemplate = async () => {
    setIsDownloadingTemplate(true);
    try {
      const response = await taiMauGhiNhan();
      if (response) {
        window.open(response, '_blank');
      }
    } catch (error) {
      console.error('Error downloading template:', error);
      toast({
        title: 'Lỗi tải mẫu',
        description: 'Không thể tải mẫu ghi nhận trạng thái',
        variant: 'destructive'
      });
    } finally {
      setIsDownloadingTemplate(false);
    }
  };

  const handleExportReport = async () => {
    setIsExportingReport(true);
    try {
      const [err, data] = await exportReport();
      if (err) {
        toast({
          title: 'Lỗi xuất báo cáo',
          description: err.message || 'Không thể xuất báo cáo',
          variant: 'destructive'
        });
        return;
      }
      if (data) {
        const link = document.createElement('a');
        link.href = data.data;
        link.target = '_blank';
        link.download = `livestock_report_${new Date().toISOString()}.xlsx`;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);

        toast({
          title: 'Xuất báo cáo thành công',
          description: 'Báo cáo đã được tạo và tải xuống.',
          variant: 'success'
        });
      }
    } catch (error) {
      console.error('Error exporting report:', error);
      toast({
        title: 'Lỗi xuất báo cáo',
        description: 'Đã xảy ra lỗi khi xuất báo cáo',
        variant: 'destructive'
      });
    } finally {
      setIsExportingReport(false);
    }
  };

  const handleMarkAsRecovered = async (livestockId) => {
    setProcessingItems((prev) => new Set(prev).add(livestockId));
    try {
      const payload = {
        livestockIds: [livestockId],
        requestedBy: __helpers.getUserId()
      };
      const [err, data] = await markLiveStockRecovered(payload);
      if (err) {
        toast({
          title: 'Lỗi cập nhật',
          description: err.message || 'Không thể cập nhật trạng thái',
          variant: 'destructive'
        });
        return;
      }
      if (data) {
        toast({
          title: 'Đã cập nhật',
          description: 'Đã đánh dấu vật nuôi khỏi bệnh',
          variant: 'success'
        });
      }
    } catch (error) {
      toast({
        title: 'Lỗi cập nhật',
        description: 'Không thể cập nhật trạng thái',
        variant: 'destructive'
      });
    } finally {
      setProcessingItems((prev) => {
        const newSet = new Set(prev);
        newSet.delete(livestockId);
        return newSet;
      });
    }
  };

  const handleMarkAsDead = async (livestockId) => {
    setProcessingItems((prev) => new Set(prev).add(livestockId));
    try {
      const payload = {
        livestockIds: [livestockId],
        requestedBy: __helpers.getUserId()
      };
      const [err, data] = await markLiveStockDead(payload);
      if (err) {
        toast({
          title: 'Lỗi cập nhật',
          description: err.message || 'Không thể cập nhật trạng thái',
          variant: 'destructive'
        });
        return;
      }
      if (data) {
        toast({
          title: 'Đã cập nhật',
          description: 'Đã đánh dấu vật nuôi đã chết',
          variant: 'success'
        });
      }
    } catch (error) {
      toast({
        title: 'Lỗi cập nhật',
        description: 'Không thể cập nhật trạng thái',
        variant: 'destructive'
      });
    } finally {
      setProcessingItems((prev) => {
        const newSet = new Set(prev);
        newSet.delete(livestockId);
        return newSet;
      });
    }
  };

  // Bulk actions
  const handleBulkMarkAsRecovered = async () => {
    if (selectedItems.length === 0) return;

    setProcessingBulkAction(true);
    try {
      const payload = {
        livestockIds: selectedItems,
        requestedBy: __helpers.getUserId()
      };
      const [err, data] = await markLiveStockRecovered(payload);
      if (err) {
        toast({
          title: 'Lỗi cập nhật',
          description: err.message || 'Không thể cập nhật trạng thái',
          variant: 'destructive'
        });
        return;
      }
      if (data) {
        toast({
          title: 'Đã cập nhật',
          description: `Đã đánh dấu ${selectedItems.length} vật nuôi khỏi bệnh`,
          variant: 'success'
        });
        setSelectedItems([]);
      }
    } catch (error) {
      toast({
        title: 'Lỗi cập nhật',
        description: 'Không thể cập nhật trạng thái',
        variant: 'destructive'
      });
    } finally {
      setProcessingBulkAction(false);
    }
  };

  const handleBulkMarkAsDead = async () => {
    if (selectedItems.length === 0) return;

    setProcessingBulkAction(true);
    try {
      const payload = {
        livestockIds: selectedItems,
        requestedBy: __helpers.getUserId()
      };
      const [err, data] = await markLiveStockDead(payload);
      if (err) {
        toast({
          title: 'Lỗi cập nhật',
          description: err.message || 'Không thể cập nhật trạng thái',
          variant: 'destructive'
        });
        return;
      }
      if (data) {
        toast({
          title: 'Đã cập nhật',
          description: `Đã đánh dấu ${selectedItems.length} vật nuôi đã chết`,
          variant: 'success'
        });
        setSelectedItems([]);
      }
    } catch (error) {
      toast({
        title: 'Lỗi cập nhật',
        description: 'Không thể cập nhật trạng thái',
        variant: 'destructive'
      });
    } finally {
      setProcessingBulkAction(false);
    }
  };

  // const handleViewDetails = (livestockId) => {
  //   router.push(`/thong-tin-vat-nuoi/${livestockId}`);
  // };

  // Count missing information livestock
  const missingInfoCount = filteredLivestock.filter(
    (item) => item.isMissingInformation
  ).length;

  const isDataLoading = isLoadingSummary || isLoadingLivestock;

  return (
    <BasePages
      className="relative flex-1 space-y-4 overflow-y-auto px-6"
      breadcrumbs={[
        { title: 'Trang chủ', link: '/' },
        { title: 'Quản lý chăn nuôi', link: '/livestock-dashboard' },
        { title: 'Thông kê vật nuôi', link: '/thong-ke-dich-benh' }
      ]}
    >
      <h1 className="mb-4 text-2xl font-bold tracking-tight text-gray-900">
        Danh sách vật nuôi
      </h1>
      <Button onClick={() => setIsQRModalOpen(true)}>
        Tạo mới mã QR vật nuôi
      </Button>

      {/* Hidden file inputs */}
      <input
        type="file"
        ref={statusFileInputRef}
        onChange={handleStatusFileUpload}
        accept=".xlsx,.xls"
        style={{ display: 'none' }}
      />
      <input
        type="file"
        ref={informationFileInputRef}
        onChange={handleInformationFileUpload}
        accept=".xlsx,.xls"
        style={{ display: 'none' }}
      />

      {/* Statistics Card with Loading */}
      {isLoadingSummary ? (
        <StatsSkeleton />
      ) : (
        <Card className="w-80">
          <CardHeader className="pb-3">
            <CardTitle className="border-b pb-2 text-sm font-medium">
              Số lượng vật nuôi theo trạng thái
            </CardTitle>
          </CardHeader>
          <CardContent className="space-y-2">
            {summaryItems.map((item) => (
              <div
                key={item.status}
                className="flex items-center justify-between"
              >
                <span className="text-sm">{item.status}</span>
                <span className="text-sm">{item.quantitiy}</span>
                <span className="text-sm">{item.ratio}%</span>
              </div>
            ))}
            <div className="flex items-center justify-between border-t pt-2 font-medium">
              <span className="text-sm">Tổng</span>
              <span className="text-sm">{totalQuantity}</span>
              <span className="text-sm">100%</span>
            </div>
            <div className="flex gap-2 pt-2">
              <Button
                variant="outline"
                size="sm"
                className="text-xs"
                onClick={handleDownloadTemplate}
                disabled={isDownloadingTemplate}
              >
                {isDownloadingTemplate ? (
                  <Loader2 className="mr-1 h-3 w-3 animate-spin" />
                ) : (
                  <Download className="mr-1 h-3 w-3" />
                )}
                Tải mẫu ghi nhận trạng thái
              </Button>
              <Button
                variant="outline"
                size="sm"
                className="text-xs"
                onClick={() => statusFileInputRef.current?.click()}
                disabled={isUploading}
              >
                {isUploading ? (
                  <Loader2 className="mr-1 h-3 w-3 animate-spin" />
                ) : (
                  <Upload className="mr-1 h-3 w-3" />
                )}
                Tải file
              </Button>
            </div>
          </CardContent>
        </Card>
      )}

      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4"></div>

        <div className="flex gap-2">
          <Button
            variant="outline"
            size="sm"
            onClick={handleDownloadTemplate}
            disabled={isDownloadingTemplate || isDataLoading}
          >
            {isDownloadingTemplate && (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            )}
            Tải mẫu ghi nhận thông tin
          </Button>
          <Button
            variant="outline"
            size="sm"
            disabled={isDataLoading || isUploading}
            onClick={() => informationFileInputRef.current?.click()}
          >
            {isUploading ? (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            ) : (
              <Upload className="mr-2 h-4 w-4" />
            )}
            Tải file lên
          </Button>
          <Button
            variant="outline"
            size="sm"
            disabled={isDataLoading || isExportingReport}
            onClick={handleExportReport}
          >
            {isExportingReport && (
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
            )}
            Xuất báo cáo
          </Button>
        </div>
      </div>

      {/* Action Buttons */}
      <div className="flex gap-2">
        <Button
          variant="outline"
          size="sm"
          disabled={
            isDataLoading || selectedItems.length === 0 || processingBulkAction
          }
          onClick={handleBulkMarkAsDead}
        >
          {processingBulkAction && (
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
          )}
          Đánh dấu đã chết ({selectedItems.length})
        </Button>
        <Button
          variant="outline"
          size="sm"
          disabled={
            isDataLoading || selectedItems.length === 0 || processingBulkAction
          }
          onClick={handleBulkMarkAsRecovered}
        >
          {processingBulkAction && (
            <Loader2 className="mr-2 h-4 w-4 animate-spin" />
          )}
          Đánh dấu khỏi bệnh ({selectedItems.length})
        </Button>
      </div>

      {/* Results Count */}
      <div className="flex items-center gap-4">
        <span className="text-sm font-medium">
          Tổng:{' '}
          {isLoadingLivestock ? (
            <Skeleton className="inline-block h-4 w-8" />
          ) : (
            `${filteredLivestock.length} con`
          )}
        </span>
        {missingInfoCount > 0 && (
          <div className="flex gap-2">
            <span className="rounded bg-red-100 px-2 py-1 text-xs text-red-800">
              Thiếu thông tin: {missingInfoCount}
            </span>
          </div>
        )}
      </div>

      {/* Data Table with Loading */}
      <Card>
        <CardContent className="p-0">
          <Table>
            <TableHeader>
              <TableRow className="bg-gray-100">
                <TableHead className="">
                  <Checkbox
                    checked={
                      selectedItems.length === currentPageData.length &&
                      currentPageData.length > 0
                    }
                    onCheckedChange={handleSelectAll}
                    disabled={isDataLoading}
                  />
                </TableHead>
                <TableHead className="text-center">Mã kiểm dịch</TableHead>
                <TableHead className="text-center">Giống</TableHead>
                <TableHead className="text-center">Trọng lượng (kg)</TableHead>
                <TableHead className="text-center">Giới tính</TableHead>
                <TableHead className="text-center">Màu sắc</TableHead>
                <TableHead className="text-center">Xuất xứ</TableHead>
                <TableHead className="text-center">Trạng thái</TableHead>
                <TableHead className="text-center">Hành động</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {isLoadingLivestock ? (
                // Show skeleton rows while loading
                [...Array(5)].map((_, i) => <TableRowSkeleton key={i} />)
              ) : currentPageData.length === 0 ? (
                <TableRow>
                  <TableCell
                    colSpan={9}
                    className="py-8 text-center text-gray-500"
                  >
                    Không có dữ liệu
                  </TableCell>
                </TableRow>
              ) : (
                currentPageData.map((item) => (
                  <TableRow key={item.id} className="hover:bg-gray-50">
                    <TableCell>
                      <div className="flex items-center gap-2">
                        <Checkbox
                          checked={selectedItems.includes(item.id)}
                          onCheckedChange={(checked) =>
                            handleSelectItem(item.id, checked)
                          }
                          disabled={processingItems.has(item.id)}
                        />
                      </div>
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="flex items-center justify-center gap-2">
                        {item.inspectionCode}
                        {item.isMissingInformation && (
                          <AlertTriangle className="h-4 w-4 text-yellow-500" />
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="text-center">
                      {item.species}
                    </TableCell>
                    <TableCell className="text-center">{item.weight}</TableCell>
                    <TableCell className="text-center">{item.gender}</TableCell>
                    <TableCell className="text-center">{item.color}</TableCell>
                    <TableCell className="text-center">{item.origin}</TableCell>
                    <TableCell className="text-center">
                      <span
                        className={
                          item.status === 'ỐM'
                            ? 'text-red-600'
                            : 'text-green-600'
                        }
                      >
                        {item.status}
                      </span>
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="flex justify-center gap-1">
                        {/* <Button
                          variant="ghost"
                          size="sm"
                          className="text-xs text-blue-600 hover:text-blue-800"
                          onClick={() => handleViewDetails(item.id)}
                          disabled={processingItems.has(item.id)}
                        >
                          Chi tiết
                        </Button> */}
                        {item.status === 'ỐM' && (
                          <Button
                            variant="ghost"
                            size="sm"
                            className="text-xs text-green-600 hover:text-green-800"
                            onClick={() => handleMarkAsRecovered(item.id)}
                            disabled={processingItems.has(item.id)}
                          >
                            {processingItems.has(item.id) && (
                              <Loader2 className="mr-1 h-3 w-3 animate-spin" />
                            )}
                            Đánh dấu khỏi bệnh
                          </Button>
                        )}
                        <Button
                          variant="ghost"
                          size="sm"
                          className="text-xs text-red-600 hover:text-red-800"
                          onClick={() => handleMarkAsDead(item.id)}
                          disabled={processingItems.has(item.id)}
                        >
                          {processingItems.has(item.id) && (
                            <Loader2 className="mr-1 h-3 w-3 animate-spin" />
                          )}
                          Đánh dấu đã chết
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>

        {/* Pagination */}
        {!isLoadingLivestock && filteredLivestock.length > 0 && (
          <Pagination
            currentPage={currentPage}
            totalPages={totalPages}
            onPageChange={handlePageChange}
            totalItems={filteredLivestock.length}
            itemsPerPage={itemsPerPage}
          />
        )}
      </Card>

      {/* QR Code Modal */}
      <QRCodeModal
        isOpen={isQRModalOpen}
        onClose={() => setIsQRModalOpen(false)}
      />
    </BasePages>
  );
}
