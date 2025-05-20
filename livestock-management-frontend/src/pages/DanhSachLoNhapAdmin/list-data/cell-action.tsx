'use client';
import { Button } from '@/components/ui/button';
import { useRouter } from '@/routes/hooks';

export const CellAction: React.FC<any> = ({ data }) => {
  const router = useRouter();

  return (
    <>
      <div className="flex items-center space-x-2">
        <Button
          onClick={() => {
            localStorage.setItem('goi-thau-tab', 'add');
            router.push(`/lo-tiem`);
          }}
          disabled={data.status != 'HOÀN_THÀNH'}
        >
          Tạo lô tiêm
        </Button>
        <Button onClick={() => router.push(`/chi-tiet-lo-nhap/${data.id}`)}>
          Chi tiết
        </Button>
      </div>
    </>
  );
};
