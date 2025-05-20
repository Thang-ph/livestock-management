'use client';

import type React from 'react';

import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter
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
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import * as z from 'zod';
import { Checkbox } from '@/components/ui/checkbox';
import { Save } from 'lucide-react';

const formSchema = z
  .object({
    userName: z.string().min(1, 'Tên đăng nhập không được để trống'),
    email: z.string().email('Email không hợp lệ'),
    phoneNumber: z.string().min(1, 'Số điện thoại không được để trống'),
    roles: z.array(z.string()).min(1, 'Phải chọn ít nhất một vai trò'),
    changePassword: z.boolean().default(false),
    password: z.string(),
    confirmPassword: z.string()
  })
  .refine(
    (data) => {
      if (!data.changePassword) return true;
      if (data.password && data.password !== data.confirmPassword) return false;
      return true;
    },
    {
      message: 'Mật khẩu và xác nhận mật khẩu phải giống nhau',
      path: ['confirmPassword']
    }
  );

type FormValues = z.infer<typeof formSchema>;

interface UserData {
  id: string;
  userName: string;
  email: string;
  phoneNumber: string;
  isLocked: boolean;
  roles: string[];
}

interface UpdateUserModalProps {
  isOpen: boolean;
  onClose: () => void;
  userData: UserData;
  roles: Array<{
    id: string;
    name: string;
    normalizedName: string;
    concurrencyStamp: string;
  }>;
  onUpdate: (values: FormValues) => Promise<void>;
}

export const UpdateUserModal: React.FC<UpdateUserModalProps> = ({
  isOpen,
  onClose,
  userData,
  roles,
  onUpdate
}) => {
  const [selectedRoles, setSelectedRoles] = useState<string[]>([]);
  const [showPasswordFields, setShowPasswordFields] = useState(false);

  const form = useForm<FormValues>({
    resolver: zodResolver(formSchema),
    defaultValues: {
      userName: '',
      email: '',
      phoneNumber: '',
      roles: [],
      changePassword: false,
      password: '',
      confirmPassword: ''
    }
  });

  useEffect(() => {
    if (isOpen && userData) {
      form.reset({
        userName: userData.userName || '',
        email: userData.email || '',
        phoneNumber: userData.phoneNumber || '',
        roles: userData.roles || [],
        changePassword: false,
        password: '',
        confirmPassword: ''
      });

      setSelectedRoles(userData.roles || []);
      setShowPasswordFields(false);
    }
  }, [isOpen, userData, form]);

  const handleSubmit = async (values: FormValues) => {
    try {
      const submitValues = {
        ...values,
        password: values.changePassword ? values.password : '',
        confirmPassword: values.changePassword ? values.confirmPassword : ''
      };

      await onUpdate(submitValues);
      onClose();
    } catch (error) {}
  };

  const handleRoleChange = (roleName: string, checked: boolean) => {
    if (checked) {
      setSelectedRoles((prev) => [...prev, roleName]);
      const currentRoles = form.getValues('roles');
      form.setValue('roles', [...currentRoles, roleName]);
    } else {
      setSelectedRoles((prev) => prev.filter((role) => role !== roleName));
      const currentRoles = form.getValues('roles');
      form.setValue(
        'roles',
        currentRoles.filter((role) => role !== roleName)
      );
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={onClose}>
      <DialogContent className="sm:max-w-[800px]">
        <DialogHeader>
          <DialogTitle>Thông tin tài khoản</DialogTitle>
        </DialogHeader>
        <Form {...form}>
          <form
            onSubmit={form.handleSubmit(handleSubmit)}
            className="space-y-4"
          >
            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="userName"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      Họ và tên:
                      <span className="text-red-500">*</span>
                    </FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />

              <FormItem>
                <FormLabel>
                  Vai trò:
                  <span className="text-red-500">*</span>
                </FormLabel>
                <div className="h-20 overflow-y-auto rounded-md border p-2">
                  {roles.map((role) => (
                    <div key={role.id} className="flex items-center space-x-2">
                      <Checkbox
                        id={role.id}
                        checked={selectedRoles.includes(role.name)}
                        onCheckedChange={(checked) =>
                          handleRoleChange(role.name, checked as boolean)
                        }
                      />
                      <label htmlFor={role.id} className="text-sm">
                        {role.name}
                      </label>
                    </div>
                  ))}
                </div>
                {form.formState.errors.roles && (
                  <p className="text-sm font-medium text-destructive">
                    {form.formState.errors.roles.message}
                  </p>
                )}
              </FormItem>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <FormField
                control={form.control}
                name="phoneNumber"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      Số điện thoại:
                      <span className="text-red-500">*</span>
                    </FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
              <FormField
                control={form.control}
                name="email"
                render={({ field }) => (
                  <FormItem>
                    <FormLabel>
                      Địa chỉ Email:
                      <span className="text-red-500">*</span>
                    </FormLabel>
                    <FormControl>
                      <Input {...field} />
                    </FormControl>
                    <FormMessage />
                  </FormItem>
                )}
              />
            </div>

            <FormField
              control={form.control}
              name="changePassword"
              render={({ field }) => (
                <FormItem className="flex flex-row items-start space-x-3 space-y-0 rounded-md border p-4">
                  <FormControl>
                    <Checkbox
                      checked={field.value}
                      onCheckedChange={(checked) => {
                        field.onChange(checked);
                        setShowPasswordFields(!!checked);
                      }}
                    />
                  </FormControl>
                  <div className="space-y-1 leading-none">
                    <FormLabel>Thay đổi mật khẩu</FormLabel>
                  </div>
                </FormItem>
              )}
            />

            {showPasswordFields && (
              <div className="grid grid-cols-2 gap-4">
                <FormField
                  control={form.control}
                  name="password"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>
                        Mật khẩu:
                        <span className="text-red-500">*</span>
                      </FormLabel>
                      <FormControl>
                        <Input type="password" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />

                <FormField
                  control={form.control}
                  name="confirmPassword"
                  render={({ field }) => (
                    <FormItem>
                      <FormLabel>
                        Nhập lại mật khẩu:
                        <span className="text-red-500">*</span>
                      </FormLabel>
                      <FormControl>
                        <Input type="password" {...field} />
                      </FormControl>
                      <FormMessage />
                    </FormItem>
                  )}
                />
              </div>
            )}

            <DialogFooter>
              <Button type="submit" className="bg-green-600 hover:bg-green-700">
                <Save className="mr-2 h-4 w-4" /> Lưu
              </Button>
            </DialogFooter>
          </form>
        </Form>
      </DialogContent>
    </Dialog>
  );
};
