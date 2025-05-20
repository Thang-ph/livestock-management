'use client';

import { useState } from 'react';
import {
  BarChart,
  CalendarDays,
  Cloud,
  CloudRain,
  Droplets,
  Layers,
  Leaf,
  PiggyBank,
  ShoppingCart,
  Sun,
  Syringe,
  Thermometer,
  Truck,
  Wind
} from 'lucide-react';
import BasePages from '@/components/shared/base-pages.js';

import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Badge } from '@/components/ui/badge';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';

// Mock data
const mockData = {
  upcomingVaccinations: 12,
  upcomingBids: 5,
  upcomingImports: 8,
  currentLivestock: 2450,
  livestockByType: [
    { name: 'Heo', count: 1200 },
    { name: 'Gà', count: 800 },
    { name: 'Vịt', count: 350 },
    { name: 'Bò', count: 100 }
  ],
  monthlyStats: [
    { month: 'T1', vaccinations: 10, imports: 5, bids: 3 },
    { month: 'T2', vaccinations: 8, imports: 6, bids: 4 },
    { month: 'T3', vaccinations: 12, imports: 8, bids: 2 },
    { month: 'T4', vaccinations: 15, imports: 7, bids: 5 },
    { month: 'T5', vaccinations: 9, imports: 9, bids: 6 },
    { month: 'T6', vaccinations: 11, imports: 4, bids: 3 }
  ],
  regions: [
    {
      name: 'Khu vực Bắc',
      weather: 'Mưa nhẹ',
      temperature: 24,
      humidity: 85,
      windSpeed: 12,
      icon: CloudRain
    },
    {
      name: 'Khu vực Trung',
      weather: 'Nắng',
      temperature: 32,
      humidity: 65,
      windSpeed: 8,
      icon: Sun
    },
    {
      name: 'Khu vực Nam',
      weather: 'Nhiều mây',
      temperature: 28,
      humidity: 75,
      windSpeed: 10,
      icon: Cloud
    },
    {
      name: 'Khu vực Tây Nguyên',
      weather: 'Nắng nhẹ',
      temperature: 26,
      humidity: 70,
      windSpeed: 5,
      icon: Sun
    }
  ]
};

export default function Dashboard() {
  const [selectedChart, setSelectedChart] = useState('all');

  return (
    <BasePages
      className="relative flex-1 space-y-4 overflow-y-auto px-4 pb-8"
      breadcrumbs={[
        { title: 'Trang chủ', link: '/' },
        { title: 'Dashboard', link: '/user' }
      ]}
    >
      <div className="flex items-center justify-between">
        <h2 className="text-3xl font-bold tracking-tight">Dashboard</h2>
        <div className="flex items-center gap-2">
          <Select defaultValue="today">
            <SelectTrigger className="w-[180px]">
              <SelectValue placeholder="Chọn thời gian" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="today">Hôm nay</SelectItem>
              <SelectItem value="week">Tuần này</SelectItem>
              <SelectItem value="month">Tháng này</SelectItem>
              <SelectItem value="year">Năm nay</SelectItem>
            </SelectContent>
          </Select>
        </div>
      </div>

      <Tabs defaultValue="overview" className="space-y-4">
        <TabsList>
          <TabsTrigger value="overview">Tổng quan</TabsTrigger>
          <TabsTrigger value="analytics">Phân tích</TabsTrigger>
          <TabsTrigger value="weather">Thời tiết</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  Lô tiêm sắp tới
                </CardTitle>
                <Syringe className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {mockData.upcomingVaccinations}
                </div>
                <p className="text-xs text-muted-foreground">
                  +2 so với tháng trước
                </p>
                <div className="mt-2">
                  <Badge
                    variant="outline"
                    className="bg-green-50 text-green-700 hover:bg-green-50"
                  >
                    Sắp tới: 3 ngày
                  </Badge>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  Lô thầu sắp tới
                </CardTitle>
                <PiggyBank className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {mockData.upcomingBids}
                </div>
                <p className="text-xs text-muted-foreground">
                  +1 so với tháng trước
                </p>
                <div className="mt-2">
                  <Badge
                    variant="outline"
                    className="bg-blue-50 text-blue-700 hover:bg-blue-50"
                  >
                    Sắp tới: 7 ngày
                  </Badge>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  Lô nhập sắp tới
                </CardTitle>
                <Truck className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {mockData.upcomingImports}
                </div>
                <p className="text-xs text-muted-foreground">
                  +3 so với tháng trước
                </p>
                <div className="mt-2">
                  <Badge
                    variant="outline"
                    className="bg-amber-50 text-amber-700 hover:bg-amber-50"
                  >
                    Sắp tới: 5 ngày
                  </Badge>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                <CardTitle className="text-sm font-medium">
                  Vật nuôi hiện tại
                </CardTitle>
                <Layers className="h-4 w-4 text-muted-foreground" />
              </CardHeader>
              <CardContent>
                <div className="text-2xl font-bold">
                  {mockData.currentLivestock}
                </div>
                <p className="text-xs text-muted-foreground">
                  +120 so với tháng trước
                </p>
                <div className="mt-2 flex flex-wrap gap-1">
                  {mockData.livestockByType.map((type) => (
                    <Badge
                      key={type.name}
                      variant="outline"
                      className="bg-purple-50 text-purple-700 hover:bg-purple-50"
                    >
                      {type.name}: {type.count}
                    </Badge>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-7">
            <Card className="col-span-4">
              <CardHeader>
                <div className="flex items-center justify-between">
                  <CardTitle>Thống kê theo tháng</CardTitle>
                  <Select
                    value={selectedChart}
                    onValueChange={setSelectedChart}
                  >
                    <SelectTrigger className="w-[180px]">
                      <SelectValue placeholder="Chọn biểu đồ" />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Tất cả</SelectItem>
                      <SelectItem value="vaccinations">Lô tiêm</SelectItem>
                      <SelectItem value="imports">Lô nhập</SelectItem>
                      <SelectItem value="bids">Lô thầu</SelectItem>
                    </SelectContent>
                  </Select>
                </div>
              </CardHeader>
              <CardContent className="pl-2">
                <MonthlyChart
                  data={mockData.monthlyStats}
                  selectedChart={selectedChart}
                />
              </CardContent>
            </Card>

            <Card className="col-span-3">
              <CardHeader>
                <CardTitle>Phân bố vật nuôi</CardTitle>
                <CardDescription>
                  Tổng số: {mockData.currentLivestock} con
                </CardDescription>
              </CardHeader>
              <CardContent>
                <LivestockDistributionChart data={mockData.livestockByType} />
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="analytics" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            <Card className="col-span-2">
              <CardHeader>
                <CardTitle>Phân tích chi tiết</CardTitle>
                <CardDescription>
                  Thống kê chi tiết về hoạt động của trang trại
                </CardDescription>
              </CardHeader>
              <CardContent>
                <p className="text-sm text-muted-foreground">
                  Đang phát triển tính năng này...
                </p>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle>Lịch sắp tới</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="flex items-center">
                    <CalendarDays className="mr-2 h-4 w-4 text-muted-foreground" />
                    <div className="space-y-1">
                      <p className="text-sm font-medium leading-none">
                        Tiêm phòng đợt 1
                      </p>
                      <p className="text-xs text-muted-foreground">
                        15/05/2024
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center">
                    <CalendarDays className="mr-2 h-4 w-4 text-muted-foreground" />
                    <div className="space-y-1">
                      <p className="text-sm font-medium leading-none">
                        Nhập thức ăn
                      </p>
                      <p className="text-xs text-muted-foreground">
                        17/05/2024
                      </p>
                    </div>
                  </div>
                  <div className="flex items-center">
                    <CalendarDays className="mr-2 h-4 w-4 text-muted-foreground" />
                    <div className="space-y-1">
                      <p className="text-sm font-medium leading-none">
                        Đấu thầu thuốc
                      </p>
                      <p className="text-xs text-muted-foreground">
                        20/05/2024
                      </p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="weather" className="space-y-4">
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
            {mockData.regions.map((region) => (
              <Card key={region.name}>
                <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
                  <CardTitle className="text-sm font-medium">
                    {region.name}
                  </CardTitle>
                  <region.icon className="h-4 w-4 text-muted-foreground" />
                </CardHeader>
                <CardContent>
                  <div className="flex items-center justify-between">
                    <div className="text-2xl font-bold">
                      {region.temperature}°C
                    </div>
                    <div className="text-sm text-muted-foreground">
                      {region.weather}
                    </div>
                  </div>
                  <div className="mt-4 space-y-2">
                    <div className="flex items-center">
                      <Droplets className="mr-2 h-4 w-4 text-blue-500" />
                      <span className="text-sm">Độ ẩm: {region.humidity}%</span>
                    </div>
                    <div className="flex items-center">
                      <Wind className="mr-2 h-4 w-4 text-gray-500" />
                      <span className="text-sm">
                        Gió: {region.windSpeed} km/h
                      </span>
                    </div>
                    <div className="flex items-center">
                      <Thermometer className="mr-2 h-4 w-4 text-red-500" />
                      <span className="text-sm">
                        Cảm giác như: {region.temperature - 2}°C
                      </span>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          <Card>
            <CardHeader>
              <CardTitle>Dự báo nông nghiệp</CardTitle>
              <CardDescription>
                Thông tin thời tiết và khuyến cáo cho hoạt động nông nghiệp
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                <div className="rounded-lg border p-3">
                  <div className="flex items-center gap-2">
                    <Leaf className="h-5 w-5 text-green-600" />
                    <h3 className="font-medium">Khuyến cáo mùa vụ</h3>
                  </div>
                  <p className="mt-2 text-sm text-muted-foreground">
                    Thời tiết ẩm ướt tại khu vực Bắc có thể ảnh hưởng đến sức
                    khỏe vật nuôi. Khuyến cáo tăng cường kiểm tra chuồng trại và
                    đảm bảo hệ thống thoát nước hoạt động tốt.
                  </p>
                </div>

                <div className="rounded-lg border p-3">
                  <div className="flex items-center gap-2">
                    <ShoppingCart className="h-5 w-5 text-amber-600" />
                    <h3 className="font-medium">Giá cả thị trường</h3>
                  </div>
                  <p className="mt-2 text-sm text-muted-foreground">
                    Giá thức ăn chăn nuôi dự kiến tăng nhẹ trong tháng tới do
                    ảnh hưởng của thời tiết đến vụ mùa ngô và đậu nành. Nên cân
                    nhắc nhập thêm dự trữ.
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </BasePages>
  );
}

// Chart components
function MonthlyChart({ data, selectedChart }) {
  // This is a simplified chart representation
  // In a real application, you would use a charting library like Recharts

  const getMaxValue = () => {
    if (selectedChart === 'vaccinations')
      return Math.max(...data.map((d) => d.vaccinations));
    if (selectedChart === 'imports')
      return Math.max(...data.map((d) => d.imports));
    if (selectedChart === 'bids') return Math.max(...data.map((d) => d.bids));
    return Math.max(
      ...data.map((d) => d.vaccinations),
      ...data.map((d) => d.imports),
      ...data.map((d) => d.bids)
    );
  };

  const maxValue = getMaxValue();

  return (
    <div className="h-[200px] w-full">
      <div className="flex h-full items-end gap-2">
        {data.map((item) => (
          <div
            key={item.month}
            className="flex flex-1 flex-col items-center gap-1"
          >
            <div className="flex w-full gap-1 px-1">
              {(selectedChart === 'all' ||
                selectedChart === 'vaccinations') && (
                <div
                  className="flex-1 rounded-t bg-blue-500"
                  style={{
                    height: `${(item.vaccinations / maxValue) * 150}px`
                  }}
                  title={`Lô tiêm: ${item.vaccinations}`}
                />
              )}
              {(selectedChart === 'all' || selectedChart === 'imports') && (
                <div
                  className="flex-1 rounded-t bg-green-500"
                  style={{ height: `${(item.imports / maxValue) * 150}px` }}
                  title={`Lô nhập: ${item.imports}`}
                />
              )}
              {(selectedChart === 'all' || selectedChart === 'bids') && (
                <div
                  className="flex-1 rounded-t bg-amber-500"
                  style={{ height: `${(item.bids / maxValue) * 150}px` }}
                  title={`Lô thầu: ${item.bids}`}
                />
              )}
            </div>
            <div className="text-xs font-medium">{item.month}</div>
          </div>
        ))}
      </div>

      <div className="mt-4 flex items-center justify-center gap-4">
        {(selectedChart === 'all' || selectedChart === 'vaccinations') && (
          <div className="flex items-center gap-1">
            <div className="h-3 w-3 rounded-full bg-blue-500"></div>
            <span className="text-xs">Lô tiêm</span>
          </div>
        )}
        {(selectedChart === 'all' || selectedChart === 'imports') && (
          <div className="flex items-center gap-1">
            <div className="h-3 w-3 rounded-full bg-green-500"></div>
            <span className="text-xs">Lô nhập</span>
          </div>
        )}
        {(selectedChart === 'all' || selectedChart === 'bids') && (
          <div className="flex items-center gap-1">
            <div className="h-3 w-3 rounded-full bg-amber-500"></div>
            <span className="text-xs">Lô thầu</span>
          </div>
        )}
      </div>
    </div>
  );
}

function LivestockDistributionChart({ data }) {
  // This is a simplified chart representation
  // In a real application, you would use a charting library like Recharts

  const total = data.reduce((sum, item) => sum + item.count, 0);
  const colors = [
    'bg-purple-500',
    'bg-pink-500',
    'bg-indigo-500',
    'bg-cyan-500'
  ];

  return (
    <div className="space-y-4">
      <div className="h-[200px] w-full">
        <div className="flex h-full flex-col justify-center">
          <div className="relative mx-auto h-40 w-40 rounded-full">
            <BarChart className="absolute left-1/2 top-1/2 h-10 w-10 -translate-x-1/2 -translate-y-1/2 text-muted-foreground" />
            {data.map((item, index) => {
              const percentage = (item.count / total) * 100;
              const previousPercentages = data
                .slice(0, index)
                .reduce(
                  (sum, prevItem) => sum + (prevItem.count / total) * 100,
                  0
                );

              return (
                <div
                  key={item.name}
                  className={`absolute left-0 top-0 h-40 w-40 rounded-full ${colors[index % colors.length]}`}
                  style={{
                    clipPath: `polygon(50% 50%, 50% 0%, ${50 + 50 * Math.cos(((previousPercentages + percentage) * 3.6 * Math.PI) / 180)}% ${50 - 50 * Math.sin(((previousPercentages + percentage) * 3.6 * Math.PI) / 180)}%, ${50 + 50 * Math.cos((previousPercentages * 3.6 * Math.PI) / 180)}% ${50 - 50 * Math.sin((previousPercentages * 3.6 * Math.PI) / 180)}%)`
                  }}
                />
              );
            })}
          </div>
        </div>
      </div>

      <div className="flex flex-wrap justify-center gap-4">
        {data.map((item, index) => (
          <div key={item.name} className="flex items-center gap-1">
            <div
              className={`h-3 w-3 rounded-full ${colors[index % colors.length]}`}
            ></div>
            <span className="text-xs">
              {item.name}: {item.count} (
              {((item.count / total) * 100).toFixed(1)}%)
            </span>
          </div>
        ))}
      </div>
    </div>
  );
}
