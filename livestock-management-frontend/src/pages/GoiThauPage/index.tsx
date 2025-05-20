import BasePages from '@/components/shared/base-pages.js';
import { Search, Calendar, RefreshCw } from 'lucide-react';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import BidPackageGantt from './components/bid-package-gantt';
import BidSummary from './components/bid-summary';
import InventoryReport from './components/inventory-report';
import DonutCharts from './components/donut-charts';
export default function GoiThauPage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Gói thầu', link: '/goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 ">
          <div className="mx-auto max-w-7xl p-4">
            <div className="grid grid-cols-1 gap-6">
              {/* Search and Date Filter */}
              <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
                <div>
                  <p className="mb-1 text-sm font-medium">
                    Tìm mã hoặc tên gói thầu:
                  </p>
                  <div className="relative">
                    <Search className="absolute left-2 top-2.5 h-4 w-4 text-gray-500" />
                    <input
                      type="text"
                      className="h-10 w-full rounded-md border border-gray-300 bg-white px-3 py-2 pl-8 text-sm"
                      placeholder="Tìm kiếm..."
                    />
                  </div>
                </div>
                <div>
                  <p className="mb-1 text-sm font-medium">Thời gian:</p>
                  <div className="flex gap-2">
                    <div className="relative flex-1">
                      <button className="flex h-10 w-full items-center justify-between rounded-md border border-gray-300 bg-white px-3 py-2 text-sm">
                        <span>Tuần này (16/03 - 23/03)</span>
                        <Calendar className="h-4 w-4 opacity-50" />
                      </button>
                    </div>
                    <Button variant="outline" size="icon" className="h-10 w-10">
                      <Calendar className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>

              {/* Gantt Chart and Summary */}
              <div className="grid grid-cols-1 gap-4 lg:grid-cols-4">
                <div className="lg:col-span-3">
                  <Card className="overflow-hidden">
                    <div className="flex items-center justify-between border-b p-4">
                      <h2 className="font-medium">Tiến độ các gói thầu</h2>
                      <Button variant="outline" size="sm">
                        Xem tất cả
                      </Button>
                    </div>
                    <BidPackageGantt />
                  </Card>
                </div>
                <div className="lg:col-span-1">
                  <BidSummary />
                </div>
              </div>

              {/* Inventory Report and Charts */}
              <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
                <div>
                  <Card>
                    <div className="flex items-center justify-between border-b p-4">
                      <h2 className="font-medium">
                        Báo cáo kho ngày 16-03-2025
                      </h2>
                      <div className="flex items-center gap-2">
                        <Button variant="ghost" size="icon" className="h-8 w-8">
                          <RefreshCw className="h-4 w-4" />
                        </Button>
                        <Select>
                          <SelectTrigger className="h-8 w-auto">
                            <SelectValue placeholder="Xem theo biểu đồ" />
                          </SelectTrigger>
                          <SelectContent>
                            <SelectItem value="chart">
                              Xem theo biểu đồ
                            </SelectItem>
                            <SelectItem value="table">Xem theo bảng</SelectItem>
                          </SelectContent>
                        </Select>
                      </div>
                    </div>
                    <InventoryReport />
                  </Card>
                </div>
                <div>
                  <Card>
                    <div className="flex items-center justify-between border-b p-4">
                      <h2 className="font-medium">
                        Thống kê nhập xuất vật nuôi
                      </h2>
                      <div className="flex items-center gap-2">
                        <button className="flex h-8 items-center justify-between rounded-md border border-gray-300 bg-white px-3 py-1 text-sm">
                          <span>Hôm nay (16/03)</span>
                          <Calendar className="ml-2 h-4 w-4 opacity-50" />
                        </button>
                        <Button
                          variant="outline"
                          size="icon"
                          className="h-8 w-8"
                        >
                          <Calendar className="h-4 w-4" />
                        </Button>
                      </div>
                    </div>
                    <DonutCharts />
                  </Card>
                </div>
              </div>
            </div>
          </div>
        </div>
      </BasePages>
    </>
  );
}
