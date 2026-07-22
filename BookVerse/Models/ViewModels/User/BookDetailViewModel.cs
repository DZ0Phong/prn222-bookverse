namespace BookVerse.Models.ViewModels.User
{
    public class BookDetailViewModel
    {
        // Thông tin cuốn sách hiện tại
        public required Book Book { get; set; }

        // Danh sách các review đã có của cuốn sách này
        public List<Review> ExistingReviews { get; set; } = new();

        // Đối tượng dùng để bind dữ liệu từ Form Review khi user gửi lên
        public Review NewReview { get; set; } = new();
    }
}
