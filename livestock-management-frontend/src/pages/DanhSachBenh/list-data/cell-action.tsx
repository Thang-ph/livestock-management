'use client';

import type React from 'react';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';

import { GeminiAnalysisDialog } from '@/components/shared/gemini-analysis';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger
} from '@/components/ui/dialog';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogTrigger
} from '@/components/ui/alert-dialog';
import {
  Form,
  FormControl,
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
import { Loader2, Edit, Trash2 } from 'lucide-react';
import __helpers from '@/helpers';
import {
  useDeleteBenh,
  useGetListLoaiBenhType,
  useUpdateBenh
} from '@/queries/benh.query';

interface BenhData {
  id: string;
  name: string;
  symptom: string;
  description: string;
  type: string;
  defaultInsuranceDuration: number;
  requestedBy?: string;
}

interface BenhCellActionProps {
  data: BenhData;
}

// Form schema for update
const updateFormSchema = z.object({
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

type UpdateFormData = z.infer<typeof updateFormSchema>;

export const CellAction: React.FC<BenhCellActionProps> = ({ data }) => {
  const [isUpdateDialogOpen, setIsUpdateDialogOpen] = useState(false);
  const [isUpdating, setIsUpdating] = useState(false);
  const [isDeleting, setIsDeleting] = useState(false);

  const { mutateAsync: updateBenh } = useUpdateBenh();
  const { mutateAsync: deleteBenh } = useDeleteBenh();
  const { data: listLoaiBenhType } = useGetListLoaiBenhType();

  // Initialize update form
  const updateForm = useForm<UpdateFormData>({
    resolver: zodResolver(updateFormSchema),
    defaultValues: {
      name: data.name,
      symptom: data.symptom,
      description: data.description,
      type: data.type,
      defaultInsuranceDuration: data.defaultInsuranceDuration
    }
  });

  // Handle update submission
  const handleUpdate = async (formData: UpdateFormData) => {
    try {
      setIsUpdating(true);
      const payload = {
        ...formData,
        requestedBy: __helpers.getUserEmail()
      };

      await updateBenh({ id: data.id, ...payload });

      toast({
        title: 'Thành công',
        description: 'Đã cập nhật thông tin bệnh thành công',
        variant: 'default'
      });

      setIsUpdateDialogOpen(false);
    } catch (error) {
      console.error('Error updating benh:', error);
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi cập nhật thông tin bệnh',
        variant: 'destructive'
      });
    } finally {
      setIsUpdating(false);
    }
  };

  // Handle delete
  const handleDelete = async () => {
    try {
      setIsDeleting(true);
      await deleteBenh(data.id);

      toast({
        title: 'Thành công',
        description: 'Đã xóa bệnh thành công',
        variant: 'default'
      });
    } catch (error) {
      console.error('Error deleting benh:', error);
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi xóa bệnh',
        variant: 'destructive'
      });
    } finally {
      setIsDeleting(false);
    }
  };

  const makePrompt = (data: BenhData) => {
    return `
Bạn là một bác sĩ thú y chuyên môn trong chăn nuôi gia súc. Hãy cung cấp thông tin chi tiết, ngắn gọn và dễ hiểu về loại bệnh sau để người chăn nuôi có thể nhận biết và xử lý kịp thời.

Thông tin bệnh:
- Tên bệnh: ${data.name}

Vui lòng trình bày bằng tiếng Việt, khoảng 250-500 từ, tập trung vào:
1. Triệu chứng thường gặp
2. Nguyên nhân
3. Hướng xử lý cơ bản hoặc phòng ngừa (nếu có)

Tránh dùng thuật ngữ chuyên môn khó hiểu. Trình bày rõ ràng.
`;
  };

  return (
    <>
      <div className="flex items-center gap-2">
        {/* Update Dialog */}
        <Dialog open={isUpdateDialogOpen} onOpenChange={setIsUpdateDialogOpen}>
          <DialogTrigger asChild>
            <Button
              variant="outline"
              size="sm"
              className="mb-2 border-blue-500"
            >
              <Edit className="mr-1 h-4 w-4" />
              Cập nhật
            </Button>
          </DialogTrigger>
          <DialogContent className="max-h-[90vh] max-w-2xl overflow-y-auto">
            <DialogHeader>
              <DialogTitle>Cập nhật thông tin bệnh</DialogTitle>
              <DialogDescription>
                Chỉnh sửa thông tin cho bệnh: {data.name}
              </DialogDescription>
            </DialogHeader>

            <Form {...updateForm}>
              <form
                onSubmit={updateForm.handleSubmit(handleUpdate)}
                className="space-y-4"
              >
                {/* Name Field */}
                <FormField
                  control={updateForm.control}
                  name="name"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Tên Bệnh *</FormLabel>
                      <FormControl>
                        <Input placeholder="Nhập tên bệnh" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Symptom Field */}
                <FormField
                  control={updateForm.control}
                  name="symptom"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Triệu Chứng *</FormLabel>
                      <FormControl>
                        <Textarea
                          placeholder="Mô tả các triệu chứng của bệnh"
                          className="min-h-[80px]"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Description Field */}
                <FormField
                  control={updateForm.control}
                  name="description"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Mô Tả *</FormLabel>
                      <FormControl>
                        <Textarea
                          placeholder="Mô tả chi tiết về bệnh"
                          className="min-h-[100px]"
                          {...field}
                        />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Type Field */}
                <FormField
                  control={updateForm.control}
                  name="type"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>Loại Bệnh *</FormLabel>
                      <Select
                        onValueChange={field.onChange}
                        value={field.value}
                      >
                        <FormControl>
                          <SelectTrigger>
                            <SelectValue placeholder="Chọn loại bệnh" />
                          </SelectTrigger>
                        </FormControl>
                        <SelectContent>
                          {listLoaiBenhType?.map((loaiBenh) => (
                            <SelectItem key={loaiBenh} value={loaiBenh}>
                              {loaiBenh.replace(/_/g, ' ')}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                {/* Default Insurance Duration Field */}
                <FormField
                  control={updateForm.control}
                  name="defaultInsuranceDuration"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>
                        Thời Gian Bảo Hiểm Mặc Định (ngày) *
                      </FormLabel>
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
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <DialogFooter>
                  <Button
                    type="button"
                    variant="outline"
                    onClick={() => setIsUpdateDialogOpen(false)}
                    disabled={isUpdating}
                  >
                    Hủy
                  </Button>
                  <Button type="submit" disabled={isUpdating}>
                    {isUpdating && (
                      <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                    )}
                    {isUpdating ? 'Đang cập nhật...' : 'Cập nhật'}
                  </Button>
                </DialogFooter>
              </form>
            </Form>
          </DialogContent>
        </Dialog>

        {/* Delete Alert Dialog */}
        <AlertDialog>
          <AlertDialogTrigger asChild>
            <Button variant="destructive" size="sm" className="mb-2">
              <Trash2 className="mr-1 h-4 w-4" />
              Xóa
            </Button>
          </AlertDialogTrigger>
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Xác nhận xóa</AlertDialogTitle>
              <AlertDialogDescription>
                Bạn có chắc chắn muốn xóa bệnh "{data.name}" không? Hành động
                này không thể hoàn tác.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Hủy</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDelete}
                disabled={isDeleting}
                className="bg-destructive text-destructive-foreground hover:bg-destructive/90"
              >
                {isDeleting && (
                  <Loader2 className="mr-2 h-4 w-4 animate-spin" />
                )}
                {isDeleting ? 'Đang xóa...' : 'Xóa'}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>
        <GeminiAnalysisDialog
          data={data}
          generatePrompt={makePrompt}
          label="Tìm hiểu"
        />
      </div>
    </>
  );
};
