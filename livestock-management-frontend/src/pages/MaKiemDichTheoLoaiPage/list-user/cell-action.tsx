// 'use client';

// import type React from 'react';

// import { useState } from 'react';
// import { Button } from '@/components/ui/button';
// import {
//   Dialog,
//   DialogContent,
//   DialogHeader,
//   DialogTitle,
//   DialogFooter
// } from '@/components/ui/dialog';
// import { Badge } from '@/components/ui/badge';
// import { Separator } from '@/components/ui/separator';
// import { Eye } from 'lucide-react';

// interface InspectionCodeData {
//   startCode: string;
//   endCode: string;
//   currentCode: string;
//   quantity: number;
//   orderNumber: number;
//   specieTypeList: string[];
//   status: number;
//   inspectionCodeCounters: any[];
//   id: string;
//   createdAt: string;
//   updatedAt: string;
//   createdBy: string;
//   updatedBy: string;
// }

// interface InspectionCodeDetailsProps {
//   data: InspectionCodeData;
// }

// export const CellAction: React.FC<InspectionCodeDetailsProps> = ({ data }) => {
//   const [open, setOpen] = useState(false);

//   // Format date to a more readable format
//   const formatDate = (dateString: string) => {
//     const date = new Date(dateString);
//     return new Intl.DateTimeFormat('vi-VN', {
//       day: '2-digit',
//       month: '2-digit',
//       year: 'numeric',
//       hour: '2-digit',
//       minute: '2-digit',
//       second: '2-digit'
//     }).format(date);
//   };

//   // Map status code to a more user-friendly label and variant
//   const getStatusInfo = (status: number) => {
//     switch (status) {
//       case 0:
//         return { label: 'Chờ xử lý', variant: 'outline' as const };
//       case 1:
//         return { label: 'Đang xử lý', variant: 'secondary' as const };
//       case 2:
//         return { label: 'Hoàn thành', variant: 'success' as const };
//       case 3:
//         return { label: 'Đã hủy', variant: 'destructive' as const };
//       default:
//         return { label: `Trạng thái ${status}`, variant: 'outline' as const };
//     }
//   };

//   const statusInfo = getStatusInfo(data.status);

//   return (
//     <>
//       <Button variant="outline" size="sm" onClick={() => setOpen(true)}>
//         <Eye className="mr-2 h-4 w-4" />
//         Xem chi tiết
//       </Button>

//       <Dialog open={open} onOpenChange={setOpen}>
//         <DialogContent className="sm:max-w-md md:max-w-lg">
//           <DialogHeader>
//             <DialogTitle>Chi tiết mã kiểm định</DialogTitle>
//           </DialogHeader>

//           <div className="grid gap-4 py-4">
//             <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Mã bắt đầu
//                 </p>
//                 <p className="font-medium">{data.startCode}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Mã kết thúc
//                 </p>
//                 <p className="font-medium">{data.endCode}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Mã hiện tại
//                 </p>
//                 <p className="font-medium">{data.currentCode}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Số lượng
//                 </p>
//                 <p className="font-medium">{data.quantity.toLocaleString()}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Số thứ tự
//                 </p>
//                 <p className="font-medium">{data.orderNumber}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Trạng thái
//                 </p>
//                 <Badge variant={statusInfo.variant}>{statusInfo.label}</Badge>
//               </div>
//             </div>

//             <Separator />

//             <div className="space-y-1">
//               <p className="text-sm font-medium text-muted-foreground">
//                 Loại động vật
//               </p>
//               <div className="flex flex-wrap gap-2">
//                 {data.specieTypeList.map((specie, index) => (
//                   <Badge key={index} variant="outline">
//                     {specie}
//                   </Badge>
//                 ))}
//               </div>
//             </div>

//             <Separator />

//             <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Ngày tạo
//                 </p>
//                 <p className="font-medium">{formatDate(data.createdAt)}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Ngày cập nhật
//                 </p>
//                 <p className="font-medium">{formatDate(data.updatedAt)}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Người tạo
//                 </p>
//                 <p className="font-medium">{data.createdBy}</p>
//               </div>
//               <div className="space-y-1">
//                 <p className="text-sm font-medium text-muted-foreground">
//                   Người cập nhật
//                 </p>
//                 <p className="font-medium">{data.updatedBy}</p>
//               </div>
//             </div>
//           </div>

//           <DialogFooter>
//             <Button onClick={() => setOpen(false)}>Đóng</Button>
//           </DialogFooter>
//         </DialogContent>
//       </Dialog>
//     </>
//   );
// };
