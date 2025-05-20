import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { ChiTiet } from './components/detail/index.js';
import VaccinationBatchManagement from './components/thongtinlotiem/index.js';

export default function ChiTietLoTiemPage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Lô tiêm', link: '/lo-tiem' },
          { title: 'Chi tiết lô tiêm', link: '/chi-tiet-lo-tiem' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 "></div>
        <Tabs defaultValue="chitiet" className="space-y-4">
          <TabsList>
            <TabsTrigger value="chitiet">Thông tin lô tiêm</TabsTrigger>
            <TabsTrigger value="danhsach">Danh sách vật nuôi</TabsTrigger>
            <TabsTrigger value="thongtin">Thống kê lô tiêm</TabsTrigger>
          </TabsList>
          <TabsContent value="chitiet" className="space-y-4">
            <ChiTiet />
          </TabsContent>
          <TabsContent value="danhsach" className="space-y-4">
            <OverViewTab />
          </TabsContent>
          <TabsContent value="thongtin" className="space-y-4">
            <VaccinationBatchManagement />
          </TabsContent>
        </Tabs>
      </BasePages>
    </>
  );
}
