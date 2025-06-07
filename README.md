# 🌟 GuideTuristike

<div align="center">

# 🚗 Enzo Travel – Guide Turistike

*Discover the Hidden Gems of Rural Albania*

![Homepage Animation](docs/screenshots/home.gif)

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-blue.svg)](https://dotnet.microsoft.com/)
[![ASP.NET MVC](https://img.shields.io/badge/ASP.NET%20MVC-5-purple.svg)](https://docs.microsoft.com/en-us/aspnet/mvc/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-LocalDB-orange.svg)](https://docs.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb)

</div>

---

## 🎯 **Mission Statement**

> *A sophisticated ASP.NET MVC web application designed to promote and simplify rural tourism in Albania, offering intuitive tools for travelers and administrators alike. Experience the authentic beauty of Albania's countryside through our comprehensive travel platform.*

---

## ✨ **Core Features**

<table>
<tr>
<td width="50%">

### 🔐 **Authentication & Security**
- **Secure user registration** with email validation
- **Login/logout** with session management
- **Role-based access** (User/Admin)
- **Password encryption** and security best practices

### 🌍 **User Experience**
- **Bilingual interface** (Albanian/English)
- **Dark/Light mode** toggle with smooth transitions
- **Responsive design** for all devices
- **Intuitive navigation** with breadcrumbs

</td>
<td width="50%">

### 🗺️ **Interactive Features**
- **Leaflet mapping** with custom markers
- **Distance calculation** from Tirana
- **Trip cost estimation** with real-time updates
- **PDF confirmation** downloads via Rotativa

### 🏞️ **Content Management**
- **35+ curated destinations** across Albania
- **Regional categorization** and filtering
- **Admin dashboard** for full CRUD operations
- **Reservation management** system

</td>
</tr>
</table>

---

## 🛠️ **Technology Stack**

<div align="center">

| **Layer** | **Technologies** |
|-----------|------------------|
| 🎨 **Frontend** | ![ASP.NET MVC](https://img.shields.io/badge/ASP.NET%20MVC-5-purple?style=flat-square) ![Bootstrap](https://img.shields.io/badge/Bootstrap-5-7952B3?style=flat-square&logo=bootstrap&logoColor=white) ![jQuery](https://img.shields.io/badge/jQuery-3.7.1-0769AD?style=flat-square&logo=jquery&logoColor=white) |
| 🗺️ **Mapping** | ![Leaflet](https://img.shields.io/badge/Leaflet.js-1.9.4-199900?style=flat-square&logo=leaflet&logoColor=white) ![OpenStreetMap](https://img.shields.io/badge/OpenStreetMap-7EBC6F?style=flat-square&logo=openstreetmap&logoColor=white) |
| ⚙️ **Backend** | ![.NET Framework](https://img.shields.io/badge/.NET%20Framework-4.8-512BD4?style=flat-square&logo=.net&logoColor=white) ![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=c-sharp&logoColor=white) |
| 🗄️ **Database** | ![SQL Server](https://img.shields.io/badge/SQL%20Server-CC2927?style=flat-square&logo=microsoft-sql-server&logoColor=white) ![Entity Framework](https://img.shields.io/badge/Entity%20Framework-6-512BD4?style=flat-square) |
| 📄 **PDF Generation** | ![Rotativa](https://img.shields.io/badge/Rotativa-wkhtmltopdf-FF6B6B?style=flat-square) |
| 🐳 **DevOps** | ![Docker](https://img.shields.io/badge/Docker-2496ED?style=flat-square&logo=docker&logoColor=white) |

</div>

---

## 🚀 **Quick Start Guide**

### **Prerequisites**
- 💻 Visual Studio 2022 or JetBrains Rider
- 🗄️ SQL Server LocalDB
- 🌐 .NET Framework 4.8
- 🐳 Docker (optional)

### **Step 1: Clone & Setup**
```bash
git clone https://github.com/enzoindabenzo/GuideTuristike.git
cd GuideTuristike
```

### **Step 2: Database Configuration**
```bash
# Update connection string in Web.config if needed
Update-Database
```

### **Step 3: Launch Application**
```bash
# Press F5 in Visual Studio
# Navigate to: https://localhost:44300/
```

### **Step 4: Docker Deployment (Optional)**
```bash
docker build -t guide-turistike .
docker run -p 8080:80 guide-turistike
# Visit: http://localhost:8080
```

---

## 📸 **Visual Showcase**

<details>
<summary>🏠 <strong>Homepage & Navigation</strong></summary>

![Homepage Animation](docs/screenshots/EnzoTravel.mp4)
*Dynamic homepage with smooth animations and intuitive navigation*

</details>

<details>
<summary>🔐 <strong>Authentication System</strong></summary>

| Registration | Login |
|-------------|--------|
| ![Register](docs/screenshots/register.png) | ![Login](docs/screenshots/login.png) |
*Secure user registration and login with modern UI design*

</details>

<details>
<summary>ℹ️ <strong>About & Information</strong></summary>

![About](docs/screenshots/about.png)
*Comprehensive overview of our mission and services*

</details>

<details>
<summary>🧮 <strong>Interactive Trip Calculator</strong></summary>

![Calculator](docs/screenshots/calculator.png)
*Real-time cost estimation with interactive map integration*

</details>

<details>
<summary>🏞️ <strong>Destination Browsing</strong></summary>

![Destinations](docs/screenshots/destinations.png)
*Explore 35+ curated destinations with advanced filtering*

</details>

<details>
<summary>📋 <strong>Reservation System</strong></summary>

| Reservation Form | PDF Export |
|------------------|------------|
| ![Reservation](docs/screenshots/reservation.png) | ![PDF](docs/screenshots/pdf.png) |
*Complete booking system with downloadable confirmations*

</details>

<details>
<summary>🛠️ <strong>Admin Dashboard</strong></summary>

### Location Management
| Overview | Edit | Delete |
|----------|------|--------|
| ![Admin Location](docs/screenshots/admin-location.png) | ![Edit Location](docs/screenshots/edit-location.png) | ![Delete Location](docs/screenshots/delete-location.png) |

### Destination Management
| Overview | Edit | View | Delete |
|----------|------|------|--------|
| ![Admin Destination](docs/screenshots/admin-destination.png) | ![Edit Destination](docs/screenshots/edit-destination.png) | ![View Destination](docs/screenshots/view-destination.png) | ![Delete Destination](docs/screenshots/delete-destination.png) |

### Reservation Management
![Admin Reservation](docs/screenshots/admin-reservation.png)
*Comprehensive admin panel for managing all aspects of the platform*

</details>

---

## 📁 **Project Architecture**

```
🏗️ GuideTuristike/
├── 🎮 Controllers/          # MVC Controllers (User & Admin)
│   ├── HomeController.cs
│   ├── AccountController.cs
│   ├── DestinationController.cs
│   └── AdminController.cs
├── 📊 Models/               # Entity Framework Models & ViewModels
│   ├── Entities/
│   ├── ViewModels/
│   └── ApplicationDbContext.cs
├── 🎨 Views/                # Razor Views & Layouts
│   ├── Shared/
│   ├── Home/
│   ├── Account/
│   └── Admin/
├── 💎 Content/              # Static Assets (CSS, Images)
│   ├── css/
│   ├── images/
│   └── themes/
├── ⚡ Scripts/              # Client-side JavaScript
│   ├── app/
│   ├── map/
│   └── vendor/
├── 🔧 Utilities/            # Custom Business Logic
│   ├── Calculators/
│   ├── Helpers/
│   └── Extensions/
├── 📦 Migrations/           # EF Code-First Migrations
├── 📄 Rotativa/             # PDF Generation Binaries
└── 🐳 Dockerfile            # Container Configuration
```

---

## 🤝 **Contributing**

<div align="center">

### We ❤️ Community Contributions!

</div>

**How to contribute:**

1. 🍴 **Fork** the repository
2. 🌿 **Create** a feature branch (`git checkout -b feature/AmazingFeature`)
3. 💾 **Commit** your changes (`git commit -m 'Add some AmazingFeature'`)
4. 🚀 **Push** to the branch (`git push origin feature/AmazingFeature`)
5. 📝 **Open** a Pull Request

### **Contribution Guidelines:**
- ✅ Follow existing code style and conventions
- ✅ Write clear, concise commit messages
- ✅ Add tests for new functionality
- ✅ Update documentation as needed
- ✅ Ensure all tests pass before submitting

---

## 📄 **License**

<div align="center">

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](https://opensource.org/licenses/MIT)

</div>

---

## 🙏 **Acknowledgements**

<div align="center">

**Special thanks to these amazing open-source projects:**

[![Leaflet](https://img.shields.io/badge/Leaflet-199900?style=for-the-badge&logo=leaflet&logoColor=white)](https://leafletjs.com/)
[![OpenStreetMap](https://img.shields.io/badge/OpenStreetMap-7EBC6F?style=for-the-badge&logo=openstreetmap&logoColor=white)](https://www.openstreetmap.org/)
[![Rotativa](https://img.shields.io/badge/Rotativa-PDF%20Generation-FF6B6B?style=for-the-badge)](https://github.com/webgio/Rotativa)

</div>

---

<div align="center">

### 🎯 **Ready to Explore Albania?**

**[🚀 Get Started](https://github.com/enzoindabenzo/GuideTuristike/fork)** • **[📖 Documentation](https://github.com/enzoindabenzo/GuideTuristike/wiki)** • **[🐛 Report Issues](https://github.com/enzoindabenzo/GuideTuristike/issues)**

---

*Crafted with 💙 by **Enzo Indabenzó** and the open-source community*

**⭐ Don't forget to star this repository if you found it helpful!**

</div>
