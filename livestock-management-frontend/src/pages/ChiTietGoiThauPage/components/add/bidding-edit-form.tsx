// 'use client';

// import { useState } from 'react';
// import { Button } from '@/components/ui/button';
// import { Input } from '@/components/ui/input';
// import { Textarea } from '@/components/ui/textarea';
// import {
//   Dialog,
//   DialogContent,
//   DialogHeader,
//   DialogTitle,
//   DialogTrigger,
//   DialogFooter
// } from '@/components/ui/dialog';
// import {
//   Form,
//   FormControl,
//   FormField,
//   FormItem,
//   FormLabel,
//   FormMessage
// } from '@/components/ui/form';
// import { useForm } from 'react-hook-form';
// import { zodResolver } from '@hookform/resolvers/zod';
// import * as z from 'zod';
// import { Pencil } from 'lucide-react';

// // Define the form schema based on the API payload
// const formSchema = z.object({
//   code: z.string().min(1, { message: 'Mã gói thầu không được để trống' }),
//   name: z.string().min(1, { message: 'Tên gói thầu không được để trống' }),
//   owner: z.string().min(1, { message: 'Bên mời thầu không được để trống' }),
//   expiredDuration: z.coerce
//     .number()
//     .positive({ message: 'Thời gian thực hiện phải lớn hơn 0' }),
//   description: z.string().optional(),
//   details: z
//     .array(
//       z.object({
//         Id: z.string().min(1, { message: 'ID gói thầu không được để trống' }),
//         speciesId: z
//           .string()
//           .min(1, { message: 'Danh mục hàng hóa không được để trống' }),
//         requiredWeightMin: z.coerce
//           .number()
//           .nonnegative({ message: 'Cân nặng tối thiểu không được âm' }),
//         requiredWeightMax: z.coerce
//           .number()
//           .positive({ message: 'Cân nặng tối đa phải lớn hơn 0' }),
//         requiredAgeMin: z.coerce
//           .number()
//           .nonnegative({ message: 'Tuổi tối thiểu không được âm' }),
//         requiredAgeMax: z.coerce
//           .number()
//           .positive({ message: 'Tuổi tối đa phải lớn hơn 0' }),
//         requiredInsuranceDuration: z.coerce
//           .number()
//           .nonnegative({ message: 'Thời gian bảo hành không được âm' }),
//         requiredQuantity: z.coerce
//           .number()
//           .positive({ message: 'Số lượng phải lớn hơn 0' }),
//         description: z.string().optional()
//       })
//     )
//     .min(1, { message: 'Phải có ít nhất một chi tiết gói thầu' }),
//   requestedBy: z
//     .string()
//     .min(1, { message: 'Người yêu cầu không được để trống' })
// });

// type BiddingFormValues = z.infer<typeof formSchema>;

// interface BiddingEditFormProps {
//   biddingData: any;
//   onSubmit: (data: BiddingFormValues) => void;
// }

// export function BiddingEditForm({
//   biddingData,
//   onSubmit
// }: BiddingEditFormProps) {
//   const [open, setOpen] = useState(false);
//   // Transform the existing data to match the form schema
//   const defaultValues: BiddingFormValues = {
//     code: biddingData.code || '',
//     name: biddingData.name || '',
//     owner: biddingData.owner || '',
//     expiredDuration: biddingData.expiredDuration || 0,
//     description: biddingData.description || '',
//     details: biddingData.details?.map((detail: any) => ({
//       id: detail.id || '',
//       speciesId: detail.speciesId || '',
//       requiredWeightMin: detail.requiredWeightMin || 0,
//       requiredWeightMax: detail.requiredWeightMax || 0,
//       requiredAgeMin: detail.requiredAgeMin || 0,
//       requiredAgeMax: detail.requiredAgeMax || 0,
//       requiredInsuranceDuration:
//         detail.requiredInsuranceDuration || detail.requiredInsurance || 0,
//       requiredQuantity: detail.requiredQuantity || 0,
//       description: detail.description || ''
//     })) || [
//       {
//         speciesId: '',
//         requiredWeightMin: 0,
//         requiredWeightMax: 0,
//         requiredAgeMin: 0,
//         requiredAgeMax: 0,
//         requiredInsuranceDuration: 0,
//         requiredQuantity: 0,
//         description: ''
//       }
//     ],
//     requestedBy: biddingData.createdBy || ''
//   };

//   const form = useForm<BiddingFormValues>({
//     resolver: zodResolver(formSchema),
//     defaultValues
//   });

//   const handleSubmit = (values: BiddingFormValues) => {
//     console.log('values', values);
//     onSubmit(values);
//     setOpen(false);
//   };

//   return (
//     <Dialog open={open} onOpenChange={setOpen}>
//       <DialogTrigger asChild>
//         <Button variant="outline" size="sm" className="gap-1">
//           <Pencil className="h-4 w-4" />
//           Chỉnh sửa
//         </Button>
//       </DialogTrigger>
//       <DialogContent className="max-h-[90vh] max-w-3xl overflow-y-auto">
//         <DialogHeader>
//           <DialogTitle>Chỉnh sửa gói thầu</DialogTitle>
//         </DialogHeader>
//         <Form {...form}>
//           <form
//             onSubmit={form.handleSubmit(handleSubmit)}
//             className="space-y-6"
//           >
//             <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
//               <FormField
//                 control={form.control}
//                 name="code"
//                 render={({ field }) => (
//                   <FormItem>
//                     <FormLabel>Mã gói thầu</FormLabel>
//                     <FormControl>
//                       <Input {...field} />
//                     </FormControl>
//                     <FormMessage />
//                   </FormItem>
//                 )}
//               />
//               <FormField
//                 control={form.control}
//                 name="name"
//                 render={({ field }) => (
//                   <FormItem>
//                     <FormLabel>Tên gói thầu</FormLabel>
//                     <FormControl>
//                       <Input {...field} />
//                     </FormControl>
//                     <FormMessage />
//                   </FormItem>
//                 )}
//               />
//               <FormField
//                 control={form.control}
//                 name="owner"
//                 render={({ field }) => (
//                   <FormItem>
//                     <FormLabel>Bên mời thầu</FormLabel>
//                     <FormControl>
//                       <Input {...field} />
//                     </FormControl>
//                     <FormMessage />
//                   </FormItem>
//                 )}
//               />
//               <FormField
//                 control={form.control}
//                 name="expiredDuration"
//                 render={({ field }) => (
//                   <FormItem>
//                     <FormLabel>Thời gian thực hiện (ngày)</FormLabel>
//                     <FormControl>
//                       <Input type="number" {...field} />
//                     </FormControl>
//                     <FormMessage />
//                   </FormItem>
//                 )}
//               />
//               <FormField
//                 control={form.control}
//                 name="requestedBy"
//                 render={({ field }) => (
//                   <FormItem>
//                     <FormLabel>Người yêu cầu</FormLabel>
//                     <FormControl>
//                       <Input {...field} />
//                     </FormControl>
//                     <FormMessage />
//                   </FormItem>
//                 )}
//               />
//             </div>

//             <FormField
//               control={form.control}
//               name="description"
//               render={({ field }) => (
//                 <FormItem>
//                   <FormLabel>Mô tả</FormLabel>
//                   <FormControl>
//                     <Textarea rows={3} {...field} />
//                   </FormControl>
//                   <FormMessage />
//                 </FormItem>
//               )}
//             />

//             <div className="rounded-md border p-4">
//               <h3 className="mb-4 font-medium">Chi tiết yêu cầu kỹ thuật</h3>
//               {form.watch('details').map((_, index) => (
//                 <div key={index} className="mb-6 space-y-4">
//                   <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
//                     <FormField
//                       control={form.control}
//                       name={`details.${index}.speciesId`}
//                       render={({ field }) => (
//                         <FormItem>
//                           <FormLabel>Danh mục hàng hóa</FormLabel>
//                           <FormControl>
//                             <Input {...field} />
//                           </FormControl>
//                           <FormMessage />
//                         </FormItem>
//                       )}
//                     />

//                     <FormField
//                       control={form.control}
//                       name={`details.${index}.requiredQuantity`}
//                       render={({ field }) => (
//                         <FormItem>
//                           <FormLabel>Số lượng (con)</FormLabel>
//                           <FormControl>
//                             <Input type="number" {...field} />
//                           </FormControl>
//                           <FormMessage />
//                         </FormItem>
//                       )}
//                     />
//                     <FormField
//                       control={form.control}
//                       name={`details.${index}.requiredWeightMin`}
//                       render={({ field }) => (
//                         <FormItem>
//                           <FormLabel>Cân nặng tối thiểu (kg)</FormLabel>
//                           <FormControl>
//                             <Input type="number" {...field} />
//                           </FormControl>
//                           <FormMessage />
//                         </FormItem>
//                       )}
//                     />
//                     <FormField
//                       control={form.control}
//                       name={`details.${index}.requiredWeightMax`}
//                       render={({ field }) => (
//                         <FormItem>
//                           <FormLabel>Cân nặng tối đa (kg)</FormLabel>
//                           <FormControl>
//                             <Input type="number" {...field} />
//                           </FormControl>
//                           <FormMessage />
//                         </FormItem>
//                       )}
//                     />
//                     <FormField
//                       control={form.control}
//                       name={`details.${index}.requiredAgeMin`}
//                       render={({ field }) => (
//                         <FormItem>
//                           <FormLabel>Tuổi tối thiểu (tháng)</FormLabel>
//                           <FormControl>
//                             <Input type="number" {...field} />
//                           </FormControl>
//                           <FormMessage />
//                         </FormItem>
//                       )}
//                     />
//                     <FormField
//                       control={form.control}
//                       name={`details.${index}.requiredAgeMax`}
//                       render={({ field }) => (
//                         <FormItem>
//                           <FormLabel>Tuổi tối đa (tháng)</FormLabel>
//                           <FormControl>
//                             <Input type="number" {...field} />
//                           </FormControl>
//                           <FormMessage />
//                         </FormItem>
//                       )}
//                     />
//                     <FormField
//                       control={form.control}
//                       name={`details.${index}.requiredInsuranceDuration`}
//                       render={({ field }) => (
//                         <FormItem>
//                           <FormLabel>Thời gian bảo hành (ngày)</FormLabel>
//                           <FormControl>
//                             <Input type="number" {...field} />
//                           </FormControl>
//                           <FormMessage />
//                         </FormItem>
//                       )}
//                     />
//                   </div>
//                   <FormField
//                     control={form.control}
//                     name={`details.${index}.description`}
//                     render={({ field }) => (
//                       <FormItem>
//                         <FormLabel>Yêu cầu khác</FormLabel>
//                         <FormControl>
//                           <Textarea rows={2} {...field} />
//                         </FormControl>
//                         <FormMessage />
//                       </FormItem>
//                     )}
//                   />
//                 </div>
//               ))}
//             </div>

//             <DialogFooter>
//               <Button
//                 type="button"
//                 variant="outline"
//                 onClick={() => setOpen(false)}
//               >
//                 Hủy
//               </Button>
//               <Button type="submit" onClick={() => handleSubmit()}>
//                 Lưu thay đổi
//               </Button>
//             </DialogFooter>
//           </form>
//         </Form>
//       </DialogContent>
//     </Dialog>
//   );
// }
