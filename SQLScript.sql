CREATE DATABASE QLRAUCU;
GO
USE QLRAUCU;
GO

-- Bảng sản phẩm
CREATE TABLE Products (
    ProductId INT PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    ImageFile NVARCHAR(255) NOT NULL,
);

-- Lịch sử click
CREATE TABLE Sequences (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    UserId INT NOT NULL,
    SessionId NVARCHAR(100) NOT NULL,
    ProductId INT NOT NULL,
    ClickTime DATETIME NOT NULL DEFAULT GETDATE(),
    SequenceOrder INT NOT NULL,
    FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);

-- Chỉ mục đề xuất
CREATE INDEX IX_Sequences_ClickTime ON Sequences(ClickTime);
CREATE INDEX IX_Sequences_Session ON Sequences(SessionId, SequenceOrder);
INSERT INTO Products (ProductId, Name, ImageFile)
VALUES 
(1, 'Bông ATiso', '1_ATiso.jpg'),
(2, 'Bắp cải tím', '2_BapCaiTim.jpg'),
(3, 'Cà chua', '3_CaChua.jpg'),
(4, 'Cà rốt', '4_CaRot.jpg'),
(5, 'Chanh vàng', '5_ChanhVang.jpg'),
(6, 'Dưa leo', '6_DuaLeo.jpg'),
(7, 'Khoai mỡ', '7_KhoaiMo.jpg'),
(8, 'Khoai tây', '8_KhoaiTay.jpg'),
(9, 'Khổ qua', '9_KhoQua.jpg'),
(10, 'Ớt chuông đỏ', '10_OtChuongDo.jpg'),
(11, 'Ớt chuông vàng', '11_OtChuongVang.jpg'),
(12, 'Tỏi', '12_Toi.jpg');