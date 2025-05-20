import { SidebarProvider } from '@/components/ui/sidebar';
import AppSidebar from '../shared/app-sidebar';
import __helpers from '@/helpers';

export default function DashboardLayout({
  children
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex h-full min-h-screen  ">
      <SidebarProvider defaultOpen={true}>
        <AppSidebar />
        <main className="w-full">{children}</main>
      </SidebarProvider>
    </div>
  );
}
