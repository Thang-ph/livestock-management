import NotFound from '@/pages/not-found';
import UserPage from '@/pages/UserPage';
import { Suspense, lazy } from 'react';
import { Navigate, Outlet, useRoutes } from 'react-router-dom';
import ProtectedRoute from './ProtectedRoute';
// import ProtectedRoute from './ProtectedRoute';

const DashboardLayout = lazy(
  () => import('@/components/layout/dashboard-layout')
);

const SignInPage = lazy(() => import('@/pages/auth/signin'));
const DanhSachGoiThauPage = lazy(() => import('@/pages/DanhSachGoiThauPage'));
const ChiTietGoiThauPage = lazy(() => import('@/pages/ChiTietGoiThauPage'));
const GoiThauPage = lazy(() => import('@/pages/GoiThauPage'));
const LoTiemPage = lazy(() => import('@/pages/DanhSachLoTiem'));
const ChiTietLoTiemPage = lazy(() => import('@/pages/ChiTietLoTiem'));
const DanhSachLoNhapPage = lazy(() => import('@/pages/DanhSachLoNhap/index'));
const ChiTietLoNhapPage = lazy(() => import('@/pages/ChiTietLoNhap/index'));
const DanhSachThuocPage = lazy(() => import('@/pages/DanhSachThuoc/index'));
const DanhSachVatNuoiPage = lazy(() => import('@/pages/DanhSachVatNuoi/index'));
const UserProfile = lazy(() => import('@/pages/UserProfile/index'));
const DanhSachBenh = lazy(() => import('@/pages/DanhSachBenh/index'));
const GoogleCallback = lazy(
  () => import('@/pages/auth/google-call-back/index')
);
const CompleteForgotPasswordPage = lazy(
  () => import('@/pages/auth/complete-forgot-password/index')
);

const DashboardPage = lazy(() => import('@/pages/Dashboard/index'));
const LandingPage = lazy(() => import('@/pages/LandingPage/index'));
const PermissionPage = lazy(() => import('@/pages/PermissionPage/index'));
const MaKiemDichPage = lazy(() => import('@/pages/MaKiemDichPage/index'));
const MaKiemDichTheoLoaiPage = lazy(
  () => import('@/pages/MaKiemDichTheoLoaiPage/index')
);

const ChiTietMaKiemDichPage = lazy(
  () => import('@/pages/ChiTietMaKiemDichVatNuoiPage/index')
);
const QuanLyLoTiemPage = lazy(() => import('@/pages/QuanLyLoTiemPage/index'));
const ChiTietTiemChungGoiThauPage = lazy(
  () => import('@/pages/ChiTietTiemChungGoiThau/index')
);
const DanhSachLoNhapAdminPage = lazy(
  () => import('@/pages/DanhSachLoNhapAdmin/index')
);

// ----------------------------------------------------------------------

export default function AppRouter() {
  const dashboardRoutes = [
    {
      element: (
        <DashboardLayout>
          <Suspense>
            <Outlet />
          </Suspense>
        </DashboardLayout>
      ),
      children: [
        {
          path: '/nguoi-dung',
          element: <UserPage />,
          index: true
        },
        {
          path: '/quyen',
          element: <PermissionPage />,
          index: true
        },
        {
          path: '/admin/lo-nhap',
          element: (
            <ProtectedRoute>
              <DanhSachLoNhapAdminPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/ma-kiem-dich',
          element: (
            <ProtectedRoute>
              <MaKiemDichPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/quan-ly-lo-tiem',
          element: (
            <ProtectedRoute>
              <QuanLyLoTiemPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/chi-tiet-tiem-chung-goi-thau/:id',
          element: (
            <ProtectedRoute>
              <ChiTietTiemChungGoiThauPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/chi-tiet-ma-kiem-dich/:speciesName',
          element: (
            <ProtectedRoute>
              <ChiTietMaKiemDichPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/ma-kiem-dich-theo-loai',
          element: (
            <ProtectedRoute>
              <MaKiemDichTheoLoaiPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/goi-thau',
          element: (
            <ProtectedRoute>
              <GoiThauPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/danh-sach-goi-thau',
          element: (
            <ProtectedRoute>
              <DanhSachGoiThauPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/chi-tiet-goi-thau/:id',
          element: (
            <ProtectedRoute>
              <ChiTietGoiThauPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/lo-tiem',
          element: (
            <ProtectedRoute>
              <LoTiemPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/chi-tiet-lo-tiem/:id',
          element: (
            <ProtectedRoute>
              <ChiTietLoTiemPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/lo-nhap',
          element: (
            <ProtectedRoute>
              <DanhSachLoNhapPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/chi-tiet-lo-nhap/:id',
          element: (
            <ProtectedRoute>
              <ChiTietLoNhapPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/danh-sach-thuoc',
          element: (
            <ProtectedRoute>
              <DanhSachThuocPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/danh-sach-vat-nuoi',
          element: (
            <ProtectedRoute>
              <DanhSachVatNuoiPage />
            </ProtectedRoute>
          )
        },
        {
          path: '/user-profile',
          element: (
            <ProtectedRoute>
              <UserProfile />
            </ProtectedRoute>
          )
        },
        {
          path: '/danh-sach-benh',
          element: (
            <ProtectedRoute>
              <DanhSachBenh />
            </ProtectedRoute>
          )
        },
        {
          path: '/dashboard',
          element: (
            <ProtectedRoute>
              <DashboardPage />
            </ProtectedRoute>
          )
        }
      ]
    }
  ];

  const publicRoutes = [
    {
      path: '/login',
      element: <SignInPage />,
      index: true
    },
    {},
    {
      path: '/auth-callback',
      element: <GoogleCallback />,
      index: true
    },
    {
      path: '/auth/forgot-password/:resetToken',
      element: <CompleteForgotPasswordPage />,
      index: true
    },
    {
      path: '/',
      element: <LandingPage />
    },
    {
      path: '/404',
      element: <NotFound />
    },
    {
      path: '*',
      element: <Navigate to="/404" replace />
    }
  ];

  const routes = useRoutes([...dashboardRoutes, ...publicRoutes]);

  return routes;
}
