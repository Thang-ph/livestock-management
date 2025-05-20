import { Card } from '@/components/ui/card';

export default function BidSummary() {
  return (
    <div className="grid grid-cols-1 gap-2">
      <Card className="border-2 border-gray-200 p-4">
        <div className="text-center">
          <h3 className="font-medium text-gray-600">
            Số gói thầu đang đấu thầu
          </h3>
          <p className="mt-2 text-2xl font-bold">1</p>
        </div>
      </Card>

      <Card className="border-2 border-red-200 p-4">
        <div className="text-center">
          <h3 className="font-medium text-gray-600">
            Số gói thầu không đáp ứng
          </h3>
          <p className="mt-2 text-2xl font-bold">0</p>
        </div>
      </Card>

      <Card className="border-2 border-gray-200 p-4">
        <div className="text-center">
          <h3 className="font-medium text-gray-600">
            Số gói thầu đang bàn giao
          </h3>
          <p className="mt-2 text-2xl font-bold">5</p>
        </div>
      </Card>

      <Card className="border-2 border-gray-200 p-4">
        <div className="text-center">
          <h3 className="font-medium text-gray-600">
            Số gói thầu đã hoàn thành
          </h3>
          <p className="mt-2 text-2xl font-bold">1</p>
        </div>
      </Card>
    </div>
  );
}
