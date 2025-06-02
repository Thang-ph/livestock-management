'use client';

import { Button } from '@/components/ui/button';
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
  return (
    <>
      <Button>Chi tiết</Button>
    </>
  );
};
