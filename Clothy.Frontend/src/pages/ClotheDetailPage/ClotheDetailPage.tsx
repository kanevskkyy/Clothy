import type { IClotheAggregatedDetailDTO } from '../../entities/clotheItem/interfaces/IClotheAggregatedDetailDTO';
import styles from "./ClotheDetailPage.module.css";
import ImageGallery from "../../features/imageGallery/ImageGallery.tsx";
import ClotheDetail from '../../entities/clotheItem/clotheInfo/ClotheDetail.tsx';
import ReviewsSection from "../../entities/reviews/reviewSection/ReviewsSection.tsx";
import {useEffect, useMemo, useState} from "react";
import {useNavigate, useParams} from "react-router-dom";
import { Helmet } from 'react-helmet';
import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";

const ClotheDetailPage = () => {
    const { slug, colorSlug } = useParams<{ slug: string; colorSlug: string }>();
    const navigate = useNavigate();

    const mockData: IClotheAggregatedDetailDTO = {
        clotheDetailDTO: {
            id: "clothe-1",
            name: "Nike Air Max T-shirt",
            slug: "nike-air-max-tshirt",
            description: "The classic Nike Air Max T-shirt is made from high-quality cotton fabric. It provides maximum comfort when worn. Ideal for everyday use and exercise.",
            price: "1299",
            gender: "Male",
            oldPrice: "1599",
            hasOldPrice: true,
            discountPercentage: 19,
            hasDiscountPercentage: true,
            brand: {
                id: "brand-1",
                name: "Nike",
                slug: "nike",
                photoURL: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcQnZYlrQP5gM5zVQT2G9HAmS1L44DMxzqLEgw&s",
                createdAt: "2025-01-10T10:00:00Z",
                updatedAt: "2025-01-10T10:00:00Z"
            },
            clothingType: {
                id: "type-1",
                name: "T-Shirt",
                slug: "tshirt",
                createdAt: "2025-01-10T10:00:00Z",
                updatedAt: "2025-01-10T10:00:00Z"
            },
            collection: {
                id: "collection-1",
                name: "Winter 2024",
                slug: "winter-2024",
                createdAt: "2025-01-15T10:00:00Z",
                updatedAt: "2025-01-15T10:00:00Z"
            },
            additionalPhotos: [
                {
                    id: "photo-1",
                    photoUrl: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769944046/%D1%84%D0%BE%D1%82%D0%BE_1_ykfs8f.webp",
                    colorId: "black",
                    isMain: true
                },
                {
                    id: "photo-2",
                    photoUrl: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769944083/%D1%84%D0%BE%D1%82%D0%BE_2_fcc36k.webp",
                    colorId: "black",
                    isMain: true
                },
                {
                    id: "photo-3",
                    photoUrl: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769944103/%D1%84%D0%BE%D1%82%D0%BE_3_r3dnvj.webp",
                    colorId: "black",
                    isMain: true
                },
                {
                    id: "photo-4",
                    photoUrl: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769944128/%D1%84%D0%BE%D1%82%D0%BE_4_zoj0iq.webp",
                    colorId: "black",
                    isMain: true
                },
                {
                    id: "photo-6",
                    photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/2c/0e/71/90174163-0-1340x1410_-jpg-84.webp",
                    colorId: "blue",
                    isMain: false
                },
                {
                    id: "photo-8",
                    photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/f8/74/3f/90174163-2792611-1340x1410_-jpg-84.webp",
                    colorId: "blue",
                    isMain: false
                },
                {
                    id: "photo-9",
                    photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/15/df/72/90174163-2792612-1340x1410_-jpg-84.webp",
                    colorId: "blue",
                    isMain: false
                },
                {
                    id: "photo-7",
                    photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/04/37/e4/90223062-0-1340x1410_-jpg-84.webp",
                    colorId: "gray",
                    isMain: false
                },
                {
                    id: "photo-1021",
                    photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/2e/77/6a/90223062-3091244-1340x1410_-jpg-84.webp",
                    colorId: "gray",
                    isMain: false
                },
                {
                    id: "photo-10",
                    photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/6f/d9/d6/90223062-3091245-1340x1410_-jpg-84.webp",
                    colorId: "gray",
                    isMain: false
                },
                {
                    id: "photo-11",
                    photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/82/6a/f8/90223062-3091246-1340x1410_-jpg-84.webp",
                    colorId: "gray",
                    isMain: false
                }
            ],
            tags: [
                { id: "tag-1", name: "New Arrival", slug: "new", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-2", name: "Bestseller", slug: "bestseller", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-3", name: "Sale", slug: "sale", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-4", name: "Limited Collection", slug: "limited", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-5", name: "Eco Materials", slug: "eco", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" }
            ],
            materials: [
                { id: "mat-1", name: "Cotton", percentage: 70 },
                { id: "mat-2", name: "Cardon", percentage: 30 }
            ],
            stocks: [
                {
                    id: "stock-1",
                    size: { id: "size-xs", name: "XS", slug: "xs", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Black", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 5
                },
                {
                    id: "stock-2",
                    size: { id: "size-s", name: "S", slug: "s", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Black", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 0
                },
                {
                    id: "stock-3",
                    size: { id: "size-m", name: "M", slug: "m", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Black", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 12
                },
                {
                    id: "stock-4",
                    size: { id: "size-l", name: "L", slug: "l", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Black", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 8
                },
                {
                    id: "stock-5",
                    size: { id: "size-xl", name: "XL", slug: "xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Black", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 3
                },
                {
                    id: "stock-6",
                    size: { id: "size-2xl", name: "2XL", slug: "2xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Black", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 7
                },
                {
                    id: "stock-7",
                    size: { id: "size-xs", name: "XS", slug: "xs", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Blue", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 3
                },
                {
                    id: "stock-8",
                    size: { id: "size-s", name: "S", slug: "s", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Blue", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 8
                },
                {
                    id: "stock-9",
                    size: { id: "size-m", name: "M", slug: "m", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Blue", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 0
                },
                {
                    id: "stock-10",
                    size: { id: "size-l", name: "L", slug: "l", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Blue", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 4
                },
                {
                    id: "stock-11",
                    size: { id: "size-xl", name: "XL", slug: "xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Blue", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 6
                },
                {
                    id: "stock-12",
                    size: { id: "size-2xl", name: "2XL", slug: "2xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Blue", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 2
                },
                {
                    id: "stock-13",
                    size: { id: "size-xs", name: "XS", slug: "xs", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Gray", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 10
                },
                {
                    id: "stock-14",
                    size: { id: "size-s", name: "S", slug: "s", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Gray", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 7
                },
                {
                    id: "stock-15",
                    size: { id: "size-m", name: "M", slug: "m", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Gray", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 15
                },
                {
                    id: "stock-16",
                    size: { id: "size-l", name: "L", slug: "l", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Gray", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 0
                },
                {
                    id: "stock-17",
                    size: { id: "size-xl", name: "XL", slug: "xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Gray", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 5
                },
                {
                    id: "stock-18",
                    size: { id: "size-2xl", name: "2XL", slug: "2xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Gray", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 9
                }
            ]
        },
        reviews: {
            currentPage: 1,
            totalPages: 1,
            pageSize: 10,
            totalCount: 1,
            hasPrevious: false,
            hasNext: false,
            items: [
                {
                    id: "review-1",
                    user: {
                        id: "user-1",
                        firstName: "Max",
                        lastName: "B.",
                        photoUrl: "https://media.gq.com/photos/55828b4f1177d66d68d528a7/master/w_1600%2Cc_limit/blogs-the-feed-2014-04-28-rapper-future-honest-album-release-music-hip-hop.jpg"
                    },
                    rating: 4,
                    comment: "Excellent quality! Very satisfied with the purchase.",
                    createdAt: "2026-01-04T12:00:00Z"
                }
            ]
        },
        statistics: {
            clotheItemId: "clothe-1",
            totalReviews: 14,
            fiveStars: 5,
            fourStars: 6,
            threeStars: 0,
            twoStars: 0,
            oneStars: 0,
            averageRating: 4.5
        },
        questions: {
            currentPage: 1,
            totalPages: 13,
            pageSize: 10,
            totalCount: 1,
            hasPrevious: false,
            hasNext: false,
            items: [
                {
                    id: "question-1",
                    user: {
                        id: "user-3",
                        firstName: "Kanovskiy",
                        lastName: "Oleksandr",
                        photoUrl: "https://media.gq.com/photos/55828b4f1177d66d68d528a7/master/w_1600%2Cc_limit/blogs-the-feed-2014-04-28-rapper-future-honest-album-release-music-hip-hop.jpg"
                    },
                    questionText: "Will it suit me?",
                    createdAt: "2026-01-04T12:00:00Z",
                    answers: [
                        {
                            id: "answer-1",
                            user: {
                                id: "user-4",
                                firstName: "Volodymyr",
                                lastName: "Ivanov",
                                photoUrl: "https://media.gq.com/photos/55828b4f1177d66d68d528a7/master/w_1600%2Cc_limit/blogs-the-feed-2014-04-28-rapper-future-honest-album-release-music-hip-hop.jpg"
                            },
                            answerText: "Yes, I bought everything for my daughter and it was a great success.",
                            createdAt: "2026-01-04T12:00:00Z"
                        }
                    ]
                }
            ]
        }
    };

    const uniqueColors = useMemo(() => {
        const colorMap = new Map();
        mockData.clotheDetailDTO.stocks.forEach(stock => {
            if (!colorMap.has(stock.color.id)) {
                colorMap.set(stock.color.id, stock.color);
            }
        });
        return Array.from(colorMap.values());
    }, [mockData.clotheDetailDTO.stocks]);

    const initialColor = useMemo(() => {
        const colorFromUrl = uniqueColors.find(c => c.slug === colorSlug);
        return colorFromUrl || uniqueColors[0];
    }, [uniqueColors, colorSlug]);

    const [selectedColor, setSelectedColor] = useState(initialColor);

    useEffect(() => {
        const colorFromUrl = uniqueColors.find(c => c.slug === colorSlug);
        if (colorFromUrl && colorFromUrl.id !== selectedColor.id) {
            setSelectedColor(colorFromUrl);
        }
    }, [colorSlug, uniqueColors]);

    const handleColorChange = (color: typeof selectedColor) => {
        setSelectedColor(color);
        navigate(`/clothe/${slug}/${color.slug}`, { replace: true });
    };

    const pageTitle = `${mockData.clotheDetailDTO.name} — Clothy`;
    const pageDescription = mockData.clotheDetailDTO.description;

    return (
        <PageWrapper>
            <div className={styles.pageWrapper}>
                <Helmet>
                    <title>{pageTitle}</title>
                    <meta name="description" content={pageDescription} />
                </Helmet>

                <div className={styles.container}>
                    <ImageGallery
                        additionalPhotos={mockData.clotheDetailDTO.additionalPhotos}
                        selectedColor={selectedColor}
                    />
                    <ClotheDetail
                        clotheDetail={mockData.clotheDetailDTO}
                        selectedColor={selectedColor}
                        onColorChange={handleColorChange}
                    />
                </div>
                <ReviewsSection
                    reviews={mockData.reviews}
                    statistics={mockData.statistics}
                    questions={mockData.questions}
                />
            </div>
        </PageWrapper>
    );
};

export default ClotheDetailPage;