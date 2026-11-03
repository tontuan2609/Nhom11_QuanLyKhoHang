USE QuanLyKhoHangDB;
GO

-- =========================================================================
-- ĐỒ ÁN CUỐI KỲ: LẬP TRÌNH TRÊN WINDOWS - QUẢN LÝ KHO HÀNG
-- PHẦN THỰC HIỆN CỦA THÀNH VIÊN:
-- 1. Tính tồn kho qua Stored Procedure
-- 2. Tra cứu lịch sử nhập xuất theo khoảng ngày
-- 3. Xuất file CSV (Báo cáo tồn kho & Lịch sử phiếu)
-- 4. Vẽ biểu đồ nhập xuất tồn bằng GDI+
-- 5. In phiếu kho bằng PrintDocument
-- =========================================================================

-- 1. STORED PROCEDURE: Tính tồn kho chi tiết, lọc danh mục, cảnh báo tồn
IF OBJECT_ID('sp_TinhTonKhoChiTiet', 'P') IS NOT NULL
    DROP PROCEDURE sp_TinhTonKhoChiTiet;
GO

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

-- 2. STORED PROCEDURE: Tra cứu lịch sử nhập xuất theo khoảng ngày và bộ lọc
IF OBJECT_ID('sp_TraCuuLichSuNhapXuat', 'P') IS NOT NULL
    DROP PROCEDURE sp_TraCuuLichSuNhapXuat;
GO

CREATE PROCEDURE sp_TraCuuLichSuNhapXuat
    @TuNgay DATETIME,
    @DenNgay DATETIME,
    @LoaiPhieu NVARCHAR(20) = N'Tất cả', -- 'Tất cả', 'Nhập', 'Xuất'
    @TuKhoa NVARCHAR(100) = N''
AS
BEGIN
    SET NOCOUNT ON;

    -- Đảm bảo mốc kết thúc tính hết ngày (23:59:59.997) nếu truyền ngày không có giờ
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

-- 3. STORED PROCEDURE: Lấy chi tiết phiếu kho để hiển thị và in bằng PrintDocument
IF OBJECT_ID('sp_LayChiTietPhieuDeIn', 'P') IS NOT NULL
    DROP PROCEDURE sp_LayChiTietPhieuDeIn;
GO

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

-- 4. STORED PROCEDURE: Lấy thông tin header phiếu kho phục vụ in ấn
IF OBJECT_ID('sp_LayThongTinPhieuIn', 'P') IS NOT NULL
    DROP PROCEDURE sp_LayThongTinPhieuIn;
GO

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
-- 5. CHÈN DỮ LIỆU MẪU BỔ SUNG PHỤC VỤ TEST ĐẦY ĐỦ 5 TÍNH NĂNG
-- (Các phiếu nhập, phiếu xuất nhiều mốc thời gian, có chi tiết mặt hàng)
-- =========================================================================

-- Bổ sung thêm mặt hàng test (nếu chưa có)
IF NOT EXISTS (SELECT 1 FROM HangHoa WHERE MaHang = 'HH04')
BEGIN
    INSERT INTO HangHoa VALUES ('HH04', N'Tai nghe Gaming Kingston HyperX Cloud II', 'LH02', N'Cái', 1850000, 0, 5, '');
END;

IF NOT EXISTS (SELECT 1 FROM HangHoa WHERE MaHang = 'HH05')
BEGIN
    INSERT INTO HangHoa VALUES ('HH05', N'Ổ cứng SSD Samsung 980 Pro 1TB NVMe', 'LH01', N'Cái', 2650000, 18, 8, '');
END;

-- Thêm phiếu kho mẫu nếu chưa có
IF NOT EXISTS (SELECT 1 FROM PhieuKho WHERE MaPhieu = 'PN20261001')
BEGIN
    -- Phiếu nhập 1: Đầu tháng 10
    INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
    VALUES ('PN20261001', N'Nhập', '2026-10-01 09:15:00', 'ND01', 'NCC01', N'Nhập lô màn hình Dell và chuột Razer', 146500000);

    INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
    VALUES ('PN20261001', 'HH01', 30, 4500000),
           ('PN20261001', 'HH03', 20, 850000);
END;

IF NOT EXISTS (SELECT 1 FROM PhieuKho WHERE MaPhieu = 'PN20261015')
BEGIN
    -- Phiếu nhập 2: Giữa tháng 10
    INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
    VALUES ('PN20261015', N'Nhập', '2026-10-15 14:30:00', 'ND02', 'NCC02', N'Nhập bàn phím cơ Logitech và SSD Samsung', 73900000);

    INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
    VALUES ('PN20261015', 'HH02', 10, 2100000),
           ('PN20261015', 'HH05', 20, 2645000);
END;

IF NOT EXISTS (SELECT 1 FROM PhieuKho WHERE MaPhieu = 'PX20261020')
BEGIN
    -- Phiếu xuất 1: Cuối tháng 10
    INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
    VALUES ('PX20261020', N'Xuất', '2026-10-20 10:00:00', 'ND02', NULL, N'Xuất giao chi nhánh đại lý Quận 5', 38200000);

    INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
    VALUES ('PX20261020', 'HH01', 5, 4800000),
           ('PX20261020', 'HH02', 5, 2300000),
           ('PX20261020', 'HH05', 1, 2700000);
END;

IF NOT EXISTS (SELECT 1 FROM PhieuKho WHERE MaPhieu = 'PN20261101')
BEGIN
    -- Phiếu nhập 3: Đầu tháng 11
    INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
    VALUES ('PN20261101', N'Nhập', '2026-11-01 08:45:00', 'ND01', 'NCC01', N'Nhập bổ sung chuột Razer tồn kho', 25500000);

    INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
    VALUES ('PN20261101', 'HH03', 30, 850000);
END;

IF NOT EXISTS (SELECT 1 FROM PhieuKho WHERE MaPhieu = 'PX20261102')
BEGIN
    -- Phiếu xuất 2: Tháng 11
    INSERT INTO PhieuKho (MaPhieu, LoaiPhieu, NgayLap, MaNguoiDung, MaNCC, GhiChu, TongTien)
    VALUES ('PX20261102', N'Xuất', '2026-11-02 16:20:00', 'ND02', NULL, N'Xuất bán dự án phòng máy trường học', 54500000);

    INSERT INTO ChiTietPhieu (MaPhieu, MaHang, SoLuong, DonGia)
    VALUES ('PX20261102', 'HH03', 10, 950000),
           ('PX20261102', 'HH05', 1, 2700000),
           ('PX20261102', 'HH01', 9, 4700000);
END;
GO
