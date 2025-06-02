import BasePages from '@/components/shared/base-pages.js';
import OverviewTab from './components/overview/index.js';

export default function BaoHanhDetailpage() {
  return (
    <>
      <BasePages
        className="relative flex-1 space-y-4 overflow-y-auto  px-4"
        breadcrumbs={[
          { title: 'Trang chủ', link: '/' },
          { title: 'Bảo hành', link: '/bao-hanh' },

          { title: 'Chi tiết bảo hành', link: '/goi-thau' }
        ]}
      >
        <div className="top-4 flex items-center justify-between space-y-2 ">
          <OverviewTab />
        </div>
      </BasePages>
    </>
  );
}
