import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { BiddingView } from './components/add/index.js';

export default function ChiTietGoiThauPage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Gói thầu', link: '/goi-thau' },
          { title: 'Danh sách gói thầu', link: '/danh-sach-goi-thau' },
          { title: 'Thông tin gói thầu', link: '/thong-tin-goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 "></div>
        <Tabs defaultValue="add" className="space-y-4">
          <TabsList>
            <TabsTrigger value="add">Thông tin gói thầu</TabsTrigger>
            <TabsTrigger value="overview">Danh sách khách hàng</TabsTrigger>
          </TabsList>
          <TabsContent value="overview" className="space-y-4">
            <OverViewTab />
          </TabsContent>
          <TabsContent value="add" className="space-y-4">
            <BiddingView />
          </TabsContent>
        </Tabs>
      </BasePages>
    </>
  );
}
