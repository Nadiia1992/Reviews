// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const reviews = document.querySelectorAll('.review');
const prevButton = document.getElementById('prevButton');
const nextButton = document.getElementById('nextButton');

if (reviews.length > 0 && prevButton && nextButton) {

    let page = 1;
    const pageSize = 10;

    function showReviews() {
        const start = (page - 1) * pageSize;
        const end = start + pageSize;

        reviews.forEach((review, index) => {
            review.style.display =
                index >= start && index < end ? 'block' : 'none';
        });

        prevButton.disabled = page === 1;
        nextButton.disabled = end >= reviews.length;
    }

    prevButton.addEventListener('click', () => {
        page--;
        showReviews();
    });

    nextButton.addEventListener('click', () => {
        page++;
        showReviews();
    });

    showReviews();
}
