public class ChiTietPhieu
{
    public string MaPhieu { get; set; }
    public string MaHang { get; set; }
    public string TenHang { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGia { get; set; }
    public decimal ThanhTien => SoLuong * DonGia;
}