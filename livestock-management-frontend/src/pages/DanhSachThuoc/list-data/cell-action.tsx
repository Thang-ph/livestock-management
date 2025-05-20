'use client';
import { GeminiAnalysisDialog } from '@/components/shared/gemini-analysis';
interface UserCellActionProps {
  data: {
    id: number;
    name: string;
    email: string;
    role: string;
  };
}

export const CellAction: React.FC<UserCellActionProps> = ({ data }) => {
  const makePrompt = (data: any) => {
    return `
  Bạn là một chuyên viên y tế trong trang trại. Hãy cung cấp thông tin chi tiết, dễ hiểu và ngắn gọn về loại thuốc sau để người chăn nuôi hiểu rõ hơn khi sử dụng.

  Thông tin thuốc:
  - Tên thuốc: ${data.name}
  - Loại thuốc: ${data.type}
  - Mô tả: ${data.description}

  Vui lòng trình bày nội dung bằng tiếng Việt, rõ ràng, khoảng 200-500 từ. Tập trung vào công dụng, công thức, cách dùng cơ bản, và những lưu ý khi sử dụng.
  `;
  };
  return (
    <>
      {/* Edit Dialog */}

      {/* Permissions Dialog */}
      <div>
        <GeminiAnalysisDialog
          data={data}
          generatePrompt={makePrompt}
          label="Tìm hiểu về bệnh"
        />
      </div>
    </>
  );
};
