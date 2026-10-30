INSERT INTO NguoiDung VALUES ('ND01', 'admin', '123456', N'Nguyễn Văn Quản Lý', N'Quản lý', 1);
INSERT INTO NguoiDung VALUES ('ND02', 'nvkho', '123456', N'Trần Thị Thủ Kho', N'Nhân viên kho', 1);

INSERT INTO LoaiHang VALUES ('LH01', N'Thiết bị điện tử', N'Linh kiện, máy tính, màn hình');
INSERT INTO LoaiHang VALUES ('LH02', N'Phụ kiện văn phòng', N'Chuột, bàn phím, cáp kết nối');

INSERT INTO NhaCungCap VALUES ('NCC01', N'Công ty TNHH Công nghệ Phong Vũ', '0901234567', N'Quận 1, TP.HCM');
INSERT INTO NhaCungCap VALUES ('NCC02', N'Nhà phân phối Digiworld', '0909888777', N'Quận 3, TP.HCM');

INSERT INTO HangHoa VALUES ('HH01', N'Màn hình Dell UltraSharp 24 inch', 'LH01', N'Cái', 4500000, 25, 10, '');
INSERT INTO HangHoa VALUES ('HH02', N'Bàn phím cơ Logitech G Pro', 'LH02', N'Cái', 2100000, 5, 10, ''); -- Dòng này sẽ kích hoạt cảnh báo đỏ do SoLuongTon (5) <= MucTonToiThieu (10)
INSERT INTO HangHoa VALUES ('HH03', N'Chuột không dây Razer DeathAdder', 'LH02', N'Cái', 850000, 40, 15, '');
GO