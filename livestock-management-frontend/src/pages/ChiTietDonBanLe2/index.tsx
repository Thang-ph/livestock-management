import BasePages from '@/components/shared/base-pages.js';
import OverviewTab from './components/overview/index.js';
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger
} from '@/components/ui/tabs.js';
import { Detail } from './components/add/index.js';
export default function ChiTietDonBanLepage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Danh sách đơn mua lẻ ', link: '/danh-sach-don-mua-le' },

          { title: 'Chi tiết đơn bán lẻ', link: '/goi-thau' }
        ]}
      >
        <div className="top-4 items-center justify-between space-y-2 ">
          <Tabs defaultValue="overview" className="space-y-4">
            <TabsList>
              <TabsTrigger value="overview">Thông tin đơn </TabsTrigger>
              <TabsTrigger value="add">Danh sách loài vật</TabsTrigger>
            </TabsList>
            <TabsContent value="overview" className="space-y-4">
              <OverviewTab />
            </TabsContent>
            <TabsContent value="add" className="space-y-4">
              <Detail />
            </TabsContent>
          </Tabs>
        </div>
      </BasePages>
    </>
  );
}
