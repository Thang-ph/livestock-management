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

export const CellAction: React.FC<UserCellActionProps> = ({ data }) => {
  const router = useRouter();

  return (
    <>
      <div>
        <Button onClick={() => router.push(`/chi-tiet-lo-tiem/${data.id}`)}>
          Xem chi tiết
        </Button>
      </div>
    </>
  );
};
