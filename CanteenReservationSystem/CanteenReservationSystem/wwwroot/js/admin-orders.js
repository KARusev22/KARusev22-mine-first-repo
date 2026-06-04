document.addEventListener("DOMContentLoaded", () => {

    document.querySelectorAll(".btn").forEach(btn => {
        btn.addEventListener("click", function (e) {
            let ripple = document.createElement("span");
            ripple.classList.add("ripple");
            this.appendChild(ripple);

            let x = e.clientX - this.getBoundingClientRect().left;
            let y = e.clientY - this.getBoundingClientRect().top;

            ripple.style.left = `${x}px`;
            ripple.style.top = `${y}px`;

            setTimeout(() => ripple.remove(), 600);
        });
    });

});