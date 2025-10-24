create database KAHYToon
use KAHYToon

-- Xóa Khóa Ngoại Bảng lichsudoc
IF OBJECT_ID('FK_LSD_NguoiDung', 'F') IS NOT NULL ALTER TABLE lichsudoc DROP CONSTRAINT FK_LSD_NguoiDung;
IF OBJECT_ID('FK_LSD_ThongTinTruyen', 'F') IS NOT NULL ALTER TABLE lichsudoc DROP CONSTRAINT FK_LSD_ThongTinTruyen;
IF OBJECT_ID('FK_LSD_Chuong', 'F') IS NOT NULL ALTER TABLE lichsudoc DROP CONSTRAINT FK_LSD_Chuong;

-- Xóa Khóa Ngoại Bảng baocao
IF OBJECT_ID('FK_BC_NguoiDung', 'F') IS NOT NULL ALTER TABLE baocao DROP CONSTRAINT FK_BC_NguoiDung;
IF OBJECT_ID('FK_BC_ThongTinTruyen', 'F') IS NOT NULL ALTER TABLE baocao DROP CONSTRAINT FK_BC_ThongTinTruyen;
IF OBJECT_ID('FK_BC_Chuong', 'F') IS NOT NULL ALTER TABLE baocao DROP CONSTRAINT FK_BC_Chuong;

-- Xóa Khóa Ngoại Bảng danhgia
IF OBJECT_ID('FK_DG_NguoiDung', 'F') IS NOT NULL ALTER TABLE danhgia DROP CONSTRAINT FK_DG_NguoiDung;
IF OBJECT_ID('FK_DG_ThongTinTruyen', 'F') IS NOT NULL ALTER TABLE danhgia DROP CONSTRAINT FK_DG_ThongTinTruyen;

-- Xóa Khóa Ngoại Bảng binhluan
IF OBJECT_ID('FK_BL_NguoiDung', 'F') IS NOT NULL ALTER TABLE binhluan DROP CONSTRAINT FK_BL_NguoiDung;
IF OBJECT_ID('FK_BL_ThongTinTruyen', 'F') IS NOT NULL ALTER TABLE binhluan DROP CONSTRAINT FK_BL_ThongTinTruyen;
IF OBJECT_ID('FK_BL_Chuong', 'F') IS NOT NULL ALTER TABLE binhluan DROP CONSTRAINT FK_BL_Chuong;

-- Xóa Khóa Ngoại Bảng chuong
IF OBJECT_ID('FK_Chuong_ThongTinTruyen', 'F') IS NOT NULL ALTER TABLE chuong DROP CONSTRAINT FK_Chuong_ThongTinTruyen;

-- Xóa Khóa Ngoại Bảng truyenyeuthich
IF OBJECT_ID('FK_YeuThich_NguoiDung', 'F') IS NOT NULL ALTER TABLE truyenyeuthich DROP CONSTRAINT FK_YeuThich_NguoiDung;
IF OBJECT_ID('FK_YeuThich_ThongTinTruyen', 'F') IS NOT NULL ALTER TABLE truyenyeuthich DROP CONSTRAINT FK_YeuThich_ThongTinTruyen;

-- Xóa Khóa Ngoại Bảng thongtintruyen
IF OBJECT_ID('FK_TTTruyen_TheLoai', 'F') IS NOT NULL ALTER TABLE thongtintruyen DROP CONSTRAINT FK_TTTruyen_TheLoai;
GO

-- Xóa các bảng
IF OBJECT_ID('truyenyeuthich', 'U') IS NOT NULL DROP TABLE truyenyeuthich;
IF OBJECT_ID('lichsudoc', 'U') IS NOT NULL DROP TABLE lichsudoc;
IF OBJECT_ID('danhgia', 'U') IS NOT NULL DROP TABLE danhgia;
IF OBJECT_ID('baocao', 'U') IS NOT NULL DROP TABLE baocao;
IF OBJECT_ID('binhluan', 'U') IS NOT NULL DROP TABLE binhluan;
IF OBJECT_ID('chuong', 'U') IS NOT NULL DROP TABLE chuong;
IF OBJECT_ID('thongtintruyen', 'U') IS NOT NULL DROP TABLE thongtintruyen;
IF OBJECT_ID('theloai', 'U') IS NOT NULL DROP TABLE theloai;
IF OBJECT_ID('nguoidung', 'U') IS NOT NULL DROP TABLE nguoidung;
GO
--------------------------------------------------------
-- Table structure for table `baocao`
--------------------------------------------------------

CREATE TABLE admin (
    MaAdmin NVARCHAR(50) NOT NULL,
    TenDangNhap NVARCHAR(255) NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL,
    HoTen NVARCHAR(100) DEFAULT NULL,
    Email NVARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(50) DEFAULT N'Admin', -- Có thể mở rộng sau: 'SuperAdmin', 'Moderator'
    NgayTao DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT PK_admin PRIMARY KEY (MaAdmin)
);
GO

-- Bảng: baocao
CREATE TABLE baocao (
    MaBaoCao NVARCHAR(50) NOT NULL,
    MaNguoiDung NVARCHAR(50) DEFAULT NULL, -- Người dùng gửi báo cáo
    MaAdminXuLy NVARCHAR(50) DEFAULT NULL, -- Admin xử lý báo cáo (MỚI)
    MaTruyen NVARCHAR(50) DEFAULT NULL,
    MaChuong NVARCHAR(50) DEFAULT NULL,
    NoiDungBaoCao NVARCHAR(MAX) DEFAULT NULL,
    NgayBaoCao DATETIME DEFAULT NULL,
    TrangThai NVARCHAR(50) DEFAULT NULL, -- ví dụ: 'Chưa xử lý', 'Đã xử lý'
    CONSTRAINT PK_baocao PRIMARY KEY (MaBaoCao)
);
GO

--
-- Dumping data for table `baocao`
--

SET DATEFORMAT ymd; -- Đảm bảo định dạng ngày tháng yyyy-mm-dd được hiểu đúng
INSERT INTO baocao (MaBaoCao, MaNguoiDung, MaTruyen, MaChuong, NoiDungBaoCao, NgayBaoCao, TrangThai) VALUES
(N'BC002', N'ND004', N'MT_1', N'C002', N'Hình ảnh không hiển thị', '2025-03-18 00:00:00', N'Đã xử lý'),
(N'BC005', N'ND005', N'MT_4', N'C002', N'Truyện có nội dung trùng lặp', '2025-03-19 00:00:00', N'Đã xử lý'),
(N'BC006', N'ND004', N'MT_5', N'C003', N'Liên kết chương bị lỗi', '2025-03-20 00:00:00', N'Đã xử lý'),
(N'BC008', N'ND008', N'MT_8', N'C008', N'Lỗi chính tả', '2025-03-25 00:00:00', N'Đã xử lý'),
(N'BC010', N'ND010', N'MT_10', N'C010', N'Truyện bị mất hình ảnh', '2025-03-27 00:00:00', N'Đã xử lý'),
(N'BC011', N'ND004', N'MT_16', N'C029', N'Load lâu quá ad ơi', '2025-05-09 19:21:47', N'Chưa xử lý');

--------------------------------------------------------
-- Table structure for table `binhluan`
--------------------------------------------------------

-- Bảng: binhluan
CREATE TABLE binhluan (
    MaBinhLuan NVARCHAR(50) NOT NULL, 
    MaNguoiDung NVARCHAR(50) DEFAULT NULL,
    MaTruyen NVARCHAR(50) DEFAULT NULL, 
    MaChuong NVARCHAR(50) DEFAULT NULL,
    NoiDung NVARCHAR(MAX) DEFAULT NULL, 
    NgayDang DATETIME DEFAULT NULL,
    SoChuong INT DEFAULT NULL,
    CONSTRAINT PK_binhluan PRIMARY KEY (MaBinhLuan)
);
GO

--
-- Dumping data for table `binhluan`
--

INSERT INTO binhluan (MaBinhLuan, MaTruyen, MaChuong, NoiDung, NgayDang, MaNguoiDung, SoChuong) VALUES
(N'BL001', N'MT_1', N'C001', N'Truyện rất hay!', '2025-03-18 00:00:00', N'ND004', NULL),
(N'BL002', N'MT_1', N'C002', N'Tôi thích cách viết của tác giả.', '2025-03-18 00:00:00', N'ND004', NULL),
(N'BL003', N'MT_1', N'C003', N'Tác giả cần cải thiện mạch truyện.', '2025-03-19 00:00:00', N'ND003', NULL),
(N'BL004', N'MT_2', N'C004', N'Nội dung hấp dẫn, mong chờ chương tiếp theo.', '2025-03-19 00:00:00', N'ND004', NULL),
(N'BL005', N'MT_2', N'C005', N'Cảnh chiến đấu rất chân thực!', '2025-03-19 00:00:00', N'ND005', NULL),
(N'BL006', N'MT_2', N'C006', N'Tôi thích nhân vật chính!', '2025-03-20 00:00:00', N'ND004', NULL),
(N'BL007', N'MT_2', N'C007', N'Mong chờ phần tiếp theo.', '2025-03-20 00:00:00', N'ND007', NULL),
(N'BL008', N'MT_8', N'C008', N'Rất hấp dẫn!', '2025-03-25 00:00:00', N'ND008', NULL),
(N'BL009', N'MT_9', N'C009', N'Tôi thích nhân vật chính.', '2025-03-26 00:00:00', N'ND009', NULL),
(N'BL010', N'MT_10', N'C010', N'Nội dung cuốn hút.', '2025-03-27 00:00:00', N'ND010', NULL),
(N'BL011', N'MT_11', N'C011', N'Phong cách viết rất hay!', '2025-03-28 00:00:00', N'ND004', NULL),
(N'BL012', N'MT_12', N'C012', N'Mong chờ chương mới.', '2025-03-29 00:00:00', N'ND004', NULL)
--------------------------------------------------------
-- Table structure for table `chuong`
--------------------------------------------------------
go
-- Bảng: chuong
CREATE TABLE chuong (
    MaChuong NVARCHAR(50) NOT NULL, 
    MaTruyen NVARCHAR(50) DEFAULT NULL,
    SoChuong INT DEFAULT NULL, 
    TenChuong NVARCHAR(255) DEFAULT NULL,
    AnhChuong NVARCHAR(MAX) DEFAULT NULL, 
    NgayDang DATETIME DEFAULT NULL,
    CONSTRAINT PK_chuong PRIMARY KEY (MaChuong)
);
GO

--
-- Dumping data for table `chuong`
--

INSERT INTO chuong (MaChuong, MaTruyen, SoChuong, TenChuong, AnhChuong, NgayDang) VALUES
(N'C001', N'MT_1', 1, N'Khởi đầu tu tiên ', N'https://drive.google.com/uc?export=download&id=138-bvhuD-HXHTEZYGLXqad0M3hwYJVq0;https://drive.google.com/uc?export=download&id=1inRYSpF8e5IRuk-rP8RkCYKzX0FR5Exg', '2025-05-07 00:00:00'),
(N'C002', N'MT_1', 2, N'Bước chân vào tiên lộ', NULL, '2025-03-18 00:00:00'),
(N'C003', N'MT_1', 3, N'Thử thách đầu tiên', NULL, '2025-03-19 00:00:00'),
(N'C004', N'MT_2', 1, N'Rắn già hóa rồng', NULL, '2025-03-19 00:00:00'),
(N'C005', N'MT_2', 2, N'Bí ẩn của sơn động', NULL, '2025-03-19 00:00:00'),
(N'C006', N'MT_2', 3, N'Luyện đan sơ cấp', NULL, '2025-03-20 00:00:00'),
(N'C007', N'MT_2', 4, N'Gặp gỡ kỳ nhân', NULL, '2025-03-20 00:00:00'),
(N'C008', N'MT_8', 1, N'Sự Khởi Đầu', NULL, '2025-03-25 00:00:00'),
(N'C009', N'MT_9', 1, N'Bí Mật Gia Tộc', N'', '2025-05-08 00:00:00'),
(N'C010', N'MT_10', 1, N'Trận Chiến Đầu Tiên', N'', '2025-05-08 00:00:00'),
(N'C011', N'MT_11', 1, N'Hành Trình Bắt Đầu', NULL, '2025-03-28 00:00:00'),
(N'C012', N'MT_12', 1, N'Mở Màn', NULL, '2025-03-29 00:00:00'),
(N'C013', N'MT_1', 4, N'Khó khăn đột phá', N'', '2025-05-06 00:00:00'),
(N'C014', N'MT_13', 1, N' ', N'https://drive.google.com/uc?export=download&id=138-bvhuD-HXHTEZYGLXqad0M3hwYJVq0;https://drive.google.com/uc?export=download&id=1inRYSpF8e5IRuk-rP8RkCYKzX0FR5Exg', '2025-05-08 00:00:00'),
(N'C015', N'MT_13', 2, N' ', N'https://drive.google.com/uc?export=download&id=1_T9ePFQ5LKojmvTyPW3ezsPHesMEymOG', '2025-05-08 00:00:00'),
(N'C016', N'MT_13', 3, N' ', N'https://drive.google.com/uc?export=download&id=1ooxMJZTCimPb4dq1MOodQjQHdN4JoSck', '2025-05-08 00:00:00'),
(N'C017', N'MT_13', 4, N' ', N'https://drive.google.com/uc?export=download&id=1vcDbg6ba7rSxXKFOHg6N5q18C3SFLl3A', '2025-05-08 00:00:00'),
(N'C018', N'MT_13', 5, N' ', N'https://drive.google.com/uc?export=download&id=1N91h9MWvm9OF8JuxappQqeiVBilDZ53Z', '2025-05-08 00:00:00'),
(N'C019', N'MT_14', 1, N' ', N'https://drive.google.com/uc?export=download&id=1-AUIcXgti_ECuIUQUWNrzP5_zxa0WRPY;https://drive.google.com/uc?export=download&id=1vGnNzFl9YDlOMOzMsddxvS50MWBQb8Wd;https://drive.google.com/uc?export=download&id=1_jf1hQ3_wHzR3oWlXgllhpfsJLPcb8rx;https://drive.google.com/uc?export=download&id=1ArxDY4aRCXbOyO2yeulEM14VAojPuH4T;https://drive.google.com/uc?export=download&id=1fdneCHYOzuhNm1QAEDqi1Id-gpYVjs1z;https://drive.google.com/uc?export=download&id=1Vm4Yl_PUrUcQ02K9R1lBcHZUXvFUThLf;https://drive.', '2025-05-08 00:00:00'),
(N'C020', N'MT_14', 2, N' ', N'https://drive.google.com/uc?export=download&id=1L_77ZgXL6t0EaQrgmptU1wQ75WRC_Xti;https://drive.google.com/uc?export=download&id=17Cnw1THLwwRzKLfixqfzmsf2-_racDCP;https://drive.google.com/uc?export=download&id=1VCJTQAuMMVMV93XL_CgatHaTzUCCepBC;https://drive.google.com/uc?export=download&id=1olXGuJt8_uAqyFTr0_rmpwbYoZGghcWi;https://drive.google.com/uc?export=download&id=19JuPDgjRY9VmWGowbbTpKknpVLfK-zg8;https://drive.google.com/uc?export=download&id=1DutZWYD_tu-jeXKzWWAR3pIwA3hGTjJp', '2025-05-08 00:00:00'),
(N'C021', N'MT_14', 3, N' ', N'https://drive.google.com/uc?export=download&id=1T-1wX_zwGgIWL7Kq8VlKxYjcnmE9gcNA;https://drive.google.com/uc?export=download&id=1aqao4uD9qRMyMXddNNo9_0MdtECfnQoe;https://drive.google.com/uc?export=download&id=1thfoJ4w9Gmz3Hz1urse3A3DcznayzsEo;https://drive.google.com/uc?export=download&id=1VGaKMX48Dl03iDS_x4sOeRy7We6mrNm1;https://drive.google.com/uc?export=download&id=1UuSsASxNSrh6z4zqxEKMmhBbQ59EkNZq', '2025-05-08 00:00:00'),
(N'C022', N'MT_14', 4, N' ', N'https://drive.google.com/uc?export=download&id=1L1Ps_pBDwFIusPWp5O8ONq1wDxgmDL4S;https://drive.google.com/uc?export=download&id=1lhCJQM9GUHnbeJ0B5E0cGK78HA4mYAjZ;https://drive.google.com/uc?export=download&id=1ASfhmG3M6szlNibvyb5I-VxUbh9l5KFr;https://drive.google.com/uc?export=download&id=18yAPDQ2AaNBMWkcvZ2-MNSLPH74WVUFc;https://drive.google.com/uc?export=download&id=161iC-nsVIk7ZuD8Z6JlHiwL9f6z8OYz8', '2025-05-08 00:00:00'),
(N'C023', N'MT_14', 5, N' ', N'https://drive.google.com/uc?export=download&id=1w1A0u72fIB0rNl2qT6PWp80njJzLkSoA;https://drive.google.com/uc?export=download&id=1IOKoqUSlgVoipl-BUo6JaqYwAoXNukML;https://drive.google.com/uc?export=download&id=1x0OIhPKKD4uwuCO1vmR_fxFtLnrZvQSO;https://drive.google.com/uc?export=download&id=1-Ko8F-Qm9dUEjVfmLJJXZ4KXvlsMcgos;https://drive.google.com/uc?export=download&id=1rd6f1_95Is9PE826s51oIjklEL8G1mb9;https://drive.google.com/uc?export=download&id=1AFTjvFJ5Ls8_Dctm-sz9ZVLJZJr8-Pta;https://drive.', '2025-05-08 00:00:00'),
(N'C024', N'MT_15', 1, N' ', N'https://drive.google.com/uc?export=download&id=1M1M5YQMuhCpjzJ6oJJfoC18VyzjSUadu;https://drive.google.com/uc?export=download&id=18zJc0oDjuoBhMSaj7aI0xCCrsXQVRTKB;https://drive.google.com/uc?export=download&id=1kD01jvDe5KQcUky86dxspjHwhnNJ4LfJ', '2025-05-08 00:00:00'),
(N'C025', N'MT_15', 2, N' ', N'https://drive.google.com/uc?export=download&id=19fi-8Qs0Bc84qYrd4XG65l5MYoM-qHeJ;https://drive.google.com/uc?export=download&id=1t6kTuf3L9waFjeoLzES9fWGVw0qNjLWk;https://drive.google.com/uc?export=download&id=1rEhLBPXUZKOAYUL92MWdZkQS7J_qFnPk', '2025-05-08 00:00:00'),
(N'C026', N'MT_15', 3, N' ', N'https://drive.google.com/uc?export=download&id=1xshmQm4tBfA9KorTBIJxrppmJsO1T2eg;https://drive.google.com/uc?export=download&id=1OEK7o0M-28NpZrxRtNZphESYFsPxzTgL;https://drive.google.com/uc?export=download&id=1GMTGFCMKPWm5qv7m4WLazuUl6fvObdST', '2025-05-08 00:00:00'),
(N'C027', N'MT_15', 4, N' ', N'https://drive.google.com/uc?export=download&id=14PV_PG55dWtfgrhoyDz1i7XkWnLtPmTL;https://drive.google.com/uc?export=download&id=1u5RnDsFqftaQvd6iXZxeHIWk-9KARoN6;https://drive.google.com/uc?export=download&id=1dD_CbC7_YLbUrQ8D0txY4jqjZfzgxp19', '2025-05-08 00:00:00'),
(N'C028', N'MT_15', 5, N' ', N'https://drive.google.com/uc?export=download&id=1Q4DLAk9t7zkXKe2Jg6IgFQiYqRC4CjjY;https://drive.google.com/uc?export=download&id=1D1TnrHeQ9KMWiXhdthrLpgc9fSRcnM4g;https://drive.google.com/uc?export=download&id=1D1TnrHeQ9KMWiXhdthrLpgc9fSRcnM4g', '2025-05-08 00:00:00'),
(N'C029', N'MT_16', 1, N' ', N'https://drive.google.com/uc?export=download&id=1MpXfA2zaX1kl4r0lszVRxJyOgWTTptSk;https://drive.google.com/uc?export=download&id=1HM3Xn6uLM_k-CYfofD-DY_K9AtyKyzgI;https://drive.google.com/uc?export=download&id=1Qv81fB0wAwKEW0Vbn73m0NRE7tQuoBlf;https://drive.google.com/uc?export=download&id=1IOAabUhqTW0fXjKrxgqKTb6_lFJxdUtQ;https://drive.google.com/uc?export=download&id=1bncInhQ5AbEpL5qv2g58yJVEVsHy_TE-;https://drive.google.com/uc?export=download&id=12j9GWxFg4y2TBc4gGRblfKbaCkgldYUR', '2025-05-08 00:00:00'),
(N'C030', N'MT_16', 2, N' ', N'https://drive.google.com/uc?export=download&id=1IX-7SFx1FoNioKxveJzJ2LA90jb6xpgh;https://drive.google.com/uc?export=download&id=1IX-7SFx1FoNioKxveJzJ2LA90jb6xpgh;https://drive.google.com/uc?export=download&id=1xk88P_oYWThSOoBzYxC3eYOEhFSap2w8;https://drive.google.com/uc?export=download&id=1u1tg1fQ4eHGz0greMb0aDYb9MwSW4iyE;https://drive.google.com/uc?export=download&id=1UO3GPugDC6XV-in59RfRC_9yuW2d1Aop;https://drive.google.com/uc?export=download&id=1SGKAJh3r6iQpFm825gbhOrN6CExDOPxu', '2025-05-08 00:00:00'),
(N'C031', N'MT_16', 3, N' ', N'https://drive.google.com/uc?export=download&id=1wx2J3Nk-oUXpv3Ggc32lhDnq0MFJ0EjU;https://drive.google.com/uc?export=download&id=1rW0Ue3Iw-rzX4Cdy3xAz_Z2PF7DGw56R;https://drive.google.com/uc?export=download&id=1SfSTDpAX3sTVqGQKk3Psc9Un3_cxHTkt;https://drive.google.com/uc?export=download&id=1knnz8nnuugsK-3ANE4wuZYWr1dl6qQnP', '2025-05-08 00:00:00'),
(N'C032', N'MT_16', 4, N' ', N'https://drive.google.com/uc?export=download&id=1zFS1Gkj9EwaGGIaJQtFFS9vDKCh54LHW;https://drive.google.com/uc?export=download&id=1PzCR_Yk6LcL9SPyP8jUgEgrJl6RnMIGP;https://drive.google.com/uc?export=download&id=1a2ikFd6nDFvY69l3iSLjt4cyP_CTUxLv;https://drive.google.com/uc?export=download&id=1V2EcMimN4I9OMxw5bt5cc6G62XePEc3D', '2025-05-08 00:00:00'),
(N'C033', N'MT_16', 5, N' ', N'https://drive.google.com/uc?export=download&id=1DlMHt2yMiP62mo01XDx1ztPkUSF6HG7V;https://drive.google.com/uc?export=download&id=1JoyTfojqVLru3M4aQaWZeOfh_2xtQ0TU;https://drive.google.com/uc?export=download&id=1DTU5H5R4M7GbPl_Kt-fK2BNQIXkDQF_8;https://drive.google.com/uc?export=download&id=1WxqKRtJTOl1Agk_sAjpCblYWOOq0J-qX', '2025-05-08 00:00:00');

--------------------------------------------------------
-- Table structure for table `danhgia`
--------------------------------------------------------

-- Bảng: danhgia 
CREATE TABLE danhgia (
    MaNguoiDung NVARCHAR(50) NOT NULL, 
    MaTruyen NVARCHAR(50) NOT NULL,
    SoSao INT DEFAULT NULL, 
    BinhLuan NVARCHAR(MAX) DEFAULT NULL,
    NgayBinhLuan DATETIME DEFAULT NULL,
    CONSTRAINT PK_danhgia PRIMARY KEY (MaNguoiDung, MaTruyen)
);
GO
--
-- Dumping data for table `danhgia`
--

INSERT INTO danhgia (MaNguoiDung, MaTruyen, SoSao, BinhLuan, NgayBinhLuan) VALUES
(N'ND003', N'MT_2', 3, N'Cốt truyện hơi chậm.', '2025-03-19 00:00:00'),
(N'ND004', N'MT_2', 5, N'Tôi thích phong cách viết của tác giả.', '2025-03-19 00:00:00'),
(N'ND005', N'MT_1', 4, N'Nhân vật chính rất thú vị.', '2025-03-19 00:00:00'),
(N'ND007', N'MT_1', 4, N'Mạch truyện ổn, nhưng cần thêm plot twist!', '2025-03-20 00:00:00'),
(N'ND008', N'MT_8', 5, N'Truyện rất hấp dẫn!', '2025-03-25 00:00:00'),
(N'ND009', N'MT_9', 4, N'Nội dung khá ổn.', '2025-03-26 00:00:00'),
(N'ND010', N'MT_10', 5, N'Tình tiết lôi cuốn!', '2025-03-27 00:00:00')
--------------------------------------------------------
-- Table structure for table `lichsudoc`
--------------------------------------------------------

CREATE TABLE lichsudoc (
    MaLichSuDoc NVARCHAR(50) NOT NULL, 
    MaNguoiDung NVARCHAR(50) NOT NULL,
    MaTruyen NVARCHAR(50) DEFAULT NULL, 
    MaChuong NVARCHAR(50) DEFAULT NULL,
    ThoiGianDoc DATETIME DEFAULT NULL, 
    NgayXem DATETIME DEFAULT NULL,
    CONSTRAINT PK_lichsudoc PRIMARY KEY (MaLichSuDoc)
);
GO

--
-- Dumping data for table `lichsudoc`
--

INSERT INTO lichsudoc (MaLichSuDoc, MaNguoiDung, MaTruyen, MaChuong, ThoiGianDoc, NgayXem) VALUES
(N'LH001', N'ND004', N'MT_1', N'C001', '2025-03-18 10:00:00', NULL),
(N'LH002', N'ND004', N'MT_1', N'C002', '2025-03-18 11:30:00', NULL),
(N'LH003', N'ND003', N'MT_1', N'C003', '2025-03-19 08:45:00', NULL),
(N'LH004', N'ND004', N'MT_2', N'C004', '2025-03-19 09:30:00', NULL),
(N'LH005', N'ND005', N'MT_2', N'C005', '2025-03-19 10:15:00', NULL),
(N'LH006', N'ND004', N'MT_2', N'C006', '2025-03-20 07:45:00', NULL),
(N'LH007', N'ND007', N'MT_2', N'C007', '2025-03-20 08:20:00', NULL),
(N'LH008', N'ND008', N'MT_8', N'C008', '2025-03-25 14:00:00', NULL),
(N'LH009', N'ND009', N'MT_9', N'C009', '2025-03-26 15:30:00', NULL),
(N'LH010', N'ND010', N'MT_10', N'C010', '2025-03-27 16:45:00', NULL),
(N'LH011', N'ND010', N'MT_11', N'C011', '2025-03-28 17:20:00', NULL),
(N'LH012', N'ND010', N'MT_12', N'C012', '2025-03-29 18:10:00', NULL),
(N'LH013', N'ND004', N'MT_1', N'C001', '2025-05-08 09:43:05', NULL),
(N'LH014', N'ND004', N'MT_9', N'C009', '2025-05-08 10:12:05', NULL),
(N'LH015', N'ND004', N'MT_10', N'C010', '2025-05-08 11:16:35', NULL),
(N'LH016', N'ND004', N'MT_1', N'C001', '2025-05-08 11:16:46', NULL),
(N'LH017', N'ND004', N'MT_13', N'C014', '2025-05-08 20:20:12', NULL),
(N'LH018', N'ND004', N'MT_13', N'C014', '2025-05-08 20:22:34', NULL),
(N'LH019', N'ND004', N'MT_13', N'C015', '2025-05-08 20:22:35', NULL),
(N'LH020', N'ND004', N'MT_13', N'C016', '2025-05-08 20:23:10', NULL),
(N'LH021', N'ND004', N'MT_15', N'C024', '2025-05-08 20:55:01', NULL),
(N'LH022', N'ND004', N'MT_13', N'C018', '2025-05-08 20:56:36', NULL),
(N'LH023', N'ND004', N'MT_13', N'C014', '2025-05-08 22:57:42', NULL),
(N'LH024', N'ND004', N'MT_13', N'C014', '2025-05-08 22:58:02', NULL),
(N'LH025', N'ND004', N'MT_14', N'C019', '2025-05-08 22:58:08', NULL),
(N'LH026', N'ND004', N'MT_15', N'C024', '2025-05-08 22:58:27', NULL),
(N'LH027', N'ND004', N'MT_15', N'C024', '2025-05-08 22:59:21', NULL),
(N'LH028', N'ND004', N'MT_15', N'C028', '2025-05-08 22:59:29', NULL),
(N'LH029', N'ND004', N'MT_1', N'C001', '2025-05-08 22:59:42', NULL),
(N'LH030', N'ND004', N'MT_1', N'C002', '2025-05-08 23:00:05', NULL),
(N'LH031', N'ND004', N'MT_15', N'C024', '2025-05-08 23:00:08', NULL),
(N'LH032', N'ND004', N'MT_15', N'C025', '2025-05-08 23:00:13', NULL),
(N'LH033', N'ND004', N'MT_14', N'C019', '2025-05-08 23:00:35', NULL),
(N'LH034', N'ND004', N'MT_9', N'C009', '2025-05-08 23:02:22', NULL),
(N'LH035', N'ND004', N'MT_13', N'C014', '2025-05-08 23:05:12', NULL),
(N'LH036', N'ND004', N'MT_16', N'C029', '2025-05-09 10:49:12', NULL),
(N'LH037', N'ND004', N'MT_16', N'C029', '2025-05-09 10:52:56', NULL),
(N'LH038', N'ND004', N'MT_16', N'C033', '2025-05-09 11:07:08', NULL),
(N'LH039', N'ND004', N'MT_16', N'C029', '2025-05-09 17:20:27', NULL),
(N'LH040', N'ND004', N'MT_16', N'C029', '2025-05-09 18:27:58', NULL),
(N'LH041', N'ND004', N'MT_16', N'C030', '2025-05-09 18:36:21', NULL),
(N'LH042', N'ND004', N'MT_1', N'C003', '2025-05-09 18:36:43', NULL),
(N'LH043', N'ND004', N'MT_16', N'C029', '2025-05-09 18:41:46', NULL),
(N'LH044', N'ND004', N'MT_16', N'C029', '2025-05-09 19:09:21', NULL),
(N'LH045', N'ND004', N'MT_16', N'C029', '2025-05-09 19:10:18', NULL),
(N'LH046', N'ND004', N'MT_16', N'C029', '2025-05-09 19:11:46', NULL),
(N'LH047', N'ND004', N'MT_16', N'C029', '2025-05-09 19:11:47', NULL),
(N'LH048', N'ND004', N'MT_16', N'C029', '2025-05-09 19:13:14', NULL),
(N'LH049', N'ND004', N'MT_16', N'C029', '2025-05-09 19:13:15', NULL),
(N'LH050', N'ND004', N'MT_16', N'C029', '2025-05-09 19:13:36', NULL),
(N'LH051', N'ND004', N'MT_16', N'C029', '2025-05-09 19:13:37', NULL),
(N'LH052', N'ND004', N'MT_16', N'C029', '2025-05-09 19:13:38', NULL),
(N'LH053', N'ND004', N'MT_16', N'C029', '2025-05-09 19:13:41', NULL),
(N'LH054', N'ND004', N'MT_16', N'C029', '2025-05-09 19:14:03', NULL),
(N'LH055', N'ND004', N'MT_16', N'C029', '2025-05-09 19:14:04', NULL),
(N'LH056', N'ND004', N'MT_16', N'C029', '2025-05-09 19:14:04', NULL),
(N'LH057', N'ND004', N'MT_16', N'C029', '2025-05-09 19:14:41', NULL),
(N'LH058', N'ND004', N'MT_16', N'C029', '2025-05-09 19:14:42', NULL),
(N'LH059', N'ND004', N'MT_16', N'C029', '2025-05-09 19:14:45', NULL),
(N'LH060', N'ND004', N'MT_16', N'C029', '2025-05-09 19:15:28', NULL),
(N'LH061', N'ND004', N'MT_16', N'C029', '2025-05-09 19:16:13', NULL),
(N'LH062', N'ND004', N'MT_16', N'C029', '2025-05-09 19:16:14', NULL),
(N'LH063', N'ND004', N'MT_16', N'C029', '2025-05-09 19:16:44', NULL),
(N'LH064', N'ND004', N'MT_16', N'C029', '2025-05-09 19:17:19', NULL),
(N'LH065', N'ND004', N'MT_16', N'C029', '2025-05-09 20:44', NULL),
(N'LH070', N'ND004', N'MT_16', N'C029', '2025-05-09 21:28', NULL),
(N'LH071', N'ND004', N'MT_16', N'C029', '2025-05-09 21:46:59', NULL),
(N'LH072', N'ND004', N'MT_16', N'C029', '2025-05-09 22:46:33', NULL),
(N'LH073', N'ND004', N'MT_15', N'C026', '2025-05-15 15:29:52', NULL),
(N'LH074', N'ND004', N'MT_9', N'C009', '2025-05-15 15:32:29', NULL),
(N'LH075', N'ND004', N'MT_10', N'C010', '2025-05-15 15:32:33', NULL),
(N'LH076', N'ND004', N'MT_11', N'C011', '2025-05-15 15:32:37', NULL),
(N'LH077', N'ND004', N'MT_16', N'C029', '2025-05-09 19:18:08', NULL),
(N'LH066', N'ND004', N'MT_16', N'C029', '2025-05-09 19:18:48', NULL),
(N'LH067', N'ND004', N'MT_16', N'C029', '2025-05-09 19:19:23', NULL),
(N'LH068', N'ND004', N'MT_16', N'C029', '2025-05-09 19:19:39', NULL),
(N'LH069', N'ND004', N'MT_8', N'C008', '2025-05-15 15:32:43', NULL);

--------------------------------------------------------
-- Table structure for table `nguoidung`
--------------------------------------------------------

CREATE TABLE nguoidung (
    MaNguoiDung NVARCHAR(50) NOT NULL, 
    TenDangNhap NVARCHAR(255) NOT NULL,
    MatKhau NVARCHAR(255) NOT NULL, 
    Email NVARCHAR(255) NOT NULL,
    NgayDangKy DATETIME DEFAULT GETDATE(), 
    VaiTro NVARCHAR(50) DEFAULT N'user', 
    ThoiGianDangNhap DATETIME DEFAULT NULL, 
    IP_DangNhap VARCHAR(45) DEFAULT NULL,
    Avatar NVARCHAR(255) DEFAULT N'../img/default-avatar.png',
    HoTen NVARCHAR(100) DEFAULT NULL, 
    SoDienThoai VARCHAR(20) DEFAULT NULL,
    CONSTRAINT PK_nguoidung PRIMARY KEY (MaNguoiDung)
);
GO

--
-- Dumping data for table `nguoidung`
--

INSERT INTO nguoidung (MaNguoiDung, TenDangNhap, MatKhau, Email, NgayDangKy, VaiTro, ThoiGianDangNhap, IP_DangNhap, Avatar, HoTen, SoDienThoai) VALUES
(N'ND003', N'thanhan', N'$2y$10$zl4xBeCigoFOQOnF2veYZejmlwn9yo1mpi5fZnGI/5TluxcrzQo3W', N'admin@example.com', '2025-03-01 00:00:00', N'admin', '2025-03-01 14:45:00', '192.168.1.3', N'../img/default-avatar.png', NULL, NULL),
(N'ND004', N'hoaimong', N'$2y$10$03v0dySKj0o8b7X4KB8UEOxIp38TogHaAm.spCnznJSLhiD5yNq76', N'mong666@gmail.com', '2025-03-12 00:00:00', N'admin', '2025-04-01 12:55:01', '::1', N'../img/default-avatar.png', NULL, NULL),
(N'ND005', N'chicuong', N'$2y$10$ALdJ67aHksGaheVS46TqOODHxChzAZjjEuKbV3Q3j3vTjPSSX074u', N'user4@example.com', '2025-03-13 00:00:00', N'user', '2025-03-13 13:30:00', '192.168.1.5', N'../img/default-avatar.png', NULL, NULL),
(N'ND007', N'vanquynh', N'$2y$10$MEQaJQkSB5gAFUHicah96esctpL1ng0cYyb.QW8UumTDm13jt/z2q', N'user6@example.com', '2025-03-15 00:00:00', N'user', '2025-03-15 15:15:00', '192.168.1.7', N'../img/default-avatar.png', NULL, NULL),
(N'ND008', N'hieu123', N'pass111', N'hieu@example.com', '2025-03-16 00:00:00', N'user', '2025-03-16 16:10:00', '192.168.1.8', N'../img/default-avatar.png', NULL, NULL),
(N'ND009', N'lam789', N'pass222', N'lam@example.com', '2025-03-17 00:00:00', N'user', '2025-03-17 17:05:00', '192.168.1.9', N'../img/default-avatar.png', NULL, NULL),
(N'ND010', N'anhtu', N'pass333', N'anhtu@example.com', '2025-03-18 00:00:00', N'user', '2025-03-18 18:00:00', '192.168.1.10', N'../img/default-avatar.png', NULL, NULL),
(N'ND013', N'oscarsilver', N'$2y$10$xqookKcspAoyVQtWr.Jovudesf4ZzhqergkuiDFMqvD8GzSnx5.EW', N'ss@gmail.com', '2025-03-30 16:39:15', N'admin', '2025-04-01 19:18:12', '::1', N'../uploads/avatars/avatar_ND013_1743614043.png', NULL, NULL);

--------------------------------------------------------
-- Table structure for table `theloai`
--------------------------------------------------------

-- Bảng: theloai
CREATE TABLE theloai (
    MaTheLoai NVARCHAR(50) NOT NULL, 
    TenTheLoai NVARCHAR(200) DEFAULT NULL,
    CONSTRAINT PK_theloai PRIMARY KEY (MaTheLoai)
);
GO

--
-- Dumping data for table `theloai`
--

INSERT INTO theloai (MaTheLoai, TenTheLoai) VALUES
(N'TL001', N'Tiên hiệp'),
(N'TL002', N'Huyền huyễn'),
(N'TL003', N'Khoa huyễn'),
(N'TL004', N'Kiếm hiệp'),
(N'TL005', N'Truyện ma'),
(N'TL006', N'Đô thị'),
(N'TL007', N'Hài hước'),
(N'TL008', N'Hành động'),
(N'TL009', N'Phiêu lưu'),
(N'TL010', N'Viễn tưởng'),
(N'TL011', N'Kinh dị'),
(N'TL012', N'Tình cảm');

--------------------------------------------------------
-- Table structure for table `thongtintruyen`
--------------------------------------------------------
-- Bảng: thongtintruyen
CREATE TABLE thongtintruyen (
    MaTruyen NVARCHAR(50) NOT NULL, 
    MaTheLoai NVARCHAR(50) DEFAULT NULL,
    TenTruyen NVARCHAR(255) DEFAULT NULL, 
    TacGia NVARCHAR(255) DEFAULT NULL,
    NgayDang DATETIME DEFAULT GETDATE(), 
    TrangThai NVARCHAR(40) NOT NULL,
    AnhTruyen NVARCHAR(500) DEFAULT NULL, 
    Slug NVARCHAR(255) NOT NULL DEFAULT N'',
    CONSTRAINT PK_thongtintruyen PRIMARY KEY (MaTruyen)
);
GO

--
-- Dumping data for table `thongtintruyen`
--

INSERT INTO thongtintruyen (MaTruyen, TenTruyen, TacGia, MaTheLoai, NgayDang, TrangThai, AnhTruyen, Slug) VALUES
(N'MT_1', N'Phàm nhân tu tiên', N'Vong Ngữ', N'TL001', '2025-03-21 00:00:00', N'Đang cập nhật', N'https://animehay.li/upload/poster/3078-1699384711.jpg', N''),
(N'MT_10', N'Đội Trưởng Đội Lính Đánh Thuê', N'Lý Hàn', N'TL010', '2025-03-14 00:00:00', N'Đang cập nhật', N'https://i.pinimg.com/736x/5d/48/a4/5d48a4dfb4cdd669e45fbf11e363e741.jpg', N''),
(N'MT_11', N'Hiệp Sĩ Sống Vì Ngày Hôm Nay', N'Nam Phong', N'TL011', '2025-02-27 00:00:00', N'Đang cập nhật', N'https://cmangaax.com/assets/tmp/album/54923.png?v=1721354728', N''),
(N'MT_12', N'Người Chơi Mới Cấp Tối Đa', N'Trần Quân', N'TL012', '2025-03-11 00:00:00', N'Đang cập nhật', N'https://image.lag.vn/upload/news/23/11/10/game-mobile-solo-max-level-newbie_QIHM.jpg', N''),
(N'MT_13', N'Cổ chân nhân', N'Cổ Chân', N'TL003', '2025-05-08 00:00:00', N'Đang cập nhật', N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQzUEpvnY8-Hluc81OIIZ0s-hR0LrSt4XTUVw&s', N''),
(N'MT_14', N'Giao ước tử thần', N'Kim Thần', N'TL008', '2025-05-08 00:00:00', N'Đang cập nhật', N'https://nettruyen.com.vn/images.php?hash=o5p4o4u5q44314p2x5x5h5t4p5c4v2o4o4l4q464v5k506i4e4z3a4v5p2a4h44694j4p274t4x4u4u445d3v3i5y204s4z2u47536q344n4t3c3j3h4s2x2u4p5u5e393s5e3y49413l4r5o3p3p4k334w2t4d3f5q3k3t4q4q4y4a325f55463z5k4n3k35463j3p3n5n4z594g435q2w4n303f3q4u473t2r2p5w346v4w4a4j4u4g3z244y206l4v46305x3r303w4q3s254w5a464m314g5r435f3k4r4y4s344r4k3x4z3w4m3o4y3t4j4e4d3n5z2m534v5w2n5i4a4j4r2l4g44424y506r2m4l524u5o2h4i4p5', N''),
(N'MT_15', N'Người giữ cửa vạn giới', N'Không rõ', N'TL008', '2025-05-08 00:00:00', N'Đang cập nhật', N'https://admin.manhuavn.top/Pictures/Truyen/Large/toan-cau-quy-di-thoi-dai.jpg', N''),
(N'MT_16', N'Thành thần bắt đầu từ Thủy Hầu Tử', N'Không rõ', N'TL010', '2025-05-08 00:00:00', N'Đang cập nhật', N'https://admin.manhuavn.top/Pictures/Truyen/Large/thanh-than-bat-dau-tu-thuy-hau-tu.jpg', N''),
(N'MT_17', N'Thương Thiên Tại Hạ', N'Không rõ', N'TL003', '2025-05-08 00:00:00', N'Đang cập nhật', N'https://admin.manhuavn.top/Pictures/Truyen/Large/thuong-thien-tai-ha.jpg', N''),
(N'MT_18', N'Trở Lại Thành Người Chơi', N'Không rõ', N'TL006', '2025-05-08 00:00:00', N'Đang cập nhật', N'https://static.nettruyenzz.com/hub/image/images/image-1732751754_OTONiLlZP3_2024112723.jpg', N''),
(N'MT_2', N'Lão xà tu tiên', N'Lạc Bỉ Hầu', N'TL002', '2025-03-15 00:00:00', N'Đang cập nhật', N'https://cmangav.com/assets/tmp/album/58962.png?v=1723883764', N''),
(N'MT_3', N'Phong vấn yêu đạo', N'Cửu Đăng', N'TL003', '2025-05-07 00:00:00', N'Đang cập nhật', N'https://admin.manhuavn.top/Pictures/Truyen/Large/phong-yeu-van-dao.jpg', N''),
(N'MT_4', N'Bách Luyện Thành Thần', N'Ân Tại', N'TL001', '2025-03-12 00:00:00', N'Hoàn thành', N'https://animehay.li/upload/poster/3641-1704638183.jpeg', N''),
(N'MT_5', N'Thế Giới Hoàn Mỹ', N'Thần Đông', N'TL004', '2025-03-16 00:00:00', N'Đang cập nhật', N'https://animehay.li/upload/poster/3247-1736719863.jpg', N''),
(N'MT_6', N'Tiên Nghịch', N'Nhĩ Căn', N'TL002', '2025-03-24 00:00:00', N'Hoàn thành', N'https://animehay.li/upload/poster/3879-1740529334.jpg', N''),
(N'MT_7', N'Đấu Phá Thương Khung', N'Thiên Tàm Thổ Đậu', N'TL005', '2025-03-24 00:00:00', N'Đang cập nhật', N'https://animehay.li/upload/poster/3518-1725729749.jpg', N''),
(N'MT_8', N'Đại Vương Tha Mạng', N'Thương Tiến', N'TL008', '2025-03-16 00:00:00', N'Đang cập nhật', N'https://animehay.li/upload/poster/3386-1637627581.jpeg', N''),
(N'MT_9', N'Con Trai Út Của Gia Đình Kiếm Thuật Danh Tiếng', N'Vô Danh', N'TL009', '2025-03-16 00:00:00', N'Đang cập nhật', N'https://thuviensohoa.vn/img/news/2024/12/larger/6775-con-trai-ut-cua-gia-dinh-kiem-thuat-danh-tieng-1.jpg?v=7155', N'');

--------------------------------------------------------
-- Table structure for table `truyenyeuthich`
--------------------------------------------------------

-- Bảng: truyenyeuthich 
CREATE TABLE truyenyeuthich (
    MaNguoiDung NVARCHAR(50) NOT NULL, 
    MaTruyen NVARCHAR(50) NOT NULL,
    NgayThem DATETIME DEFAULT NULL,
    CONSTRAINT PK_truyenyeuthich PRIMARY KEY (MaNguoiDung, MaTruyen)
);
GO

--
-- Dumping data for table `truyenyeuthich`
--

INSERT INTO truyenyeuthich (MaNguoiDung, MaTruyen, NgayThem) VALUES
(N'ND004', N'MT_1', '2025-03-18 00:00:00'),
(N'ND005', N'MT_1', '2025-03-19 00:00:00'),
(N'ND007', N'MT_1', '2025-03-20 00:00:00'),
(N'ND008', N'MT_8', '2025-03-25 00:00:00'),
(N'ND009', N'MT_9', '2025-03-26 00:00:00'),
(N'ND010', N'MT_10', '2025-03-27 00:00:00')



---------------------------KHOA NGOAI-----------------------------
-- Bảng: thongtintruyen
ALTER TABLE thongtintruyen ADD CONSTRAINT FK_TTTruyen_TheLoai
    FOREIGN KEY (MaTheLoai) REFERENCES theloai (MaTheLoai);
GO
-- Bảng: chuong
ALTER TABLE chuong ADD CONSTRAINT FK_Chuong_ThongTinTruyen
    FOREIGN KEY (MaTruyen) REFERENCES thongtintruyen (MaTruyen);
GO
-- Bảng: binhluan
ALTER TABLE binhluan ADD CONSTRAINT FK_BL_NguoiDung
    FOREIGN KEY (MaNguoiDung) REFERENCES nguoidung (MaNguoiDung);
ALTER TABLE binhluan ADD CONSTRAINT FK_BL_ThongTinTruyen
    FOREIGN KEY (MaTruyen) REFERENCES thongtintruyen (MaTruyen);
ALTER TABLE binhluan ADD CONSTRAINT FK_BL_Chuong
    FOREIGN KEY (MaChuong) REFERENCES chuong (MaChuong);
GO

-- Bảng: baocao (CẬP NHẬT KHÓA NGOẠI MỚI)
ALTER TABLE baocao ADD CONSTRAINT FK_BC_NguoiDung -- Ai là người báo cáo
    FOREIGN KEY (MaNguoiDung) REFERENCES nguoidung (MaNguoiDung);
ALTER TABLE baocao ADD CONSTRAINT FK_BC_Admin_XuLy -- Ai là người xử lý (MỚI)
    FOREIGN KEY (MaAdminXuLy) REFERENCES admin (MaAdmin);
ALTER TABLE baocao ADD CONSTRAINT FK_BC_ThongTinTruyen
    FOREIGN KEY (MaTruyen) REFERENCES thongtintruyen (MaTruyen);
ALTER TABLE baocao ADD CONSTRAINT FK_BC_Chuong
    FOREIGN KEY (MaChuong) REFERENCES chuong (MaChuong);
GO

-- Bảng: lichsudoc
ALTER TABLE lichsudoc ADD CONSTRAINT FK_LSD_NguoiDung
    FOREIGN KEY (MaNguoiDung) REFERENCES nguoidung (MaNguoiDung);
ALTER TABLE lichsudoc ADD CONSTRAINT FK_LSD_ThongTinTruyen
    FOREIGN KEY (MaTruyen) REFERENCES thongtintruyen (MaTruyen);
ALTER TABLE lichsudoc ADD CONSTRAINT FK_LSD_Chuong
    FOREIGN KEY (MaChuong) REFERENCES chuong (MaChuong);
GO
-- Bảng: danhgia
ALTER TABLE danhgia ADD CONSTRAINT FK_DG_NguoiDung
    FOREIGN KEY (MaNguoiDung) REFERENCES nguoidung (MaNguoiDung);
ALTER TABLE danhgia ADD CONSTRAINT FK_DG_ThongTinTruyen
    FOREIGN KEY (MaTruyen) REFERENCES thongtintruyen (MaTruyen);
GO
-- Bảng: truyenyeuthich
ALTER TABLE truyenyeuthich ADD CONSTRAINT FK_YeuThich_NguoiDung
    FOREIGN KEY (MaNguoiDung) REFERENCES nguoidung (MaNguoiDung);
ALTER TABLE truyenyeuthich ADD CONSTRAINT FK_YeuThich_ThongTinTruyen
    FOREIGN KEY (MaTruyen) REFERENCES thongtintruyen (MaTruyen);
GO
/*
CÁC CHỈ MỤC (INDEXES) ĐỂ TĂNG TỐC ĐỘ TRUY VẤN . 
-----------------------------------------------------------------
*/

-- Indexes cho bảng baocao
ALTER TABLE admin ADD CONSTRAINT UQ_Admin_TenDangNhap UNIQUE (TenDangNhap);
ALTER TABLE admin ADD CONSTRAINT UQ_Admin_Email UNIQUE (Email);

-- Indexes cho bảng nguoidung
ALTER TABLE nguoidung ADD CONSTRAINT UQ_NguoiDung_TenDangNhap UNIQUE (TenDangNhap);
ALTER TABLE nguoidung ADD CONSTRAINT UQ_NguoiDung_Email UNIQUE (Email);

-- Indexes cho bảng baocao
CREATE NONCLUSTERED INDEX IX_MaNguoiDung_BC ON baocao (MaNguoiDung);
CREATE NONCLUSTERED INDEX IX_MaAdminXuLy_BC ON baocao (MaAdminXuLy); -- (MỚI)
CREATE NONCLUSTERED INDEX IX_MaTruyen_BC ON baocao (MaTruyen);
CREATE NONCLUSTERED INDEX IX_MaChuong_BC ON baocao (MaChuong);

-- Indexes cho bảng binhluan
CREATE NONCLUSTERED INDEX IX_MaNguoiDung_BL ON binhluan (MaNguoiDung);
CREATE NONCLUSTERED INDEX IX_MaTruyen_BL ON binhluan (MaTruyen);
CREATE NONCLUSTERED INDEX IX_MaChuong_BL ON binhluan (MaChuong);

-- Indexes cho bảng chuong
CREATE NONCLUSTERED INDEX IX_MaTruyen_Chuong ON chuong (MaTruyen);

-- Indexes cho bảng lichsudoc
CREATE NONCLUSTERED INDEX IX_MaNguoiDung_LSD ON lichsudoc (MaNguoiDung);
CREATE NONCLUSTERED INDEX IX_MaTruyen_LSD ON lichsudoc (MaTruyen);
CREATE NONCLUSTERED INDEX IX_MaChuong_LSD ON lichsudoc (MaChuong);

-- Indexes cho bảng thongtintruyen
CREATE NONCLUSTERED INDEX IX_MaTheLoai_TTT ON thongtintruyen (MaTheLoai);
GO
--Lấy top 5 truyện bằng random dùng NEWID() --
create proc sp_GetTruyenHotSilder
as begin
select top 5 t.MaTruyen, t.TenTruyen, t.AnhTruyen, t.TacGia, tl.TenTheLoai, t.Slug as MoTa from thongtintruyen t left join theloai tl on t.MaTheLoai=tl.MaTheLoai order by NEWID(); end
go 
--Lấy top 10 truyện dựa vào số sao đánh giá của truyện, dùng rankedTruyen để sắp xếp--
create proc sp_GetTruyenDeCu 
as begin 
with rankedTruyen as (select MaTruyen,AVG(CAST(ISNULL(SoSao,0) as float)) as avgrating from danhgia group by MaTruyen)
select top 10 t.MaTruyen, t.TenTruyen, t.AnhTruyen, t.TacGia,  tl.TenTheLoai, t.Slug as MoTa from thongtintruyen t join rankedTruyen r on t.MaTruyen=r.MaTruyen left join theloai tl on t.MaTheLoai = tl.MaTheLoai order by r.avgrating desc;
end 

go
-- Lấy truyện mới nhất dựa vào ngày đăng-- 
create proc sp_GetTruyenMoiNhat
@SoLuong int = 18 as begin
select top (@SoLuong) t.MaTruyen, t.TenTruyen, t.AnhTruyen, t.TacGia,  tl.TenTheLoai, t.Slug as MoTa from thongtintruyen t left join theloai tl on t.MaTheLoai = tl.MaTheLoai order by t.NgayDang desc; end

create proc sp_GetTopTruyenByView
@ThoiGian nvarchar(10)
as begin 
declare @StartDate datetime;
if @ThoiGian ='Ngay' set @StartDate = GETDATE() - 1; -- 1 ngày qua --
else if @ThoiGian = 'Tuan'
set @StartDate = GETDATE() - 7;
else if @ThoiGian = 'Thang'
set @StartDate = GETDATE() - 30;
else 
set @StartDate = '1900-01-01';
;with viewcount as(
select MaTruyen, COUNT(MaLichSuDoc) as LuotXem from lichsudoc where ThoiGianDoc >=@StartDate group by MaTruyen
)
select top 10 t.MaTruyen, t.TenTruyen, t.AnhTruyen, t.TacGia,  tl.TenTheLoai, t.Slug as MoTa from thongtintruyen t join viewcount v on t.MaTruyen=v.MaTruyen left join theloai tl on t.MaTheLoai = tl.MaTheLoai order by v.LuotXem desc;
end
go


