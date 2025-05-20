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
      {/* Edit Dialog */}

      {/* Permissions Dialog */}
      <Button onClick={() => router.push(`/chi-tiet-goi-thau/${data.id}`)}>
        Xem chi tiết
      </Button>
    </>
  );
};
