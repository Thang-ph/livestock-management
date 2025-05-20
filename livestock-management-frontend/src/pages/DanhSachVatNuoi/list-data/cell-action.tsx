'use client';
import { GeminiAnalysisDialog } from '@/components/shared/gemini-analysis';
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

  const makePrompt = (data: any) => {
    return `
Bạn là chuyên gia chăn nuôi, hãy phân tích các chỉ số bên dưới và đưa ra nhận xét chi tiết về con vật này để hỗ trợ người nông dân hiểu rõ tiềm năng kinh tế của nó.

Thông tin con vật:
- Loài: ${data.type}
- Tốc độ tăng trưởng (growthRate): ${data.growthRate} cm
- Tỷ lệ giết mổ (dressingPercentage): ${data.dressingPercentage}%

Yêu cầu:
- Phân tích ý nghĩa của từng chỉ số.
- Nhận xét tổng thể về sức tăng trưởng, khả năng sinh lợi hoặc rủi ro.
- Trình bày bằng tiếng Việt, ngắn gọn (200–500 từ), dễ hiểu với người chăn nuôi, không dùng thuật ngữ quá chuyên sâu.
`;
  };

  return (
    <>
      {/* Edit Dialog */}

      {/* Permissions Dialog */}
      <div className="flex items-center space-x-2">
        <GeminiAnalysisDialog
          data={data}
          generatePrompt={makePrompt}
          label="Phân tích vật nuôi"
        />
        <Button onClick={() => router.push(`/chi-tiet-lo-nhap/${data.id}`)}>
          Xem chi tiết
        </Button>
      </div>
    </>
  );
};
