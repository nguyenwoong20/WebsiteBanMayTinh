document.addEventListener("DOMContentLoaded", function () {
    const searchInput = document.getElementById("searchInput");
    const suggestionBox = document.createElement("div");
    suggestionBox.classList.add("suggestion-box");
    document.body.appendChild(suggestionBox);

    searchInput.addEventListener("input", function () {
        let searchTerm = searchInput.value.trim();
        if (searchTerm.length === 0) {
            suggestionBox.style.display = "none";
            return;
        }

        fetch(`/Home/SearchSuggestions?term=${encodeURIComponent(searchTerm)}`)
            .then(response => response.json())
            .then(data => {
                if (data.length === 0) {
                    suggestionBox.style.display = "none";
                    return;
                }

                suggestionBox.innerHTML = "";
                data.forEach(product => {
                    let item = document.createElement("div");
                    item.classList.add("suggestion-item");
                    item.innerHTML = `
                        <img src="${product.imageUrl}" alt="${product.name}" class="suggestion-img">
                        <div>
                            <p class="suggestion-name">${product.name}</p>
                            <p class="suggestion-price">${product.price.toLocaleString()}đ</p>
                        </div>
                    `;
                    item.onclick = () => {
                        window.location.href = `/Home/Details/${product.id}`;
                    };
                    suggestionBox.appendChild(item);
                });

                let rect = searchInput.getBoundingClientRect();
                suggestionBox.style.left = `${rect.left}px`;
                suggestionBox.style.top = `${rect.bottom + window.scrollY}px`;
                suggestionBox.style.width = `${rect.width}px`;
                suggestionBox.style.display = "block";
            })
            .catch(error => console.error("Error fetching search suggestions:", error));
    });

    document.addEventListener("click", function (event) {
        if (!searchInput.contains(event.target) && !suggestionBox.contains(event.target)) {
            suggestionBox.style.display = "none";
        }
    });
});
