# seed-products.ps1
# Poblar WareFlow con 60+ productos de prueba via API
# Uso: .\seed-products.ps1
# Requiere: backend corriendo en http://localhost:8080

$API = "https://wareflow-api-csvera-eweqd3c9ctanbvcj.brazilsouth-01.azurewebsites.net/api/v1"

# 1. Registrar usuario admin (o login si ya existe)
$adminEmail = "test@test.com"
$adminPass = "Test123."

$registerBody = @{
    firstName = "Admin"
    lastName = "WareFlow"
    email = $adminEmail
    password = $adminPass
} | ConvertTo-Json

try {
    Invoke-RestMethod -Uri "$API/Auth/register" -Method Post -Body $registerBody -ContentType "application/json" | Out-Null
    Write-Host "Usuario admin registrado" -ForegroundColor Green
} catch {
    Write-Host "Usuario admin ya existe (o error: $($_.Exception.Message))" -ForegroundColor Yellow
}

# 2. Login
$loginBody = @{
    email = $adminEmail
    password = $adminPass
} | ConvertTo-Json

$loginResponse = Invoke-RestMethod -Uri "$API/Auth/login" -Method Post -Body $loginBody -ContentType "application/json"
$token = $loginResponse.accessToken
Write-Host "Login OK. Rol: $($loginResponse.role)" -ForegroundColor Green

$headers = @{
    Authorization = "Bearer $token"
    "Content-Type" = "application/json"
}

# 3. Productos
$productos = @(
    # Laptops
    @{ name="Laptop HP Pavilion 14"; description="Intel Core i5, 8GB RAM, 256GB SSD"; sku="LAP-HP-001"; price=499990; stock=12; minimumStock=3; category="Laptops" },
    @{ name="Laptop HP Pavilion 15"; description="Intel Core i7, 16GB RAM, 512GB SSD"; sku="LAP-HP-002"; price=699990; stock=8; minimumStock=2; category="Laptops" },
    @{ name="Laptop Dell Inspiron 14"; description="AMD Ryzen 5, 8GB RAM, 256GB SSD"; sku="LAP-DEL-001"; price=459990; stock=5; minimumStock=2; category="Laptops" },
    @{ name="Laptop Dell Inspiron 15"; description="Intel Core i5, 16GB RAM, 512GB SSD"; sku="LAP-DEL-002"; price=649990; stock=7; minimumStock=2; category="Laptops" },
    @{ name="Laptop Lenovo ThinkPad"; description="Intel Core i7, 32GB RAM, 1TB SSD"; sku="LAP-LEN-001"; price=1299990; stock=3; minimumStock=1; category="Laptops" },
    @{ name="Laptop Lenovo IdeaPad"; description="AMD Ryzen 7, 16GB RAM, 512GB SSD"; sku="LAP-LEN-002"; price=799990; stock=6; minimumStock=2; category="Laptops" },
    @{ name="Laptop Asus ZenBook"; description="Intel Core i7, 16GB RAM, 1TB SSD"; sku="LAP-ASU-001"; price=1199990; stock=4; minimumStock=1; category="Laptops" },
    @{ name="Laptop Acer Swift"; description="Intel Core i5, 8GB RAM, 256GB SSD"; sku="LAP-ACE-001"; price=529990; stock=9; minimumStock=3; category="Laptops" },
    @{ name="MacBook Air M2"; description="Apple M2, 8GB RAM, 256GB SSD"; sku="LAP-APP-001"; price=999990; stock=10; minimumStock=2; category="Laptops" },
    @{ name="MacBook Pro M3"; description="Apple M3 Pro, 18GB RAM, 512GB SSD"; sku="LAP-APP-002"; price=1599990; stock=5; minimumStock=1; category="Laptops" },

    # Monitores
    @{ name="Monitor Samsung 24 FHD"; description="IPS, 75Hz, FreeSync"; sku="MON-SAM-001"; price=189990; stock=15; minimumStock=4; category="Monitores" },
    @{ name="Monitor Samsung 27 Curvo"; description="144Hz, 1ms, QLED"; sku="MON-SAM-002"; price=349990; stock=8; minimumStock=2; category="Monitores" },
    @{ name="Monitor LG 24 UltraGear"; description="144Hz, 1ms, G-Sync"; sku="MON-LG-001"; price=249990; stock=10; minimumStock=3; category="Monitores" },
    @{ name="Monitor LG 32 4K"; description="UHD, HDR10, USB-C"; sku="MON-LG-002"; price=499990; stock=5; minimumStock=1; category="Monitores" },
    @{ name="Monitor Dell 27 QHD"; description="165Hz, IPS, Calibrado"; sku="MON-DEL-001"; price=399990; stock=6; minimumStock=2; category="Monitores" },
    @{ name="Monitor Dell 34 Ultrawide"; description="Curvo, WQHD, 144Hz"; sku="MON-DEL-002"; price=699990; stock=3; minimumStock=1; category="Monitores" },
    @{ name="Monitor Acer 21.5"; description="Full HD, 60Hz, Economico"; sku="MON-ACE-001"; price=129990; stock=20; minimumStock=5; category="Monitores" },
    @{ name="Monitor Asus ROG 27"; description="240Hz, 0.5ms, G-Sync"; sku="MON-ASU-001"; price=549990; stock=4; minimumStock=1; category="Monitores" },

    # Teclados
    @{ name="Teclado Redragon Kumara"; description="Mecanico, switches rojos, RGB"; sku="TEC-RED-001"; price=49990; stock=25; minimumStock=5; category="Teclados" },
    @{ name="Teclado Redragon Horus"; description="Mecanico, switches azules, RGB"; sku="TEC-RED-002"; price=69990; stock=18; minimumStock=4; category="Teclados" },
    @{ name="Teclado Logitech G213"; description="Membrana, RGB, resistente agua"; sku="TEC-LOG-001"; price=39990; stock=30; minimumStock=8; category="Teclados" },
    @{ name="Teclado Logitech G Pro"; description="Mecanico, switches tactiles, RGB"; sku="TEC-LOG-002"; price=129990; stock=10; minimumStock=2; category="Teclados" },
    @{ name="Teclado Corsair K70"; description="Mecanico, switches rojos, RGB"; sku="TEC-COR-001"; price=149990; stock=8; minimumStock=2; category="Teclados" },
    @{ name="Teclado Razer BlackWidow"; description="Mecanico, switches verdes, RGB"; sku="TEC-RAZ-001"; price=139990; stock=7; minimumStock=2; category="Teclados" },
    @{ name="Teclado HyperX Alloy"; description="Mecanico, switches rojos, RGB"; sku="TEC-HYP-001"; price=89990; stock=12; minimumStock=3; category="Teclados" },
    @{ name="Teclado Apple Magic"; description="Inalambrico, diseno delgado"; sku="TEC-APP-001"; price=119990; stock=15; minimumStock=3; category="Teclados" },

    # Mouses
    @{ name="Mouse Logitech G203"; description="RGB, 8000 DPI, 6 botones"; sku="MOU-LOG-001"; price=24990; stock=40; minimumStock=10; category="Mouses" },
    @{ name="Mouse Logitech G502"; description="Hero, 25600 DPI, 11 botones"; sku="MOU-LOG-002"; price=59990; stock=25; minimumStock=5; category="Mouses" },
    @{ name="Mouse Logitech MX Master 3"; description="Inalambrico, ergonomico"; sku="MOU-LOG-003"; price=89990; stock=20; minimumStock=4; category="Mouses" },
    @{ name="Mouse Razer DeathAdder"; description="Essential, 6400 DPI"; sku="MOU-RAZ-001"; price=29990; stock=35; minimumStock=8; category="Mouses" },
    @{ name="Mouse Razer Viper"; description="Ultraligero, 20000 DPI"; sku="MOU-RAZ-002"; price=69990; stock=15; minimumStock=3; category="Mouses" },
    @{ name="Mouse Corsair M65"; description="RGB, 18000 DPI, ajustable"; sku="MOU-COR-001"; price=49990; stock=18; minimumStock=4; category="Mouses" },
    @{ name="Mouse Redragon Griffin"; description="RGB, 12400 DPI, 7 botones"; sku="MOU-RED-001"; price=19990; stock=50; minimumStock=15; category="Mouses" },
    @{ name="Mouse Apple Magic"; description="Inalambrico, tactil multi-touch"; sku="MOU-APP-001"; price=79990; stock=12; minimumStock=3; category="Mouses" },

    # Audifonos
    @{ name="Audifonos HyperX Cloud Stinger"; description="Alambrico, 50mm, microfono"; sku="AUD-HYP-001"; price=49990; stock=22; minimumStock=5; category="Audifonos" },
    @{ name="Audifonos HyperX Cloud Alpha"; description="Dual chamber, sonido mejorado"; sku="AUD-HYP-002"; price=89990; stock=15; minimumStock=3; category="Audifonos" },
    @{ name="Audifonos Logitech G433"; description="7.1 Surround, microfono"; sku="AUD-LOG-001"; price=69990; stock=18; minimumStock=4; category="Audifonos" },
    @{ name="Audifonos Razer BlackShark V2"; description="THX Spatial Audio, microfono"; sku="AUD-RAZ-001"; price=109990; stock=10; minimumStock=2; category="Audifonos" },
    @{ name="Audifonos Corsair HS60"; description="7.1 Surround, microfono"; sku="AUD-COR-001"; price=59990; stock=20; minimumStock=5; category="Audifonos" },
    @{ name="Audifonos Sony WH-1000XM4"; description="Inalambrico, Noise Cancelling"; sku="AUD-SON-001"; price=249990; stock=8; minimumStock=2; category="Audifonos" },
    @{ name="Audifonos JBL Tune 500"; description="Alambrico, graves potentes"; sku="AUD-JBL-001"; price=39990; stock=30; minimumStock=8; category="Audifonos" },
    @{ name="Audifonos Apple AirPods Pro"; description="Inalambrico, Noise Cancelling"; sku="AUD-APP-001"; price=199990; stock=12; minimumStock=3; category="Audifonos" },

    # Componentes
    @{ name="SSD Samsung 1TB"; description="NVMe, 7000MB/s lectura"; sku="COM-SSD-001"; price=129990; stock=35; minimumStock=10; category="Componentes" },
    @{ name="SSD WD Black 500GB"; description="NVMe, 5000MB/s lectura"; sku="COM-SSD-002"; price=69990; stock=40; minimumStock=12; category="Componentes" },
    @{ name="RAM Corsair 16GB"; description="DDR4, 3200MHz, RGB"; sku="COM-RAM-001"; price=79990; stock=45; minimumStock=15; category="Componentes" },
    @{ name="RAM Kingston 32GB"; description="DDR5, 5600MHz"; sku="COM-RAM-002"; price=149990; stock=20; minimumStock=5; category="Componentes" },
    @{ name="GPU NVIDIA RTX 3060"; description="12GB GDDR6, Ray Tracing"; sku="COM-GPU-001"; price=399990; stock=8; minimumStock=2; category="Componentes" },
    @{ name="GPU NVIDIA RTX 4070"; description="12GB GDDR6X, DLSS 3"; sku="COM-GPU-002"; price=699990; stock=5; minimumStock=1; category="Componentes" },
    @{ name="GPU AMD RX 6700 XT"; description="12GB GDDR6, 1440p Gaming"; sku="COM-GPU-003"; price=449990; stock=6; minimumStock=2; category="Componentes" },
    @{ name="CPU Intel i7-13700K"; description="16 nucleos, 5.4GHz"; sku="COM-CPU-001"; price=429990; stock=10; minimumStock=2; category="Componentes" },
    @{ name="CPU AMD Ryzen 7 7800X3D"; description="8 nucleos, 5.0GHz, 3D Cache"; sku="COM-CPU-002"; price=499990; stock=7; minimumStock=2; category="Componentes" },
    @{ name="Fuente Corsair 750W"; description="80 Plus Gold, Modular"; sku="COM-PSU-001"; price=129990; stock=15; minimumStock=4; category="Componentes" },

    # Smartphones
    @{ name="iPhone 15 Pro"; description="128GB, Titanio, A17 Pro"; sku="PHN-APP-001"; price=1199990; stock=12; minimumStock=3; category="Smartphones" },
    @{ name="iPhone 15 Pro Max"; description="256GB, Titanio, A17 Pro"; sku="PHN-APP-002"; price=1399990; stock=8; minimumStock=2; category="Smartphones" },
    @{ name="Samsung Galaxy S24"; description="128GB, 8GB RAM, IA"; sku="PHN-SAM-001"; price=899990; stock=15; minimumStock=4; category="Smartphones" },
    @{ name="Samsung Galaxy S24 Ultra"; description="256GB, 12GB RAM, S Pen"; sku="PHN-SAM-002"; price=1299990; stock=10; minimumStock=2; category="Smartphones" },
    @{ name="Xiaomi 13 Pro"; description="256GB, 12GB RAM, Leica"; sku="PHN-XIA-001"; price=799990; stock=20; minimumStock=5; category="Smartphones" },
    @{ name="Xiaomi Redmi Note 13"; description="128GB, 8GB RAM, 120Hz"; sku="PHN-XIA-002"; price=249990; stock=30; minimumStock=8; category="Smartphones" },
    @{ name="Google Pixel 8 Pro"; description="128GB, Tensor G3, IA"; sku="PHN-GOO-001"; price=999990; stock=7; minimumStock=2; category="Smartphones" },
    @{ name="Motorola Edge 40"; description="256GB, 144Hz, carga rapida"; sku="PHN-MOT-001"; price=499990; stock=12; minimumStock=3; category="Smartphones" },

    # Tablets
    @{ name="iPad Pro 11"; description="M2, 128GB, Wi-Fi"; sku="TAB-APP-001"; price=799990; stock=10; minimumStock=2; category="Tablets" },
    @{ name="iPad Pro 12.9"; description="M2, 256GB, Wi-Fi"; sku="TAB-APP-002"; price=1099990; stock=6; minimumStock=2; category="Tablets" },
    @{ name="Samsung Tab S9"; description="11, 128GB, S Pen"; sku="TAB-SAM-001"; price=599990; stock=12; minimumStock=3; category="Tablets" },
    @{ name="Samsung Tab S9 Ultra"; description="14.6, 256GB, S Pen"; sku="TAB-SAM-002"; price=999990; stock=5; minimumStock=1; category="Tablets" },
    @{ name="Xiaomi Pad 6"; description="11, 128GB, 144Hz"; sku="TAB-XIA-001"; price=349990; stock=15; minimumStock=4; category="Tablets" },
    @{ name="Lenovo Tab P11"; description="11.5, 128GB, 2K"; sku="TAB-LEN-001"; price=249990; stock=18; minimumStock=5; category="Tablets" }
)

Write-Host "`nInsertando $($productos.Count) productos..." -ForegroundColor Cyan

$creados = 0
$existentes = 0
$errores = 0

foreach ($p in $productos) {
    $body = $p | ConvertTo-Json
    try {
        Invoke-RestMethod -Uri "$API/Products" -Method Post -Body $body -Headers $headers | Out-Null
        $creados++
        Write-Host "  OK $($p.name)" -ForegroundColor Green
    } catch {
        if ($_.Exception.Response.StatusCode.value__ -eq 400) {
            $existentes++
            Write-Host "  SKIP $($p.name) (ya existe)" -ForegroundColor Yellow
        } else {
            $errores++
            Write-Host "  ERROR $($p.name): $($_.Exception.Message)" -ForegroundColor Red
        }
    }
}

Write-Host "`nResumen:" -ForegroundColor Cyan
Write-Host "  Creados: $creados" -ForegroundColor Green
Write-Host "  Existentes: $existentes" -ForegroundColor Yellow
Write-Host "  Errores: $errores" -ForegroundColor Red
Write-Host "`nProductos en catalogo:" -ForegroundColor Cyan
$all = Invoke-RestMethod -Uri "$API/Products?pageSize=100" -Method Get
Write-Host "  Total: $($all.totalItems)"