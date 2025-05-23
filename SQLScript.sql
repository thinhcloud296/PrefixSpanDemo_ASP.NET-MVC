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
(1, N'Bông ATiso', '1_ATiso.jpg'),
(2, N'Bắp cải tím', '2_BapCaiTim.jpg'),
(3, N'Cà chua', '3_CaChua.jpg'),
(4, N'Cà rốt', '4_CaRot.jpg'),
(5, N'Chanh vàng', '5_ChanhVang.jpg'),
(6, N'Dưa leo', '6_DuaLeo.jpg'),
(7, N'Khoai mỡ', '7_KhoaiMo.jpg'),
(8, N'Khoai tây', '8_KhoaiTay.jpg'),
(9, N'Khổ qua', '9_KhoQua.jpg'),
(10, N'Ớt chuông đỏ', '10_OtChuongDo.jpg'),
(11, N'Ớt chuông vàng', '11_OtChuongVang.jpg'),
(12, N'Tỏi', '12_Toi.jpg');