'use client';

import { Button } from '@/components/ui/button';
import { toast } from '@/components/ui/use-toast';
import { useRemoveChooese, useRequestChoose } from '@/queries/admin.query';

import { useRouter } from '@/routes/hooks';

export const CellAction: React.FC<any> = ({ data }) => {
  const router = useRouter();
  const { mutateAsync: requestChoose } = useRequestChoose();
  const { mutateAsync: removeChoose } = useRemoveChooese();

  return (
    <div className="flex items-center gap-2">
      <Button onClick={() => router.push(`/chi-tiet-don-mua-le/${data.id}`)}>
        Xem chi tiếtt
      </Button>
      {data.type == 'KHÔNG_YÊU_CẦU' ? (
        <Button
          className="bg-blue-500 text-white hover:bg-blue-600"
          onClick={async () => {
            const [err] = await requestChoose(data.id);
            if (err) {
              toast({
                title: 'Lỗi',
                description:
                  err.data?.data || 'Đã xảy ra lỗi khi chọn đơn hàng',
                variant: 'destructive'
              });
            } else {
              toast({
                title: 'Thành công',
                description: 'Đã chọn đơn hàng',
                variant: 'success'
              });
            }
          }}
        >
          Chọn
        </Button>
      ) : data.type == 'YÊU_CẦU_CHỌN' ? (
        <Button
          className="bg-red-500 text-white hover:bg-red-600"
          onClick={async () => {
            const [err] = await removeChoose(data.id);
            if (err) {
              toast({
                title: 'Lỗi',
                description:
                  err.data?.data || 'Đã xảy ra lỗi khi bỏ chọn đơn hàng',
                variant: 'destructive'
              });
            } else {
              toast({
                title: 'Thành công',
                description: 'Đã bỏ chọn đơn hàng',
                variant: 'success'
              });
            }
          }}
        >
          Bỏ chọn
        </Button>
      ) : (
        <Button disabled={true}>Đang chờ xuất</Button>
      )}
    </div>
  );
};
