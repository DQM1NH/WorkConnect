
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WorkConnect.Models.Employer
{
    public class CreateJobViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập tiêu đề tin tuyển dụng.")]
        [StringLength(200)]
        [Display(Name = "Tiêu đề tin tuyển dụng")]
        public string TieuDe { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập vị trí tuyển dụng.")]
        [StringLength(150)]
        [Display(Name = "Vị trí tuyển dụng")]
        public string ViTri { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngành nghề.")]
        [Display(Name = "Ngành nghề")]
        public int MaNganhNghe { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn loại hình công việc.")]
        [Display(Name = "Loại hình công việc")]
        public int MaLoaiHinh { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hình thức làm việc.")]
        [Display(Name = "Hình thức làm việc")]
        public int MaHinhThuc { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn địa điểm.")]
        [Display(Name = "Địa điểm")]
        public int MaDiaDiem { get; set; }

        [Display(Name = "Mức kinh nghiệm")]
        public int? MaMucKinhNghiem { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Mức lương tối thiểu không hợp lệ.")]
        [Display(Name = "Lương tối thiểu")]
        public decimal? LuongToiThieu { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Mức lương tối đa không hợp lệ.")]
        [Display(Name = "Lương tối đa")]
        public decimal? LuongToiDa { get; set; }

        [Display(Name = "Lương thỏa thuận")]
        public bool LuongThoaThuan { get; set; }

        [Range(1, 10000, ErrorMessage = "Số lượng tuyển phải lớn hơn 0.")]
        [Display(Name = "Số lượng cần tuyển")]
        public int? SoLuongTuyen { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả công việc.")]
        [Display(Name = "Mô tả công việc")]
        public string MoTaCongViec { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu công việc.")]
        [Display(Name = "Yêu cầu công việc")]
        public string YeuCauCongViec { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập yêu cầu ứng viên.")]
        [Display(Name = "Yêu cầu ứng viên")]
        public string YeuCauUngVien { get; set; } = string.Empty;

        [Display(Name = "Quyền lợi")]
        public string? QuyenLoi { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn hạn ứng tuyển.")]
        [DataType(DataType.Date)]
        [Display(Name = "Hạn ứng tuyển")]
        public DateTime HanUngTuyen { get; set; }

        public List<SelectListItem> NganhNghes { get; set; } = new();
        public List<SelectListItem> LoaiHinhs { get; set; } = new();
        public List<SelectListItem> HinhThucs { get; set; } = new();
        public List<SelectListItem> DiaDiems { get; set; } = new();
        public List<SelectListItem> MucKinhNghiems { get; set; } = new();
    }
}
