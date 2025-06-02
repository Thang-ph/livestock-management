import ListData from '../../list-data';
import { DataTableSkeleton } from '@/components/shared/data-table-skeleton';
import { useParams, useSearchParams } from 'react-router-dom';
import {
  useDownloadDonMuaLeTemplate,
  useGetListVatNuoiDonMuaLe,
  useYeuCauXuatDonMuaLe
} from '@/queries/donmuale.query';
import { Button } from '@/components/ui/button';
import { Download, Upload } from 'lucide-react';
import { toast } from '@/components/ui/use-toast';
import __helpers from '@/helpers';

export function Detail() {
  const [searchParams] = useSearchParams();
  const pageLimit = Number(searchParams.get('limit') || 10);
  const { id } = useParams();
  const { data, isPending } = useGetListVatNuoiDonMuaLe(id);
  const { mutateAsync: downloadTemplate } = useDownloadDonMuaLeTemplate();
  const { mutateAsync: xuat } = useYeuCauXuatDonMuaLe();
  console.log('data', data);
  const listObjects = data?.items || [];
  const totalRecords = data?.items?.length || 0;
  const pageCount = Math.ceil(totalRecords / pageLimit);

  return (
    <>
      <div className="grid gap-6 rounded-md p-4 pt-0 ">
        <h1 className="text-center font-bold">DANH SÁCH LOÀI VẬT NUÔI</h1>
        <div className="flex justify-end gap-2">
          <Button
            variant="default"
            size="sm"
            className="gap-1 bg-yellow-600 text-white hover:bg-yellow-700"
            onClick={async () => {
              const [err] = await xuat(id);
              if (err) {
                toast({
                  title: 'Lỗi',
                  description: err.data?.data || 'Không thể yêu cầu xuất',
                  variant: 'destructive'
                });
                return;
              }
              toast({
                title: 'Thành công',
                description: 'Đã yêu cầu xuất thành công',
                variant: 'success'
              });
            }}
          >
            Yêu cầu xuất
          </Button>
          <Button
            variant="default"
            size="sm"
            className="gap-1 bg-blue-600 text-white hover:bg-blue-700"
            onClick={() => {
              // Tạo input file ẩn và trigger click
              const fileInput = document.createElement('input');
              fileInput.type = 'file';
              fileInput.accept = '.xlsx,.xls,.csv'; // Chỉ chấp nhận file Excel/CSV
              fileInput.style.display = 'none';

              fileInput.onchange = async (event: any) => {
                const file = event.target.files[0];
                if (!file) return;

                try {
                  // Tạo FormData
                  const formData = new FormData();
                  formData.append('orderId', id as string); // Sử dụng id từ useParams
                  formData.append('requestedBy', __helpers.getUserEmail()); // Hoặc lấy từ user context
                  formData.append('file', file);

                  // Gọi API upload
                  const response = await fetch(
                    'https://lms.autopass.blog/api/order-management/import-list-chosed-livestock',
                    {
                      method: 'POST',
                      headers: {
                        accept: 'text/plain'
                        // Không set Content-Type cho multipart/form-data, browser sẽ tự set
                      },
                      body: formData
                    }
                  );

                  if (response.ok) {
                    toast({
                      title: 'Thành công',
                      description: 'Đã tải lên file thành công',
                      variant: 'success'
                    });
                    window.location.reload();
                  } // Tải lại trang để cập nhật danh sách
                } catch (error: any) {
                  console.error('Upload error:', error);
                  toast({
                    title: 'Lỗi',
                    description: error.data.data || 'Không thể tải lên file',
                    variant: 'destructive'
                  });
                }
              };

              // Trigger file selector
              document.body.appendChild(fileInput);
              fileInput.click();
              document.body.removeChild(fileInput);
            }}
          >
            Tải file lên
            <Upload className="ml-2 h-4 w-4 " />
          </Button>
          <Button
            variant="default"
            size="sm"
            className="gap-1 bg-green-600 text-white hover:bg-green-700"
            onClick={async () => {
              const [err, res] = await downloadTemplate(id);

              if (err) {
                toast({
                  title: 'Lỗi',
                  description: err.data?.data || 'Không thể tải xuống mẫu đơn',
                  variant: 'destructive'
                });
                return;
              }

              const urlDownload = res.data;
              console.log('urlDownload', urlDownload);
              const link = document.createElement('a');
              link.href = urlDownload;
              link.download = `danh-sach-vat-nuoi-${id}.xlsx`; // Hoặc tên file bạn muốn
              link.target = '_blank';
              document.body.appendChild(link);
              link.click();
              document.body.removeChild(link);

              // Hiển thị thông báo thành công
              toast({
                title: 'Thành công',
                description: 'Đã tải xuống file thành công',
                variant: 'default'
              });
            }}
          >
            Tải file xuống
            <Download className="ml-2 h-4 w-4" />
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
    </>
  );
}
