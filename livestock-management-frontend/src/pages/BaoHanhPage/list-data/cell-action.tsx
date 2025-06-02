'use client';

import { Button } from '@/components/ui/button';
import { useRouter } from '@/routes/hooks';
import type React from 'react';

export const CellAction: React.FC<any> = ({ data }) => {
  const router = useRouter();
  return (
    <>
      <Button onClick={() => router.push(`/list-bao-hanh/${data.status}`)}>
        Chi tiết
      </Button>
    </>
  );
};
