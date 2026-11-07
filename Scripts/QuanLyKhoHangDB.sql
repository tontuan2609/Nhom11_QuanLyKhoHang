CREATE DATABASE QuanLyKhoHangDB;
GO
USE QuanLyKhoHangDB;
GO

-- =========================================================================
-- ĐỒ ÁN CUỐI KỲ: LẬP TRÌNH TRÊN WINDOWS - QUẢN LÝ KHO HÀNG (NHÓM 11)
-- THÀNH VIÊN:
-- 1. Thành viên 1: CSDL, Database.cs, Đăng nhập phân quyền, FrmMain, Hàng hóa & Loại hàng
-- 2. Thành viên 2: Nhà cung cấp, Lập phiếu nhập/xuất kho (SqlTransaction)
-- 3. Thành viên 3: Tính tồn kho qua SP, Lịch sử nhập xuất, Xuất CSV, Biểu đồ GDI+, In phiếu kho
-- =========================================================================

-- 1. Bảng Người dùng (Phục vụ đăng nhập và phân quyền: Quản lý / Nhân viên kho)
CREATE TABLE NguoiDung (
    MaNguoiDung VARCHAR(20) PRIMARY KEY,
    TenDangNhap VARCHAR(50) NOT NULL UNIQUE,
    MatKhau VARCHAR(100) NOT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    VaiTro NVARCHAR(30) NOT NULL CHECK (VaiTro IN (N'Quản lý', N'Nhân viên kho')),
    TrangThai BIT DEFAULT 1
);

-- 2. Bảng Loại hàng
CREATE TABLE LoaiHang (
    MaLoai VARCHAR(20) PRIMARY KEY,
    TenLoai NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(255)
);

-- 3. Bảng Nhà cung cấp
CREATE TABLE NhaCungCap (
    MaNCC VARCHAR(20) PRIMARY KEY,
    TenNCC NVARCHAR(150) NOT NULL,
    DienThoai VARCHAR(15) NOT NULL,
    DiaChi NVARCHAR(255)
);

-- 4. Bảng Hàng hóa
CREATE TABLE HangHoa (
    MaHang VARCHAR(20) PRIMARY KEY,
    TenHang NVARCHAR(150) NOT NULL,
    MaLoai VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES LoaiHang(MaLoai),
    DonViTinh NVARCHAR(30) NOT NULL,
    DonGiaNhap DECIMAL(18,2) NOT NULL CHECK (DonGiaNhap >= 0),
    SoLuongTon INT NOT NULL DEFAULT 0 CHECK (SoLuongTon >= 0),
    MucTonToiThieu INT NOT NULL DEFAULT 10,
    HinhAnh NVARCHAR(255) NULL
);

-- 5. Bảng Phiếu kho (Dùng chung cho Phiếu Nhập và Phiếu Xuất)
CREATE TABLE PhieuKho (
    MaPhieu VARCHAR(20) PRIMARY KEY,
    LoaiPhieu NVARCHAR(10) NOT NULL CHECK (LoaiPhieu IN (N'Nhập', N'Xuất')),
    NgayLap DATETIME NOT NULL DEFAULT GETDATE(),
    MaNguoiDung VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES NguoiDung(MaNguoiDung),
    MaNCC VARCHAR(20) NULL FOREIGN KEY REFERENCES NhaCungCap(MaNCC),
    GhiChu NVARCHAR(255),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0
);

-- 6. Bảng Chi tiết phiếu
CREATE TABLE ChiTietPhieu (
    MaPhieu VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES PhieuKho(MaPhieu),
    MaHang VARCHAR(20) NOT NULL FOREIGN KEY REFERENCES HangHoa(MaHang),
    SoLuong INT NOT NULL CHECK (SoLuong > 0),
    DonGia DECIMAL(18,2) NOT NULL CHECK (DonGia >= 0),
    ThanhTien AS (SoLuong * DonGia),
    PRIMARY KEY (MaPhieu, MaHang)
);
GO

-- =========================================================================
-- CÁC STORED PROCEDURES DÙNG TRONG HỆ THỐNG
-- =========================================================================

-- 1. SP lấy danh sách hàng hóa kèm tên loại và cảnh báo tồn thấp (Thành viên 1)
CREATE PROCEDURE sp_LayDanhSachHangHoa
    @TuKhoa NVARCHAR(100) = N'',
    @MaLoai VARCHAR(20) = ''
AS
BEGIN
    SELECT 
        h.MaHang,
        h.TenHang,
        h.MaLoai,
        l.TenLoai,
        h.DonViTinh,
        h.DonGiaNhap,
        h.SoLuongTon,
        h.MucTonToiThieu,
        h.HinhAnh,
        CASE 
            WHEN h.SoLuongTon <= h.MucTonToiThieu THEN N'Cảnh báo tồn thấp'
            ELSE N'Bình thường'
        END AS TrangThaiTon
    FROM HangHoa h
    INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
    WHERE (@TuKhoa = N'' OR h.MaHang LIKE '%' + @TuKhoa + '%' OR h.TenHang LIKE N'%' + @TuKhoa + '%')
      AND (@MaLoai = '' OR h.MaLoai = @MaLoai)
    ORDER BY h.MaHang ASC;
END;
GO

-- 2. SP thống kê Nhập - Xuất - Tồn theo từng mặt hàng phục vụ vẽ biểu đồ GDI+ (Thành viên 3)
CREATE PROCEDURE sp_ThongKeNhapXuatTon
    @TuNgay DATETIME,
    @DenNgay DATETIME
AS
BEGIN
    SELECT 
        h.MaHang,
        h.TenHang,
        ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Nhập' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongNhap,
        ISNULL(SUM(CASE WHEN p.LoaiPhieu = N'Xuất' AND p.NgayLap BETWEEN @TuNgay AND @DenNgay THEN ct.SoLuong ELSE 0 END), 0) AS TongXuat,
        h.SoLuongTon AS TonHienTai
    FROM HangHoa h
    LEFT JOIN ChiTietPhieu ct ON h.MaHang = ct.MaHang
    LEFT JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
    GROUP BY h.MaHang, h.TenHang, h.SoLuongTon;
END;
GO

-- 3. SP Tính tồn kho chi tiết, lọc theo loại, từ khóa, trạng thái tồn (Thành viên 3)
CREATE PROCEDURE sp_TinhTonKhoChiTiet
    @MaLoai VARCHAR(20) = '',
    @TuKhoa NVARCHAR(100) = N'',
    @TrangThaiTon NVARCHAR(30) = N'Tất cả' -- 'Tất cả', 'Cảnh báo tồn thấp', 'Hết hàng', 'An toàn'
AS
BEGIN
    SET NOCOUNT ON;

    WITH BangThongKe AS (
        SELECT 
            h.MaHang,
            h.TenHang,
            h.MaLoai,
            l.TenLoai,
            h.DonViTinh,
            h.DonGiaNhap,
            h.SoLuongTon,
            h.MucTonToiThieu,
            CAST(h.SoLuongTon * h.DonGiaNhap AS DECIMAL(18,2)) AS GiaTriTon,
            ISNULL((
                SELECT SUM(ct.SoLuong)
                FROM ChiTietPhieu ct
                INNER JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                WHERE ct.MaHang = h.MaHang AND p.LoaiPhieu = N'Nhập'
            ), 0) AS TongNhap,
            ISNULL((
                SELECT SUM(ct.SoLuong)
                FROM ChiTietPhieu ct
                INNER JOIN PhieuKho p ON ct.MaPhieu = p.MaPhieu
                WHERE ct.MaHang = h.MaHang AND p.LoaiPhieu = N'Xuất'
            ), 0) AS TongXuat,
            CASE 
                WHEN h.SoLuongTon = 0 THEN N'Hết hàng'
                WHEN h.SoLuongTon <= h.MucTonToiThieu THEN N'Cảnh báo tồn thấp'
                ELSE N'An toàn'
            END AS TrangThai
        FROM HangHoa h
        INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
        WHERE (@MaLoai = '' OR h.MaLoai = @MaLoai)
          AND (@TuKhoa = N'' OR h.MaHang LIKE '%' + @TuKhoa + '%' OR h.TenHang LIKE N'%' + @TuKhoa + '%')
    )
    SELECT * 
    FROM BangThongKe
    WHERE (@TrangThaiTon = N'Tất cả' OR TrangThai = @TrangThaiTon)
    ORDER BY 
        CASE TrangThai 
            WHEN N'Hết hàng' THEN 1 
            WHEN N'Cảnh báo tồn thấp' THEN 2 
            ELSE 3 
        END,
        MaHang ASC;
END;
GO

-- 4. SP Tra cứu lịch sử nhập xuất theo khoảng ngày và bộ lọc (Thành viên 3)
CREATE PROCEDURE sp_TraCuuLichSuNhapXuat
    @TuNgay DATETIME,
    @DenNgay DATETIME,
    @LoaiPhieu NVARCHAR(20) = N'Tất cả', -- 'Tất cả', 'Nhập', 'Xuất'
    @TuKhoa NVARCHAR(100) = N''
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @DenNgayChuan DATETIME = DATEADD(DAY, 1, CAST(CONVERT(VARCHAR(10), @DenNgay, 120) AS DATETIME));

    SELECT 
        p.MaPhieu,
        p.LoaiPhieu,
        p.NgayLap,
        p.MaNguoiDung,
        nd.HoTen AS NguoiLap,
        p.MaNCC,
        ISNULL(ncc.TenNCC, N'(Không có / Khách hàng)') AS TenNCC,
        p.GhiChu,
        p.TongTien,
        COUNT(ct.MaHang) AS SoMatHang,
        ISNULL(SUM(ct.SoLuong), 0) AS TongSoLuong
    FROM PhieuKho p
    INNER JOIN NguoiDung nd ON p.MaNguoiDung = nd.MaNguoiDung
    LEFT JOIN NhaCungCap ncc ON p.MaNCC = ncc.MaNCC
    LEFT JOIN ChiTietPhieu ct ON p.MaPhieu = ct.MaPhieu
    WHERE p.NgayLap >= @TuNgay 
      AND p.NgayLap < @DenNgayChuan
      AND (@LoaiPhieu = N'Tất cả' OR p.LoaiPhieu = @LoaiPhieu)
      AND (
          @TuKhoa = N'' 
          OR p.MaPhieu LIKE '%' + @TuKhoa + '%'
          OR nd.HoTen LIKE N'%' + @TuKhoa + '%'
          OR ISNULL(ncc.TenNCC, '') LIKE N'%' + @TuKhoa + '%'
          OR ISNULL(p.GhiChu, '') LIKE N'%' + @TuKhoa + '%'
      )
    GROUP BY 
        p.MaPhieu,
        p.LoaiPhieu,
        p.NgayLap,
        p.MaNguoiDung,
        nd.HoTen,
        p.MaNCC,
        ncc.TenNCC,
        p.GhiChu,
        p.TongTien
    ORDER BY p.NgayLap DESC;
END;
GO

-- 5. SP Lấy chi tiết phiếu kho để hiển thị và in bằng PrintDocument (Thành viên 3)
CREATE PROCEDURE sp_LayChiTietPhieuDeIn
    @MaPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        ct.MaPhieu,
        ct.MaHang,
        h.TenHang,
        l.TenLoai,
        h.DonViTinh,
        ct.SoLuong,
        ct.DonGia,
        ct.ThanhTien
    FROM ChiTietPhieu ct
    INNER JOIN HangHoa h ON ct.MaHang = h.MaHang
    INNER JOIN LoaiHang l ON h.MaLoai = l.MaLoai
    WHERE ct.MaPhieu = @MaPhieu
    ORDER BY ct.MaHang ASC;
END;
GO

-- 6. SP Lấy thông tin header phiếu kho phục vụ in ấn (Thành viên 3)
CREATE PROCEDURE sp_LayThongTinPhieuIn
    @MaPhieu VARCHAR(20)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT 
        p.MaPhieu,
        p.LoaiPhieu,
        p.NgayLap,
        p.MaNguoiDung,
        nd.HoTen AS NguoiLap,
        nd.VaiTro AS VaiTroNguoiLap,
        p.MaNCC,
        ISNULL(ncc.TenNCC, N'Đối tác nhận hàng / Khách xuất') AS TenDoiTac,
        ISNULL(ncc.DienThoai, N'-') AS DienThoaiDoiTac,
        ISNULL(ncc.DiaChi, N'-') AS DiaChiDoiTac,
        p.GhiChu,
        p.TongTien
    FROM PhieuKho p
    INNER JOIN NguoiDung nd ON p.MaNguoiDung = nd.MaNguoiDung
    LEFT JOIN NhaCungCap ncc ON p.MaNCC = ncc.MaNCC
    WHERE p.MaPhieu = @MaPhieu;
END;
GO

-- =========================================================================
-- DỮ LIỆU KHỞI TẠO VÀ DỮ LIỆU MẪU ĐẦY ĐỦ
-- =========================================================================

-- Tài khoản người dùng
INSERT INTO NguoiDung VALUES ('ND01', 'admin', '123456', N'Nguyễn Văn Quản Lý', N'Quản lý', 1);
INSERT INTO NguoiDung VALUES ('ND02', 'nvkho', '123456', N'Trần Thị Thủ Kho', N'Nhân viên kho', 1);

-- Danh mục loại hàng
INSERT INTO LoaiHang VALUES ('LH01', N'Thiết bị điện tử', N'Linh kiện máy tính, màn hình, ổ cứng');
INSERT INTO LoaiHang VALUES ('LH02', N'Phụ kiện văn phòng', N'Chuột, bàn phím, tai nghe');
INSERT INTO LoaiHang VALUES ('LH03', N'Thiết bị mạng', N'Router wifi, switch, cáp mạng');

-- Danh mục nhà cung cấp
INSERT INTO NhaCungCap VALUES ('NCC01', N'Công ty TNHH Công nghệ Phong Vũ', '0901234567', N'Quận 1, TP.HCM');
INSERT INTO NhaCungCap VALUES ('NCC02', N'Nhà phân phối Digiworld', '0909888777', N'Quận 3, TP.HCM');
INSERT INTO NhaCungCap VALUES ('NCC03', N'Công ty CP Dịch vụ Công nghệ FPT', '0912345678', N'Quận Cầu Giấy, Hà Nội');

-- Danh mục hàng hóa
INSERT INTO HangHoa VALUES ('HH01', N'Màn hình Dell UltraSharp 24 inch', 'LH01', N'Cái', 4500000, 25, 10, '');
INSERT INTO HangHoa VALUES ('HH02', N'Bàn phím cơ Logitech G Pro', 'LH02', N'Cái', 2100000, 5, 10, ''); -- Tồn thấp <= 10
INSERT INTO HangHoa VALUES ('HH03', N'Chuột không dây Razer DeathAdder', 'LH02', N'Cái', 850000, 40, 15, '');
INSERT INTO HangHoa VALUES ('HH04', N'Tai nghe Gaming Kingston HyperX Cloud II', 'LH02', N'Cái', 1850000, 0, 5, ''); -- Hết hàng = 0
INSERT INTO HangHoa VALUES ('HH05', N'Ổ cứng SSD Samsung 980 Pro 1TB NVMe', 'LH01', N'Cái', 2650000, 18, 8, '');

-- Dữ liệu Phiếu nhập và Phiếu xuất mẫu (Nhiều mốc thời gian để test lọc ngày, in phiếu và biểu đồ GDI+)
-- Phiếu 1: Nhập tháng 10
INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
VALUES ('PN20261001', N'Nhập', '2026-10-01 09:15:00', 'ND01', 'NCC01', N'Nhập lô màn hình Dell và chuột Razer', 146500000);

INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
VALUES ('PN20261001', 'HH01', 30, 4500000),
       ('PN20261001', 'HH03', 20, 850000);

-- Phiếu 2: Nhập tháng 10
INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
VALUES ('PN20261015', N'Nhập', '2026-10-15 14:30:00', 'ND02', 'NCC02', N'Nhập bàn phím cơ Logitech và SSD Samsung', 73900000);

INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
VALUES ('PN20261015', 'HH02', 10, 2100000),
       ('PN20261015', 'HH05', 20, 2645000);

-- Phiếu 3: Xuất tháng 10
INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
VALUES ('PX20261020', N'Xuất', '2026-10-20 10:00:00', 'ND02', NULL, N'Xuất giao chi nhánh đại lý Quận 5', 38200000);

INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
VALUES ('PX20261020', 'HH01', 5, 4800000),
       ('PX20261020', 'HH02', 5, 2300000),
       ('PX20261020', 'HH05', 1, 2700000);

-- Phiếu 4: Nhập tháng 11
INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
VALUES ('PN20261101', N'Nhập', '2026-11-01 08:45:00', 'ND01', 'NCC01', N'Nhập bổ sung chuột Razer tồn kho', 25500000);

INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
VALUES ('PN20261101', 'HH03', 30, 850000);

-- Phiếu 5: Xuất tháng 11
INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
VALUES ('PX20261102', N'Xuất', '2026-11-02 16:20:00', 'ND02', NULL, N'Xuất bán dự án phòng máy trường học', 54500000);

INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
VALUES ('PX20261102', 'HH03', 10, 950000),
       ('PX20261102', 'HH05', 1, 2700000),
       ('PX20261102', 'HH01', 9, 4700000);
GO
