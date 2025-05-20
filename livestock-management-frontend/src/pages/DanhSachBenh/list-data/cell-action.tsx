'use client';

import type React from 'react';

import { GeminiAnalysisDialog } from '@/components/shared/gemini-analysis';
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
  const makePrompt = (data: any) => {
    return `
Bạn là một bác sĩ thú y chuyên môn trong chăn nuôi gia súc. Hãy cung cấp thông tin chi tiết, ngắn gọn và dễ hiểu về loại bệnh sau để người chăn nuôi có thể nhận biết và xử lý kịp thời.

Thông tin bệnh:
- Tên bệnh: ${data.name}

Vui lòng trình bày bằng tiếng Việt, khoảng 250-500 từ, tập trung vào:
1. Triệu chứng thường gặp
2. Nguyên nhân
3. Hướng xử lý cơ bản hoặc phòng ngừa (nếu có)

Tránh dùng thuật ngữ chuyên môn khó hiểu. Trình bày rõ ràng.
`;
  };
  return (
    <>
      <GeminiAnalysisDialog
        data={data}
        generatePrompt={makePrompt}
        label="Tìm hiểu về bệnh"
      />
    </>
  );
};
