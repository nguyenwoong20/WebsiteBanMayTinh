## 🖥️ LV COMPUTER – E-Commerce Website Specification

### 1. 📌 Background and Objectives
**LV COMPUTER** is a store specializing in computer parts and custom-built PCs.  
To expand its business and reach more customers, a professional e-commerce website is required. This website will serve as an online storefront, allowing customers to conveniently browse, select, and purchase products, while providing efficient management tools for store staff.

---

### 2. 👥 Users and Information Management

#### • Customers (Users):
- **Stored Info:**  
  - Customer ID (auto-generated)  
  - Full Name  
  - Phone Number  
  - Email (used for login)  
  - Encrypted Password  
  - Multiple Shipping Addresses  
  - Order History  

- **Future Features:**  
  - Loyalty points system  
  - Discount codes or promotional campaigns  

#### • Admins:
- Employees with login credentials to access the admin panel.
- **Permissions:**  
  - Manage products, categories, orders, brands, and customer info (password is view-only restricted).  

---

### 3. 📦 Product and Service Management

#### • Products:
- Unique SKU for each item (e.g., CPU, RAM, VGA, etc.).
- Belongs to one or more **categories** (e.g., PC Components > CPU).
- Linked to a **brand** (e.g., Intel, AMD, ASUS).
- **Details include:**  
  - Name, Short/Full Description  
  - Technical Specs  
  - Main Image + Gallery  
  - Stock Quantity  
  - Status (In Stock, Out of Stock, Discontinued)  
  - Price (Original and Promotional)  

#### • Categories:
- Identified by **Category ID** and **Name**
- Support **nested structure** (Parent → Child), e.g., PC Components → CPU → Intel.

#### • Implied Services:
- Online consultation (via product info or contact forms)
- Delivery and warranty information
- Customer support and FAQs  

---

### 4. 🛒 Website User Flow

1. **Browsing Products:**  
   - Home page and category navigation  
   - Search bar and filters  
2. **Product Details:**  
   - Images, descriptions, specifications, stock, and pricing  
3. **Adding to Cart:**  
   - Choose quantity → “Add to Cart” → View cart summary  
4. **Cart Review:**  
   - Adjust quantity, remove items, view subtotal  
5. **Checkout Process:**  
   - **Login/Register** (or guest checkout if enabled)  
   - **Shipping Info:** Choose existing address or input new one  
   - **Shipping & Payment Methods:**  
     - COD, Bank Transfer, Online Gateway  
   - **Order Confirmation:**  
     - Review all info → Apply promo code → Place Order  
6. **Order Completion:**  
   - Success page with order code  
   - Confirmation email sent  
   - Order saved in system  

---

### 5. ⚙️ Admin Features

- **Login:** Secure access to the admin dashboard
- **Product Management:**  
  - Add, edit, delete, search, and filter products  
- **Category & Brand Management:**  
  - Create, update, remove categories/brands  
- **Order Management:**  
  - View all orders, update statuses (e.g., Pending → Completed)  
- **Customer Management:**  
  - View customer details and order history  

---

### 6. 🧰 Technology Stack

- **Backend:** ASP.NET Core (MVC or Razor Pages)  
- **ORM:** Entity Framework Core  
- **Database:** SQL Server  
- **Frontend:** HTML, CSS, JavaScript  
- **UI Libraries:** Bootstrap, jQuery (optional)

---

> 📌 *This README section outlines the project goals, features, and structure for the LV COMPUTER e-commerce system. Contributions and feedback are welcome!*
