'use client';
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
import { useDeleteThuoc, useUpdateThuoc } from '@/queries/vacxin.query';
import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { useGetListDanhSachBenh } from '@/queries/donmuale.query';
import { toast } from '@/components/ui/use-toast';

// Schema validation cho form update - đã thêm diseaseId
const updateThuocSchema = z.object({
  name: z.string().min(1, 'Tên thuốc không được để trống'),
  description: z.string().min(1, 'Mô tả không được để trống'),
  type: z.enum(['VACCINE', 'THUỐC_CHỮA_BỆNH', 'KHÁNG_SINH'], {
    required_error: 'Vui lòng chọn loại thuốc'
  }),
  diseaseId: z.string().min(1, 'Vui lòng chọn loại bệnh')
});

type UpdateThuocFormData = z.infer<typeof updateThuocSchema>;

interface UserCellActionProps {
  data: {
    id: number;
    name: string;
    email: string;
    role: string;
    type?: string;
    description?: string;
    diseaseId?: string; // Thêm diseaseId vào interface
  };
}

export const CellAction: React.FC<UserCellActionProps> = ({ data }) => {
  const { mutateAsync: updateThuoc, isPending: isUpdating } = useUpdateThuoc();
  const { mutateAsync: deleteThuoc, isPending: isDeleting } = useDeleteThuoc();
  const { data: resListDanhSach } = useGetListDanhSachBenh();
  const listDanhSachBenh = resListDanhSach?.items || [];
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [isUpdateDialogOpen, setIsUpdateDialogOpen] = useState(false);

  // Form cho update - đã thêm diseaseId
  const form = useForm<UpdateThuocFormData>({
    resolver: zodResolver(updateThuocSchema),
    defaultValues: {
      name: data.name || '',
      description: data.description || '',
      type:
        (data.type as 'VACCINE' | 'THUỐC_CHỮA_BỆNH' | 'KHÁNG_SINH') ||
        undefined,
      diseaseId: data.diseaseId || '' // Thêm default value cho diseaseId
    }
  });

  const makePrompt = (data: any) => {
    return `
  Bạn là một chuyên viên y tế trong trang trại. Hãy cung cấp thông tin chi tiết, dễ hiểu và ngắn gọn về loại thuốc sau để người chăn nuôi hiểu rõ hơn khi sử dụng.

  Thông tin thuốc:
  - Tên thuốc: ${data.name}
  - Loại thuốc: ${data.type}
  - Mô tả: ${data.description}

  Vui lòng trình bày nội dung bằng tiếng Việt, rõ ràng, khoảng 200-500 từ. Tập trung vào công dụng, công thức, cách dùng cơ bản, và những lưu ý khi sử dụng.
  `;
  };

  // Xử lý xóa
  const handleDelete = async () => {
    try {
      const [err] = await deleteThuoc(data.id as any);
      console.log('Delete response:', err);
      if (err) {
        toast({
          title: 'Lỗi',
          description:
            err?.data?.data || 'Có lỗi xảy ra khi xóa thuốc. Vui lòng thử lại.',
          variant: 'destructive'
        });
        return;
      }
      toast({
        title: 'Xóa thuốc thành công',
        description: `Đã xóa thuốc ${data.name} khỏi hệ thống`,
        variant: 'success'
      });
      setIsDeleteDialogOpen(false);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi xóa thuốc. Vui lòng thử lại.',
        variant: 'destructive'
      });
      console.error('Delete error:', error);
    }
  };

  // Xử lý cập nhật - đã thêm diseaseId vào payload
  const handleUpdate = async (formData: UpdateThuocFormData) => {
    try {
      const updatePayload = {
        id: data.id.toString(),
        name: formData.name,
        description: formData.description,
        type: formData.type,
        diseaseId: formData.diseaseId, // Thêm diseaseId vào payload
        createdAt: new Date().toISOString()
      };

      await updateThuoc(updatePayload);
      toast({
        title: 'Cập nhật thuốc thành công',
        description: `Đã cập nhật thông tin thuốc ${data.name}`,
        variant: 'success'
      });
      setIsUpdateDialogOpen(false);
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Có lỗi xảy ra khi cập nhật thuốc. Vui lòng thử lại.',
        variant: 'destructive'
      });
      console.error('Update error:', error);
    }
  };

  return (
    <div className="flex items-center space-x-2">
      {/* Delete Dialog */}
      <Dialog open={isDeleteDialogOpen} onOpenChange={setIsDeleteDialogOpen}>
        <DialogTrigger asChild>
          <Button variant="destructive" size="sm">
            Xóa
          </Button>
        </DialogTrigger>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Xác nhận xóa thuốc</DialogTitle>
            <DialogDescription>
              Bạn có chắc chắn muốn xóa thuốc "{data.name}"? Hành động này không
              thể hoàn tác.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button
              variant="outline"
              onClick={() => setIsDeleteDialogOpen(false)}
              disabled={isDeleting}
            >
              Hủy
            </Button>
            <Button
              variant="destructive"
              onClick={handleDelete}
              disabled={isDeleting}
            >
              {isDeleting ? 'Đang xóa...' : 'Xóa'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {/* Update Dialog */}
      <Dialog open={isUpdateDialogOpen} onOpenChange={setIsUpdateDialogOpen}>
        <DialogTrigger asChild>
          <Button variant="outline" size="sm">
            Cập nhật
          </Button>
        </DialogTrigger>
        <DialogContent className="max-w-md">
          <DialogHeader>
            <DialogTitle>Cập nhật thông tin thuốc</DialogTitle>
            <DialogDescription>
              Chỉnh sửa thông tin thuốc "{data.name}"
            </DialogDescription>
          </DialogHeader>

          <Form {...form}>
            <form
              onSubmit={form.handleSubmit(handleUpdate)}
              className="space-y-4"
            >
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
                        <SelectItem value="VACCINE">Vaccine</SelectItem>
                        <SelectItem value="THUỐC_CHỮA_BỆNH">
                          Thuốc chữa bệnh
                        </SelectItem>
                        <SelectItem value="KHÁNG_SINH">Kháng sinh</SelectItem>
                      </SelectContent>
                    </Select>
                    <FormMessage />
                  </FormItem>
                )}
              />

              {/* Thêm FormField cho chọn loại bệnh */}
              <FormField
                control={form.control}
                name="diseaseId"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>Loại bệnh</FormLabel>
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
                        {listDanhSachBenh.map((benh: any) => (
                          <SelectItem key={benh.id} value={benh.id.toString()}>
                            {benh.name}
                          </SelectItem>
                        ))}
                      </SelectContent>
                    </Select>
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
                        placeholder="Nhập mô tả thuốc"
                        className="min-h-[80px]"
                        {...field}
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
                  {isUpdating ? 'Đang cập nhật...' : 'Cập nhật'}
                </Button>
              </DialogFooter>
            </form>
          </Form>
        </DialogContent>
      </Dialog>

      {/* Gemini Analysis Dialog */}
      <div>
        <GeminiAnalysisDialog
          data={data}
          generatePrompt={makePrompt}
          label="Tìm hiểu về thuốc"
        />
      </div>
    </div>
  );
};
