'use client';

import { useGetLoaiThuocV2, useThemThuocV2 } from '@/queries/vacxin.query';
import { zodResolver } from '@hookform/resolvers/zod';
import { useForm } from 'react-hook-form';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import {
  Form,
  FormControl,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import { Textarea } from '@/components/ui/textarea';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { useState } from 'react';
import { Loader2 } from 'lucide-react';
import { toast } from '@/components/ui/use-toast';
import __helpers from '@/helpers';

// Define form schema with validation
const formSchema = z.object({
  name: z.string().min(2, { message: 'Tên thuốc phải có ít nhất 2 ký tự' }),
  description: z.string().min(5, { message: 'Mô tả phải có ít nhất 5 ký tự' }),
  type: z.string({ required_error: 'Vui lòng chọn loại thuốc' }),
  createdBy: z.string().min(2, { message: 'Người tạo phải có ít nhất 2 ký tự' })
});

type FormValues = z.infer<typeof formSchema>;

export default function AddForm() {
  const { data: loaiThuoc = [], isPending } = useGetLoaiThuocV2();
  const { mutateAsync: createThuoc } = useThemThuocV2();
  const [isSubmitting, setIsSubmitting] = useState(false);

  // Initialize form
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      description: '',
      type: '',
      createdBy: __helpers.getUserEmail()
    }
  });

  // Handle form submission
  const onSubmit = async (values: FormValues) => {
    try {
      setIsSubmitting(true);
      await createThuoc(values);
      toast({
        title: 'Thêm thuốc thành công',
        description: `Đã thêm thuốc ${values.name} vào hệ thống`,
        variant: 'success'
      });
      form.reset();
    } catch (error) {
      console.error('Lỗi khi thêm thuốc:', error);
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi thêm thuốc. Vui lòng thử lại.',
        variant: 'destructive'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  if (isPending) {
    return (
      <div className="flex items-center justify-center">
        <Loader2 className="h-6 w-6 animate-spin" />
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6 rounded-lg border p-6 shadow-sm">
      <div>
        <h2 className="text-2xl font-bold">Thêm thuốc mới</h2>
        <p className="text-muted-foreground">
          Điền thông tin để thêm thuốc mới vào hệ thống
        </p>
      </div>

      <Form {...form}>
        <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-4">
          <FormField
            control={form.control}
            name="name"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Tên thuốc</FormLabel>
                <FormControl>
                  <Input placeholder="Nhập tên thuốc" {...field} />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="description"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Mô tả</FormLabel>
                <FormControl>
                  <Textarea
                    placeholder="Nhập mô tả về thuốc"
                    className="min-h-[100px]"
                    {...field}
                  />
                </FormControl>
                <FormMessage />
              </FormItem>
            )}
          />

          <FormField
            control={form.control}
            name="type"
            render={({ field }) => (
              <FormItem>
                <FormLabel>Loại thuốc</FormLabel>
                <Select
                  onValueChange={field.onChange}
                  defaultValue={field.value}
                >
                  <FormControl>
                    <SelectTrigger>
                      <SelectValue placeholder="Chọn loại thuốc" />
                    </SelectTrigger>
                  </FormControl>
                  <SelectContent>
                    {loaiThuoc
                      .filter((t): t is string => typeof t === 'string')
                      .map((type) => (
                        <SelectItem key={type} value={type}>
                          {type.replace(/_/g, ' ')}
                        </SelectItem>
                      ))}
                  </SelectContent>
                </Select>
                <FormMessage />
              </FormItem>
            )}
          />

          <Button type="submit" className="w-full" disabled={isSubmitting}>
            {isSubmitting ? (
              <>
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                Đang xử lý...
              </>
            ) : (
              'Thêm thuốc'
            )}
          </Button>
        </form>
      </Form>
    </div>
  );
}
