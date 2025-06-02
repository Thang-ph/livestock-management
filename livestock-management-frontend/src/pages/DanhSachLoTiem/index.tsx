import { useEffect, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import BasePages from '@/components/shared/base-pages.js';
import { OverViewTab } from './components/overview/index.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import AddForm from './components/add/index.js';

const TABS_STORAGE_KEY = 'goi-thau-tab';
const VALID_TABS = ['overview', 'add'];

export default function DanhSachGoiThauPage() {
  const [searchParams, setSearchParams] = useSearchParams();
  const [selectedTab, setSelectedTab] = useState('overview');

  // Khởi tạo tab từ URL hoặc localStorage
  useEffect(() => {
    const urlTab = searchParams.get('tab');
    const savedTab = localStorage.getItem(TABS_STORAGE_KEY);

    // Ưu tiên tab từ URL, sau đó từ localStorage
    const initialTab =
      urlTab && VALID_TABS.includes(urlTab)
        ? urlTab
        : savedTab && VALID_TABS.includes(savedTab)
          ? savedTab
          : 'overview';

    setSelectedTab(initialTab);

    // Đồng bộ URL nếu cần
    if (!urlTab || urlTab !== initialTab) {
      const newParams = new URLSearchParams(searchParams);
      newParams.set('tab', initialTab);
      setSearchParams(newParams, { replace: true });
    }
  }, [searchParams, setSearchParams]);

  const handleTabChange = (value) => {
    if (!VALID_TABS.includes(value)) return;

    setSelectedTab(value);
    localStorage.setItem(TABS_STORAGE_KEY, value);

    // Cập nhật URL
    const newParams = new URLSearchParams(searchParams);
    newParams.set('tab', value);

    // Reset trang về 1 khi chuyển tab (nếu có pagination)
    if (newParams.has('page')) {
      newParams.set('page', '1');
    }

    setSearchParams(newParams);
  };

  return (
    <BasePages
      className="relative flex-1 space-y-4 overflow-y-auto px-4"
      breadcrumbs={[
        { title: 'Trang chủ', link: '/' },
        { title: 'Lô tiêm', link: '/goi-thau' }
      ]}
    >
      <div className="flex items-center justify-between space-y-2">
        {/* Có thể thêm các action buttons ở đây */}
      </div>

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
