import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import AddForm from './components/add/index.js';
import { AddType } from './components/add-type/index.js';

export default function DanhSachThuoc() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Danh sách thuốc', link: '/goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 "></div>
        <Tabs defaultValue="overview" className="space-y-4">
          <TabsList>
            <TabsTrigger value="overview">Danh sách thuốc</TabsTrigger>
            <TabsTrigger value="add">Thêm thuốc</TabsTrigger>
            {/* <TabsTrigger value="add-type">Thêm loại thuốc</TabsTrigger> */}
          </TabsList>
          <TabsContent value="overview" className="space-y-4">
            <OverViewTab />
          </TabsContent>
          <TabsContent value="add" className="space-y-4">
            <AddForm />
          </TabsContent>
          <TabsContent value="add-type" className="space-y-4">
            <AddType />
          </TabsContent>
        </Tabs>
      </BasePages>
    </>
  );
}
