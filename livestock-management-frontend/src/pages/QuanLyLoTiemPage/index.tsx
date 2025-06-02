'use client';

import { useState } from 'react';
import {
  ChevronDown,
  ChevronUp,
  ChevronLeft,
  ChevronRight,
  Search
} from 'lucide-react';
import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './lotiemnhaclai/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Card } from '@/components/ui/card.js';
import { Input } from '@/components/ui/input.js';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';

import { Button } from '@/components/ui/button';
import { LoTiemSapToiTab } from './lotiemsaptoi/overview/index.js';
import { useRouter } from '@/routes/hooks/use-router.js';
import { useGetGoiThauChuaDamBaoYeuCauTiemChung } from '@/queries/vacxin.query.js';

export default function QuanLyLoTiemPage() {
  const [sortBy, setSortBy] = useState('none');
  const [searchTerm, setSearchTerm] = useState('');
  const [showAllTenders, setShowAllTenders] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const itemsPerPage = 8;
  const { data: goiThauChuaDamBao } = useGetGoiThauChuaDamBaoYeuCauTiemChung();

  const getColorForProgress = (value, total) => {
    const percentage = (value / total) * 100;
    if (percentage >= 75) return 'bg-green-500';
    if (percentage >= 50) return 'bg-yellow-400';
    if (percentage >= 25) return 'bg-amber-500';
    return 'bg-red-500';
  };

  const filteredTenders = goiThauChuaDamBao
    ? goiThauChuaDamBao
        .filter((tender) => {
          if (
            searchTerm &&
            !tender.procurementName
              .toLowerCase()
              .includes(searchTerm.toLowerCase()) &&
            !tender.procurementCode
              .toLowerCase()
              .includes(searchTerm.toLowerCase())
          ) {
            return false;
          }
          return true;
        })
        .sort((a, b) => {
          if (sortBy === 'date') {
            const dateA = new Date(a.expirationDate);
            const dateB = new Date(b.expirationDate);
            return dateA.getTime() - dateB.getTime();
          } else if (sortBy === 'name') {
            return a.procurementName.localeCompare(b.procurementName);
          }
          return 0;
        })
    : [];

  // Tính toán phân trang
  const totalPages = Math.ceil(filteredTenders.length / itemsPerPage);
  const indexOfLastItem = currentPage * itemsPerPage;
  const indexOfFirstItem = indexOfLastItem - itemsPerPage;
  const currentItems = filteredTenders.slice(indexOfFirstItem, indexOfLastItem);

  // Xử lý chuyển trang
  const paginate = (pageNumber: number) => setCurrentPage(pageNumber);
  const nextPage = () =>
    setCurrentPage((prev) => Math.min(prev + 1, totalPages));
  const prevPage = () => setCurrentPage((prev) => Math.max(prev - 1, 1));

  // Hiển thị danh sách ban đầu hoặc danh sách đầy đủ
  const displayedTenders = showAllTenders
    ? currentItems
    : filteredTenders.slice(0, 4);

  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto px-2 sm:px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Quản lý lô tiêm', link: '/goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2">
          <div className="mx-auto w-full p-4">
            <div className="mb-6 flex flex-col items-start justify-between gap-2 sm:flex-row sm:items-center">
              <h1 className="text-xl font-bold sm:text-2xl">
                Các gói thầu chưa đảm bảo yêu cầu tiêm chủng
              </h1>
              <Button
                variant="outline"
                onClick={() => {
                  setShowAllTenders(!showAllTenders);
                  if (!showAllTenders) setCurrentPage(1);
                }}
                className="mt-2 flex items-center gap-1 sm:mt-0"
              >
                {showAllTenders ? 'Thu gọn' : 'Xem thêm'}
                {showAllTenders ? (
                  <ChevronUp className="h-4 w-4" />
                ) : (
                  <ChevronDown className="h-4 w-4" />
                )}
              </Button>
            </div>

            <div className="mb-6 flex flex-col items-end justify-between gap-4 md:flex-row">
              <div className="flex items-center gap-2">
                <span className="whitespace-nowrap">Sắp xếp theo</span>
                <Select value={sortBy} onValueChange={setSortBy}>
                  <SelectTrigger className="w-[180px]">
                    <SelectValue placeholder="Không sắp xếp" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="none">Không sắp xếp</SelectItem>
                    <SelectItem value="date">Ngày hoàn thành</SelectItem>
                    <SelectItem value="name">Tên gói thầu</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="w-full md:w-[30%]">
                <label className="mb-1 block text-sm">Tên/Mã gói thầu</label>
                <div className="relative">
                  <Input
                    type="text"
                    className="pl-9"
                    placeholder="Tìm kiếm..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                  />
                  <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 transform text-gray-400" />
                </div>
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
              {displayedTenders.map((tender) => (
                <TenderCard
                  key={tender.procurementId}
                  code={tender.procurementCode}
                  id={tender.procurementId}
                  title={tender.procurementName}
                  completionDate={new Date(
                    tender.expirationDate
                  ).toLocaleDateString('vi-VN')}
                  quantity={tender.livestockQuantity}
                  metrics={tender.diseaseRequires.map((disease) => ({
                    name: disease.diseaseName,
                    value: disease.hasDone,
                    total: tender.livestockQuantity,
                    percentage: Math.round(
                      (disease.hasDone / tender.livestockQuantity) * 100
                    ),
                    color: getColorForProgress(
                      disease.hasDone,
                      tender.livestockQuantity
                    )
                  }))}
                />
              ))}
            </div>

            {showAllTenders && totalPages > 1 && (
              <div className="mt-6 flex flex-wrap items-center justify-center gap-2">
                <Button
                  variant="outline"
                  size="icon"
                  onClick={prevPage}
                  disabled={currentPage === 1}
                  className="h-8 w-8"
                >
                  <ChevronLeft className="h-4 w-4" />
                </Button>

                <div className="flex items-center gap-1">
                  {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                    // Hiển thị tối đa 5 nút phân trang
                    let pageNum;
                    if (totalPages <= 5) {
                      pageNum = i + 1;
                    } else if (currentPage <= 3) {
                      pageNum = i + 1;
                    } else if (currentPage >= totalPages - 2) {
                      pageNum = totalPages - 4 + i;
                    } else {
                      pageNum = currentPage - 2 + i;
                    }

                    return (
                      <Button
                        key={i}
                        variant={
                          currentPage === pageNum ? 'default' : 'outline'
                        }
                        size="sm"
                        onClick={() => paginate(pageNum)}
                        className="h-8 w-8"
                      >
                        {pageNum}
                      </Button>
                    );
                  })}
                </div>

                <Button
                  variant="outline"
                  size="icon"
                  onClick={nextPage}
                  disabled={currentPage === totalPages}
                  className="h-8 w-8"
                >
                  <ChevronRight className="h-4 w-4" />
                </Button>
              </div>
            )}
          </div>
        </div>
        <Tabs defaultValue="overview" className="space-y-4">
          <TabsList className="w-full sm:w-auto">
            <TabsTrigger value="overview" className="flex-1 sm:flex-none">
              Các lô tiêm nhắc lại
            </TabsTrigger>
            <TabsTrigger value="add" className="flex-1 sm:flex-none">
              Các lô tiêm sắp tới
            </TabsTrigger>
          </TabsList>
          <TabsContent value="overview" className="space-y-4">
            <OverViewTab />
          </TabsContent>
          <TabsContent value="add" className="space-y-4">
            <LoTiemSapToiTab />
          </TabsContent>
        </Tabs>
      </BasePages>
    </>
  );
}

interface TenderCardProps {
  id: string;
  title: string;
  completionDate: string;
  code: string;
  quantity: number;
  metrics: {
    name: string;
    value: number;
    total: number;
    percentage: number;
    color: string;
  }[];
}

function TenderCard({
  id,
  title,
  code,
  completionDate,
  quantity,
  metrics
}: TenderCardProps) {
  const router = useRouter();
  return (
    <Card
      className="border-2 p-3 transition-colors duration-200 hover:border-primary/50 sm:p-4"
      onClick={() => router.push(`/chi-tiet-tiem-chung-goi-thau/${id}`)}
    >
      <div className="mb-3 flex flex-col items-start justify-between gap-2 sm:mb-4 sm:flex-row">
        <div>
          <h2 className="text-lg font-bold sm:text-xl">{title}</h2>
          <p className="text-gray-600">{code}</p>
        </div>
        <div className="text-left sm:text-right">
          <p className="text-sm text-gray-500">Hạn hoàn thành</p>
          <p className="font-medium text-red-500">{completionDate}</p>
        </div>
      </div>

      <p className="mb-3 sm:mb-4">Số lượng: {quantity} con</p>
      <p className="mb-3 border-t pt-2 font-bold">Các yêu cầu tiêm chủng</p>

      <div className="space-y-3 sm:space-y-4">
        {metrics.map((metric, index) => (
          <div key={index}>
            <div className="mb-1 flex justify-between">
              <span className="text-sm">{metric.name}</span>
              <span className="text-sm font-medium">
                {metric.value}/{metric.total} ({metric.percentage}%)
              </span>
            </div>
            <ProgressBar value={metric.percentage} color={metric.color} />
          </div>
        ))}
      </div>
    </Card>
  );
}

interface ProgressBarProps {
  value: number;
  color: string;
  max?: number;
}

export function ProgressBar({ value, color, max = 100 }: ProgressBarProps) {
  return (
    <div className="h-4 w-full overflow-hidden rounded-full bg-gray-200">
      <div
        className={`h-full ${color} rounded-full`}
        style={{ width: `${value}%` }}
      />
    </div>
  );
}
