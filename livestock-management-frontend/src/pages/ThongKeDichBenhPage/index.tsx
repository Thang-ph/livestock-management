'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table';
import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  CartesianGrid,
  ResponsiveContainer,
  Legend
} from 'recharts';
import { Calendar, Plus } from 'lucide-react';
import BasePages from '@/components/shared/base-pages';

const chartData = [
  {
    month: '01/2025',
    'Lở mồm long móng': 5,
    'Viêm da nói cục': 32,
    'Tụ huyết trùng': 28,
    'Nhiễm kí sinh trùng': 15,
    'Đau mắt': 8
  },
  {
    month: '02/2025',
    'Lở mồm long móng': 5,
    'Viêm da nói cục': 30,
    'Tụ huyết trùng': 25,
    'Nhiễm kí sinh trùng': 12,
    'Đau mắt': 6
  },
  {
    month: '03/2025',
    'Lở mồm long móng': 6,
    'Viêm da nói cục': 28,
    'Tụ huyết trùng': 22,
    'Nhiễm kí sinh trùng': 10,
    'Đau mắt': 7
  },
  {
    month: '04/2025',
    'Lở mồm long móng': 7,
    'Viêm da nói cục': 26,
    'Tụ huyết trùng': 20,
    'Nhiễm kí sinh trùng': 8,
    'Đau mắt': 5
  },
  {
    month: '05/2025',
    'Lở mồm long móng': 8,
    'Viêm da nói cục': 24,
    'Tụ huyết trùng': 18,
    'Nhiễm kí sinh trùng': 6,
    'Đau mắt': 4
  },
  {
    month: '06/2025',
    'Lở mồm long móng': 15,
    'Viêm da nói cục': 22,
    'Tụ huyết trùng': 16,
    'Nhiễm kí sinh trùng': 4,
    'Đau mắt': 3
  },
  {
    month: '07/2025',
    'Lở mồm long móng': 22,
    'Viêm da nói cục': 20,
    'Tụ huyết trùng': 14,
    'Nhiễm kí sinh trùng': 2,
    'Đau mắt': 2
  },
  {
    month: '08/2025',
    'Lở mồm long móng': 18,
    'Viêm da nói cục': 18,
    'Tụ huyết trùng': 12,
    'Nhiễm kí sinh trùng': 1,
    'Đau mắt': 1
  },
  {
    month: '09/2025',
    'Lở mồm long móng': 20,
    'Viêm da nói cục': 16,
    'Tụ huyết trùng': 10,
    'Nhiễm kí sinh trùng': 1,
    'Đau mắt': 1
  },
  {
    month: '10/2025',
    'Lở mồm long móng': 25,
    'Viêm da nói cục': 14,
    'Tụ huyết trùng': 8,
    'Nhiễm kí sinh trùng': 0,
    'Đau mắt': 0
  },
  {
    month: '11/2025',
    'Lở mồm long móng': 30,
    'Viêm da nói cục': 12,
    'Tụ huyết trùng': 6,
    'Nhiễm kí sinh trùng': 0,
    'Đau mắt': 0
  },
  {
    month: '12/2025',
    'Lở mồm long móng': 35,
    'Viêm da nói cục': 10,
    'Tụ huyết trùng': 4,
    'Nhiễm kí sinh trùng': 0,
    'Đau mắt': 0
  }
];

const diseaseStats = [
  { name: 'Suy tí sinh trùng', rate: '60%' },
  { name: 'Lở mồm long móng', rate: '80%' },
  { name: 'Tụ huyết trùng', rate: '100%' },
  { name: 'Ung thư thận', rate: '100%' },
  { name: 'Viêm da nói cục', rate: '100%' }
];

const diseaseList = [
  {
    name: 'Lở mồm long móng',
    category: 'Lây nhiễm nguy hiểm',
    infected: 1,
    vaccinated: 1,
    action: 'Chi tiết'
  },
  {
    name: 'Viêm da nói cục',
    category: 'Lây nhiễm nguy hiểm',
    infected: 0,
    vaccinated: 1,
    action: 'Chi tiết'
  },
  {
    name: 'Tụ huyết trùng',
    category: 'Lây nhiễm nguy hiểm',
    infected: 0,
    vaccinated: 1,
    action: 'Chi tiết'
  },
  {
    name: 'Đau mắt',
    category: 'Không lây nhiễm',
    infected: 10,
    vaccinated: 0,
    action: 'Chi tiết'
  }
];

export default function Component() {
  return (
    <BasePages
      className="relative flex-1 space-y-4 overflow-y-auto  px-6"
      breadcrumbs={[
        { title: 'Trang chủ', link: '/' },
        { title: 'Quản lý chăn nuôi', link: '/livestock-dashboard' },
        { title: 'Thống kê dịch bệnh', link: '/thong-ke-dich-benh' }
      ]}
    >
      <h1 className="pt-4 text-2xl font-bold">Thống kê dịch bệnh</h1>
      <div className="grid grid-cols-1 gap-6 lg:grid-cols-3">
        {/* Chart Section */}
        <Card className="lg:col-span-2">
          <CardHeader className="flex flex-row items-center justify-between">
            <CardTitle className="text-lg font-medium">
              Thống kê số lượng vật nuôi mắc bệnh theo tháng
            </CardTitle>
            <div className="flex items-center gap-2">
              <Select defaultValue="nam-nay">
                <SelectTrigger className="w-32">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="nam-nay">Năm nay</SelectItem>
                  <SelectItem value="nam-truoc">Năm trước</SelectItem>
                </SelectContent>
              </Select>
              <Button variant="outline" size="icon">
                <Calendar className="h-4 w-4" />
              </Button>
            </div>
          </CardHeader>
          <CardContent>
            <div className="h-80">
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={chartData}>
                  <CartesianGrid strokeDasharray="3 3" />
                  <XAxis
                    dataKey="month"
                    tick={{ fontSize: 12 }}
                    angle={-45}
                    textAnchor="end"
                    height={60}
                  />
                  <YAxis
                    label={{
                      value: 'Số lượng (con)',
                      angle: -90,
                      position: 'insideLeft'
                    }}
                    tick={{ fontSize: 12 }}
                  />
                  <Legend />
                  <Line
                    type="monotone"
                    dataKey="Lở mồm long móng"
                    stroke="#22c55e"
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Viêm da nói cục"
                    stroke="#64748b"
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Tụ huyết trùng"
                    stroke="#eab308"
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Nhiễm kí sinh trùng"
                    stroke="#f59e0b"
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                  <Line
                    type="monotone"
                    dataKey="Đau mắt"
                    stroke="#ef4444"
                    strokeWidth={2}
                    dot={{ r: 4 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            </div>
          </CardContent>
        </Card>

        {/* Statistics Section */}
        <Card>
          <CardHeader>
            <CardTitle className="text-lg font-medium">
              Tỷ lệ tiềm phòng theo dịch bệnh
            </CardTitle>
          </CardHeader>
          <CardContent>
            <div className="space-y-4">
              <div className="flex justify-between border-b pb-2 text-sm font-medium">
                <span>Tên dịch bệnh</span>
                <span>Tỷ lệ</span>
              </div>
              {diseaseStats.map((disease, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between text-sm"
                >
                  <span className="text-gray-700">{disease.name}</span>
                  <span className="font-medium">{disease.rate}</span>
                </div>
              ))}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Disease List Section */}
      <Card>
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle className="text-lg font-medium">
            Danh sách dịch bệnh
          </CardTitle>
          <Button className="bg-gray-600 hover:bg-gray-700">
            <Plus className="mr-2 h-4 w-4" />
            Thêm mới
          </Button>
        </CardHeader>
        <CardContent>
          <div className="mb-4 flex items-center gap-4">
            <span className="text-sm font-medium">Tìm theo tên:</span>
            <Input placeholder="Nhập tên dịch bệnh..." className="max-w-xs" />
          </div>

          <div className="overflow-hidden rounded-lg border">
            <Table>
              <TableHeader className="bg-gray-400">
                <TableRow>
                  <TableHead className="text-center font-medium text-white">
                    Tên bệnh
                  </TableHead>
                  <TableHead className="text-center font-medium text-white">
                    Phân loại
                  </TableHead>
                  <TableHead className="text-center font-medium text-white">
                    Số lượng vật nuôi hiện mắc (con)
                  </TableHead>
                  <TableHead className="text-center font-medium text-white">
                    Số loại vắc xin hiện có (loại)
                  </TableHead>
                  <TableHead className="text-center font-medium text-white">
                    Hành động
                  </TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {diseaseList.map((disease, index) => (
                  <TableRow
                    key={index}
                    className={index % 2 === 0 ? 'bg-gray-100' : 'bg-white'}
                  >
                    <TableCell className="text-center">
                      {disease.name}
                    </TableCell>
                    <TableCell className="text-center">
                      {disease.category}
                    </TableCell>
                    <TableCell className="text-center">
                      {disease.infected}
                    </TableCell>
                    <TableCell className="text-center">
                      {disease.vaccinated}
                    </TableCell>
                    <TableCell className="text-center">
                      <Button variant="link" className="p-0 text-blue-600">
                        {disease.action}
                      </Button>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </CardContent>
      </Card>
    </BasePages>
  );
}
