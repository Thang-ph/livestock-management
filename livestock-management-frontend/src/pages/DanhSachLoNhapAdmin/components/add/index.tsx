'use client';

import { useState } from 'react';
import { Calendar } from '@/components/ui/calendar';
import { Button } from '@/components/ui/button';
import {
  Popover,
  PopoverContent,
  PopoverTrigger
} from '@/components/ui/popover';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import { CalendarIcon, Loader2, Save } from 'lucide-react';
import { format } from 'date-fns';
import {
  Card,
  CardHeader,
  CardTitle,
  CardContent,
  CardFooter
} from '@/components/ui/card';
import { useToast } from '@/components/ui/use-toast';
import { useCreateLoNhap, useGetTraiNhap } from '@/queries/lo-nhap.query';

export default function AddForm() {
  const { toast } = useToast();

  // State cho các trường dữ liệu
  const [name, setName] = useState<string>('Nhập bò lai Sind cái');
  const [quantity, setQuantity] = useState<number>(100);
  const [originLocation, setOriginLocation] =
    useState<string>('Trại giống Sơn La');
  const [barnId, setBarnId] = useState<string>('');
  const [date, setDate] = useState<Date>(new Date());

  const { data: danhSachTrai, isLoading: isLoadingTraiNhap } = useGetTraiNhap();

  // Mutation để tạo lô nhập
  const { mutate: createLoNhap, isPending: isCreating } = useCreateLoNhap();

  // Xử lý khi lưu
  const handleSave = () => {
    // Kiểm tra dữ liệu
    if (!name.trim()) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng nhập tên lô nhập',
        variant: 'destructive'
      });
      return;
    }

    if (quantity <= 0) {
      toast({
        title: 'Lỗi',
        description: 'Số lượng dự kiến phải lớn hơn 0',
        variant: 'destructive'
      });
      return;
    }

    if (!originLocation.trim()) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng nhập nơi nhập',
        variant: 'destructive'
      });
      return;
    }

    if (!barnId) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng chọn trại nhập về',
        variant: 'destructive'
      });
      return;
    }

    // Tạo payload
    const payload = {
      name: name,
      estimatedQuantity: quantity,
      expectedCompletionDate: date.toISOString(),
      originLocation: originLocation,
      barnId: barnId,
      createdBy: 'SYS'
    };

    // Gọi API tạo lô nhập
    createLoNhap(payload, {
      onSuccess: () => {
        toast({
          title: 'Thành công',
          description: 'Đã lưu thông tin lô nhập',
          variant: 'success'
        });

        // Reset form sau khi lưu thành công (tùy chọn)
        // setName('');
        // setQuantity(0);
        // setOriginLocation('');
        // setBarnId('');
        // setDate(new Date());
      },
      onError: (error) => {
        toast({
          title: 'Lỗi',
          description: `Không thể lưu thông tin lô nhập: ${error instanceof Error ? error.message : 'Lỗi không xác định'}`,
          variant: 'destructive'
        });
      }
    });
  };

  return (
    <Card className="shadow-sm">
      <CardHeader className="pb-3">
        <CardTitle>Thông tin lô nhập</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="grid grid-cols-[180px_1fr] items-start gap-y-4">
          <div className="pt-2">Tên lô nhập:</div>
          <Textarea
            className="resize-none"
            value={name}
            onChange={(e) => setName(e.target.value)}
          />

          <div className="pt-2">Số lượng dự kiến:</div>
          <div className="flex items-center">
            <div className="relative w-32">
              <Input
                type="number"
                className="pr-8 text-center"
                value={quantity}
                onChange={(e) =>
                  setQuantity(Number.parseInt(e.target.value) || 0)
                }
              />
              <div className="absolute right-1 top-1 flex flex-col">
                <button
                  className="h-4 border border-gray-300 px-1 text-xs"
                  onClick={() => setQuantity(quantity + 1)}
                  type="button"
                >
                  ▲
                </button>
                <button
                  className="h-4 border border-gray-300 px-1 text-xs"
                  onClick={() => setQuantity(Math.max(0, quantity - 1))}
                  type="button"
                >
                  ▼
                </button>
              </div>
            </div>
            <span className="ml-2 text-gray-500">(con)</span>
          </div>

          <div className="pt-2">Nơi nhập:</div>
          <Textarea
            className="resize-none"
            value={originLocation}
            onChange={(e) => setOriginLocation(e.target.value)}
          />

          <div className="pt-2">Trại nhập về:</div>
          <Select value={barnId} onValueChange={setBarnId}>
            <SelectTrigger className="h-10 rounded-md">
              <SelectValue placeholder="Chọn trại nhập về" />
            </SelectTrigger>
            <SelectContent>
              {isLoadingTraiNhap ? (
                <div className="flex items-center justify-center p-2">
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  Đang tải...
                </div>
              ) : (
                danhSachTrai?.map((trai) => (
                  <SelectItem key={trai.id} value={trai.id}>
                    {trai.name} - {trai.address}
                  </SelectItem>
                ))
              )}
            </SelectContent>
          </Select>

          <div className="pt-2">Ngày dự kiến hoàn thành:</div>
          <Popover>
            <PopoverTrigger asChild>
              <Button
                variant="outline"
                className="h-10 w-48 justify-start px-3 text-left font-normal"
              >
                {date ? format(date, 'dd-MM-yyyy') : 'Chọn ngày'}
                <CalendarIcon className="ml-auto h-4 w-4 opacity-50" />
              </Button>
            </PopoverTrigger>
            <PopoverContent className="w-auto p-0" align="start">
              <Calendar
                mode="single"
                selected={date}
                onSelect={(date) => date && setDate(date)}
                initialFocus
              />
            </PopoverContent>
          </Popover>
        </div>
      </CardContent>
      <CardFooter className="flex justify-end pt-4">
        <Button onClick={handleSave} disabled={isCreating}>
          {isCreating ? (
            <>
              <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              Đang lưu...
            </>
          ) : (
            <>
              <Save className="mr-2 h-4 w-4" />
              Lưu
            </>
          )}
        </Button>
      </CardFooter>
    </Card>
  );
}
