import { useEffect, useState } from 'react';
import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import AddForm from './components/add/index.js';

const TABS_STORAGE_KEY = 'goi-thau-tab';

export default function DanhSachGoiThauPage() {
  const [selectedTab, setSelectedTab] = useState('overview');

  useEffect(() => {
    const savedTab = localStorage.getItem(TABS_STORAGE_KEY);
    if (savedTab) {
      setSelectedTab(savedTab);
    }
  }, []);

  const handleTabChange = (value) => {
    setSelectedTab(value);
    localStorage.setItem(TABS_STORAGE_KEY, value);
  };

  return (
    <BasePages
      className="relative flex-1 space-y-4 overflow-y-auto px-4"
      breadcrumbs={[
        { title: 'Trang chủ', link: '/' },
        { title: 'Lô tiêm', link: '/goi-thau' }
      ]}
    >
      <div className="top-4 flex items-center justify-between space-y-2 "></div>
      <Tabs
        value={selectedTab}
        onValueChange={handleTabChange}
        className="space-y-4"
      >
        <TabsList>
          <TabsTrigger value="overview">Danh sách lô tiêm</TabsTrigger>
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
  );
}
