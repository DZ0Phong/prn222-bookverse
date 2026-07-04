using System.ComponentModel.DataAnnotations;

namespace BookVerse.Models.ViewModels.Admin
{
    public class BookListViewModel
    {
        public List<BookListItem> Books { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? FilterCategoryId { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
        public int PageSize { get; set; } = 10;
    }

    public class BookListItem
    {
        public int BookId { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? CategoryName { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public string? Image { get; set; }
        public bool? IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool HasOrders { get; set; }
    }

    public class BookFormViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Tên sách không được để trống")]
        [MaxLength(255, ErrorMessage = "Tên sách tối đa 255 ký tự")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tác giả không được để trống")]
        [MaxLength(150)]
        public string Author { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Publisher { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Giá không được để trống")]
        [Range(1000, 100000000, ErrorMessage = "Giá phải lớn hơn 1.000 ₫")]
        public decimal Price { get; set; }

        [Range(0, 999999, ErrorMessage = "Số lượng không hợp lệ")]
        public int Quantity { get; set; }

        public string? Description { get; set; }

        public string? ExistingImage { get; set; }

        public IFormFile? ImageFile { get; set; }

        public bool IsActive { get; set; } = true;

        // For dropdown
        public List<Category> Categories { get; set; } = new();
    }
}
