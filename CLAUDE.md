# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Nguồn chính

[AGENTS.md](AGENTS.md) là tài liệu bắt buộc đọc và tuân theo: định hướng đã chốt, trạng thái thật của gameplay, quy tắc sửa code/asset và checklist kiểm tra theo loại thay đổi. Khi code hoặc danh mục asset thay đổi, cập nhật AGENTS.md và README.md tương ứng. Giao tiếp và tài liệu người dùng bằng tiếng Việt; tên class, file, định danh code bằng tiếng Anh.

## Lệnh thường dùng

Chạy tại thư mục gốc project (PowerShell, Windows):

```powershell
# Test domain Core (compile FarmModel.cs + Tools/Tests/FarmModelTests.cs bằng csc.exe, không cần Unity)
powershell -NoProfile -ExecutionPolicy Bypass -File Tools/test-core.ps1

# Kiểm tra cấu trúc project, GUID, asset nguồn, cú pháp C# (nếu có Tree-sitter)
python Tools/check_project.py

# Kiểm tra độ đầy đủ và ảnh của thư viện model
python Tools/check_model_library.py

git diff --check
```

Core tests là một file test duy nhất (`Tools/Tests/FarmModelTests.cs`) chạy thành một exe; không có cơ chế chạy từng test riêng — sửa/chạy cả file.

- Unity: **6000.3.25f1** (xác nhận qua `ProjectSettings/ProjectVersion.txt`), Editor tại `C:\Users\PC\AppData\Local\Unity\Editors\6000.3.25f1\Editor\Unity.exe` (đường dẫn máy hiện tại, không hardcode).
- Build qua menu Unity **Harvest Isle → 3. Build Android APK** (ra `Builds/Android/HarvestIsle.apk`) hoặc **4. Build Windows demo**. `ProjectSetup.Configure()` chỉ đưa scene game chính vào build list.

### Điều khiển Unity Editor đang mở (EditorWorkBridge)

Không mở Editor/batch thứ hai trên project đang bị khóa. Gửi lệnh qua file:

```powershell
Set-Content -LiteralPath .tools/model-work/request.json -Value '{"command":"play"}' -Encoding utf8
```

Lệnh: `play`, `structures`, `levels`, `production`, `complete`, `weather` (dựng asset, có thể dừng/mở lại Play) và `library-check`, `weather-check`, `production-view` (**phải đang Play**). Gửi tuần tự, không ghi đè khi còn `active.json`. Kết quả ở `.tools/model-work/result.json`, `library-result.json`, `.tools/weather/status.json` — kiểm tra thời gian cập nhật để tránh đọc báo cáo cũ. Đợi Unity import/compile xong trước khi gửi lệnh dùng code vừa sửa.

## Kiến trúc tổng thể

Ba tầng code trong `Assets/HarvestIsle/Scripts/`:

1. **`Core/`** — chỉ có `FarmModel.cs`: domain thuần C#, **không phụ thuộc Unity** (compile được bằng csc.exe trần). Chứa `CropCatalog` (giá/thời gian cây — sửa `CropCatalog.All` để đổi kinh tế), trạng thái ô ruộng, kho, tiền, lệnh gieo/tưới/thu hoạch/bán, tăng trưởng offline và validate dữ liệu. Giữ tầng này độc lập Unity để test nhanh.
2. **`Runtime/`** — gameplay và trình diễn: `FarmGame` (điều phối, autosave), `FarmWorld`/`FarmPlot` (dựng thế giới, model cây theo giai đoạn), `FarmHud` (UI tiếng Việt, safe area), `FarmCamera` (tap/kéo/chụm, chặn thao tác xuyên UI), `FarmSaveStore` (JSON + file tạm + `.bak`, ghi qua `Application.persistentDataPath`), rig nhân vật/con vật (`FarmerAvatar`, `FarmAnimalAvatar` — rig bằng transform, không Animator Controller), scene chạy thử (`PlaygroundDirector`, `ModelLibraryViewer`, `PlaygroundWeather`).
3. **`Editor/`** — mọi code dùng `UnityEditor`: `ProjectSetup` (cấu hình URP/scene/build), `EditorWorkBridge`, và **pipeline sinh model bằng code**: các `*ModelBuilder`/`*Assets` dựng mesh/material procedurally → bake thành prefab trong `Resources/` → render ảnh gallery vào `Art/Previews/`.

### Pipeline asset sinh bằng code (điểm dễ sai)

- **Prefab đã bake là nguồn runtime chính**; không dựng lại mesh mỗi frame/mỗi lần load. Muốn đổi hình dáng model: sửa builder trong `Editor/` rồi bake lại — **rebuild ghi đè mọi chỉnh sửa thủ công** trên prefab sinh ra.
- Giữ GUID và file `.meta`; cập nhật asset hiện có, không xóa rồi tạo lại.
- Prefab nằm trong `Assets/HarvestIsle/Resources/` (Crops/, Animals/, Structures/, Production/, Collection/, Environment/…); mesh/material gộp trong các `.asset` dưới `Assets/HarvestIsle/Art/`.
- Gallery HTML sinh từ template trong `Tools/Templates/` — sửa template trước khi sinh lại HTML. Số lượng model xác nhận bằng `Art/Previews/Library/models.json` và `coverage.json`, không sửa số bằng tay trong tài liệu.
- Asset Quaternius CC0 (manifest + SHA-256 trong `Tools/asset-manifest.json`) là bộ dự phòng — giữ nguyên, không sửa checksum.

### Save và tương thích

Save là JSON tại `Application.persistentDataPath/harvest-isle-v1.json`. Giữ tương thích dữ liệu save và giá trị enum đã lưu (giá trị mới nối vào cuối, ví dụ `FarmAnimalKind`). Không bỏ logic kiểm tra tiền/kho/save để làm một thao tác hiển thị thành công.

### Phân biệt trạng thái thật

Project có 477 prefab nhưng gameplay thật chỉ có 3 cây (cà rốt/ngô/bí đỏ) lúc mở màn, 24 ô khởi đầu (khai hoang tới 36), 120 coins khởi đầu (tiền hiển thị bằng đồng Việt Nam, 1 coin = 1.000đ). Playground/thời tiết/thư viện model là demo hình ảnh, **độc lập với kinh tế/save**. Không mô tả tính năng là hoàn thành chỉ vì đã có prefab, và phân biệt rõ: đã kiểm tra cú pháp ≠ đã compile Unity ≠ đã chạy Play ≠ đã chạy trên điện thoại.
