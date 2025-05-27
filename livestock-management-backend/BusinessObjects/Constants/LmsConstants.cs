namespace BusinessObjects.Constants
{
    public static class LmsConstants
    {
        public const string OrganizationName = "HỢP TÁC XÃ DỊCH VỤ TỔNG HỢP VÀ SẢN XUẤT NÔNG NGHIỆP LÚA VÀNG";
        public const string CloudFolderFileTemplateName = "livestock-management-templates";
        public const string CloudFolderFileErrorName = "livetock-managment-error-files";
        public const string CloudFolderFileQrCodesName = "livestock-management-qr-codes";
        public const string CloudFolderFileReportsName = "livestock-management-reports";
        public const string CloudFolderFileSuggestionsName = "livestocks-suggestion-files";

        public enum livestock_gender
        {
            ĐỰC,
            CÁI
        }

        public enum livestock_status
        {
            TRỐNG,
            CHỜ_NHẬP,
            CHỜ_ĐỊNH_DANH,
            KHỎE_MẠNH,
            ỐM,
            CHỜ_XUẤT,
            ĐÃ_XUẤT,
            CHẾT,
            XUẤT_BÁN_THỊT,
        }

        public enum batch_export_status
        {
            CHỜ_CHỌN,
            CHỜ_BÀN_GIAO,
            ĐÃ_BÀN_GIAO,
        }
      
        public enum batch_import_status
        {
            ĐÃ_HỦY,
            CHỜ_NHẬP,
            ĐANG_NHẬP,
            ĐÃ_NHẬP,
            HOÀN_THÀNH
        }

        public enum procurement_status
        {
            ĐANG_ĐẤU_THẦU,
            CHỜ_BÀN_GIAO,
            ĐANG_BÀN_GIAO,
            HOÀN_THÀNH,
            ĐÃ_HỦY,
            ĐANG_CHỜ_CHỌN
        }

        public enum medicine_type
        {
            VACCINE,
            THUỐC_CHỮA_BỆNH,
            KHÁNG_SINH
        }

        public enum batch_vaccination_status
        {
            CHỜ_THỰC_HIỆN,
            ĐANG_THỰC_HIỆN,
            HOÀN_THÀNH,
            ĐÃ_HỦY
        }

        public enum batch_vaccination_type
        {
            TIÊM_VACCINE,
            TIÊM_CHỮA_BỆNH,
            TIÊM_CHO_GÓI_THẦU
        }

        public enum specie_type
        {
            TRÂU,
            BÒ,
            LỢN,
            GÀ,
            DÊ,
            CỪU,
            NGỰA,
            LA,
            LỪA
        }

        public enum inspection_code_range_status
        { 
            DÙNG_HẾT,
            ĐANG_SỬ_DỤNG,
            MỚI
        }

        public enum disease_type
        {
            TRUYỀN_NHIỄM_NGUY_HIỂM,
            TRUYỀN_NHIỄM,
            KHÔNG_TRUYỀN_NHIỄM
        }

        public enum medical_history_status
        {
            CHỜ_KHÁM,       
            ĐANG_ĐIỀU_TRỊ,  
            ĐÃ_KHỎI,         
            TÁI_PHÁT,        
            NGỪNG_ĐIỀU_TRỊ
        }

        public enum request_status
        {
            THÀNH_CÔNG,
            THẤT_BẠI
        }

        public enum entity_action
        {
            TẠO_MỚI,
            CHỈNH_SỬA,
            XÓA,
            BÁO_CÁO
        }

        public enum entity_type
        {
            VẬT_NUÔI,
            LÔ_TIÊM,
            LÔ_NHẬP,
            LÔ_XUẤT,
            GÓI_THẦU,
            ĐƠN_HÀNG_LẺ,
            ĐƠN_BẢO_HÀNH
        }

        public enum order_status
        {
            MỚI,
            ĐANG_CHUẨN_BỊ,
            CHỜ_BÀN_GIAO,
            ĐANG_BÀN_GIAO,
            HOÀN_THÀNH,
            ĐÃ_HỦY
        }

        public enum order_type
        {
            KHÔNG_YÊU_CẦU,
            YÊU_CẦU_CHỌN,
            YÊU_CẦU_XUẤT,
        }

        public enum insurance_request_status
        {
            CHỜ_DUYỆT,
            ĐANG_CHUẨN_BỊ,
            CHỜ_BÀN_GIAO,
            TỪ_CHỐI,
            HOÀN_THÀNH,
            ĐÃ_HỦY
        }

        public enum OrderBy
        {
            HẠN_HOÀN_THÀNH_TĂNG_DẦN,
            HẠN_HOÀN_THÀNH_GIẢM_DẦN,
            SỐ_LƯỢNG_CÒN_THIẾU_GIẢM_DẦN,
            SỐ_LƯỢNG_CÒN_THIẾU_TĂNG_DẦN
        }

        public enum IsCreated
        {
           ĐÃ_TẠO,
           CHƯA_TẠO,
           ĐÃ_HOÀN_THÀNH
        }

        public enum insurance_request_livestock_status
        {
            KHÔNG_THU_HỒI,
            CHỜ_THU_HỒI,
            ĐÃ_THU_HỒI
        }

        public enum severity
        {
            HIGH,
            MEDIUM,
            LOW
        }
         
        public static String urlDeploy = "lms.com/";
    }
}
