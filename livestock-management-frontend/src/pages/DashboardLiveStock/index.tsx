'use client';

import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow
} from '@/components/ui/table';
import { MoreHorizontal } from 'lucide-react';
import { useEffect, useRef } from 'react';
import Chart from 'chart.js/auto';
import BasePages from '@/components/shared/base-pages';
import { useRouter } from '@/routes/hooks';
import { useGetLiveStockManagementDashboard } from '@/queries/admin.query';

export default function LivestockDashboard() {
  const pieChartRef = useRef<HTMLCanvasElement>(null);
  const barChartRef = useRef<HTMLCanvasElement>(null);
  const pieChartInstance = useRef<Chart | null>(null);
  const barChartInstance = useRef<Chart | null>(null);
  const router = useRouter();
  const { data: dataDashboard } = useGetLiveStockManagementDashboard();
  // Helper function to get severity color
  const getSeverityColor = (severity: string) => {
    switch (severity) {
      case 'HIGH':
        return 'text-red-500';
      case 'MEDIUM':
        return 'text-orange-500';
      case 'LOW':
        return 'text-gray-600';
      default:
        return 'text-gray-600';
    }
  };

  // Helper function to get species type name
  const getSpeciesTypeName = (specieType: number) => {
    switch (specieType) {
      case 0:
        return 'Bò';
      case 1:
        return 'Trâu';
      case 4:
        return 'Dê';
      default:
        return 'Khác';
    }
  };

  // Helper function to get species type color for pie chart
  const getSpeciesTypeColor = (specieType: number) => {
    switch (specieType) {
      case 0:
        return '#22c55e'; // Green for Bò
      case 1:
        return '#84cc16'; // Lime for Trâu
      case 4:
        return '#eab308'; // Yellow for Dê
      default:
        return '#9ca3af'; // Gray for others
    }
  };

  // Weight ranges and their colors
  const weightRanges = [
    { range: '<90 kg', color: '#69d509' },
    { range: '90 - 130 kg', color: '#2e990a' },
    { range: '130 - 160 kg', color: '#eab308' },
    { range: '160 - 190 kg', color: '#f97316' },
    { range: '190 - 250 kg', color: '#ef4444' },
    { range: '>250 kg', color: '#dc2626' }
  ];

  useEffect(() => {
    if (!dataDashboard) return;

    // Pie Chart for species distribution
    if (pieChartRef.current) {
      if (pieChartInstance.current) {
        pieChartInstance.current.destroy();
      }

      const ctx = pieChartRef.current.getContext('2d');
      if (ctx) {
        // Group species data by type (assuming we need to aggregate by species type)
        const speciesTypeMap = new Map();

        dataDashboard.inspectionCodeQuantitySummary.items.forEach((item) => {
          const typeName = getSpeciesTypeName(item.specie_Type);
          const existing = speciesTypeMap.get(typeName) || 0;
          speciesTypeMap.set(typeName, existing + item.totalQuantity);
        });

        const labels = Array.from(speciesTypeMap.keys());
        const data = Array.from(speciesTypeMap.values());
        const colors = labels.map((_, index) => {
          const specieType =
            dataDashboard.inspectionCodeQuantitySummary.items.find(
              (item) => getSpeciesTypeName(item.specie_Type) === labels[index]
            )?.specie_Type || 0;
          return getSpeciesTypeColor(specieType);
        });

        pieChartInstance.current = new Chart(ctx, {
          type: 'pie',
          data: {
            labels,
            datasets: [
              {
                data,
                backgroundColor: colors,
                borderWidth: 0
              }
            ]
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
              legend: {
                display: false
              }
            }
          }
        });
      }
    }

    // Stacked Bar Chart for weight distribution
    if (barChartRef.current) {
      if (barChartInstance.current) {
        barChartInstance.current.destroy();
      }

      const ctx = barChartRef.current.getContext('2d');
      if (ctx) {
        const weightData = dataDashboard.weightRatioSummary.items;

        // Prepare datasets for stacked bar chart
        const datasets = weightRanges.map((weightRange) => ({
          label: weightRange.range,
          data: weightData.map((species) => {
            const rangeData = species.weightRatios.find(
              (r) => r.weightRange === weightRange.range
            );
            return rangeData ? rangeData.quantity : 0;
          }),
          backgroundColor: weightRange.color,
          borderWidth: 0
        }));

        barChartInstance.current = new Chart(ctx, {
          type: 'bar',
          data: {
            labels: weightData.map((item) =>
              item.specieName.replace(' ', '\n')
            ),
            datasets: datasets
          },
          options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
              x: {
                stacked: true,
                grid: {
                  display: false
                },
                ticks: {
                  font: {
                    size: 9
                  },
                  maxRotation: 0,
                  minRotation: 0
                }
              },
              y: {
                stacked: true,
                beginAtZero: true,
                grid: {
                  color: '#e5e7eb'
                },
                ticks: {
                  font: {
                    size: 10
                  }
                }
              }
            },
            plugins: {
              legend: {
                display: false
              },
              tooltip: {
                mode: 'index',
                intersect: false,
                callbacks: {
                  title: (context) => context[0].label,
                  label: (context) => {
                    const datasetLabel = context.dataset.label;
                    const value = context.parsed.y;
                    return `${datasetLabel}: ${value} con`;
                  },
                  footer: (context) => {
                    const total = context.reduce(
                      (sum, item) => sum + item.parsed.y,
                      0
                    );
                    return `Tổng: ${total} con`;
                  }
                }
              }
            },
            interaction: {
              mode: 'index',
              intersect: false
            }
          }
        });
      }
    }

    return () => {
      if (pieChartInstance.current) {
        pieChartInstance.current.destroy();
      }
      if (barChartInstance.current) {
        barChartInstance.current.destroy();
      }
    };
  }, [dataDashboard]);

  if (!dataDashboard) {
    return (
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto px-6"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Quản lý chăn nuôi', link: '/goi-thau' }
        ]}
      >
        <div className="flex h-64 items-center justify-center">
          <div className="text-lg">Đang tải dữ liệu...</div>
        </div>
      </BasePages>
    );
  }

  // Calculate total animals from species data
  const totalAnimals = dataDashboard.specieRatioSummary.total;

  // Calculate total animals from weight data
  const totalAnimalsFromWeight = dataDashboard.weightRatioSummary.total;

  return (
    <BasePages
      className="relative flex-1 space-y-4 overflow-y-auto px-6"
      breadcrumbs={[
        { title: 'Trang chủ', link: '/' },
        { title: 'Quản lý chăn nuôi', link: '/goi-thau' }
      ]}
    >
      {/* Top Row - 4 cards */}
      <h1 className="mb-4 text-2xl font-semibold text-gray-800 dark:text-gray-200">
        Quản lý chăn nuôi
      </h1>
      <div className="mb-4 grid grid-cols-12 gap-4">
        {/* Disease in herd table - 2 columns */}
        <Card className="col-span-2">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium">
                Dịch bệnh trong đàn
              </CardTitle>
              <Button variant="outline" size="sm" className="h-6 text-xs">
                Xuất báo cáo
              </Button>
            </div>
          </CardHeader>
          <CardContent className="pt-0">
            <Table>
              <TableHeader>
                <TableRow className="text-xs">
                  <TableHead className="p-1 text-xs">Tên dịch bệnh</TableHead>
                  <TableHead className="p-1 text-xs">Số lượng</TableHead>
                  <TableHead className="p-1 text-xs">Tỷ lệ</TableHead>
                  <TableHead className="p-1 text-xs">Chi tiết</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {dataDashboard.diseaseRatioSummary.items.map(
                  (disease, index) => (
                    <TableRow key={index} className="text-xs">
                      <TableCell
                        className={`p-1 font-medium ${getSeverityColor(disease.severity)}`}
                      >
                        {disease.diseaseName}
                      </TableCell>
                      <TableCell
                        className={`p-1 ${getSeverityColor(disease.severity)}`}
                      >
                        {disease.quantity}
                      </TableCell>
                      <TableCell
                        className={`p-1 ${getSeverityColor(disease.severity)}`}
                      >
                        {Math.round(disease.ratio * 100)}%
                      </TableCell>
                      <TableCell className="p-1">
                        <MoreHorizontal className="h-3 w-3" />
                      </TableCell>
                    </TableRow>
                  )
                )}
                <TableRow className="text-xs font-semibold">
                  <TableCell className="p-1 text-red-500">Tổng</TableCell>
                  <TableCell className="p-1 text-red-500">
                    {dataDashboard.diseaseRatioSummary.total}
                  </TableCell>
                  <TableCell className="p-1 text-red-500">
                    {Math.round(
                      dataDashboard.diseaseRatioSummary.totalRatio * 100
                    )}
                    %
                  </TableCell>
                  <TableCell className="p-1"></TableCell>
                </TableRow>
              </TableBody>
            </Table>
          </CardContent>
        </Card>

        {/* Vaccination rates - 4 columns */}
        <Card className="col-span-4">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">
              Tỷ lệ vật nuôi đã tiêm phòng theo dịch bệnh
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-0">
            <Table>
              <TableHeader>
                <TableRow className="text-xs">
                  <TableHead className="p-1 text-xs">Tên dịch bệnh</TableHead>
                  <TableHead className="p-1 text-xs">Tỷ lệ</TableHead>
                  <TableHead className="p-1 text-xs">Hành động</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {dataDashboard.vaccinationRatioSummary.items.map(
                  (item, index) => (
                    <TableRow key={index} className="text-xs">
                      <TableCell
                        className={`p-1 font-medium ${getSeverityColor(item.severity)}`}
                      >
                        {item.diseaseName}
                      </TableCell>
                      <TableCell className="p-1">
                        {Math.round(item.ratio * 100)}%
                      </TableCell>
                      <TableCell className="flex space-x-2 p-1 text-blue-600">
                        <button
                          className="text-blue-600 hover:underline"
                          onClick={() => {
                            router.push(`/quan-ly-vat-nuoi-thong-ke`);
                          }}
                        >
                          Danh sách
                        </button>
                        {item.ratio < 0.7 && (
                          <button className="text-blue-600 hover:underline">
                            | Tạo lô tiêm
                          </button>
                        )}
                      </TableCell>
                    </TableRow>
                  )
                )}
              </TableBody>
            </Table>
            <p className="mt-2 text-xs text-gray-500">
              * Tỷ lệ tiêm chủng dưới 70% là chưa đạt yêu cầu miễn dịch
            </p>
          </CardContent>
        </Card>

        {/* Animals without identification - 3 columns */}
        <div className="col-span-3 space-y-4">
          <Card>
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium">
                Vật nuôi chưa đủ thông tin định danh
              </CardTitle>
            </CardHeader>
            <CardContent className="flex h-24 flex-col items-center justify-center pt-0">
              <div className="mb-2 text-4xl font-bold">
                {dataDashboard.totalLivestockMissingInformation}
              </div>
              <Button
                variant="outline"
                size="sm"
                className="h-6 text-xs"
                onClick={() => {
                  router.push('/quan-ly-vat-nuoi-thong-ke');
                }}
              >
                Xem danh sách
              </Button>
            </CardContent>
          </Card>
          <Card className="col-span-2">
            <CardHeader className="pb-2">
              <CardTitle className="text-sm font-medium">
                Số dịch bệnh đã ghi nhận trong đàn
              </CardTitle>
            </CardHeader>
            <CardContent className="flex h-24 flex-col items-center justify-center pt-0">
              <div className="mb-2 text-4xl font-bold">
                {dataDashboard.totalDisease}
              </div>
              <Button
                variant="outline"
                size="sm"
                className="h-6 text-xs"
                onClick={() => {
                  router.push('/thong-ke-dich-benh');
                }}
              >
                Thống kê dịch bệnh
              </Button>
            </CardContent>
          </Card>
        </div>

        {/* Quarantine code distribution - 3 columns */}
        <Card className="col-span-3">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium">
                Phân bố mã kiểm dịch theo loại
              </CardTitle>
              <Button variant="outline" size="sm" className="h-6 text-xs">
                Xuất báo cáo
              </Button>
            </div>
          </CardHeader>
          <CardContent className="pt-0">
            <Table>
              <TableHeader>
                <TableRow className="text-xs">
                  <TableHead className="p-1 text-xs">Loại</TableHead>
                  <TableHead className="p-1 text-xs">Còn lại</TableHead>
                  <TableHead className="p-1 text-xs">Chi tiết</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {dataDashboard.inspectionCodeQuantitySummary.items.map(
                  (item, index) => (
                    <TableRow key={index} className="text-xs">
                      <TableCell className="p-1 font-medium">
                        {getSpeciesTypeName(item.specie_Type)}
                      </TableCell>
                      <TableCell className="p-1">
                        {item.remainingQuantity}
                      </TableCell>
                      <TableCell className="p-1">
                        <MoreHorizontal className="h-3 w-3" />
                      </TableCell>
                    </TableRow>
                  )
                )}
              </TableBody>
            </Table>
          </CardContent>
        </Card>
      </div>

      {/* Bottom Row - 3 cards */}
      <div className="grid grid-cols-12 gap-4">
        {/* Breed distribution pie chart - 3 columns */}
        <Card className="col-span-3">
          <CardHeader className="pb-2">
            <CardTitle className="text-sm font-medium">
              Phân bố vật nuôi theo giống
            </CardTitle>
          </CardHeader>
          <CardContent className="pt-0">
            <div className="flex items-center justify-between">
              <div className="h-24 w-24">
                <canvas ref={pieChartRef}></canvas>
              </div>
              <div className="ml-4 flex flex-col space-y-1">
                {dataDashboard.inspectionCodeQuantitySummary.items.map(
                  (item, index) => (
                    <div
                      key={index}
                      className="flex items-center space-x-2 text-xs"
                    >
                      <div
                        className="h-3 w-3 rounded-sm"
                        style={{
                          backgroundColor: getSpeciesTypeColor(item.specie_Type)
                        }}
                      ></div>
                      <span>{getSpeciesTypeName(item.specie_Type)}</span>
                    </div>
                  )
                )}
                <div className="mt-2 text-xs font-medium">
                  Tổng: {totalAnimals} con
                </div>
              </div>
            </div>
            <div className="mt-3">
              <Button
                variant="outline"
                size="sm"
                className="h-6 w-full text-xs"
                onClick={() => {
                  router.push('/quan-ly-vat-nuoi-thong-ke');
                }}
              >
                Xem danh sách các loại
              </Button>
            </div>
          </CardContent>
        </Card>

        {/* Weight distribution stacked bar chart - 9 columns */}
        <Card className="col-span-9">
          <CardHeader className="pb-2">
            <div className="flex items-center justify-between">
              <CardTitle className="text-sm font-medium">
                Phân bố vật nuôi theo cân nặng
              </CardTitle>
              <Button variant="outline" size="sm" className="h-6 text-xs">
                Xuất báo cáo
              </Button>
            </div>
          </CardHeader>
          <CardContent className="pt-0">
            <div className="mb-2 flex justify-end">
              <div className="flex flex-col space-y-1 text-xs">
                {weightRanges.map((range, index) => (
                  <div key={index} className="flex items-center space-x-2">
                    <div
                      className="h-2 w-3"
                      style={{ backgroundColor: range.color }}
                    ></div>
                    <span>{range.range}</span>
                  </div>
                ))}
                <div className="font-medium">
                  Tổng: {totalAnimalsFromWeight} con
                </div>
              </div>
            </div>
            <div className="h-40">
              <canvas ref={barChartRef}></canvas>
            </div>
          </CardContent>
        </Card>
      </div>
    </BasePages>
  );
}
