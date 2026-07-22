// ================================
// Contact Page
// ================================

document.addEventListener("DOMContentLoaded", () => {

    // ============================
    // Fade In Animation
    // ============================

    const animatedElements = document.querySelectorAll(
        ".contact-card, .map-card, .social-section, .hero-content"
    );

    const observer = new IntersectionObserver((entries) => {

        entries.forEach(entry => {

            if (entry.isIntersecting) {

                entry.target.classList.add("show");

            }

        });

    }, {
        threshold: .15
    });

    animatedElements.forEach(el => {

        el.classList.add("fade-start");

        observer.observe(el);

    });

    // ============================
    // Hover Tilt Effect
    // ============================

    const cards = document.querySelectorAll(".contact-card");

    cards.forEach(card => {

        card.addEventListener("mousemove", (e) => {

            const rect = card.getBoundingClientRect();

            const x = e.clientX - rect.left;

            const y = e.clientY - rect.top;

            const rotateY = ((x / rect.width) - 0.5) * 8;

            const rotateX = ((y / rect.height) - 0.5) * -8;

            card.style.transform =
                `perspective(800px)
                 rotateX(${rotateX}deg)
                 rotateY(${rotateY}deg)
                 translateY(-5px)`;

        });

        card.addEventListener("mouseleave", () => {

            card.style.transform =
                "perspective(800px) rotateX(0) rotateY(0)";

        });

    });

    // ============================
    // Buttons Ripple Effect
    // ============================

    document.querySelectorAll(".btn").forEach(btn => {

        btn.addEventListener("click", function (e) {

            const circle = document.createElement("span");

            const d = Math.max(this.clientWidth, this.clientHeight);

            circle.style.width = circle.style.height = d + "px";

            const rect = this.getBoundingClientRect();

            circle.style.left = (e.clientX - rect.left - d / 2) + "px";

            circle.style.top = (e.clientY - rect.top - d / 2) + "px";

            circle.className = "ripple";

            this.appendChild(circle);

            setTimeout(() => {

                circle.remove();

            }, 600);

        });

    });

});