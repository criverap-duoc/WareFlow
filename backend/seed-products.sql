-- Productos de prueba con imágenes y stock variado
USE WareFlowDB;
GO

-- Insertar productos de prueba
INSERT INTO Products (Name, Description, SKU, Price, Stock, MinimumStock, Category, ImageUrl, CreatedAt, IsActive)
VALUES 
-- Electrónica
('Laptop HP Pavilion', 'Laptop HP 14 pulgadas, 8GB RAM, 256GB SSD', 'HP-LAP-001', 499990, 10, 2, 'Electrónica', 'https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=300', GETUTCDATE(), 1),
('Laptop Dell Inspiron', 'Laptop Dell 15 pulgadas, 16GB RAM, 512GB SSD', 'DEL-LAP-002', 699990, 5, 2, 'Electrónica', 'https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=300', GETUTCDATE(), 1),
('Laptop Lenovo ThinkPad', 'Laptop Lenovo 14 pulgadas, 16GB RAM, 512GB SSD', 'LEN-LAP-003', 899990, 3, 1, 'Electrónica', 'https://images.unsplash.com/photo-1588872657578-7efd1f1555ed?w=300', GETUTCDATE(), 1),

-- Monitores
('Monitor Samsung 24', 'Monitor 24 pulgadas Full HD IPS', 'SAM-MON-001', 189990, 12, 3, 'Electrónica', 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=300', GETUTCDATE(), 1),
('Monitor LG 27', 'Monitor 27 pulgadas 4K UHD', 'LG-MON-002', 349990, 8, 2, 'Electrónica', 'https://images.unsplash.com/photo-1585792180666-f7347c490ee2?w=300', GETUTCDATE(), 1),
('Monitor Dell 32', 'Monitor curvo 32 pulgadas QHD', 'DEL-MON-003', 499990, 4, 1, 'Electrónica', 'https://images.unsplash.com/photo-1593642634367-d91a135587b5?w=300', GETUTCDATE(), 1),

-- Accesorios
('Mouse Logitech MX Master 3', 'Mouse inalámbrico ergonómico', 'LOG-MOU-001', 89990, 25, 5, 'Accesorios', 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=300', GETUTCDATE(), 1),
('Teclado Mecánico Redragon', 'Teclado mecánico RGB switches rojos', 'RED-TEC-001', 65990, 20, 4, 'Accesorios', 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=300', GETUTCDATE(), 1),
('Headset HyperX Cloud', 'Headset gaming con micrófono', 'HYP-HEA-001', 129990, 15, 3, 'Accesorios', 'https://images.unsplash.com/photo-1583394838336-acd977736f90?w=300', GETUTCDATE(), 1),

-- Componentes
('SSD Samsung 1TB', 'Disco sólido NVMe PCIe 4.0', 'SAM-SSD-001', 129990, 30, 10, 'Componentes', 'https://images.unsplash.com/photo-1597878400966-7c9d0e8a7b8a?w=300', GETUTCDATE(), 1),
('RAM Corsair 16GB', 'Memoria DDR4 3200MHz', 'COR-RAM-001', 79990, 40, 15, 'Componentes', 'https://images.unsplash.com/photo-1562976540-1502c2145186?w=300', GETUTCDATE(), 1),
('GPU NVIDIA RTX 3060', 'Tarjeta gráfica 12GB GDDR6', 'NVI-GPU-001', 399990, 7, 2, 'Componentes', 'https://images.unsplash.com/photo-1591488322449-0a4a5f1c7b5a?w=300', GETUTCDATE(), 1),

-- Stock duplicado (mismos productos con diferente cantidad)
('Laptop HP Pavilion - Lote 2', 'Laptop HP 14 pulgadas, 8GB RAM, 256GB SSD', 'HP-LAP-002', 499990, 8, 2, 'Electrónica', 'https://images.unsplash.com/photo-1541807084-5c52b6b3adef?w=300', GETUTCDATE(), 1),
('Mouse Logitech MX Master 3 - Lote 2', 'Mouse inalámbrico ergonómico', 'LOG-MOU-002', 89990, 15, 5, 'Accesorios', 'https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=300', GETUTCDATE(), 1),
('SSD Samsung 1TB - Lote 2', 'Disco sólido NVMe PCIe 4.0', 'SAM-SSD-002', 129990, 20, 10, 'Componentes', 'https://images.unsplash.com/photo-1597878400966-7c9d0e8a7b8a?w=300', GETUTCDATE(), 1);

PRINT 'Productos de prueba insertados correctamente';
GO
