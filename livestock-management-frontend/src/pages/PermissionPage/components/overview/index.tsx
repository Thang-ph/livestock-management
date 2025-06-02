'use client';

import { useState, useEffect } from 'react';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import {
  Check,
  Settings,
  Plus,
  Filter,
  Pencil,
  Trash,
  Search,
  Save,
  X,
  ChevronDown,
  ChevronRight
} from 'lucide-react';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogClose
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import {
  AlertDialog,
  AlertDialogAction,
  AlertDialogCancel,
  AlertDialogContent,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogHeader,
  AlertDialogTitle
} from '@/components/ui/alert-dialog';
import { toast } from '@/components/ui/use-toast';
import { Toaster } from '@/components/ui/toaster';
import {
  useGetAllRole,
  useGetPermissionByRoleId,
  useAssignPermissionToRole,
  useGetAllPermission,
  useAddRole,
  useUpdateRole,
  useDeleteRole
} from '@/queries/role.query';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger
} from '@/components/ui/dropdown-menu';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';

interface Role {
  id: string;
  name: string;
  normalizedName: string;
  concurrencyStamp: string;
}

interface PermissionGroup {
  name: string;
  permissions: {
    key: string;
    name: string;
    checked: boolean;
  }[];
}

type FilterType = 'all' | 'assigned' | 'unassigned';

export function OverViewTab() {
  const {
    data: rolesData,
    isLoading: isLoadingRoles,
    refetch: refetchRoles
  } = useGetAllRole();
  const {
    mutateAsync: getPermissionByRoleId,
    isPending: isLoadingPermissions
  } = useGetPermissionByRoleId();
  const {
    mutateAsync: assignPermissionToRole,
    isPending: isSavingPermissions
  } = useAssignPermissionToRole();
  const { data: allPermissions, isLoading: isLoadingAllPermissions } =
    useGetAllPermission();
  const { mutateAsync: addRole, isPending: isAddingRole } = useAddRole();
  const { mutateAsync: updateRole, isPending: isUpdatingRole } =
    useUpdateRole();

  const [roles, setRoles] = useState<Role[]>([]);
  const [selectedRoleId, setSelectedRoleId] = useState<string>('');
  const [permissionGroups, setPermissionGroups] = useState<PermissionGroup[]>(
    []
  );
  const [newRoleName, setNewRoleName] = useState<string>('');
  const [editRoleName, setEditRoleName] = useState<string>('');
  const [editRoleId, setEditRoleId] = useState<string | null>(null);
  const [isAddDialogOpen, setIsAddDialogOpen] = useState<boolean>(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState<boolean>(false);
  const [isAddPermissionDialogOpen, setIsAddPermissionDialogOpen] =
    useState<boolean>(false);
  const [activeFilter, setActiveFilter] = useState<FilterType>('all');
  const [searchTerm, setSearchTerm] = useState<string>('');
  const [selectedRoleName, setSelectedRoleName] = useState<string>('');
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState<boolean>(false);
  const [roleToDelete, setRoleToDelete] = useState<Role | null>(null);
  const { mutateAsync: deleteRole, isPending: isDeletingRole } =
    useDeleteRole();
  const [hasChanges, setHasChanges] = useState<boolean>(false);

  // New state to track expanded permission groups
  const [expandedGroups, setExpandedGroups] = useState<Record<string, boolean>>(
    {}
  );

  // Load roles from API
  useEffect(() => {
    if (rolesData) {
      setRoles(rolesData);
    }
  }, [rolesData]);

  // Create permission groups from all available permissions and role permissions
  const createPermissionGroups = (rolePermissions: string[] = []) => {
    if (!allPermissions) return [];

    const groups: PermissionGroup[] = [];

    // Process each permission category
    Object.entries(allPermissions).forEach(([category, permissions]: any) => {
      const permissionItems = permissions.map((permKey) => ({
        key: permKey,
        name: permKey.split('.').slice(2).join('.'), // Extract the permission name
        checked: rolePermissions.includes(permKey)
      }));

      groups.push({
        name: category,
        permissions: permissionItems
      });
    });

    return groups;
  };

  // Initialize expanded groups when permissions are loaded
  useEffect(() => {
    if (permissionGroups.length > 0) {
      const initialExpandedState: Record<string, boolean> = {};
      permissionGroups.forEach((group) => {
        initialExpandedState[group.name] = true; // Default to expanded
      });
      setExpandedGroups(initialExpandedState);
    }
  }, [permissionGroups]);

  // Toggle expansion of a permission group
  const toggleGroupExpansion = (groupName: string) => {
    setExpandedGroups((prev) => ({
      ...prev,
      [groupName]: !prev[groupName]
    }));
  };

  // Load permissions when a role is selected
  const handleRoleSelect = async (roleId: string) => {
    if (hasChanges) {
      const confirmChange = window.confirm(
        'Bạn có thay đổi chưa được lưu. Bạn có muốn tiếp tục không?'
      );
      if (!confirmChange) return;
    }

    setSelectedRoleId(roleId);
    setActiveFilter('all'); // Reset filter when changing roles
    setSearchTerm(''); // Reset search when changing roles
    setHasChanges(false);

    try {
      const permissionData = await getPermissionByRoleId(roleId);
      const groups = createPermissionGroups(permissionData.permissions);
      setPermissionGroups(groups);

      // Find the role name
      const role = roles.find((r) => r.id === roleId);
      if (role) {
        setSelectedRoleName(role.name);
      }
    } catch (error) {
      toast({
        title: 'Lỗi',
        description: 'Không thể tải thông tin phân quyền',
        variant: 'destructive'
      });
    }
  };

  const togglePermission = (groupName: string, permissionKey: string) => {
    setPermissionGroups(
      permissionGroups.map((group) => {
        if (group.name === groupName) {
          return {
            ...group,
            permissions: group.permissions.map((permission) => {
              if (permission.key === permissionKey) {
                return { ...permission, checked: !permission.checked };
              }
              return permission;
            })
          };
        }
        return group;
      })
    );
    setHasChanges(true);
  };

  const toggleAllPermissionsInGroup = (groupName: string, checked: boolean) => {
    setPermissionGroups(
      permissionGroups.map((group) => {
        if (group.name === groupName) {
          return {
            ...group,
            permissions: group.permissions.map((permission) => ({
              ...permission,
              checked
            }))
          };
        }
        return group;
      })
    );
    setHasChanges(true);
  };

  const handleAddRole = async () => {
    if (newRoleName.trim() === '') {
      toast({
        title: 'Lỗi',
        description: 'Tên vai trò không được để trống',
        variant: 'destructive'
      });
      return;
    }

    try {
      // Call API to add the role
      await addRole({ name: newRoleName.trim() });

      // Refresh the roles list
      await refetchRoles();

      setNewRoleName('');
      setIsAddDialogOpen(false);

      toast({
        title: 'Thành công',
        description: 'Đã thêm vai trò mới'
      });
    } catch (error) {
      console.error('Error adding role:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể thêm vai trò mới',
        variant: 'destructive'
      });
    }
  };

  const handleEditRole = async () => {
    if (editRoleName.trim() === '') {
      toast({
        title: 'Lỗi',
        description: 'Tên vai trò không được để trống',
        variant: 'destructive'
      });
      return;
    }

    if (!editRoleId) {
      toast({
        title: 'Lỗi',
        description: 'Không tìm thấy vai trò để cập nhật',
        variant: 'destructive'
      });
      return;
    }

    try {
      // Call API to update the role
      await updateRole({
        id: editRoleId,
        name: editRoleName.trim()
      });

      // Refresh the roles list
      await refetchRoles();

      // If the edited role is the currently selected role, update the selected role name
      if (editRoleId === selectedRoleId) {
        setSelectedRoleName(editRoleName.trim());
      }

      setEditRoleName('');
      setEditRoleId(null);
      setIsEditDialogOpen(false);

      toast({
        title: 'Thành công',
        description: 'Đã cập nhật vai trò'
      });
    } catch (error) {
      console.error('Error updating role:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể cập nhật vai trò',
        variant: 'destructive'
      });
    }
  };

  const openEditDialog = (role: Role) => {
    setEditRoleId(role.id);
    setEditRoleName(role.name);
    setIsEditDialogOpen(true);
  };

  const openDeleteDialog = (role: Role) => {
    setRoleToDelete(role);
    setIsDeleteDialogOpen(true);
  };

  const handleDeleteRole = async () => {
    if (!roleToDelete) return;

    try {
      await deleteRole(roleToDelete.id);

      // Refresh roles list
      await refetchRoles();

      // If the deleted role was selected, clear the selection
      if (roleToDelete.id === selectedRoleId) {
        setSelectedRoleId('');
        setSelectedRoleName('');
        setPermissionGroups([]);
      }

      toast({
        title: 'Thành công',
        description: `Đã xóa vai trò "${roleToDelete.name}"`
      });
    } catch (error) {
      console.error('Error deleting role:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể xóa vai trò',
        variant: 'destructive'
      });
    }

    setRoleToDelete(null);
    setIsDeleteDialogOpen(false);
  };

  const handleSavePermissions = async () => {
    try {
      // Collect all checked permissions
      const checkedPermissions: string[] = [];

      permissionGroups.forEach((group) => {
        group.permissions.forEach((permission) => {
          if (permission.checked) {
            checkedPermissions.push(permission.key);
          }
        });
      });

      // Call API to save permissions
      await assignPermissionToRole({
        roleId: selectedRoleId,
        permissions: checkedPermissions
      });

      setHasChanges(false);

      toast({
        title: 'Thành công',
        description: 'Đã lưu cấu hình phân quyền'
      });
    } catch (error) {
      console.error('Error saving permissions:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể lưu cấu hình phân quyền',
        variant: 'destructive'
      });
    }
  };

  const openAddPermissionDialog = () => {
    if (!selectedRoleId) {
      toast({
        title: 'Lỗi',
        description: 'Vui lòng chọn vai trò trước khi thêm quyền',
        variant: 'destructive'
      });
      return;
    }
    setIsAddPermissionDialogOpen(true);
  };

  // Filter permissions based on the active filter
  const getFilteredPermissionGroups = () => {
    if (activeFilter === 'all') {
      return permissionGroups;
    }

    return permissionGroups
      .map((group) => {
        const filteredPermissions = group.permissions.filter((permission) => {
          if (activeFilter === 'assigned') {
            return permission.checked;
          } else if (activeFilter === 'unassigned') {
            return !permission.checked;
          }
          return true;
        });

        // Only include groups that have permissions after filtering
        if (filteredPermissions.length === 0) {
          return null;
        }

        return {
          ...group,
          permissions: filteredPermissions
        };
      })
      .filter(Boolean) as PermissionGroup[];
  };

  // Filter permissions based on search term
  const getSearchFilteredPermissionGroups = (groups: PermissionGroup[]) => {
    if (!searchTerm) return groups;

    const lowerSearchTerm = searchTerm.toLowerCase();

    return groups
      .map((group) => {
        const filteredPermissions = group.permissions.filter(
          (permission) =>
            permission.key.toLowerCase().includes(lowerSearchTerm) ||
            permission.name.toLowerCase().includes(lowerSearchTerm)
        );

        if (filteredPermissions.length === 0) {
          return null;
        }

        return {
          ...group,
          permissions: filteredPermissions
        };
      })
      .filter(Boolean) as PermissionGroup[];
  };

  const filteredGroups = getSearchFilteredPermissionGroups(
    getFilteredPermissionGroups()
  );

  // Count assigned permissions
  const countAssignedPermissions = () => {
    let count = 0;
    permissionGroups.forEach((group) => {
      group.permissions.forEach((permission) => {
        if (permission.checked) count++;
      });
    });
    return count;
  };

  // Count total permissions
  const countTotalPermissions = () => {
    let count = 0;
    permissionGroups.forEach((group) => {
      count += group.permissions.length;
    });
    return count;
  };

  return (
    <div className="grid grid-cols-1 gap-6 md:grid-cols-[300px_1fr]">
      <div className="space-y-6">
        <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
          <DialogTrigger asChild>
            <Button className="w-full bg-blue-600 text-white hover:bg-blue-700">
              <Plus className="mr-2 h-4 w-4" /> Thêm vai trò
            </Button>
          </DialogTrigger>
          <DialogContent className="max-w-[625px]">
            <DialogHeader>
              <DialogTitle>Thêm vai trò mới</DialogTitle>
              <DialogDescription>
                Nhập tên vai trò mới để thêm vào hệ thống.
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="name" className="text-right">
                  Tên vai trò
                </Label>
                <Input
                  id="name"
                  value={newRoleName}
                  onChange={(e) => setNewRoleName(e.target.value)}
                  className="col-span-3"
                  placeholder="Nhập tên vai trò"
                />
              </div>
            </div>
            <DialogFooter>
              <DialogClose asChild>
                <Button variant="outline">Hủy</Button>
              </DialogClose>
              <Button
                onClick={handleAddRole}
                disabled={isAddingRole || newRoleName.trim() === ''}
                className="bg-blue-600 hover:bg-blue-700"
              >
                {isAddingRole ? 'Đang thêm...' : 'Thêm'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
          <DialogContent className="max-w-[625px]">
            <DialogHeader>
              <DialogTitle>Chỉnh sửa vai trò</DialogTitle>
              <DialogDescription>
                Chỉnh sửa tên vai trò đã chọn.
              </DialogDescription>
            </DialogHeader>
            <div className="grid gap-4 py-4">
              <div className="grid grid-cols-4 items-center gap-4">
                <Label htmlFor="edit-name" className="text-right">
                  Tên vai trò
                </Label>
                <Input
                  id="edit-name"
                  value={editRoleName}
                  onChange={(e) => setEditRoleName(e.target.value)}
                  className="col-span-3"
                />
              </div>
            </div>
            <DialogFooter>
              <DialogClose asChild>
                <Button variant="outline">Hủy</Button>
              </DialogClose>
              <Button
                onClick={handleEditRole}
                disabled={isUpdatingRole || editRoleName.trim() === ''}
                className="bg-blue-600 hover:bg-blue-700"
              >
                {isUpdatingRole ? 'Đang lưu...' : 'Lưu thay đổi'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>

        <AlertDialog
          open={isDeleteDialogOpen}
          onOpenChange={setIsDeleteDialogOpen}
        >
          <AlertDialogContent>
            <AlertDialogHeader>
              <AlertDialogTitle>Xác nhận xóa vai trò</AlertDialogTitle>
              <AlertDialogDescription>
                Bạn có chắc chắn muốn xóa vai trò "{roleToDelete?.name}"? Hành
                động này không thể hoàn tác.
              </AlertDialogDescription>
            </AlertDialogHeader>
            <AlertDialogFooter>
              <AlertDialogCancel>Hủy</AlertDialogCancel>
              <AlertDialogAction
                onClick={handleDeleteRole}
                className="bg-red-500 text-white hover:bg-red-600"
                disabled={isDeletingRole}
              >
                {isDeletingRole ? 'Đang xóa...' : 'Xóa'}
              </AlertDialogAction>
            </AlertDialogFooter>
          </AlertDialogContent>
        </AlertDialog>

        <Card className="overflow-hidden border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between bg-blue-600 p-4 text-white">
            <div className="font-medium">Danh sách vai trò</div>
            <div>Thao tác</div>
          </div>
          <div className="divide-y">
            {isLoadingRoles ? (
              Array(5)
                .fill(0)
                .map((_, index) => (
                  <div key={index} className="p-4">
                    <Skeleton className="h-6 w-full" />
                  </div>
                ))
            ) : roles.length === 0 ? (
              <div className="p-6 text-center text-gray-500">
                Chưa có vai trò nào. Hãy thêm vai trò mới.
              </div>
            ) : (
              roles.map((role) => (
                <div
                  key={role.id}
                  className={`flex cursor-pointer items-center justify-between p-4 transition-colors hover:bg-gray-50 ${
                    selectedRoleId === role.id ? 'bg-blue-50' : ''
                  }`}
                  onClick={() => handleRoleSelect(role.id)}
                >
                  <div className="font-medium">
                    {role.name}
                    {selectedRoleId === role.id && (
                      <Badge className="ml-2 bg-blue-100 text-blue-800">
                        Đang chọn
                      </Badge>
                    )}
                  </div>
                  <DropdownMenu>
                    <DropdownMenuTrigger asChild>
                      <Button
                        variant="ghost"
                        size="icon"
                        className="h-8 w-8"
                        onClick={(e) => e.stopPropagation()}
                      >
                        <Settings className="h-4 w-4" />
                        <span className="sr-only">Mở menu</span>
                      </Button>
                    </DropdownMenuTrigger>
                    <DropdownMenuContent align="end">
                      <DropdownMenuItem
                        className="cursor-pointer"
                        onClick={(e) => {
                          e.stopPropagation();
                          openEditDialog(role);
                        }}
                      >
                        <Pencil className="mr-2 h-4 w-4" />
                        Chỉnh sửa
                      </DropdownMenuItem>
                      <DropdownMenuItem
                        className="cursor-pointer text-red-500 focus:text-red-500"
                        onClick={(e) => {
                          e.stopPropagation();
                          openDeleteDialog(role);
                        }}
                      >
                        <Trash className="mr-2 h-4 w-4" />
                        Xóa
                      </DropdownMenuItem>
                    </DropdownMenuContent>
                  </DropdownMenu>
                </div>
              ))
            )}
          </div>
        </Card>
      </div>

      <div className="space-y-6">
        <Card className="border border-gray-200 shadow-sm">
          <div className="flex flex-col gap-4 p-4 sm:flex-row sm:items-center sm:justify-between">
            <div className="flex-1">
              <Select value={selectedRoleId} onValueChange={handleRoleSelect}>
                <SelectTrigger className="w-full sm:w-[300px]">
                  <SelectValue placeholder="-- Chọn vai trò --" />
                </SelectTrigger>
                <SelectContent>
                  {roles.map((role) => (
                    <SelectItem key={role.id} value={role.id}>
                      {role.name}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
              {selectedRoleId && (
                <div className="mt-2 text-sm text-gray-500">
                  {isLoadingPermissions ? (
                    <Skeleton className="h-4 w-40" />
                  ) : (
                    <>
                      Đã gán {countAssignedPermissions()}/
                      {countTotalPermissions()} quyền
                    </>
                  )}
                </div>
              )}
            </div>

            <div className="flex gap-2">
              {selectedRoleId && hasChanges && (
                <Button
                  onClick={handleSavePermissions}
                  className="bg-green-600 text-white hover:bg-green-700"
                  disabled={isSavingPermissions}
                >
                  <Save className="mr-2 h-4 w-4" />
                  {isSavingPermissions ? 'Đang lưu...' : 'Lưu thay đổi'}
                </Button>
              )}

              <Button
                onClick={openAddPermissionDialog}
                className="bg-blue-600 text-white hover:bg-blue-700"
                disabled={!selectedRoleId}
              >
                <Plus className="mr-2 h-4 w-4" /> Quản lý quyền
              </Button>
            </div>
          </div>
        </Card>

        {selectedRoleId && (
          <Card className="border border-gray-200 shadow-sm">
            <div className="space-y-4 p-4">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
                <h3 className="text-lg font-medium">Bộ lọc quyền</h3>
                <div className="flex items-center gap-2">
                  <div className="relative flex-1 sm:w-64">
                    <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                    <Input
                      placeholder="Tìm kiếm quyền..."
                      value={searchTerm}
                      onChange={(e) => setSearchTerm(e.target.value)}
                      className="pl-9"
                    />
                    {searchTerm && (
                      <Button
                        variant="ghost"
                        size="icon"
                        className="absolute right-1 top-1/2 h-6 w-6 -translate-y-1/2"
                        onClick={() => setSearchTerm('')}
                      >
                        <X className="h-4 w-4" />
                        <span className="sr-only">Xóa tìm kiếm</span>
                      </Button>
                    )}
                  </div>
                </div>
              </div>

              <RadioGroup
                value={activeFilter}
                onValueChange={(value) => setActiveFilter(value as FilterType)}
                className="flex flex-wrap gap-4"
              >
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="all" id="all" />
                  <Label htmlFor="all" className="cursor-pointer">
                    Tất cả quyền
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="assigned" id="assigned" />
                  <Label htmlFor="assigned" className="cursor-pointer">
                    Quyền đã có
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="unassigned" id="unassigned" />
                  <Label htmlFor="unassigned" className="cursor-pointer">
                    Quyền chưa có
                  </Label>
                </div>
              </RadioGroup>
            </div>
          </Card>
        )}

        <Card className="overflow-hidden border border-gray-200 shadow-sm">
          <div className="flex items-center justify-between bg-blue-600 p-4 text-white">
            <div className="font-medium">Chức năng</div>
            <div className="flex items-center">
              Phân quyền <Check className="ml-2 h-5 w-5" />
            </div>
          </div>

          {isLoadingPermissions || isLoadingAllPermissions ? (
            <div className="p-4">
              <div className="space-y-4">
                {Array(3)
                  .fill(0)
                  .map((_, groupIndex) => (
                    <div key={groupIndex}>
                      <Skeleton className="mb-2 h-8 w-full" />
                      {Array(4)
                        .fill(0)
                        .map((_, permIndex) => (
                          <Skeleton
                            key={permIndex}
                            className="mb-2 h-12 w-full"
                          />
                        ))}
                    </div>
                  ))}
              </div>
            </div>
          ) : selectedRoleId ? (
            filteredGroups.length > 0 ? (
              <div className="divide-y">
                {filteredGroups.map((group) => (
                  <div key={group.name} className="overflow-hidden">
                    <div
                      className="flex cursor-pointer items-center justify-between bg-gray-100 p-4"
                      onClick={() => toggleGroupExpansion(group.name)}
                    >
                      <div className="flex items-center font-medium">
                        {expandedGroups[group.name] ? (
                          <ChevronDown className="mr-2 h-5 w-5 text-blue-600" />
                        ) : (
                          <ChevronRight className="mr-2 h-5 w-5 text-blue-600" />
                        )}
                        {group.name}
                      </div>
                      <div
                        className="cursor-pointer"
                        onClick={(e) => {
                          e.stopPropagation();
                          toggleAllPermissionsInGroup(
                            group.name,
                            !group.permissions.every((p) => p.checked)
                          );
                        }}
                      >
                        <Checkbox
                          checked={group.permissions.every((p) => p.checked)}
                          className="h-5 w-5 rounded-sm"
                        />
                      </div>
                    </div>

                    {expandedGroups[group.name] && (
                      <div className="divide-y">
                        {group.permissions.map((permission) => (
                          <div
                            key={permission.key}
                            className={`flex items-center justify-between p-4 transition-colors ${
                              permission.checked ? 'bg-green-50' : 'bg-white'
                            }`}
                          >
                            <div className="flex items-center pl-4">
                              <span className="font-medium">
                                {permission.name}
                              </span>
                              {permission.checked && (
                                <Badge className="ml-2 bg-green-100 text-green-800">
                                  Đã gán
                                </Badge>
                              )}
                            </div>
                            <div
                              className="cursor-pointer"
                              onClick={() =>
                                togglePermission(group.name, permission.key)
                              }
                            >
                              <Checkbox
                                checked={permission.checked}
                                className={`h-5 w-5 rounded-sm ${
                                  permission.checked
                                    ? 'border-green-500 bg-green-500 text-white'
                                    : ''
                                }`}
                              />
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <div className="flex flex-col items-center justify-center p-8 text-center text-gray-500">
                {searchTerm ? (
                  <>
                    <Search className="mb-2 h-10 w-10 text-gray-400" />
                    <p>
                      Không tìm thấy quyền nào phù hợp với từ khóa "{searchTerm}
                      "
                    </p>
                    <Button
                      variant="ghost"
                      className="mt-2 text-blue-600"
                      onClick={() => setSearchTerm('')}
                    >
                      Xóa tìm kiếm
                    </Button>
                  </>
                ) : (
                  <>
                    <Filter className="mb-2 h-10 w-10 text-gray-400" />
                    <p>Không có quyền nào phù hợp với bộ lọc đã chọn</p>
                    <Button
                      variant="ghost"
                      className="mt-2 text-blue-600"
                      onClick={() => setActiveFilter('all')}
                    >
                      Hiển thị tất cả
                    </Button>
                  </>
                )}
              </div>
            )
          ) : (
            <div className="flex flex-col items-center justify-center p-8 text-center text-gray-500">
              <Settings className="mb-2 h-10 w-10 text-gray-400" />
              <p>Vui lòng chọn vai trò để xem phân quyền</p>
            </div>
          )}
        </Card>

        {/* Dialog để thêm quyền */}
        <Dialog
          open={isAddPermissionDialogOpen}
          onOpenChange={setIsAddPermissionDialogOpen}
        >
          <DialogContent className="max-w-[700px]">
            <DialogHeader>
              <DialogTitle>
                Quản lý quyền cho vai trò: {selectedRoleName}
              </DialogTitle>
              <DialogDescription>
                Chọn các quyền bạn muốn thêm hoặc xóa cho vai trò này.
              </DialogDescription>
            </DialogHeader>

            <div className="mb-4">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-gray-400" />
                <Input
                  placeholder="Tìm kiếm quyền..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-9"
                />
                {searchTerm && (
                  <Button
                    variant="ghost"
                    size="icon"
                    className="absolute right-1 top-1/2 h-6 w-6 -translate-y-1/2"
                    onClick={() => setSearchTerm('')}
                  >
                    <X className="h-4 w-4" />
                    <span className="sr-only">Xóa tìm kiếm</span>
                  </Button>
                )}
              </div>
            </div>

            <div className="mb-4">
              <RadioGroup
                value={activeFilter}
                onValueChange={(value) => setActiveFilter(value as FilterType)}
                className="flex flex-wrap gap-4"
              >
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="all" id="dialog-all" />
                  <Label htmlFor="dialog-all" className="cursor-pointer">
                    Tất cả quyền
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="assigned" id="dialog-assigned" />
                  <Label htmlFor="dialog-assigned" className="cursor-pointer">
                    Quyền đã có
                  </Label>
                </div>
                <div className="flex items-center space-x-2">
                  <RadioGroupItem value="unassigned" id="dialog-unassigned" />
                  <Label htmlFor="dialog-unassigned" className="cursor-pointer">
                    Quyền chưa có
                  </Label>
                </div>
              </RadioGroup>
            </div>

            <div className="max-h-[400px] overflow-y-auto rounded-md border">
              {filteredGroups.length > 0 ? (
                filteredGroups.map((group) => (
                  <div key={group.name} className="border-b last:border-b-0">
                    <div
                      className="flex cursor-pointer items-center justify-between bg-gray-100 p-3"
                      onClick={() => toggleGroupExpansion(group.name)}
                    >
                      <div className="flex items-center font-medium">
                        {expandedGroups[group.name] ? (
                          <ChevronDown className="mr-2 h-4 w-4 text-blue-600" />
                        ) : (
                          <ChevronRight className="mr-2 h-4 w-4 text-blue-600" />
                        )}
                        {group.name}
                      </div>
                      <Checkbox
                        checked={group.permissions.every((p) => p.checked)}
                        onCheckedChange={(checked) =>
                          toggleAllPermissionsInGroup(
                            group.name,
                            checked === true
                          )
                        }
                        className="h-5 w-5 rounded-sm"
                        onClick={(e) => e.stopPropagation()}
                      />
                    </div>
                    {expandedGroups[group.name] && (
                      <div className="divide-y">
                        {group.permissions.map((permission) => (
                          <div
                            key={permission.key}
                            className={`flex items-center justify-between p-3 ${
                              permission.checked ? 'bg-green-50' : 'bg-white'
                            }`}
                          >
                            <div className="flex items-center">
                              <Checkbox
                                id={`dialog-${permission.key}`}
                                checked={permission.checked}
                                onCheckedChange={() =>
                                  togglePermission(group.name, permission.key)
                                }
                                className={`mr-2 h-5 w-5 rounded-sm ${
                                  permission.checked
                                    ? 'border-green-500 bg-green-500 text-white'
                                    : ''
                                }`}
                              />
                              <label
                                htmlFor={`dialog-${permission.key}`}
                                className="cursor-pointer text-sm"
                              >
                                {permission.name}
                                {permission.checked && (
                                  <Badge className="ml-2 bg-green-100 text-green-800">
                                    Đã gán
                                  </Badge>
                                )}
                              </label>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>
                ))
              ) : (
                <div className="flex flex-col items-center justify-center p-8 text-center text-gray-500">
                  {searchTerm ? (
                    <>
                      <Search className="mb-2 h-8 w-8 text-gray-400" />
                      <p>
                        Không tìm thấy quyền nào phù hợp với từ khóa "
                        {searchTerm}"
                      </p>
                      <Button
                        variant="ghost"
                        className="mt-2 text-blue-600"
                        onClick={() => setSearchTerm('')}
                      >
                        Xóa tìm kiếm
                      </Button>
                    </>
                  ) : (
                    <>
                      <Filter className="mb-2 h-8 w-8 text-gray-400" />
                      <p>Không có quyền nào phù hợp với bộ lọc đã chọn</p>
                      <Button
                        variant="ghost"
                        className="mt-2 text-blue-600"
                        onClick={() => setActiveFilter('all')}
                      >
                        Hiển thị tất cả
                      </Button>
                    </>
                  )}
                </div>
              )}
            </div>

            <DialogFooter className="mt-4">
              <Button
                variant="outline"
                onClick={() => setIsAddPermissionDialogOpen(false)}
              >
                Đóng
              </Button>
              <Button
                onClick={handleSavePermissions}
                disabled={isSavingPermissions}
                className="bg-green-600 hover:bg-green-700"
              >
                {isSavingPermissions ? 'Đang lưu...' : 'Lưu thay đổi'}
              </Button>
            </DialogFooter>
          </DialogContent>
        </Dialog>
      </div>
      <Toaster />
    </div>
  );
}
