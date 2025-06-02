import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import ChiTiet from './components/detail/index.js';

export default function ChiTietLoNhapPage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Lộ nhập', link: '/admin/lo-nhap' },
          { title: 'Chi tiết lô nhập', link: '/goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 "></div>
        <Tabs defaultValue="chitiet" className="space-y-4">
          <TabsList>
            <TabsTrigger value="chitiet">Thông tin lô nhập</TabsTrigger>
            <TabsTrigger value="danhsach">Danh sách vật nuôi</TabsTrigger>
          </TabsList>
          <TabsContent value="chitiet" className="space-y-4">
            <ChiTiet />
          </TabsContent>
          <TabsContent value="danhsach" className="space-y-4">
            <OverViewTab />
          </TabsContent>
        </Tabs>
      </BasePages>
    </>
  );
}
