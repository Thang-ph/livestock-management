'use client';

import type React from 'react';
import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useParams, useSearchParams } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Download, Upload } from 'lucide-react';
import { useRef, useState } from 'react';
import { toast } from '@/components/ui/use-toast';
import { useGetDanhSachKhachHangGoiThau } from '@/queries/admin.query';

export function OverViewTab() {
  const [searchParams] = useSearchParams();
  const { id } = useParams();
  const pageLimit = Number(searchParams.get('limit') || 10);
  const { data, isPending, refetch } = useGetDanhSachKhachHangGoiThau(
    String(id)
  );
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [uploading, setUploading] = useState(false);
  const listObjects = data?.data?.items;
  const totalRecords = data?.data?.length;
  const pageCount = Math.ceil(totalRecords / pageLimit);

  const handleFileUpload = async (
    event: React.ChangeEvent<HTMLInputElement>
  ) => {
    const file = event.target.files?.[0];
    if (!file) return;

    try {
      setUploading(true);

      const formData = new FormData();
      formData.append('procurementId', String(id));
      formData.append('requestedBy', 'test');
      formData.append('file', file);

      toast({
        title: 'Tải lên thành công',
        description: 'Danh sách khách hàng đã được cập nhật',
        variant: 'success'
      });
      refetch();
    } catch (error) {
      console.error('Upload error:', error);
      toast({
        title: 'Lỗi tải lên',
        description: 'Đã xảy ra lỗi khi tải file lên. Vui lòng thử lại.',
        variant: 'destructive'
      });
    } finally {
      setUploading(false);
      // Reset the file input
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    }
  };

  const handleDownloadTemplate = async () => {
    try {
      // Gọi GET API download

      const link = document.createElement('a');
      link.href =
        'https://res.cloudinary.com/livestock-management-cloudinary/raw/upload/v1744376733/1744217286956-Mẫudanhsáchkháchhàng04032025.xlsx';
      link.setAttribute('download', 'customer-template.xlsx');
      document.body.appendChild(link);

      link.click();

      document.body.removeChild(link);

      toast({
        title: 'Tải xuống thành công',
        description: 'File mẫu đã được tải xuống',
        variant: 'success'
      });
    } catch (error) {
      console.error('Download error:', error);
      toast({
        title: 'Lỗi tải xuống',
        description: 'Đã xảy ra lỗi khi tải file mẫu. Vui lòng thử lại.',
        variant: 'destructive'
      });
    }
  };

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0 ">
        <h1 className="text-center font-bold">DANH SÁCH KHÁCH HÀNG</h1>
        <div className="flex gap-2">
          <Button
            variant="outline"
            className="flex items-center gap-2"
            onClick={handleDownloadTemplate}
          >
            <Download className="h-4 w-4" />
            <span>Tải file mẫu</span>
          </Button>

          {/* Hidden file input */}
          <input
            type="file"
            ref={fileInputRef}
            onChange={handleFileUpload}
            accept=".xlsx, .xls"
            className="hidden"
          />

          <Button
            variant="outline"
            className="flex items-center gap-2"
            onClick={() => fileInputRef.current?.click()}
            disabled={uploading}
          >
            <Upload className="h-4 w-4" />
            <span>{uploading ? 'Đang tải lên...' : 'Tải file lên'}</span>
          </Button>
        </div>
        {listObjects && listObjects.length > 0 ? (
          <div>
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
        ) : (
          <div className="text-center text-red-500">
            Danh sách khách hàng trống, vui lòng tải lên dữ liệu
          </div>
        )}
      </div>
    </>
  );
}
