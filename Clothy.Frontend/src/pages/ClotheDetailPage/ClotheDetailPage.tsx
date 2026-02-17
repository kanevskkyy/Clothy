import type { IClotheAggregatedDetailDTO } from '../../entities/catalogService/clotheItem/IClotheAggregatedDetailDTO.ts';
import styles from "./ClotheDetailPage.module.css";
import ImageGallery from "../../features/clothe/imageGallery/ImageGallery.tsx";
import ClotheDetail from '../../entities/catalogService/clotheItem/clotheInfo/ClotheDetail.tsx';
import ReviewsSection from "../../entities/reviewsService/reviews/reviewSection/ReviewsSection.tsx";
import {useEffect, useMemo, useState} from "react";
import {useNavigate, useParams} from "react-router-dom";
import { Helmet } from 'react-helmet';
import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";
import {catalogApi} from "../../app/api/catalogApi.ts";
import Loader from "../../shared/Loader/Loader.tsx";
import {toast} from "sonner";
import {getErrorMessage} from "../../shared/utils/errorHandler.ts";

const ClotheDetailPage = () => {
    const { slug, colorSlug } = useParams<{ slug: string; colorSlug: string }>();
    const navigate = useNavigate();

    const [clotheItem, setClotheItem] = useState<IClotheAggregatedDetailDTO | null>(null);
    const [loading, setLoading] = useState<boolean>(true);

    useEffect(() => {
        const fetchClotheDetails = async () => {
            try{
                const response = await catalogApi.getClotheBySlugAsync(slug ?? "");
                setClotheItem(response);
            }
            catch (error) {
                toast.error(getErrorMessage(error));
            }
            finally {
                setLoading(false);
            }
        }

        fetchClotheDetails();
    }, [slug]);

    const uniqueColors = useMemo(() => {
        if (!clotheItem) return [];
        const colorMap = new Map();
        clotheItem.clotheDetailDTO.stocks.forEach(stock => {
            if (!colorMap.has(stock.color.id)) {
                colorMap.set(stock.color.id, stock.color);
            }
        });
        return Array.from(colorMap.values());
    }, [clotheItem]);

    const initialColor = useMemo(() => {
        if (uniqueColors.length === 0) return null;
        const colorFromUrl = uniqueColors.find(c => c.slug === colorSlug);
        return colorFromUrl || uniqueColors[0];
    }, [uniqueColors, colorSlug]);

    const [selectedColor, setSelectedColor] = useState(initialColor);

    useEffect(() => {
        if (initialColor) {
            setSelectedColor(initialColor);
        }
    }, [initialColor]);

    useEffect(() => {
        if (!initialColor) return;
        const colorFromUrl = uniqueColors.find(c => c.slug === colorSlug);
        if (colorFromUrl && selectedColor && colorFromUrl.id !== selectedColor.id) {
            setSelectedColor(colorFromUrl);
        }
    }, [colorSlug, uniqueColors, initialColor, selectedColor]);

    if (loading || !clotheItem || !selectedColor) {
        return <Loader marginTop="75px" />
    }

    const handleColorChange = (color: typeof selectedColor) => {
        setSelectedColor(color);
        navigate(`/clothe/${slug}/${color.slug}`, { replace: true });
    };

    const pageTitle = `${clotheItem.clotheDetailDTO.name} — Clothy`;
    const pageDescription = clotheItem.clotheDetailDTO.description;

    return (
        <PageWrapper>
            <div className={styles.pageWrapper}>
                <Helmet>
                    <title>{pageTitle}</title>
                    <meta name="description" content={pageDescription} />
                </Helmet>

                <div className={styles.container}>
                    <ImageGallery
                        additionalPhotos={clotheItem.clotheDetailDTO.additionalPhotos}
                        selectedColor={selectedColor}
                    />
                    <ClotheDetail
                        clotheDetail={clotheItem.clotheDetailDTO}
                        selectedColor={selectedColor}
                        onColorChange={handleColorChange}
                    />
                </div>
                <ReviewsSection
                    clotheId={clotheItem.clotheDetailDTO.id}
                    initialReviews={clotheItem.reviews}
                    statistics={clotheItem.statistics}
                    initialQuestions={clotheItem.questions}
                />
            </div>
        </PageWrapper>
    );
};

export default ClotheDetailPage;