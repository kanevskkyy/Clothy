import styles from "./ClotheDetailPage.module.css";
import ImageGallery from "../../../features/clothe/imageGallery/ImageGallery.tsx";
import ClotheDetail from '../../../entities/catalogService/clotheInfo/ClotheDetail.tsx';
import ReviewsSection from "../../../entities/reviewsService/reviewSection/ReviewsSection.tsx";
import {useMemo, useState} from "react";
import {useNavigate, useParams} from "react-router-dom";
import {Helmet} from 'react-helmet';
import {catalogApi} from "../../../app/api/catalogApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import ClotheDetailSkeleton from './skeleton/ClotheDetailSkeleton.tsx';
import {useQuery} from '@tanstack/react-query';
import type {IColorReadDTO} from "../../../entities/catalogService/interfaces/color/IColorReadDTO.ts";
import Container from "../../../shared/layout/Container/Container.tsx";


const ClotheDetailPage = () => {
    const {slug, colorSlug} = useParams<{ slug: string; colorSlug: string }>();
    const navigate = useNavigate();

    const {data: clotheItem, isLoading} = useQuery({
        queryKey: ["clothe", slug],
        queryFn: () => catalogApi.getClotheBySlugAggregatorAsync(slug ?? ""),
        throwOnError: (error) => {
            toast.error(getErrorMessage(error));
            return false;
        }
    });

    const uniqueColors = useMemo(() => {
        if (!clotheItem) return [];
        const colorMap = new Map<string, IColorReadDTO>();
        clotheItem.clotheDetailDTO.stocks.forEach((stock: { color: IColorReadDTO }) => {
            if (!colorMap.has(stock.color.id)) {
                colorMap.set(stock.color.id, stock.color);
            }
        });
        return Array.from(colorMap.values());
    }, [clotheItem]);

    const initialColor = useMemo(() => {
        if (!uniqueColors.length) return null;
        return uniqueColors.find(c => c.slug === colorSlug) ?? uniqueColors[0];
    }, [uniqueColors, colorSlug]);

    const [selectedColor, setSelectedColor] = useState<IColorReadDTO | null>(null);

    const activeColor = useMemo(() => {
        if (selectedColor && uniqueColors.find(c => c.id === selectedColor.id)) {
            return selectedColor;
        }
        return initialColor;
    }, [selectedColor, initialColor, uniqueColors]);

    const handleColorChange = (color: IColorReadDTO) => {
        setSelectedColor(color);
        navigate(`/clothe/${slug}/${color.slug}`, {replace: true});
    };

    if (isLoading) {
        return (
            <div className={styles.pageWrapper}>
                <ClotheDetailSkeleton/>
            </div>
        );
    }

    if (!clotheItem || !activeColor) return null;

    const pageTitle = `${clotheItem.clotheDetailDTO.name} — Clothy`;
    const pageDescription = clotheItem.clotheDetailDTO.description;

    return (
        <Container paddingY={20}>
            <div className={styles.pageWrapper}>
                <Helmet>
                    <title>{pageTitle}</title>
                    <meta name="description" content={pageDescription}/>
                </Helmet>

                <div className={styles.container}>
                    <ImageGallery
                        additionalPhotos={clotheItem.clotheDetailDTO.additionalPhotos}
                        selectedColor={activeColor}
                    />
                    <ClotheDetail
                        clotheDetail={clotheItem.clotheDetailDTO}
                        selectedColor={activeColor}
                        onColorChange={handleColorChange}
                    />
                </div>
                <ReviewsSection
                    slug={slug!}
                    clotheId={clotheItem.clotheDetailDTO.id}
                    initialReviews={clotheItem.reviews}
                    statistics={clotheItem.statistics}
                    initialQuestions={clotheItem.questions}
                />
            </div>
        </Container>
    );
};

export default ClotheDetailPage;