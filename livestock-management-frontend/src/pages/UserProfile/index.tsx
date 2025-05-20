import BasePages from '@/components/shared/base-pages.js';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { useGetMyInfo } from '@/queries/user.query.js';
import { Card, CardContent, CardHeader } from '@/components/ui/card';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { Badge } from '@/components/ui/badge';
import { Mail, Phone, Briefcase, Shield } from 'lucide-react';
import { Skeleton } from '@/components/ui/skeleton';
import {
  useCompletedChangePassword,
  useInitChangePassword
} from '@/queries/auth.query';
import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { AlertCircle, Check, Eye, EyeOff, Lock } from 'lucide-react';

export default function UserProfile() {
  const { data, isLoading } = useGetMyInfo();
  const { mutateAsync: initChangePassword } = useInitChangePassword();
  const { mutateAsync: completed } = useCompletedChangePassword();

  const getInitials = (name) => {
    if (!name) return 'U';
    return name
      .split(' ')
      .map((n) => n[0])
      .join('')
      .toUpperCase()
      .substring(0, 2);
  };

  return (
    <BasePages
      className="relative flex-1 space-y-4 overflow-y-auto px-4 pb-8"
      breadcrumbs={[
        { title: 'Trang chủ', link: '/' },
        { title: 'Tài khoản của tôi', link: '/user' }
      ]}
    >
      <div className="top-4 flex items-center justify-between space-y-2">
        <h2 className="text-2xl font-bold tracking-tight">Tài khoản của tôi</h2>
      </div>

      <Tabs defaultValue="overview" className="w-full">
        <TabsList className="mb-4">
          <TabsTrigger value="overview">Tổng quan</TabsTrigger>
          <TabsTrigger value="security">Bảo mật</TabsTrigger>
          <TabsTrigger value="settings">Cài đặt</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="space-y-6">
          {isLoading ? (
            <ProfileSkeleton />
          ) : (
            <>
              <Card className="overflow-hidden border-0 shadow-md">
                <div className="h-32 bg-gradient-to-r from-teal-500 to-emerald-500"></div>
                <div className="relative px-6">
                  <div className="absolute -top-16 flex h-32 w-32 items-center justify-center rounded-full border-4 border-white bg-white shadow-md">
                    <Avatar className="h-full w-full">
                      <AvatarFallback className="bg-teal-100 text-3xl font-semibold text-teal-600">
                        {getInitials(data?.userName)}
                      </AvatarFallback>
                    </Avatar>
                  </div>
                </div>

                <CardContent className="mt-20 px-6 pb-6">
                  <div className="flex flex-wrap items-start justify-between gap-4">
                    <div>
                      <h3 className="text-2xl font-bold text-gray-900">
                        {data?.userName}
                      </h3>
                      <div className="mt-1 flex items-center gap-2 text-gray-500">
                        <Shield className="h-4 w-4 fill-green-500 " />
                        <span>Đã xác thực</span>
                      </div>
                    </div>
                    <div className="flex flex-wrap gap-2">
                      {data?.roles?.map((role, index) => (
                        <Badge
                          key={index}
                          className="bg-teal-100 text-teal-800 hover:bg-teal-200"
                        >
                          {role}
                        </Badge>
                      ))}
                    </div>
                  </div>
                </CardContent>
              </Card>

              <div className="grid gap-6 md:grid-cols-2">
                <Card className="border-0 shadow-sm">
                  <CardHeader className="border-b pb-3">
                    <h3 className="text-lg font-medium">Thông tin liên hệ</h3>
                  </CardHeader>
                  <CardContent className="space-y-4 pt-4">
                    <div className="flex items-start gap-3">
                      <div className="flex h-9 w-9 items-center justify-center rounded-full bg-teal-100 text-teal-600">
                        <Mail className="h-5 w-5" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-gray-500">
                          Email
                        </p>
                        <p className="text-gray-900">{data?.email}</p>
                      </div>
                    </div>

                    <div className="flex items-start gap-3">
                      <div className="flex h-9 w-9 items-center justify-center rounded-full bg-teal-100 text-teal-600">
                        <Phone className="h-5 w-5" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-gray-500">
                          Số điện thoại
                        </p>
                        <p className="text-gray-900">{data?.phoneNumber}</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>

                <Card className="border-0 shadow-sm">
                  <CardHeader className="border-b pb-3">
                    <h3 className="text-lg font-medium">Vai trò & Quyền hạn</h3>
                  </CardHeader>
                  <CardContent className="space-y-4 pt-4">
                    <div className="flex items-start gap-3">
                      <div className="flex h-9 w-9 items-center justify-center rounded-full bg-teal-100 text-teal-600">
                        <Briefcase className="h-5 w-5" />
                      </div>
                      <div>
                        <p className="text-sm font-medium text-gray-500">
                          Vai trò
                        </p>
                        <div className="mt-1 flex flex-wrap gap-2">
                          {data?.roles?.map((role, index) => (
                            <Badge
                              key={index}
                              variant="outline"
                              className="border-teal-200 bg-teal-50 text-teal-700"
                            >
                              {role}
                            </Badge>
                          ))}
                        </div>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </div>
            </>
          )}
        </TabsContent>

        <TabsContent value="security">
          <Card className="border-0 shadow-sm">
            <CardHeader>
              <h3 className="text-lg font-medium">Bảo mật tài khoản</h3>
              <p className="text-sm text-gray-500">
                Quản lý các thiết lập bảo mật cho tài khoản của bạn
              </p>
            </CardHeader>
            <CardContent className="py-12 text-center">
              <p className="text-gray-500">Tính năng đang được phát triển</p>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="settings">
          <Card className="border-0 shadow-sm">
            <CardHeader>
              <h3 className="text-lg font-medium">Cài đặt tài khoản</h3>
              <p className="text-sm text-gray-500">
                Quản lý các thiết lập cho tài khoản của bạn
              </p>
            </CardHeader>
            <CardContent className="space-y-6">
              <PasswordChangeForm
                initChangePassword={initChangePassword}
                completedChangePassword={completed}
              />
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>
    </BasePages>
  );
}

function PasswordChangeForm({ initChangePassword, completedChangePassword }) {
  const [step, setStep] = useState('initial'); // initial, verification, success
  const [verificationCode, setVerificationCode] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  // Password validation
  const hasMinLength = newPassword.length >= 8;
  const hasSpecialChar = /[^a-zA-Z0-9]/.test(newPassword);
  const hasNumber = /\d/.test(newPassword);
  const hasLetter = /[a-zA-Z]/.test(newPassword);
  const passwordsMatch = newPassword === confirmPassword;
  const isPasswordValid =
    hasMinLength && hasSpecialChar && hasNumber && hasLetter;

  const handleInitPasswordChange = async () => {
    try {
      setLoading(true);
      setError('');
      await initChangePassword({ verificationMethod: 'Email' });
      setStep('verification');
    } catch (err) {
      setError('Không thể gửi mã xác thực. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  const handleCompletePasswordChange = async (e) => {
    e.preventDefault();

    if (!isPasswordValid || !passwordsMatch) {
      setError('Vui lòng kiểm tra lại mật khẩu của bạn.');
      return;
    }

    try {
      setLoading(true);
      setError('');
      await completedChangePassword({
        verificationCode,
        newPassword,
        confirmNewPassword: confirmPassword
      });
      setStep('success');
    } catch (err) {
      setError(
        'Không thể đổi mật khẩu. Vui lòng kiểm tra mã xác thực và thử lại.'
      );
    } finally {
      setLoading(false);
    }
  };

  if (step === 'success') {
    return (
      <div className="flex flex-col items-center justify-center py-8 text-center">
        <div className="mb-4 flex h-16 w-16 items-center justify-center rounded-full bg-green-100">
          <Check className="h-8 w-8 text-green-600" />
        </div>
        <h3 className="mb-2 text-xl font-medium">Đổi mật khẩu thành công!</h3>
        <p className="text-gray-500">Mật khẩu của bạn đã được cập nhật.</p>
        <Button className="mt-6" onClick={() => setStep('initial')}>
          Quay lại
        </Button>
      </div>
    );
  }

  if (step === 'verification') {
    return (
      <form onSubmit={handleCompletePasswordChange} className="space-y-6">
        <div className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="otp">Mã xác thực</Label>
            <Input
              id="otp"
              placeholder="Nhập mã xác thực từ email"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value)}
              required
            />
            <p className="text-sm text-gray-500">
              Mã xác thực đã được gửi đến email của bạn
            </p>
          </div>

          <div className="space-y-2">
            <Label htmlFor="new-password">Mật khẩu mới</Label>
            <div className="relative">
              <Input
                id="new-password"
                type={showPassword ? 'text' : 'password'}
                placeholder="Nhập mật khẩu mới"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
              />
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="absolute right-0 top-0 h-full px-3"
                onClick={() => setShowPassword(!showPassword)}
              >
                {showPassword ? (
                  <EyeOff className="h-4 w-4" />
                ) : (
                  <Eye className="h-4 w-4" />
                )}
                <span className="sr-only">
                  {showPassword ? 'Ẩn mật khẩu' : 'Hiện mật khẩu'}
                </span>
              </Button>
            </div>

            <div className="mt-2 space-y-2">
              <p className="text-sm font-medium">Mật khẩu phải có:</p>
              <ul className="grid gap-1 text-sm">
                <li
                  className={`flex items-center gap-2 ${hasMinLength ? 'text-green-600' : 'text-gray-500'}`}
                >
                  <div
                    className={`h-2 w-2 rounded-full ${hasMinLength ? 'bg-green-600' : 'bg-gray-300'}`}
                  />
                  Ít nhất 8 ký tự
                </li>
                <li
                  className={`flex items-center gap-2 ${hasSpecialChar ? 'text-green-600' : 'text-gray-500'}`}
                >
                  <div
                    className={`h-2 w-2 rounded-full ${hasSpecialChar ? 'bg-green-600' : 'bg-gray-300'}`}
                  />
                  Ít nhất 1 ký tự đặc biệt
                </li>
                <li
                  className={`flex items-center gap-2 ${hasNumber ? 'text-green-600' : 'text-gray-500'}`}
                >
                  <div
                    className={`h-2 w-2 rounded-full ${hasNumber ? 'bg-green-600' : 'bg-gray-300'}`}
                  />
                  Ít nhất 1 chữ số
                </li>
                <li
                  className={`flex items-center gap-2 ${hasLetter ? 'text-green-600' : 'text-gray-500'}`}
                >
                  <div
                    className={`h-2 w-2 rounded-full ${hasLetter ? 'bg-green-600' : 'bg-gray-300'}`}
                  />
                  Ít nhất 1 chữ cái
                </li>
              </ul>
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="confirm-password">Xác nhận mật khẩu</Label>
            <Input
              id="confirm-password"
              type={showPassword ? 'text' : 'password'}
              placeholder="Xác nhận mật khẩu mới"
              value={confirmPassword}
              onChange={(e) => setConfirmPassword(e.target.value)}
              required
            />
            {confirmPassword && !passwordsMatch && (
              <p className="text-sm text-red-500">Mật khẩu không khớp</p>
            )}
          </div>
        </div>

        {error && (
          <Alert variant="destructive">
            <AlertCircle className="h-4 w-4" />
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        <div className="flex gap-3">
          <Button
            type="button"
            variant="outline"
            onClick={() => setStep('initial')}
            disabled={loading}
          >
            Hủy
          </Button>
          <Button
            type="submit"
            disabled={
              loading ||
              !isPasswordValid ||
              !passwordsMatch ||
              !verificationCode
            }
          >
            {loading ? 'Đang xử lý...' : 'Hoàn tất'}
          </Button>
        </div>
      </form>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-start gap-4">
        <div className="flex h-10 w-10 items-center justify-center rounded-full bg-teal-100 text-teal-600">
          <Lock className="h-5 w-5" />
        </div>
        <div>
          <h4 className="text-base font-medium">Đổi mật khẩu</h4>
          <p className="text-sm text-gray-500">
            Cập nhật mật khẩu của bạn để bảo vệ tài khoản
          </p>
        </div>
      </div>

      {error && (
        <Alert variant="destructive">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <Button onClick={handleInitPasswordChange} disabled={loading}>
        {loading ? 'Đang xử lý...' : 'Đổi mật khẩu'}
      </Button>
    </div>
  );
}

function ProfileSkeleton() {
  return (
    <>
      <Card className="overflow-hidden border-0 shadow-md">
        <div className="h-32 bg-gradient-to-r from-gray-200 to-gray-300"></div>
        <div className="relative px-6">
          <div className="absolute -top-16 h-32 w-32 rounded-full border-4 border-white bg-gray-200"></div>
        </div>

        <CardContent className="mt-20 px-6 pb-6">
          <div className="flex flex-wrap items-start justify-between gap-4">
            <div>
              <Skeleton className="h-8 w-48" />
              <Skeleton className="mt-2 h-4 w-32" />
            </div>
            <div className="flex gap-2">
              <Skeleton className="h-6 w-20" />
            </div>
          </div>
        </CardContent>
      </Card>

      <div className="grid gap-6 md:grid-cols-2">
        <Card className="border-0 shadow-sm">
          <CardHeader className="border-b pb-3">
            <Skeleton className="h-6 w-36" />
          </CardHeader>
          <CardContent className="space-y-4 pt-4">
            <div className="flex items-start gap-3">
              <Skeleton className="h-9 w-9 rounded-full" />
              <div className="flex-1">
                <Skeleton className="h-4 w-16" />
                <Skeleton className="mt-1 h-5 w-40" />
              </div>
            </div>
            <div className="flex items-start gap-3">
              <Skeleton className="h-9 w-9 rounded-full" />
              <div className="flex-1">
                <Skeleton className="h-4 w-24" />
                <Skeleton className="mt-1 h-5 w-32" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="border-0 shadow-sm">
          <CardHeader className="border-b pb-3">
            <Skeleton className="h-6 w-40" />
          </CardHeader>
          <CardContent className="space-y-4 pt-4">
            <div className="flex items-start gap-3">
              <Skeleton className="h-9 w-9 rounded-full" />
              <div className="flex-1">
                <Skeleton className="h-4 w-20" />
                <div className="mt-2 flex gap-2">
                  <Skeleton className="h-6 w-24" />
                </div>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    </>
  );
}
