let prices = {};
let storedPartIdForDetails = null;

function navigateToDetails(itemId, partId) {
	storedPartIdForDetails = partId;
	window.location.href = `/PC/Details/${itemId}?fromBuild=true&partId=${partId}`;
}

//Chọn sản phẩm
function selectProduct(category, productId, productName, imageUrl, price) {
	// Hiển thị phần chi tiết sản phẩm
	document.getElementById("details-" + category).style.display = "flex";

	// Ẩn nút "+ Chọn"
	document.getElementById("selectButton-" + category).style.display = "none";

	// Cập nhật thông tin sản phẩm
	document.getElementById("Selected" + category + "Id").value = productId;
	document.getElementById("Selected" + category + "Name").innerText = productName;
	document.getElementById("Selected" + category + "Price").innerText = price + " VNĐ";

	// Cập nhật hình ảnh
	const imgDiv = document.getElementById("Selected" + category + "Image");
	imgDiv.innerHTML = '<img src="' + imageUrl + '" class="product-image" alt="' + productName + '" />';

	// Đóng modal
	closeModal("modal" + category);

	// Cập nhật giá và số lượng
	prices[category] = parseInt(price.replace(/\D/g, ""));
	document.getElementById("quantity-" + category).value = 1;
	updateTotal(category);
}

function editProduct(category) {
	// Xóa tất cả backdrop trước khi mở modal mới
	const modalBackdrops = document.getElementsByClassName("modal-backdrop");
	while (modalBackdrops.length > 0) {
		modalBackdrops[0].parentNode.removeChild(modalBackdrops[0]);
	}

	// Xóa class modal-open khỏi body
	document.body.classList.remove("modal-open");

	// Mở lại modal để chỉnh sửa
	const modal = document.getElementById("modal" + category);
	if (modal) {
		$(modal).modal("show");
		initializePagination(category); // <-- thêm dòng này
	}
}

function closeModal(modalId) {
	const modal = document.getElementById(modalId);
	if (modal) {
		// Sử dụng Bootstrap để đóng modal
		$(modal).modal("hide");

		// Đóng thủ công để đảm bảo không còn trạng thái dư thừa
		modal.classList.remove("show");
		modal.setAttribute("aria-hidden", "true");
		modal.style.display = "none";
		document.body.classList.remove("modal-open");

		// Xóa tất cả backdrop
		const modalBackdrops = document.getElementsByClassName("modal-backdrop");
		while (modalBackdrops.length > 0) {
			modalBackdrops[0].parentNode.removeChild(modalBackdrops[0]);
		}
	}
}

function resetForm() {
	// Danh sách các danh mục
	const categories = [
		"CPU", "Mainboard", "RAM", "SSD", "HDD",
		"VGA", "PSU", "Case", "AirCooler", "LiquidCooler"
	];

	// Lặp qua từng danh mục để làm mới
	categories.forEach(category => {
		// Ẩn phần chi tiết sản phẩm
		const details = document.getElementById("details-" + category);
		if (details) {
			details.style.display = "none";
		}

		// Hiển thị lại nút "+ Chọn"
		const selectButton = document.getElementById("selectButton-" + category);
		if (selectButton) {
			selectButton.style.display = "block";
		}

		// Xóa thông tin sản phẩm
		const selectedId = document.getElementById("Selected" + category + "Id");
		const selectedName = document.getElementById("Selected" + category + "Name");
		const selectedPrice = document.getElementById("Selected" + category + "Price");
		const selectedImage = document.getElementById("Selected" + category + "Image");
		const quantity = document.getElementById("quantity-" + category);
		const total = document.getElementById("total-" + category);

		if (selectedId) selectedId.value = "";
		if (selectedName) selectedName.innerText = "";
		if (selectedPrice) selectedPrice.innerText = "";
		if (selectedImage) selectedImage.innerHTML = '<span class="text-muted small">Ảnh</span>';
		if (quantity) quantity.value = 0;
		if (total) total.innerText = "";

		// Xóa giá trong prices
		delete prices[category];
	});

	// Cập nhật lại tổng giá
	updateGrandTotal();
}

function changeQuantity(category, change) {
	const quantityInput = document.getElementById("quantity-" + category);
	let quantity = parseInt(quantityInput.value) + change;
	if (quantity < 0) quantity = 0;
	quantityInput.value = quantity;
	updateTotal(category);
}

function updateTotal(category) {
	const quantity = parseInt(document.getElementById("quantity-" + category).value);
	const total = prices[category] ? (prices[category] * quantity).toLocaleString() : 0;
	document.getElementById("total-" + category).innerText = "= " + total + " ₫";
	updateGrandTotal();
}

function removeProduct(category) {
	// Ẩn phần chi tiết sản phẩm
	document.getElementById("details-" + category).style.display = "none";

	// Hiển thị lại nút "+ Chọn"
	document.getElementById("selectButton-" + category).style.display = "block";

	// Xóa thông tin sản phẩm
	document.getElementById("Selected" + category + "Id").value = "";
	document.getElementById("Selected" + category + "Name").innerText = "";
	document.getElementById("Selected" + category + "Price").innerText = "";
	document.getElementById("Selected" + category + "Image").innerHTML = '<span class="text-muted small">Ảnh</span>';
	document.getElementById("quantity-" + category).value = 0;
	document.getElementById("total-" + category).innerText = "";
	delete prices[category];
	updateGrandTotal();
}

function updateGrandTotal() {
	let grandTotal = 0;
	for (let category in prices) {
		const quantity = parseInt(document.getElementById("quantity-" + category).value);
		grandTotal += prices[category] * quantity;
	}
	document.querySelector(".total-price").innerText = "Chi phí dự tính: " + grandTotal.toLocaleString() + " ₫";
}

// Tìm kiếm sản phẩm theo tên
function searchProducts(category) {
	const searchInput = document.getElementById("search-" + category).value.toLowerCase();
	const productList = document.getElementById("product-list-" + category);
	const products = productList.getElementsByClassName("product-item");

	for (let i = 0; i < products.length; i++) {
		const productName = products[i].getAttribute("data-name");
		if (productName.includes(searchInput)) {
			products[i].style.display = "block";
		} else {
			products[i].style.display = "none";
		}
	}
}

// Sắp xếp sản phẩm theo tên hoặc giá
// Sắp xếp sản phẩm theo lựa chọn từ dropdown
function sortProducts(category) {
	const sortValue = document.getElementById("sort-" + category).value;
	const productList = document.getElementById("product-list-" + category);
	const products = Array.from(productList.getElementsByClassName("product-item"));

	products.sort((a, b) => {
		if (sortValue === "name-asc") {
			const nameA = a.getAttribute("data-name");
			const nameB = b.getAttribute("data-name");
			return nameA.localeCompare(nameB);
		} else if (sortValue === "name-desc") {
			const nameA = a.getAttribute("data-name");
			const nameB = b.getAttribute("data-name");
			return nameB.localeCompare(nameA);
		} else if (sortValue === "price-asc") {
			const priceA = parseFloat(a.getAttribute("data-price"));
			const priceB = parseFloat(b.getAttribute("data-price"));
			return priceA - priceB;
		} else if (sortValue === "price-desc") {
			const priceA = parseFloat(a.getAttribute("data-price"));
			const priceB = parseFloat(b.getAttribute("data-price"));
			return priceB - priceA;
		}
		return 0;
	});

	// Xóa danh sách hiện tại và thêm lại danh sách đã sắp xếp
	while (productList.firstChild) {
		productList.removeChild(productList.firstChild);
	}
	products.forEach(product => productList.appendChild(product));
}

// Xuất dữ liệu ra Excel
function exportToExcel() {
	// Tiêu đề
	const header = [
		["LV Computer - Máy tính trả góp Long An"],
		["Xây dựng cấu hình PC"],
		[] // Dòng trống để phân cách
	];

	// Danh sách các danh mục
	const categories = [
		"CPU", "Mainboard", "RAM", "SSD", "HDD",
		"VGA", "PSU", "Case", "AirCooler", "LiquidCooler"
	];

	// Tạo mảng dữ liệu cho bảng chính
	const data = [
		["Loại sản phẩm", "Tên sản phẩm", "Số lượng", "Giá (VNĐ)", "Tổng giá (VNĐ)"]
	];

	let hasData = false;

	// Lặp qua từng danh mục để lấy thông tin
	categories.forEach(category => {
		const selectedName = document.getElementById("Selected" + category + "Name").innerText;
		const quantity = parseInt(document.getElementById("quantity-" + category).value);
		const priceText = document.getElementById("Selected" + category + "Price").innerText;
		const totalText = document.getElementById("total-" + category).innerText;

		if (selectedName && quantity > 0) {
			hasData = true;
			const price = parseInt(priceText.replace(/\D/g, "")) || 0;
			const total = parseInt(totalText.replace(/\D/g, "")) || 0;

			// Định dạng giá và tổng giá
			const formattedPrice = price.toLocaleString('vi-VN');
			const formattedTotal = total.toLocaleString('vi-VN');

			data.push([
				category,              // Loại sản phẩm
				selectedName,          // Tên sản phẩm
				quantity,              // Số lượng
				price,                 // Giá (giữ nguyên số để áp dụng định dạng trong Excel)
				total                  // Tổng giá (giữ nguyên số để áp dụng định dạng trong Excel)
			]);
		}
	});

	// Thêm dòng tổng chi phí dự tính
	const grandTotalText = document.querySelector(".total-price").innerText;
	const grandTotal = parseInt(grandTotalText.replace(/\D/g, "")) || 0;
	data.push(["", "", "", "Tổng chi phí dự tính", grandTotal]);

	// Nếu không có dữ liệu, thông báo và thoát
	if (!hasData) {
		alert("Chưa có sản phẩm nào được chọn để xuất Excel!");
		return;
	}

	// Kết hợp header và data
	const finalData = [...header, ...data];

	// Tạo worksheet từ dữ liệu
	const ws = XLSX.utils.aoa_to_sheet(finalData);

	// Áp dụng định dạng cho cột "Giá (VNĐ)" và "Tổng giá (VNĐ)"
	const range = XLSX.utils.decode_range(ws['!ref']);
	for (let row = range.s.r + header.length; row <= range.e.r; row++) { // Bắt đầu từ sau header
		// Cột "Giá (VNĐ)" (cột D, index 3)
		const priceCell = XLSX.utils.encode_cell({ r: row, c: 3 });
		if (ws[priceCell]) {
			ws[priceCell].z = '#,##0'; // Định dạng số với dấu phân cách hàng nghìn
		}

		// Cột "Tổng giá (VNĐ)" (cột E, index 4)
		const totalCell = XLSX.utils.encode_cell({ r: row, c: 4 });
		if (ws[totalCell]) {
			ws[totalCell].z = '#,##0'; // Định dạng số với dấu phân cách hàng nghìn
		}
	}

	// Tùy chỉnh độ rộng cột
	ws['!cols'] = [
		{ wch: 15 },  // Độ rộng cột "Loại sản phẩm"
		{ wch: 40 },  // Độ rộng cột "Tên sản phẩm"
		{ wch: 10 },  // Độ rộng cột "Số lượng"
		{ wch: 15 },  // Độ rộng cột "Giá"
		{ wch: 15 }   // Độ rộng cột "Tổng giá"
	];

	// Tạo workbook và thêm worksheet
	const wb = XLSX.utils.book_new();
	XLSX.utils.book_append_sheet(wb, ws, "PC Build");

	// Xuất file Excel
	XLSX.writeFile(wb, "PC_Build_" + new Date().toISOString().slice(0, 10) + ".xlsx");
}

window.addEventListener("beforeunload", () => {
	const categories = [
		"CPU", "Mainboard", "RAM", "SSD", "HDD",
		"VGA", "PSU", "Case", "AirCooler", "LiquidCooler"
	];

	const selections = {};

	categories.forEach(cat => {
		const id = document.getElementById("Selected" + cat + "Id")?.value;
		const name = document.getElementById("Selected" + cat + "Name")?.innerText;
		const price = document.getElementById("Selected" + cat + "Price")?.innerText;
		const quantity = document.getElementById("quantity-" + cat)?.value;
		const total = document.getElementById("total-" + cat)?.innerText;
		const image = document.getElementById("Selected" + cat + "Image")?.innerHTML;

		if (id) {
			selections[cat] = { id, name, price, quantity, total, image };
		}
	});

	localStorage.setItem("pcBuildSelections", JSON.stringify(selections));
});

window.addEventListener("load", () => {
	const data = localStorage.getItem("pcBuildSelections");
	if (data) {
		const selections = JSON.parse(data);
		for (let cat in selections) {
			const s = selections[cat];
			document.getElementById("details-" + cat).style.display = "flex";
			document.getElementById("selectButton-" + cat).style.display = "none";
			document.getElementById("Selected" + cat + "Id").value = s.id;
			document.getElementById("Selected" + cat + "Name").innerText = s.name;
			document.getElementById("Selected" + cat + "Price").innerText = s.price;
			document.getElementById("quantity-" + cat).value = s.quantity;
			document.getElementById("total-" + cat).innerText = s.total;
			document.getElementById("Selected" + cat + "Image").innerHTML = s.image;

			prices[cat] = parseInt(s.price.replace(/\D/g, ""));
		}
		updateGrandTotal();
	}
});

const ITEMS_PER_PAGE = 6;
let currentPage = {};

function showPage(category, page) {
	const products = document.querySelectorAll(`#product-list-${category} .product-item`);
	const totalPages = Math.ceil(products.length / ITEMS_PER_PAGE);
	if (page < 1 || page > totalPages) return;

	currentPage[category] = page;

	// Ẩn/hiện theo trang
	products.forEach((item, index) => {
		item.style.display = (index >= (page - 1) * ITEMS_PER_PAGE && index < page * ITEMS_PER_PAGE) ? "block" : "none";
	});

	// Tạo lại các nút phân trang
	const paginationContainer = document.getElementById(`pagination-${category}`);
	paginationContainer.innerHTML = "";

	const createPageItem = (label, active, disabled, onClick) => {
		const li = document.createElement("li");
		li.className = "page-item" + (active ? " active" : "") + (disabled ? " disabled" : "");
		const a = document.createElement("a");
		a.className = "page-link";
		a.href = "javascript:void(0)";
		a.innerText = label;
		a.onclick = onClick;
		li.appendChild(a);
		return li;
	};

	// « nút trước
	paginationContainer.appendChild(createPageItem("«", false, page === 1, () => showPage(category, page - 1)));

	// Các trang
	for (let i = 1; i <= totalPages; i++) {
		paginationContainer.appendChild(createPageItem(i, i === page, false, () => showPage(category, i)));
	}

	// » nút sau
	paginationContainer.appendChild(createPageItem("»", false, page === totalPages, () => showPage(category, page + 1)));
}

// Tự động hiển thị trang đầu tiên khi mở modal
function initializePagination(category) {
	currentPage[category] = 1;
	showPage(category, 1);
}

function submitBuildToCart() {
	const selections = JSON.parse(localStorage.getItem("pcBuildSelections") || "{}");
	const cartItems = [];

	for (let key in selections) {
		const item = selections[key];
		if (item.id && parseInt(item.quantity) > 0) {
			cartItems.push({
				productId: parseInt(item.id),
				quantity: parseInt(item.quantity)
			});
		}
	}

	fetch("/Cart/AddBuildToCart", {
		method: "POST",
		headers: {
			"Content-Type": "application/json"
		},
		body: JSON.stringify(cartItems)
	}).then(res => {
		if (res.ok) {
			localStorage.removeItem("pcBuildSelections");
			window.location.href = "/Cart/Index";
		} else {
			alert("Lỗi khi thêm vào giỏ hàng");
		}
	});
}