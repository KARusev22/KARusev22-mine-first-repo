document.addEventListener("DOMContentLoaded", () => {

    document.querySelectorAll(".orders-scroll-wrapper").forEach(wrapper => {

        const scrollContainer = wrapper.querySelector(".orders-scroll");
        const leftBtn = wrapper.querySelector(".left-btn");
        const rightBtn = wrapper.querySelector(".right-btn");

        leftBtn.addEventListener("click", () => {
            scrollContainer.scrollBy({ left: -300, behavior: "smooth" });
        });

        rightBtn.addEventListener("click", () => {
            scrollContainer.scrollBy({ left: 300, behavior: "smooth" });
        });

    });

});