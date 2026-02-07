import { Swiper, SwiperSlide } from "swiper/react";
import { Autoplay, FreeMode } from "swiper/modules";
import "swiper/swiper.css";
import styles from './BrandsCarousel.module.css';
import type {IBrandReadDTO} from "../../entities/brand/interfaces/IBrandReadDTO.ts";

const brands: IBrandReadDTO[] = [
    { id: "550e8400-e29b-41d4-a716-446655440000", name: "Nike", slug: "nike", photoURL: "https://pngimg.com/d/nike_PNG18.png", createdAt: "", updatedAt: "" },
    { id: "f47ac10b-58cc-4372-a567-0e02b2c3d479", name: "Adidas", slug: "adidas", photoURL: "https://download.logo.wine/logo/Adidas/Adidas-Logo.wine.png", createdAt: "", updatedAt: "" },
    { id: "9b2c3a4e-6e5f-4d2f-8d3a-1c2b3e4f5a6b", name: "Puma", slug: "puma", photoURL: "https://e7.pngegg.com/pngimages/147/564/png-clipart-puma-logo-brand-adidas-sneakers-adidas-cat-like-mammal-carnivoran.png", createdAt: "", updatedAt: "" },
    { id: "3f2504e0-4f89-11d3-9a0c-0305e82c3301", name: "Reebok", slug: "reebok", photoURL: "https://download.logo.wine/logo/Reebok/Reebok-Logo.wine.png", createdAt: "", updatedAt: "" },
    { id: "1c4e28ba-2fa1-4f1c-bf4a-2f8e6d5a3b7c", name: "Levi's", slug: "levis", photoURL: "https://www.citypng.com/public/uploads/preview/hd-png-levis-logo-701751694707134u7bn8eyab1.png?v=2026010316", createdAt: "", updatedAt: "" },
    { id: "7f1d4c6e-9b3a-4e2d-8d5b-1a2c3e4f5d6e", name: "Ralph Lauren", slug: "ralph-lauren", photoURL: "https://e7.pngegg.com/pngimages/280/106/png-clipart-polo-ralph-lauren-logo-ralph-lauren-full-logo-icons-logos-emojis-shop-logos-thumbnail.png", createdAt: "", updatedAt: "" },
    { id: "2b4e6c8a-3d5f-4b2e-9f3a-4c5d6e7f8a9b", name: "Tommy Hilfiger", slug: "tommy-hilfiger", photoURL: "https://www.citypng.com/public/uploads/preview/hd-tommy-hilfiger-horizontal-logo-transparent-png-701751694773717btfzz4ex4c.png", createdAt: "", updatedAt: "" },
    { id: "8c1f4b2e-3d5f-4e2a-8d3b-1a2c3e4f5b6c", name: "Hugo Boss", slug: "hugo-boss", photoURL: "https://upload.wikimedia.org/wikipedia/commons/5/50/Hugo_Boss_Logo.svg", createdAt: "", updatedAt: "" },
    { id: "9d2e4c3f-4f5b-4a2d-8c3a-1b2c3e4f6d7e", name: "Calvin Klein", slug: "calvin-klein", photoURL: "https://upload.wikimedia.org/wikipedia/commons/3/34/Calvin_Klein_logo.svg", createdAt: "", updatedAt: "" },
    { id: "1a3b5c6d-7e8f-4a2b-9c3d-2e3f4g5h6i7j", name: "Under Armour", slug: "under-armour", photoURL: "https://upload.wikimedia.org/wikipedia/commons/2/2f/Under_armour_logo.svg", createdAt: "", updatedAt: "" },
    { id: "2b4c6d7e-8f9g-4a3b-9d2e-3f4g5h6i7j8k", name: "The North Face", slug: "the-north-face", photoURL: "https://upload.wikimedia.org/wikipedia/commons/3/33/The_North_Face_logo.svg", createdAt: "", updatedAt: "" },
    { id: "3c5d7e8f-9g0h-4b2c-8e3f-4g5h6i7j8k9l", name: "Columbia", slug: "columbia", photoURL: "https://upload.wikimedia.org/wikipedia/commons/4/46/Columbia_Sportswear_logo.svg", createdAt: "", updatedAt: "" },
    { id: "4d6e8f9g-0h1i-4c3d-9f4g-5h6i7j8k9l0m", name: "Jack & Jones", slug: "jack-jones", photoURL: "https://upload.wikimedia.org/wikipedia/commons/0/0b/Jack_%26_Jones_logo.svg", createdAt: "", updatedAt: "" },
    { id: "5e7f9g0h-1i2j-4d3e-8g5h-6i7j8k9l0m1n", name: "Guess", slug: "guess", photoURL: "https://upload.wikimedia.org/wikipedia/commons/f/f6/Guess_logo.svg", createdAt: "", updatedAt: "" },
];

const BrandsCarousel = () => {
    // #TODO: Implement API CALL

    return (
        <div className={styles.carouselContainer}>
            <Swiper
                modules={[Autoplay, FreeMode]}
                slidesPerView="auto"
                spaceBetween={50}
                loop={true}
                freeMode={true}
                speed={4000}
                autoplay={{
                    delay: 0,
                    disableOnInteraction: false,
                    pauseOnMouseEnter: false,
                    stopOnLastSlide: false,
                }}
                allowTouchMove={false}
                simulateTouch={false}
                touchStartPreventDefault={false}
                noSwiping={true}
                noSwipingClass="swiper-slide"
                className={styles.swiper}
            >
                {[...brands, ...brands, ...brands, ...brands].map((brand, index) => (
                    <SwiperSlide key={`${brand.id}-${index}`} className={styles.slide}>
                        <div className={styles.brandItem}>
                            <span>{brand.name}</span>
                        </div>
                    </SwiperSlide>
                ))}
            </Swiper>
        </div>
    );
};

export default BrandsCarousel;