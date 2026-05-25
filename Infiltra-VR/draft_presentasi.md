# Draft Presentasi Progress: Infiltra-VR

Berikut adalah rancangan konten untuk presentasi progress proyek **Infiltra-VR**, disusun berdasarkan log commit dari branch `main`, `feature/terrain-and-asset`, dan `basic/feature/inventory-shop`.

---

## Slide 1 — Judul Project (±30 detik)

**Isi:**
*   **Nama project:** Infiltra-VR
*   **Jenis:** Virtual Reality (VR) 3D Game
*   **Nama tim:** *[Isi dengan nama tim/kelompok Anda]*
*   **Progress implementasi:** ± 70%

**Visual:**
*   Screenshot terbaik project (Pilih scene/tampilan game yang paling menarik secara visual, misalnya saat memegang senjata atau melihat environment terrain).

---

## Slide 2 — Progress Implementasi Unity (±1 menit)

**Isi:**

| Fitur | Status | Keterangan Branch/Commit |
| :--- | :---: | :--- |
| Environment & Terrain | ✅ | `feature/terrain-and-asset` |
| Character Movement (Locomotion) | ✅ | `main` (Smooth locomotion & teleportation) |
| VR Interaction (Grab & Bimanual) | ✅ | `main` (Implementasi grab & bimanual) |
| UI System (Inventory, Chest, Shop)| ✅ | `basic/feature/inventory-shop` |`
| Audio | 🔄 | *Ongoing* |
| Level Design & Polish | 🔄 | *Ongoing* |

**Wajib:**
*   Screenshot Hierarchy / Scene Unity yang menampilkan struktur object VR (XR Rig / Player Controller, Canvas UI Inventory, dan Terrain).

---

## Slide 3 — Lingkungan Simulasi (±1 menit)
*(Bab 4 laporan)*

**Isi:**
*   **Environment yang dibuat:** Penataan terrain dan peletakan aset lingkungan untuk area permainan.
*   **Asset utama:** *[Sebutkan nama aset utama, misal: bangunan, rintangan, pepohonan, dll. yang digunakan]*
*   **Lighting/terrain/object placement:** Penyesuaian pencahayaan global (Directional light & baked lighting) serta penempatan objek agar sesuai dengan navigasi VR.

**Visual:**
*   *Before vs After* (Tampilan scene kosong vs Scene yang sudah terisi terrain & asset lengkap).
*   Screenshot scene gameplay dari sudut pandang kamera (Player POV).

---

## Slide 4 — Technical Interaction (±2 menit)
*(Bab 5 laporan)*

**Isi:**
Fokus pada mekanisme VR yang telah diimplementasikan:
*   **Controller Movement:** Menggunakan sistem *Smooth Locomotion* dan *Teleportation* untuk pergerakan pemain agar meminimalisir *motion sickness*.
*   **Raycast/Grab:** Implementasi fitur *Grab* biasa dan *Bimanual Interaction* (interaksi dua tangan) untuk memegang objek (misal: senjata atau item).
*   **UI Interaction:** Interaksi pemain dengan sistem **Inventory**, **Chest**, dan **Shop UI** menggunakan *XR Ray Interactor*.

**Tambahkan:**
*   Diagram kontrol sederhana (contoh: *Thumbstick* kiri untuk *Locomotion*, tombol *Grip* untuk *Grab*).
*   Screenshot Inspector Unity pada komponen `XR Grab Interactable` atau Script Inventory/Shop.

**Fokus Penyampaian:**
*“Bagaimana user berinteraksi dengan sistem secara imersif menggunakan controller VR, mulai dari bergerak hingga mengelola inventory.”*

---

## Slide 6 — Kendala & Solusi (±30 detik)
*(Catatan: Slide 5 dilewati sesuai dengan urutan permintaan Anda)*

**Isi:**

| Kendala | Solusi |
| :--- | :--- |
| **Konflik Pergerakan VR:** *Smooth locomotion* dan *teleportation* sempat bertabrakan. | Melakukan *fix* dan penyesuaian mekanisme agar pemain dapat beralih metode pergerakan dengan mulus (Berdasarkan commit `Fix smooth locomotion...`). |
| **Interaksi UI VR:** Canvas standar tidak bisa diklik menggunakan controller VR. | Implementasi *Tracked Device Graphic Raycaster* pada Canvas UI Inventory & Shop. |
| **Tata Letak Aset:** Objek seringkali menembus *terrain* (*Collision error*). | Menyesuaikan *Collider* dan *Rigidbody constraints* pada aset lingkungan. |

---

## Slide 7 — Demo Unity (±5 menit)

**Demo wajib menunjukkan:**
1.  **Scene berjalan:** Memulai game dari Unity Editor (Play Mode) atau *build* langsung di Headset VR.
2.  **Movement/Control:** Menunjukkan *Smooth Locomotion* dan *Teleportation* secara bergantian.
3.  **Interaksi Object:** Mencontohkan mengambil barang (*Grab*) dan memegang barang dengan dua tangan (*Bimanual Interaction*).
4.  **UI / Menu:** Menampilkan UI *Inventory*, membuka *Chest*, dan mensimulasikan interaksi pembelian di *Shop UI*.

---

## Slide 8 — Closing & Next Progress (±30 detik)

**Isi:**
*   **Progress saat ini:** Core mechanics VR (Movement, Interaksi, UI) dan fondasi *Environment* sudah berhasil diimplementasikan.
*   **Target milestone berikutnya:**
    *   Finalisasi *gameplay loop* (Objektif permainan).
    *   Integrasi efek *Audio* (BGM dan SFX interaksi).
    *   *Testing usability* secara langsung kepada pengguna menggunakan Headset VR.
    *   Optimasi performa (FPS) untuk VR.