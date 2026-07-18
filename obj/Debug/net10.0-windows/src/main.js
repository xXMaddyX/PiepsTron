/* =============================================
   NXTLVL LOUNGE - Modern Animated JavaScript
   ============================================= */

document.addEventListener('DOMContentLoaded', () => {
    // Initialize all modules
    initLoader();
    initNavigation();
    initParticles();
    initScrollReveal();
    initCursorGlow();
    initBackToTop();
    initSmoothScroll();
    initParallax();
});

/* =============================================
   LOADING SCREEN
   ============================================= */
function initLoader() {
    const loader = document.getElementById('loader');
    
    // Hide loader after content loads
    window.addEventListener('load', () => {
        setTimeout(() => {
            loader.classList.add('hidden');
            document.body.style.overflow = 'auto';
        }, 800);
    });

    // Fallback: Hide loader after 3 seconds max
    setTimeout(() => {
        loader.classList.add('hidden');
        document.body.style.overflow = 'auto';
    }, 3000);
}

/* =============================================
   MOBILE NAVIGATION
   ============================================= */
function initNavigation() {
    const menuToggle = document.getElementById('menuToggle');
    const navLinks = document.getElementById('navLinks');
    const navItems = navLinks.querySelectorAll('a');

    // Toggle mobile menu
    menuToggle.addEventListener('click', () => {
        menuToggle.classList.toggle('active');
        navLinks.classList.toggle('active');
        document.body.style.overflow = navLinks.classList.contains('active') ? 'hidden' : 'auto';
    });

    // Close menu when link is clicked
    navItems.forEach(item => {
        item.addEventListener('click', () => {
            menuToggle.classList.remove('active');
            navLinks.classList.remove('active');
            document.body.style.overflow = 'auto';
        });
    });

    // Close menu when clicking outside
    document.addEventListener('click', (e) => {
        if (!navLinks.contains(e.target) && !menuToggle.contains(e.target)) {
            menuToggle.classList.remove('active');
            navLinks.classList.remove('active');
            document.body.style.overflow = 'auto';
        }
    });

    // Add active state to nav links based on scroll position
    const sections = document.querySelectorAll('section[id]');
    
    function highlightNav() {
        const scrollY = window.pageYOffset;

        sections.forEach(section => {
            const sectionHeight = section.offsetHeight;
            const sectionTop = section.offsetTop - 150;
            const sectionId = section.getAttribute('id');
            const navLink = document.querySelector(`.nav-links a[href="#${sectionId}"]`);

            if (navLink) {
                if (scrollY > sectionTop && scrollY <= sectionTop + sectionHeight) {
                    navLink.classList.add('active');
                } else {
                    navLink.classList.remove('active');
                }
            }
        });
    }

    window.addEventListener('scroll', highlightNav);
}

/* =============================================
   FLOATING PARTICLES
   ============================================= */
function initParticles() {
    const particlesContainer = document.getElementById('particles');
    const particleCount = window.innerWidth < 768 ? 15 : 30;

    for (let i = 0; i < particleCount; i++) {
        createParticle(particlesContainer, i);
    }
}

function createParticle(container, index) {
    const particle = document.createElement('div');
    particle.classList.add('particle');
    
    // Random positioning
    particle.style.left = `${Math.random() * 100}%`;
    particle.style.animationDelay = `${Math.random() * 15}s`;
    particle.style.animationDuration = `${15 + Math.random() * 10}s`;
    
    // Random sizes
    const size = 2 + Math.random() * 4;
    particle.style.width = `${size}px`;
    particle.style.height = `${size}px`;
    
    container.appendChild(particle);
}

/* =============================================
   SCROLL REVEAL ANIMATIONS
   ============================================= */
function initScrollReveal() {
    const revealElements = document.querySelectorAll('.reveal');
    
    function checkReveal() {
        const windowHeight = window.innerHeight;
        const revealPoint = 100;

        revealElements.forEach(element => {
            const elementTop = element.getBoundingClientRect().top;
            
            if (elementTop < windowHeight - revealPoint) {
                element.classList.add('active');
            }
        });
    }

    // Initial check
    checkReveal();
    
    // Check on scroll with throttling
    let ticking = false;
    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                checkReveal();
                ticking = false;
            });
            ticking = true;
        }
    });
}

/* =============================================
   CURSOR GLOW EFFECT (Desktop)
   ============================================= */
function initCursorGlow() {
    const cursorGlow = document.getElementById('cursorGlow');
    
    // Only on desktop with hover capability
    if (window.matchMedia('(hover: hover)').matches && window.innerWidth >= 992) {
        let mouseX = 0;
        let mouseY = 0;
        let currentX = 0;
        let currentY = 0;

        document.addEventListener('mousemove', (e) => {
            mouseX = e.clientX;
            mouseY = e.clientY;
        });

        // Smooth cursor following
        function animateCursor() {
            const dx = mouseX - currentX;
            const dy = mouseY - currentY;
            
            currentX += dx * 0.1;
            currentY += dy * 0.1;
            
            cursorGlow.style.left = `${currentX}px`;
            cursorGlow.style.top = `${currentY}px`;
            
            requestAnimationFrame(animateCursor);
        }

        animateCursor();

        // Show/hide on enter/leave
        document.addEventListener('mouseenter', () => {
            cursorGlow.style.opacity = '1';
        });

        document.addEventListener('mouseleave', () => {
            cursorGlow.style.opacity = '0';
        });
    }
}

/* =============================================
   BACK TO TOP BUTTON
   ============================================= */
function initBackToTop() {
    const backToTop = document.getElementById('backToTop');
    
    window.addEventListener('scroll', () => {
        if (window.pageYOffset > 500) {
            backToTop.classList.add('visible');
        } else {
            backToTop.classList.remove('visible');
        }
    });

    backToTop.addEventListener('click', () => {
        window.scrollTo({
            top: 0,
            behavior: 'smooth'
        });
    });
}

/* =============================================
   SMOOTH SCROLLING
   ============================================= */
function initSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function(e) {
            e.preventDefault();
            const targetId = this.getAttribute('href');
            const target = document.querySelector(targetId);
            
            if (target) {
                const headerOffset = 80;
                const elementPosition = target.getBoundingClientRect().top;
                const offsetPosition = elementPosition + window.pageYOffset - headerOffset;

                window.scrollTo({
                    top: offsetPosition,
                    behavior: 'smooth'
                });
            }
        });
    });
}

/* =============================================
   PARALLAX EFFECTS
   ============================================= */
function initParallax() {
    const hero = document.querySelector('.hero');
    const heroBg = document.querySelector('.hero-bg');
    
    let ticking = false;

    window.addEventListener('scroll', () => {
        if (!ticking) {
            window.requestAnimationFrame(() => {
                const scrolled = window.pageYOffset;
                
                // Only apply parallax in hero section
                if (scrolled < window.innerHeight) {
                    if (heroBg) {
                        heroBg.style.transform = `translateY(${scrolled * 0.3}px)`;
                    }
                }
                
                ticking = false;
            });
            ticking = true;
        }
    });
}

/* =============================================
   CARD TILT EFFECT (Desktop)
   ============================================= */
function initCardTilt() {
    if (window.matchMedia('(hover: hover)').matches && window.innerWidth >= 992) {
        const cards = document.querySelectorAll('.gaming-card, .feature-card');
        
        cards.forEach(card => {
            card.addEventListener('mousemove', (e) => {
                const rect = card.getBoundingClientRect();
                const x = e.clientX - rect.left;
                const y = e.clientY - rect.top;
                
                const centerX = rect.width / 2;
                const centerY = rect.height / 2;
                
                const rotateX = (y - centerY) / 10;
                const rotateY = (centerX - x) / 10;
                
                card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-10px)`;
            });
            
            card.addEventListener('mouseleave', () => {
                card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) translateY(0)';
            });
        });
    }
}

// Initialize card tilt after reveal animations
setTimeout(initCardTilt, 1000);

/* =============================================
   INTERSECTION OBSERVER FOR PERFORMANCE
   ============================================= */
if ('IntersectionObserver' in window) {
    const lazyElements = document.querySelectorAll('[data-lazy]');
    
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('loaded');
                observer.unobserve(entry.target);
            }
        });
    }, {
        rootMargin: '50px'
    });
    
    lazyElements.forEach(el => observer.observe(el));
}

/* =============================================
   DYNAMIC NAV BACKGROUND
   ============================================= */
(function() {
    const nav = document.querySelector('nav');
    
    window.addEventListener('scroll', () => {
        if (window.pageYOffset > 50) {
            nav.style.background = 'rgba(10, 10, 10, 0.98)';
            nav.style.borderBottomColor = 'rgba(0, 212, 255, 0.4)';
        } else {
            nav.style.background = 'rgba(10, 10, 10, 0.9)';
            nav.style.borderBottomColor = 'rgba(0, 212, 255, 0.2)';
        }
    });
})();

/* =============================================
   HOVER SOUND EFFECT (Optional)
   ============================================= */
function initHoverSounds() {
    // Uncomment to enable subtle hover sounds
    /*
    const buttons = document.querySelectorAll('.cta-button, .gaming-card, .feature-card');
    const hoverSound = new Audio('data:audio/wav;base64,UklGRnoGAABXQVZFZm10...');
    hoverSound.volume = 0.1;
    
    buttons.forEach(btn => {
        btn.addEventListener('mouseenter', () => {
            hoverSound.currentTime = 0;
            hoverSound.play().catch(() => {});
        });
    });
    */
}
