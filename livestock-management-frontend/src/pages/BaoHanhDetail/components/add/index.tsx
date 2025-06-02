'use client';

import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { useGetSpecieType } from '@/queries/admin.query';
import { usseCreateSpecie } from '@/queries/vatnuoi.query';
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

// Define the form schema with validation
const formSchema = z.object({
  name: z.string().min(1, { message: 'Tên không được để trống' }),
  description: z.string().min(1, { message: 'Mô tả không được để trống' }),
  growthRate: z.coerce
    .number()
    .min(0, { message: 'Tỉ lệ tăng trưởng phải lớn hơn hoặc bằng 0' }),
  dressingPercentage: z.coerce
    .number()
    .min(0, { message: 'Tỉ lệ mặc định phải lớn hơn hoặc bằng 0' }),
  type: z.string().min(1, { message: 'Loại không được để trống' }),
  requestedBy: z
    .string()
    .min(1, { message: 'Người yêu cầu không được để trống' })
});

type FormValues = z.infer<typeof formSchema>;

export default function AddForm() {
  const { data: specieTypes = [] } = useGetSpecieType();
  const { mutateAsync: createSpecie } = usseCreateSpecie();
  const [isSubmitting, setIsSubmitting] = useState(false);
  // Initialize the form
  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      name: '',
      description: '',
      growthRate: 0,
      dressingPercentage: 0,
      type: '',
      requestedBy: __helpers.getUserEmail()
    }
  });

  // Handle form submission
  const onSubmit = async (values: FormValues) => {
    try {
      setIsSubmitting(true);
      const [err] = await createSpecie(values);
      if (err) {
        toast({
          title: 'Thêm thất bại',
          description: 'Đã xảy ra lỗi khi thêm vật nuôi. Vui lòng thử lại.',
          variant: 'destructive'
        });
        return;
      }
      toast({
        title: 'Thêm thành công',
        description: `Đã thêm ${values.name} vào danh sách vật nuôi.`,
        variant: 'success'
      });
      form.reset();
    } catch (error) {
      console.error('Error creating specie:', error);
      toast({
        title: 'Thêm thất bại',
        description: 'Đã xảy ra lỗi khi thêm vật nuôi. Vui lòng thử lại.',
        variant: 'destructive'
      });
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <Card className="mx-auto w-full max-w-2xl">
      <CardHeader>
        <CardTitle>Thêm vật nuôi mới</CardTitle>
        <CardDescription>
          Điền thông tin để thêm một loài vật nuôi mới vào hệ thống.
        </CardDescription>
      </CardHeader>
      <CardContent>
        <Form {...form}>
          <form onSubmit={form.handleSubmit(onSubmit)} className="space-y-6">
            <div className="grid grid-cols-1 gap-6 md:grid-cols-2">
              <FormField
                control={form.control}
                name="name"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tên</FormLabel>
                    <FormControl>
                      <Input placeholder="Nhập tên vật nuôi" {...field} />
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
                    <FormLabel>Loại</FormLabel>
                    <Select
                      onValueChange={field.onChange}
                      defaultValue={field.value}
                    >
                      <FormControl>
                        <SelectTrigger>
                          <SelectValue placeholder="Chọn loại vật nuôi" />
                        </SelectTrigger>
                      </FormControl>
                      <SelectContent>
                        {specieTypes.map((type) => {
                          console.log('type', type);
                          return (
                            <SelectItem key={type} value={type}>
                              {type.charAt(0) + type.slice(1).toLowerCase()}
                            </SelectItem>
                          );
                        })}
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="growthRate"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tỉ lệ tăng trưởng</FormLabel>
                    <FormControl>
                      <Input type="number" step="0.01" {...field} />
                    </FormControl>
                    <FormDescription>Đơn vị: %</FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormField
                control={form.control}
                name="dressingPercentage"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Tỉ lệ mặc định</FormLabel>
                    <FormControl>
                      <Input type="number" step="0.01" {...field} />
                    </FormControl>
                    <FormDescription>Đơn vị: %</FormDescription>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="description"
              render={({ field }) => (
                <FormItem>
                  <FormLabel>Mô tả</FormLabel>
                  <FormControl>
                    <Textarea
                      placeholder="Nhập mô tả về vật nuôi"
                      className="min-h-[120px]"
                      {...field}
                    />
                  </FormControl>
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
                'Thêm vật nuôi'
              )}
            </Button>
          </form>
        </Form>
      </CardContent>
    </Card>
  );
}
