let selectedSizes = [];

document.addEventListener("DOMContentLoaded", function () {
    const input = document.getElementById("sizesInput");

    if (input && input.value.trim() !== "") {
        selectedSizes = input.value
            .split(",")
            .map(size => size.trim())
            .filter(size => size.length > 0);

        renderSizes();
    }
});

function toggleSize(size) {
    const index = selectedSizes.indexOf(size);

    if (index === -1) {
        selectedSizes.push(size);
    } else {
        selectedSizes.splice(index, 1);
    }

    renderSizes();
}

function removeSize(size) {
    selectedSizes = selectedSizes.filter(s => s !== size);
    renderSizes();
}

function renderSizes() {
    const container = document.getElementById("selectedSizes");
    const input = document.getElementById("sizesInput");

    if (!container || !input) return;

    container.innerHTML = "";

    selectedSizes.forEach(size => {
        const item = document.createElement("span");
        item.className = "selected-size";
        item.innerHTML = `${size} <b onclick="removeSize('${size}')">×</b>`;
        container.appendChild(item);
    });

    input.value = selectedSizes.join(", ");
}