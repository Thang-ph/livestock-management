import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar';

export default function RecentSales() {
  return (
    <div className="space-y-8 overflow-auto">
      <div className="flex items-center">
        <Avatar className="h-9 w-9">
          <AvatarImage src="/avatars/01.png" alt="Avatar" />
          <AvatarFallback>OM</AvatarFallback>
        </Avatar>
        <div className="ml-4 space-y-1">
          <p className="text-sm font-medium leading-none">
            Giày thể theo Air Jordan
          </p>
          <p className="text-sm text-muted-foreground">Mã sản phẩm : G-002</p>
        </div>
        <div className="ml-auto font-medium">
          <p className="text-end">Số lượng: 34 sản phẩm</p>
          <p className="text-green-500">Tổng doanh thu: 20.000.000 đ</p>
        </div>
      </div>
      <div className="flex items-center">
        <Avatar className="h-9 w-9">
          <AvatarImage src="/avatars/01.png" alt="Avatar" />
          <AvatarFallback>OM</AvatarFallback>
        </Avatar>
        <div className="ml-4 space-y-1">
          <p className="text-sm font-medium leading-none">
            Giày thể theo Air Jordan
          </p>
          <p className="text-sm text-muted-foreground">Mã sản phẩm : G-002</p>
        </div>
        <div className="ml-auto font-medium">
          <p className="text-end">Số lượng: 12 sản phẩm</p>
          <p className="text-green-500">Tổng doanh thu: 12.000.000 đ</p>
        </div>
      </div>
      <div className="flex items-center">
        <Avatar className="h-9 w-9">
          <AvatarImage src="/avatars/01.png" alt="Avatar" />
          <AvatarFallback>OM</AvatarFallback>
        </Avatar>
        <div className="ml-4 space-y-1">
          <p className="text-sm font-medium leading-none">
            Giày thể theo Air Jordan
          </p>
          <p className="text-sm text-muted-foreground">Mã sản phẩm : G-002</p>
        </div>
        <div className="ml-auto font-medium">
          <p className="text-end">Số lượng: 12 sản phẩm</p>
          <p className="text-green-500">Tổng doanh thu: 12.000.000 đ</p>
        </div>
      </div>
    </div>
  );
}
