'use client';

import type React from 'react';

import { useState } from 'react';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle
} from '@/components/ui/dialog';
import { MoreHorizontal, UnlockIcon, LockIcon } from 'lucide-react';
import { useToast } from '@/components/ui/use-toast';

import { UpdateIcon } from '@radix-ui/react-icons';
import { useUpdateLockUser, useUpdateUser } from '@/queries/user.query';
import { useGetAllRole } from '@/queries/role.query';
import { UpdateUserModal } from './update-user-modal';

interface UserData {
  id: string;
  userName: string;
  email: string;
  phoneNumber: string;
  isLocked: boolean;
  roles: string[];
}

interface UserCellActionProps {
  data: UserData;
}

export const CellAction: React.FC<UserCellActionProps> = ({ data }) => {
  const { toast } = useToast();
  const { mutateAsync: updateUser } = useUpdateUser();
  const { mutateAsync: updateLockUser } = useUpdateLockUser();
  const { data: listRole } = useGetAllRole();
  const [isUpdateModalOpen, setIsUpdateModalOpen] = useState(false);
  const [isLockDialogOpen, setIsLockDialogOpen] = useState(false);
  const [isLoading, setIsLoading] = useState(false);

  const handleUpdate = async (values: {
    userName: string;
    email: string;
    phoneNumber: string;
    roles: string[];
    password: string;
    confirmPassword: string;
  }) => {
    const payload = {
      userName: values.userName,
      email: values.email,
      phoneNumber: values.phoneNumber,
      roles: values.roles,
      password: values.password || '',
      confirmPassword: values.confirmPassword || ''
    };

    const [err] = await updateUser({ id: data.id, ...payload });
    if (err) {
      toast({
        title: 'Cập nhập thất bại',
        description:
          'Có lỗi xảy ra trong quá trình cập nhập thông tin người dùng.',
        variant: 'destructive'
      });
    } else {
      toast({
        title: 'Cập nhập thành công',
        description: 'Thông tin người dùng đã được cập nhập.',
        variant: 'success'
      });
    }
  };

  const handleLockToggle = async () => {
    setIsLoading(true);
    try {
      // status: true là mở khóa, false là khóa
      // Nếu user đang bị khóa (isLocked = true), thì gửi status = true để mở khóa
      // Nếu user đang mở (isLocked = false), thì gửi status = false để khóa
      const status = data.isLocked; // Gửi ngược lại trạng thái hiện tại

      // Looking at the API call, we need to send the boolean value directly
      // The API expects the boolean value in the request body
      await updateLockUser({
        id: data.id,
        status: status // This should be sent as a boolean value
      });

      toast({
        title: data.isLocked ? 'Mở khóa thành công' : 'Khóa thành công',
        description: data.isLocked
          ? `Người dùng ${data.userName} đã được mở khóa.`
          : `Người dùng ${data.userName} đã bị khóa.`,
        variant: 'success'
      });

      setIsLockDialogOpen(false);
    } catch (error) {
      console.error('Lock/unlock error:', error);
      toast({
        title: 'Thao tác thất bại',
        description: 'Có lỗi xảy ra trong quá trình thực hiện thao tác.',
        variant: 'destructive'
      });
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <>
      <DropdownMenu modal={false}>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="h-8 w-8 p-0">
            <span className="sr-only">Mở menu</span>
            <MoreHorizontal className="h-4 w-4" />
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent align="end" className="bg-primary-foreground">
          <DropdownMenuLabel>Lựa chọn</DropdownMenuLabel>
          <DropdownMenuItem onClick={() => setIsUpdateModalOpen(true)}>
            <UpdateIcon className="mr-2 h-4 w-4" /> Cập nhập
          </DropdownMenuItem>
          <DropdownMenuItem onClick={() => setIsLockDialogOpen(true)}>
            {data.isLocked ? (
              <UnlockIcon className="mr-2 h-4 w-4" />
            ) : (
              <LockIcon className="mr-2 h-4 w-4" />
            )}
            {data.isLocked ? 'Mở khóa' : 'Khóa'}
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>

      {/* Lock/Unlock Confirmation Dialog */}
      <Dialog open={isLockDialogOpen} onOpenChange={setIsLockDialogOpen}>
        <DialogContent className="sm:max-w-md">
          <DialogHeader>
            <DialogTitle>
              {data.isLocked
                ? 'Xác nhận mở khóa người dùng'
                : 'Xác nhận khóa người dùng'}
            </DialogTitle>
            <DialogDescription>
              {data.isLocked
                ? `Bạn có chắc chắn muốn mở khóa người dùng ${data.userName}?`
                : `Bạn có chắc chắn muốn khóa người dùng ${data.userName}? Người dùng sẽ không thể đăng nhập vào hệ thống.`}
            </DialogDescription>
          </DialogHeader>
          <DialogFooter className="flex flex-row justify-end gap-2 sm:justify-end">
            <Button
              type="button"
              variant="outline"
              onClick={() => setIsLockDialogOpen(false)}
              disabled={isLoading}
            >
              Hủy
            </Button>
            <Button
              type="button"
              onClick={handleLockToggle}
              disabled={isLoading}
              variant={data.isLocked ? 'default' : 'destructive'}
            >
              {isLoading ? 'Đang xử lý...' : data.isLocked ? 'Mở khóa' : 'Khóa'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>

      {listRole && (
        <UpdateUserModal
          isOpen={isUpdateModalOpen}
          onClose={() => setIsUpdateModalOpen(false)}
          userData={data}
          roles={listRole}
          onUpdate={handleUpdate}
        />
      )}
    </>
  );
};
