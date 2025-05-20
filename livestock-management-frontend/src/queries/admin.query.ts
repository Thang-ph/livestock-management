import BaseRequest, { BaseRequestV2 } from '@/config/axios.config';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

export const useGetAllCategory = () => {
  return useQuery({
    queryKey: ['get-all-category'],
    queryFn: async () => {
      return await BaseRequest.Get(`/api/categories`);
    }
  });
};

export const useCreateCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationKey: ['create-category'],
    mutationFn: async (model: any) => {
      return await BaseRequest.Post(`/api/categories`, model);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['get-all-category']
      });
    }
  });
};

export const useDeleteCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationKey: ['delete-category'],
    mutationFn: async (id: string) => {
      return await BaseRequest.Delete(`/api/categories/${id}`);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['get-all-category']
      });
    }
  });
};

export const useUpdateCategory = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationKey: ['update-category'],
    mutationFn: async (model: any) => {
      return await BaseRequest.Put(
        `/api/categories/${model.category_code}`,
        model
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['get-all-category']
      });
    }
  });
};

export const useGetAllProduct = () => {
  return useQuery({
    queryKey: ['get-all-product'],
    queryFn: async () => {
      return await BaseRequest.Get(`/api/products`);
    }
  });
};

export const useGetGoiThau = (keyword) => {
  return useQuery({
    queryKey: ['get-goi-thau', keyword],
    queryFn: async () => {
      const params = new URLSearchParams();
      params.append('keyword', keyword);
      return await BaseRequest.Get(
        `api/procurement-management/get-list?${params.toString()}`
      );
    }
  });
};

export const useGetSpecie = () => {
  return useQuery({
    queryKey: ['get-specie'],
    queryFn: async () => {
      return await BaseRequest.Get(`/api/specie-mangament/get-all`);
    }
  });
};
export const useGetSpecieType = () => {
  return useQuery({
    queryKey: ['get-specie-type'],
    queryFn: async () => {
      const res = await BaseRequest.Get(
        `/api/specie-mangament/get-list-specie-type`
      );
      return res.data;
    }
  });
};

export const useGetSpeciceName = (id: string) => {
  return useQuery({
    queryKey: ['get-specie-name', id],
    queryFn: async () => {
      var res = await BaseRequest.Get(
        `/api/specie-mangament/get-list-specie-name/${id}`
      );
      return res.data;
    }
  });
};

export const useCreateGoiThau = () => {
  return useMutation({
    mutationKey: ['create-goi-thau'],
    mutationFn: async (model: any) => {
      return await BaseRequest.Post(`api/procurement-management/create`, model);
    }
  });
};

export const useGetThongTinGoiThau = (id: string) => {
  return useQuery({
    queryKey: ['get-thong-tin-goi-thau', id],
    queryFn: async () => {
      return await BaseRequest.Get(
        `/api/procurement-management/general-info/${id}`
      );
    }
  });
};

export const useGetDanhSachKhachHangGoiThau = (id: string) => {
  return useQuery({
    queryKey: ['get-danh-sach-khach-hang-goi-thau', id],
    queryFn: async () => {
      return await BaseRequest.Post(
        `/api/export-management/get-list-customers/${id}`,
        {}
      );
    }
  });
};

export const useChinhSuaGoiThau = () => {
  return useMutation({
    mutationKey: ['chinh-sua-goi-thau'],
    mutationFn: async (model: any) => {
      return await BaseRequest.Post(`api/procurement-management/update`, model);
    }
  });
};

export const useUploadExcelKhachHang = () => {};

export const useGetListVatNuoiKhachHang = (id) => {
  return useQuery({
    queryKey: ['get-list-vat-nuoi-khach-hang'],
    queryFn: async () => {
      return await BaseRequest.Get(
        `/api/procurement-management/list-export-details/${id}`
      );
    }
  });
};

export const useGetBatchVaccinList = () => {
  return useQuery({
    queryKey: ['get-batch-vaccin-list'],
    queryFn: async () => {
      return await BaseRequest.Get(
        `/api/vaccination-management/get-batch-vaccinations-list`
      );
    }
  });
};

export const useGetNguoiThucHien = () => {
  return useMutation({
    mutationKey: ['get-nguoi-thuc-hien'],
    mutationFn: async (dateTime: any) => {
      return await BaseRequest.Get(
        `/api/vaccination-management/get-list-conductor?dateSchedule=${dateTime}`
      );
    }
  });
};

export const useGetListLoTiemNhacLai = () => {
  return useQuery({
    queryKey: ['get-list-lo-tiem-nhac-lai'],
    queryFn: async () => {
      var data = await BaseRequest.Get(
        `/api/vaccination-management/get-list-suggest-re-vaccination`
      );
      return data.data;
    }
  });
};

export const useGetListLoTiemSapToi = () => {
  return useQuery({
    queryKey: ['get-list-lo-tiem-sap-toi'],
    queryFn: async () => {
      var res = await BaseRequest.Get(
        `/api/vaccination-management/get-list-future-vaccination`
      );
      return res.data;
    }
  });
};

export const useAddLiveStockToBatch = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationKey: ['add-live-stock-to-batch'],
    mutationFn: async (model: any) => {
      return await BaseRequestV2.Put(
        `/api/import-management/add-livestock-to-batchimport/${model.id}`,
        model.item
      );
    },
    onSuccess: () => {
      queryClient.invalidateQueries({
        queryKey: ['get-list-lo-tiem-sap-toi']
      });
    }
  });
};
