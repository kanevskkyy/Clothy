import {Swiper, SwiperSlide} from "swiper/react";
import {Autoplay, FreeMode} from "swiper/modules";
import "swiper/swiper.css";
import styles from './BrandsCarousel.module.css';
import {useEffect} from "react";
import {catalogApi} from "../../../app/api/catalogApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import {useQuery} from "@tanstack/react-query";

const SKELETON_WIDTHS = [88, 120, 72, 104, 96, 80, 112, 92];

const BrandsCarousel = () => {
    const {data: brands = [], isLoading, error} = useQuery({
        queryKey: ["brands"],
        queryFn: () => catalogApi.getAllBrandsAsync(),
    });

    useEffect(() => {
        if (error) toast.error(getErrorMessage(error));
    }, [error]);

    if (isLoading) {
        return (
            <div className={styles.carouselContainer}>
                <div className={styles.skeletonRow}>
                    {SKELETON_WIDTHS.map((w, i) => (
                        <div
                            key={i}
                            className={styles.skeletonItem}
                            style={{width: w}}
                        />
                    ))}
                </div>
            </div>
        );
    }

    const duplicatedBrands = [...brands, ...brands];

    return (
        <div className={styles.carouselContainer}>
            <Swiper
                modules={[Autoplay, FreeMode]}
                slidesPerView="auto"
                spaceBetween={44}
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
                {duplicatedBrands.map((brand, index) => (
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