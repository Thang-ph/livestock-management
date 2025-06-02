'use client';

import { Button } from '@/components/ui/button';
import { useRouter } from '@/routes/hooks';
import type React from 'react';

interface UserData {
  id: string;
  userName: string;
  email: string;
  phoneNumber: string;
  isLocked: boolean;
  roles: string[];
}

interface UserCellActionProps {
  data: UserData;
}

export const CellAction: React.FC<UserCellActionProps> = ({ data }) => {
  const router = useRouter();
  return (
    <>
      <Button
        onClick={() => {
          router.push(`/chi-tiet-bao-hanh/${data.id}`);
        }}
      >
        Chi tiết
      </Button>
    </>
  );
};
