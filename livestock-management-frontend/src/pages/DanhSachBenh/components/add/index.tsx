'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Button } from '@/components/ui/button';
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle
} from '@/components/ui/card';
import {
  Form,
  FormControl,
  FormDescription,
  FormField,
  FormItem,
  FormLabel,
  FormMessage
} from '@/components/ui/form';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import { Textarea } from '@/components/ui/textarea';
import { toast } from '@/components/ui/use-toast';
import { Loader2 } from 'lucide-react';
import __helpers from '@/helpers';
import { useCreateBenh, useGetListLoaiBenhType } from '@/queries/benh.query';

// Define the form schema with validation
const formSchema = z.object({
  name: z
    .string()
    .min(1, 'Tên bệnh là bắt buộc')
    .max(255, 'Tên bệnh không được quá 255 ký tự'),
  symptom: z.string().min(1, 'Triệu chứng là bắt buộc'),
  description: z.string().min(1, 'Mô tả là bắt buộc'),
  type: z.string().min(1, 'Loại bệnh là bắt buộc'),
  defaultInsuranceDuration: z
    .number()
    .min(0, 'Thời gian bảo hiểm mặc định phải là số không âm')
});

type FormData = z.infer<typeof formSchema>;

export default function AddForm() {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const { mutateAsync: createBenh } = useCreateBenh();
  const { data: listLoaiBenh } = useGetListLoaiBenhType();
  console.log('List Loai Benh:', listLoaiBenh);

  // Initialize the form
  const form = useForm<FormData>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      symptom: '',
      description: '',
      type: '',
      defaultInsuranceDuration: 0
    }
  });

  const onSubmit = async (data: FormData) => {
    try {
      setIsSubmitting(true);
      // Add requestedBy from helper function
      const payload = {
        ...data,
        requestedBy: __helpers.getUserEmail()
      };
      const [err] = await createBenh(payload);

      if (err) {
        toast({
          title: 'Lỗi',
          description: 'Có lỗi xảy ra khi tạo bệnh mới',
          variant: 'destructive'
        });
        return;
      }

      toast({
        title: 'Thành công',
        description: 'Đã tạo bệnh mới thành công',
        variant: 'success'
      });

      // Reset form after successful submission
      form.reset();
    } catch (error) {
      console.error('Error creating benh:', error);
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi tạo bệnh mới',
        variant: 'destructive'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="container mx-auto py-6">
      <Card className="mx-auto max-w-2xl">
        <CardHeader>
          <CardTitle>Thêm Bệnh Mới</CardTitle>
          <CardDescription>
            Điền thông tin để tạo một loại bệnh mới trong hệ thống
          </CardDescription>
        </CardHeader>
        <CardContent>
          <Form {...form}>
            <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
              {/* Name Field */}
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tên Bệnh *</FormLabel>
                    <FormControl>
                      <Input placeholder="Nhập tên bệnh" {...field} />
                    </FormControl>
                    <FormDescription>
                      Tên của loại bệnh (tối đa 255 ký tự)
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Symptom Field */}
              <FormField
                control={form.control}
                name="symptom"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Triệu Chứng *</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Mô tả các triệu chứng của bệnh"
                        className="min-h-[100px]"
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Mô tả chi tiết các triệu chứng đặc trưng của bệnh
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Description Field */}
              <FormField
                control={form.control}
                name="description"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Mô Tả *</FormLabel>
                    <FormControl>
                      <Textarea
                        placeholder="Mô tả chi tiết về bệnh"
                        className="min-h-[120px]"
                        {...field}
                      />
                    </FormControl>
                    <FormDescription>
                      Thông tin chi tiết về bệnh, nguyên nhân, cách điều trị,
                      v.v.
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Type Field */}
              <FormField
                control={form.control}
                name="type"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Loại Bệnh *</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Chọn loại bệnh" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {listLoaiBenh?.map((loaiBenh) => (
                          <SelectItem key={loaiBenh} value={loaiBenh}>
                            {loaiBenh.replace(/_/g, ' ')}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
                    <FormDescription>
                      Phân loại bệnh theo mức độ nghiêm trọng hoặc tính chất
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Default Insurance Duration Field */}
              <FormField
                control={form.control}
                name="defaultInsuranceDuration"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Thời Gian Bảo Hiểm Mặc Định (ngày) *</FormLabel>
                    <FormControl>
                      <Input
                        type="number"
                        min="0"
                        placeholder="Nhập số ngày"
                        {...field}
                        onChange={(e) =>
                          field.onChange(parseInt(e.target.value, 10) || 0)
                        }
                      />
                    </FormControl>
                    <FormDescription>
                      Số ngày bảo hiểm mặc định cho loại bệnh này
                    </FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Submit Button */}
              <div className="flex justify-end space-x-4">
                <Button
                  type="button"
                  variant="outline"
                  onClick={() => form.reset()}
                  disabled={isSubmitting}
                >
                  Đặt Lại
                </Button>
                <Button type="submit" disabled={isSubmitting}>
                  {isSubmitting && (
                    <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                  )}
                  {isSubmitting ? 'Đang Tạo...' : 'Tạo Bệnh'}
                </Button>
              </div>
            </form>
          </Form>
        </CardContent>
      </Card>
    </div>
  );
}
