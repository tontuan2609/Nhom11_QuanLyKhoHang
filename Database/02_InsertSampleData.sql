USE QuanLyKhoHangDB;
GO

-- Chèn tài khoản mẫu để chạy thử các vai trò
INSERT INTO TaiKhoan (TenDangNhap, MatKhau, HoTen, VaiTro, TrangThai) VALUES
('admin', '123456', N'Nguyễn Văn Quản Lý', N'Quản lý', 1),
('nvkho1', '123456', N'Trần Thị Nhân Viên', N'Nhân viên kho', 1);

-- Chèn Loại hàng
INSERT INTO LoaiHang (MaLoai, TenLoai, MoTa) VALUES
('LH01', N'Linh kiện điện tử', N'RAM, SSD, CPU, Mainboard'),
('LH02', N'Thiết bị ngoại vi', N'Chuột, bàn phím, màn hình'),
('LH03', N'Phụ kiện mạng', N'Router, Switch, Cáp mạng');

-- Chèn Nhà cung cấp
INSERT INTO NhaCungCap (MaNCC, TenNCC, DienThoai, DiaChi, Email) VALUES
('NCC01', N'Công ty TNHH Phân phối FPT', '02873000911', N'Quận 7, TP.HCM', 'contact@fpt.com.vn'),
('NCC02', N'Công ty Cổ phần Viễn Sơn', '02839250707', N'Quận 1, TP.HCM', 'info@vienson.vn');

-- Chèn Hàng hóa (Có mặt hàng tồn thấp <= MucTonToiThieu để test cảnh báo đỏ)
INSERT INTO HangHoa (MaHang, TenHang, MaLoai, DonViTinh, DonGiaNhap, DonGiaXuat, SoLuongTon, MucTonToiThieu) VALUES
('HH01', N'Ổ cứng SSD Kingston NV2 500GB', 'LH01', N'Cái', 850000, 1050000, 25, 10),
('HH02', N'RAM Desktop Corsair 16GB DDR4', 'LH01', N'Thanh', 920000, 1150000, 5, 10), -- Tồn thấp (5 <= 10)
('HH03', N'Chuột Logitech G102 Lightsync', 'LH02', N'Cái', 320000, 450000, 40, 15),
('HH04', N'Router Wifi TP-Link Archer C6', 'LH03', N'Bộ', 650000, 820000, 0, 5);   -- Hết hàng (0)

-- Chèn Phiếu nhập mẫu
INSERT INTO PhieuNhap (MaPhieuNhap, NgayNhap, MaNCC, NguoiLap, TongTien, GhiChu) VALUES
('PN001', '2026-10-01 08:30:00', 'NCC01', 'nvkho1', 25850000, N'Nhập hàng đợt 1 tháng 10');

INSERT INTO ChiTietPhieuNhap (MaPhieuNhap, MaHang, SoLuong, DonGia) VALUES
('PN001', 'HH01', 25, 850000),
('PN001', 'HH02', 5, 920000);
GO