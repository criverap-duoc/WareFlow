-- ============================================
-- SCRIPT MASIVO DE PRODUCTOS DE PRUEBA
-- Inventario realista con 60+ productos
-- ============================================
USE WareFlowDB;
GO

-- Limpiar productos existentes (opcional - comentar si no quieres borrar)
-- DELETE FROM Products;
-- DBCC CHECKIDENT ('Products', RESEED, 0);
-- GO

DECLARE @i INT = 1;
DECLARE @Category NVARCHAR(50);
DECLARE @ProductName NVARCHAR(100);
DECLARE @SKU NVARCHAR(50);
DECLARE @Price INT;
DECLARE @Stock INT;
DECLARE @MinStock INT;

-- ============================================
-- 1. LAPTOPS (10 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('Laptop HP Pavilion 14', 'Intel Core i5, 8GB RAM, 256GB SSD', 'LAP-HP-001', 499990, 12, 3, 'Laptops', GETUTCDATE(), 1),
('Laptop HP Pavilion 15', 'Intel Core i7, 16GB RAM, 512GB SSD', 'LAP-HP-002', 699990, 8, 2, 'Laptops', GETUTCDATE(), 1),
('Laptop Dell Inspiron 14', 'AMD Ryzen 5, 8GB RAM, 256GB SSD', 'LAP-DEL-001', 459990, 5, 2, 'Laptops', GETUTCDATE(), 1),
('Laptop Dell Inspiron 15', 'Intel Core i5, 16GB RAM, 512GB SSD', 'LAP-DEL-002', 649990, 7, 2, 'Laptops', GETUTCDATE(), 1),
('Laptop Lenovo ThinkPad', 'Intel Core i7, 32GB RAM, 1TB SSD', 'LAP-LEN-001', 1299990, 3, 1, 'Laptops', GETUTCDATE(), 1),
('Laptop Lenovo IdeaPad', 'AMD Ryzen 7, 16GB RAM, 512GB SSD', 'LAP-LEN-002', 799990, 6, 2, 'Laptops', GETUTCDATE(), 1),
('Laptop Asus ZenBook', 'Intel Core i7, 16GB RAM, 1TB SSD', 'LAP-ASU-001', 1199990, 4, 1, 'Laptops', GETUTCDATE(), 1),
('Laptop Acer Swift', 'Intel Core i5, 8GB RAM, 256GB SSD', 'LAP-ACE-001', 529990, 9, 3, 'Laptops', GETUTCDATE(), 1),
('MacBook Air M2', 'Apple M2, 8GB RAM, 256GB SSD', 'LAP-APP-001', 999990, 10, 2, 'Laptops', GETUTCDATE(), 1),
('MacBook Pro M3', 'Apple M3 Pro, 18GB RAM, 512GB SSD', 'LAP-APP-002', 1599990, 5, 1, 'Laptops', GETUTCDATE(), 1);

-- ============================================
-- 2. MONITORES (8 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('Monitor Samsung 24" FHD', 'IPS, 75Hz, FreeSync', 'MON-SAM-001', 189990, 15, 4, 'Monitores', GETUTCDATE(), 1),
('Monitor Samsung 27" Curvo', '144Hz, 1ms, QLED', 'MON-SAM-002', 349990, 8, 2, 'Monitores', GETUTCDATE(), 1),
('Monitor LG 24" UltraGear', '144Hz, 1ms, G-Sync', 'MON-LG-001', 249990, 10, 3, 'Monitores', GETUTCDATE(), 1),
('Monitor LG 32" 4K', 'UHD, HDR10, USB-C', 'MON-LG-002', 499990, 5, 1, 'Monitores', GETUTCDATE(), 1),
('Monitor Dell 27" QHD', '165Hz, IPS, Calibrado', 'MON-DEL-001', 399990, 6, 2, 'Monitores', GETUTCDATE(), 1),
('Monitor Dell 34" Ultrawide', 'Curvo, WQHD, 144Hz', 'MON-DEL-002', 699990, 3, 1, 'Monitores', GETUTCDATE(), 1),
('Monitor Acer 21.5"', 'Full HD, 60Hz, Económico', 'MON-ACE-001', 129990, 20, 5, 'Monitores', GETUTCDATE(), 1),
('Monitor Asus ROG 27"', '240Hz, 0.5ms, G-Sync', 'MON-ASU-001', 549990, 4, 1, 'Monitores', GETUTCDATE(), 1);

-- ============================================
-- 3. TECLADOS (8 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('Teclado Redragon Kumara', 'Mecánico, switches rojos, RGB', 'TEC-RED-001', 49990, 25, 5, 'Teclados', GETUTCDATE(), 1),
('Teclado Redragon Horus', 'Mecánico, switches azules, RGB', 'TEC-RED-002', 69990, 18, 4, 'Teclados', GETUTCDATE(), 1),
('Teclado Logitech G213', 'Membrana, RGB, resistente agua', 'TEC-LOG-001', 39990, 30, 8, 'Teclados', GETUTCDATE(), 1),
('Teclado Logitech G Pro', 'Mecánico, switches táctiles, RGB', 'TEC-LOG-002', 129990, 10, 2, 'Teclados', GETUTCDATE(), 1),
('Teclado Corsair K70', 'Mecánico, switches rojos, RGB', 'TEC-COR-001', 149990, 8, 2, 'Teclados', GETUTCDATE(), 1),
('Teclado Razer BlackWidow', 'Mecánico, switches verdes, RGB', 'TEC-RAZ-001', 139990, 7, 2, 'Teclados', GETUTCDATE(), 1),
('Teclado HyperX Alloy', 'Mecánico, switches rojos, RGB', 'TEC-HYP-001', 89990, 12, 3, 'Teclados', GETUTCDATE(), 1),
('Teclado Apple Magic', 'Inalámbrico, diseño delgado', 'TEC-APP-001', 119990, 15, 3, 'Teclados', GETUTCDATE(), 1);

-- ============================================
-- 4. MOUSES (8 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('Mouse Logitech G203', 'RGB, 8000 DPI, 6 botones', 'MOU-LOG-001', 24990, 40, 10, 'Mouses', GETUTCDATE(), 1),
('Mouse Logitech G502', 'Hero, 25600 DPI, 11 botones', 'MOU-LOG-002', 59990, 25, 5, 'Mouses', GETUTCDATE(), 1),
('Mouse Logitech MX Master 3', 'Inalámbrico, ergonómico', 'MOU-LOG-003', 89990, 20, 4, 'Mouses', GETUTCDATE(), 1),
('Mouse Razer DeathAdder', 'Essential, 6400 DPI', 'MOU-RAZ-001', 29990, 35, 8, 'Mouses', GETUTCDATE(), 1),
('Mouse Razer Viper', 'Ultraligero, 20000 DPI', 'MOU-RAZ-002', 69990, 15, 3, 'Mouses', GETUTCDATE(), 1),
('Mouse Corsair M65', 'RGB, 18000 DPI, ajustable', 'MOU-COR-001', 49990, 18, 4, 'Mouses', GETUTCDATE(), 1),
('Mouse Redragon Griffin', 'RGB, 12400 DPI, 7 botones', 'MOU-RED-001', 19990, 50, 15, 'Mouses', GETUTCDATE(), 1),
('Mouse Apple Magic', 'Inalámbrico, táctil multi-touch', 'MOU-APP-001', 79990, 12, 3, 'Mouses', GETUTCDATE(), 1);

-- ============================================
-- 5. AUDÍFONOS (8 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('HyperX Cloud Stinger', 'Alámbrico, 50mm, micrófono', 'AUD-HYP-001', 49990, 22, 5, 'Audífonos', GETUTCDATE(), 1),
('HyperX Cloud Alpha', 'Dual chamber, sonido mejorado', 'AUD-HYP-002', 89990, 15, 3, 'Audífonos', GETUTCDATE(), 1),
('Logitech G433', '7.1 Surround, micrófono', 'AUD-LOG-001', 69990, 18, 4, 'Audífonos', GETUTCDATE(), 1),
('Razer BlackShark V2', 'THX Spatial Audio, micrófono', 'AUD-RAZ-001', 109990, 10, 2, 'Audífonos', GETUTCDATE(), 1),
('Corsair HS60', '7.1 Surround, micrófono', 'AUD-COR-001', 59990, 20, 5, 'Audífonos', GETUTCDATE(), 1),
('Sony WH-1000XM4', 'Inalámbrico, Noise Cancelling', 'AUD-SON-001', 249990, 8, 2, 'Audífonos', GETUTCDATE(), 1),
('JBL Tune 500', 'Alámbrico, graves potentes', 'AUD-JBL-001', 39990, 30, 8, 'Audífonos', GETUTCDATE(), 1),
('Apple AirPods Pro', 'Inalámbrico, Noise Cancelling', 'AUD-APP-001', 199990, 12, 3, 'Audífonos', GETUTCDATE(), 1);

-- ============================================
-- 6. COMPONENTES PC (10 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('SSD Samsung 1TB', 'NVMe, 7000MB/s lectura', 'COM-SSD-001', 129990, 35, 10, 'Componentes', GETUTCDATE(), 1),
('SSD WD Black 500GB', 'NVMe, 5000MB/s lectura', 'COM-SSD-002', 69990, 40, 12, 'Componentes', GETUTCDATE(), 1),
('RAM Corsair 16GB', 'DDR4, 3200MHz, RGB', 'COM-RAM-001', 79990, 45, 15, 'Componentes', GETUTCDATE(), 1),
('RAM Kingston 32GB', 'DDR5, 5600MHz', 'COM-RAM-002', 149990, 20, 5, 'Componentes', GETUTCDATE(), 1),
('GPU NVIDIA RTX 3060', '12GB GDDR6, Ray Tracing', 'COM-GPU-001', 399990, 8, 2, 'Componentes', GETUTCDATE(), 1),
('GPU NVIDIA RTX 4070', '12GB GDDR6X, DLSS 3', 'COM-GPU-002', 699990, 5, 1, 'Componentes', GETUTCDATE(), 1),
('GPU AMD RX 6700 XT', '12GB GDDR6, 1440p Gaming', 'COM-GPU-003', 449990, 6, 2, 'Componentes', GETUTCDATE(), 1),
('CPU Intel i7-13700K', '16 núcleos, 5.4GHz', 'COM-CPU-001', 429990, 10, 2, 'Componentes', GETUTCDATE(), 1),
('CPU AMD Ryzen 7 7800X3D', '8 núcleos, 5.0GHz, 3D Cache', 'COM-CPU-002', 499990, 7, 2, 'Componentes', GETUTCDATE(), 1),
('Fuente Corsair 750W', '80 Plus Gold, Modular', 'COM-PSU-001', 129990, 15, 4, 'Componentes', GETUTCDATE(), 1);

-- ============================================
-- 7. SMARTPHONES (8 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('iPhone 15 Pro', '128GB, Titanio, A17 Pro', 'PHN-APP-001', 1199990, 12, 3, 'Smartphones', GETUTCDATE(), 1),
('iPhone 15 Pro Max', '256GB, Titanio, A17 Pro', 'PHN-APP-002', 1399990, 8, 2, 'Smartphones', GETUTCDATE(), 1),
('Samsung Galaxy S24', '128GB, 8GB RAM, IA', 'PHN-SAM-001', 899990, 15, 4, 'Smartphones', GETUTCDATE(), 1),
('Samsung Galaxy S24 Ultra', '256GB, 12GB RAM, S Pen', 'PHN-SAM-002', 1299990, 10, 2, 'Smartphones', GETUTCDATE(), 1),
('Xiaomi 13 Pro', '256GB, 12GB RAM, Leica', 'PHN-XIA-001', 799990, 20, 5, 'Smartphones', GETUTCDATE(), 1),
('Xiaomi Redmi Note 13', '128GB, 8GB RAM, 120Hz', 'PHN-XIA-002', 249990, 30, 8, 'Smartphones', GETUTCDATE(), 1),
('Google Pixel 8 Pro', '128GB, Tensor G3, IA', 'PHN-GOO-001', 999990, 7, 2, 'Smartphones', GETUTCDATE(), 1),
('Motorola Edge 40', '256GB, 144Hz, carga rápida', 'PHN-MOT-001', 499990, 12, 3, 'Smartphones', GETUTCDATE(), 1);

-- ============================================
-- 8. TABLETS (6 productos)
-- ============================================
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, CreatedAt, IsActive) VALUES
('iPad Pro 11"', 'M2, 128GB, Wi-Fi', 'TAB-APP-001', 799990, 10, 2, 'Tablets', GETUTCDATE(), 1),
('iPad Pro 12.9"', 'M2, 256GB, Wi-Fi', 'TAB-APP-002', 1099990, 6, 2, 'Tablets', GETUTCDATE(), 1),
('Samsung Tab S9', '11", 128GB, S Pen', 'TAB-SAM-001', 599990, 12, 3, 'Tablets', GETUTCDATE(), 1),
('Samsung Tab S9 Ultra', '14.6", 256GB, S Pen', 'TAB-SAM-002', 999990, 5, 1, 'Tablets', GETUTCDATE(), 1),
('Xiaomi Pad 6', '11", 128GB, 144Hz', 'TAB-XIA-001', 349990, 15, 4, 'Tablets', GETUTCDATE(), 1),
('Lenovo Tab P11', '11.5", 128GB, 2K', 'TAB-LEN-001', 249990, 18, 5, 'Tablets', GETUTCDATE(), 1);

PRINT '==========================================';
PRINT 'PRODUCTOS INSERTADOS EXITOSAMENTE';
PRINT 'Total: 60+ productos en 8 categorías';
PRINT '==========================================';

-- Verificar cantidad
SELECT Category, COUNT(*) as Total FROM Products WHERE IsActive = 1 GROUP BY Category ORDER BY Category;
GO
