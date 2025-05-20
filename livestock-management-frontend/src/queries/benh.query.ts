import BaseRequest from '@/config/axios.config';
import { useQuery } from '@tanstack/react-query';

export const useGetLoaiBenh = () => {
  return useQuery({
    queryKey: ['get-my-info'],
    queryFn: async () => {
      const res = await BaseRequest.Get(
        `/api/disease-management/get-list-diseases`
      );
      return res.data;
    }
  });
};
