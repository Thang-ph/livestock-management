import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import AddForm from './components/add/index.js';

export default function DanhSachGoiThauPage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Gói thầu', link: '/goi-thau' },
          { title: 'Danh sách gói thầu', link: '/danh-sach-goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 "></div>
        <Tabs defaultValue="overview" className="space-y-4">
          <TabsList>
            <TabsTrigger value="overview">Danh sách gói thầu</TabsTrigger>
            <TabsTrigger value="add">Thêm mới</TabsTrigger>
          </TabsList>
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
