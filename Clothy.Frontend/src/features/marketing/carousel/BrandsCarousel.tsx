import { Swiper, SwiperSlide } from "swiper/react";
import { Autoplay, FreeMode } from "swiper/modules";
import "swiper/swiper.css";
import styles from './BrandsCarousel.module.css';
import type {IBrandReadDTO} from "../../../entities/catalogService/brand/IBrandReadDTO.ts";
import {useEffect, useState} from "react";
import {catalogApi} from "../../../app/api/catalogApi.ts";
import Loader from "../../../shared/Loader/Loader.tsx";
import { toast } from "sonner";
import {getErrorMessage} from "../../../shared/utils/errorHandler.ts";

const BrandsCarousel = () => {
    const [loading, setLoading] = useState(true);
    const [brands, setBrands] = useState<IBrandReadDTO[]>([]);

    useEffect(() => {
        const fetchBrands = async () => {
            try {
                const response = await catalogApi.getAllBrandsAsync();
                setBrands(response);
            }
            catch (error) {
                toast.error(getErrorMessage(error));
            }
            finally {
                setLoading(false);
            }
        }

        fetchBrands();
    }, []);

    if(loading) {
        return <Loader />
    }


    const duplicatedBrands = [...brands, ...brands];

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
                {duplicatedBrands.map((brand, index) => (
                    <SwiperSlide key={`${brand.id}-${index}`} className={styles.slide}>
                        <div className={styles.brandItem}>
                            <div className={styles.logoBox}>
                                <img
                                    src={brand.photoURL}
                                    alt={`${brand.name} brand logo`}
                                    loading="lazy"
                                />
                            </div>
                            <span>{brand.name}</span>
                        </div>
                    </SwiperSlide>
                ))}
            </Swiper>
        </div>
    );
};

export default BrandsCarousel;