import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Add } from './components/add/index.js';

export default function ChiTietMaKiemDichPage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Mã kiểm dịch theo loài', link: '/ma-kiem-dich-theo-loai' },
          { title: 'Chi tiết mã của vật nuôi', link: '/user' }
        ]}
      >
        <Tabs defaultValue="overview" className="space-y-4">
          <TabsList>
            <TabsTrigger value="overview">
              Danh sách mã kiểm dịch của vật nuôi
            </TabsTrigger>
          </TabsList>
          <TabsContent value="overview" className="space-y-4">
            <OverViewTab />
          </TabsContent>
          <TabsContent value="add" className="space-y-4">
            <Add />
          </TabsContent>
        </Tabs>
      </BasePages>
    </>
  );
}
