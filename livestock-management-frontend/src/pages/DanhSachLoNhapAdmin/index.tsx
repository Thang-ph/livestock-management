import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import AddForm from './components/add/index.js';
import { PinPage } from './components/pin/index.js';

export default function DanhSachLoNhapAdmin() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Lô nhập', link: '/goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 "></div>
        <Tabs defaultValue="pin" className="space-y-4">
          <TabsList>
            <TabsTrigger value="pin">Ghim</TabsTrigger>
            <TabsTrigger value="overview">Danh sách lô nhập</TabsTrigger>
            <TabsTrigger value="add">Tạo lô nhập</TabsTrigger>
          </TabsList>
          <TabsContent value="pin" className="space-y-4">
            <PinPage />
          </TabsContent>
          <TabsContent value="overview" className="space-y-4">
            <OverViewTab />
          </TabsContent>
          <TabsContent value="add" className="space-y-4">
            <AddForm />
          </TabsContent>
        </Tabs>
      </BasePages>
    </>
  );
}
