import { Button } from '@/components/ui/button';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Checkbox } from '@/components/ui/checkbox';
import { Badge } from '@/components/ui/badge';

export default function Component() {
  return (
    <div className="mx-auto max-w-6xl space-y-6 p-6">
      {/* Header Section */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold text-gray-800">
            Thông tin vật nuôi - 000011
          </h1>
          <div className="mt-2 flex items-center gap-2">
            <span className="text-sm text-gray-600">Trạng thái:</span>
            <Badge
              variant="secondary"
              className="border-red-200 bg-red-100 text-red-800"
            >
              ỐM
            </Badge>
          </div>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" size="sm">
            Ghi nhận mắc bệnh
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="border-green-200 text-green-600 hover:bg-green-50"
          >
            Đánh dấu khỏi bệnh
          </Button>
          <Button
            variant="outline"
            size="sm"
            className="border-red-200 text-red-600 hover:bg-red-50"
          >
            Đánh dấu đã chết
          </Button>
        </div>
      </div>

      {/* Main Information Card */}
      <Card>
        <CardHeader>
          <CardTitle className="text-lg">Thông tin định danh</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-2 gap-x-8 gap-y-4">
            <div className="space-y-4">
              <div className="flex">
                <span className="w-32 font-medium text-gray-700">Giống:</span>
                <span className="text-gray-600">Bò lai Sind đực</span>
              </div>
              <div className="flex">
                <span className="w-32 font-medium text-gray-700">
                  Màu lông:
                </span>
                <span className="text-gray-600">Vàng cánh gián</span>
              </div>
              <div className="flex">
                <span className="w-32 font-medium text-gray-700">
                  Trọng lượng:
                </span>
                <span className="text-gray-600">210 kg</span>
              </div>
              <div className="flex">
                <span className="w-32 font-medium text-gray-700">
                  Nguồn gốc:
                </span>
                <span className="text-gray-600">Việt Nam</span>
              </div>
              <div className="flex">
                <span className="w-32 font-medium text-gray-700">
                  Cơ sở chăn nuôi:
                </span>
                <span className="text-gray-600">
                  Hợp tác xã dịch vụ và sản xuất nông nghiệp Lúa Vàng
                </span>
              </div>
            </div>
            <div className="space-y-4">
              <div className="flex">
                <span className="w-40 font-medium text-gray-700">
                  Ngày nhập:
                </span>
                <span className="text-gray-600">03-01-2025</span>
              </div>
              <div className="flex">
                <span className="w-40 font-medium text-gray-700">
                  Trọng lượng nhập:
                </span>
                <span className="text-gray-600">100 kg</span>
              </div>
              <div className="flex">
                <span className="w-40 font-medium text-gray-700">
                  Ngày xuất:
                </span>
                <span className="text-gray-600">N/A</span>
              </div>
              <div className="flex">
                <span className="w-40 font-medium text-gray-700">
                  Trọng lượng xuất:
                </span>
                <span className="text-gray-600">N/A</span>
              </div>
              <div className="flex">
                <span className="w-40 font-medium text-gray-700">
                  Chỉnh sửa thông tin lần cuối bởi:
                </span>
                <span className="text-gray-600">Kim Văn Dư</span>
              </div>
            </div>
          </div>
        </CardContent>
      </Card>

      {/* Bottom Section with Two Cards */}
      <div className="grid grid-cols-2 gap-6">
        {/* Vaccination History */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Các bệnh đã tiêm phòng</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-3">
              <div className="flex items-center space-x-2">
                <Checkbox id="disease1" checked />
                <label htmlFor="disease1" className="text-sm text-gray-700">
                  Lở mồm long móng
                </label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox id="disease2" checked />
                <label htmlFor="disease2" className="text-sm text-gray-700">
                  Tụ huyết trùng
                </label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox id="disease3" checked />
                <label htmlFor="disease3" className="text-sm text-gray-700">
                  Viêm da nổi cục
                </label>
              </div>
              <div className="flex items-center space-x-2">
                <Checkbox id="disease4" checked />
                <label htmlFor="disease4" className="text-sm text-gray-700">
                  Ung khí thán
                </label>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Current Disease Status */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg">Dịch bệnh đang mắc</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 border-b pb-2 text-sm font-medium text-gray-700">
                <span>Tên bệnh</span>
                <span>Ngày mắc</span>
              </div>
              <div className="grid grid-cols-2 gap-4 text-sm">
                <span className="text-gray-600">Lở mồm long móng</span>
                <span className="text-gray-600">01-01-2025</span>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
