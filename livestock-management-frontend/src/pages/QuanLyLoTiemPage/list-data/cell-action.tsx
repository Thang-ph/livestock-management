'use client';
import { Button } from '@/components/ui/button';
import { useRouter } from '@/routes/hooks';
interface UserCellActionProps {
  data: {
    id: number;
    name: string;
    email: string;
    role: string;
  };
}

export const CellAction: React.FC<UserCellActionProps> = () => {
  const router = useRouter();

  return (
    <div className="flex items-center gap-2">
      <Button
        onClick={() => {
          localStorage.setItem('goi-thau-tab', 'add');
          router.push(`/lo-tiem`);
        }}
      >
        Xác nhận
      </Button>
      <Button>Hủy</Button>
    </div>
  );
};
