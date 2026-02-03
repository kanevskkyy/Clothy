import type { IClotheAggregatedDetailDTO } from '../../entities/clotheItem/interfaces/IClotheAggregatedDetailDTO';
import styles from "./ClotheDetailPage.module.css";
import ImageGallery from "../../features/imageGallery/ImageGallery.tsx";
import ClotheDetail from '../../entities/clotheItem/clotheInfo/ClotheDetail.tsx';
import ReviewsSection from "../../entities/reviews/reviewSection/ReviewsSection.tsx";
import {useEffect, useMemo, useState} from "react";
import {useNavigate, useParams} from "react-router-dom";

const ClotheDetailPage = () => {
    const { slug, colorSlug } = useParams<{ slug: string; colorSlug: string }>();
    const navigate = useNavigate();

    const mockData: IClotheAggregatedDetailDTO = {
        clotheDetailDTO: {
            id: "clothe-1",
            name: "Nike Air Max Футболка",
            slug: "nike-air-max-tshirt",
            description: "Класична футболка Nike Air Max виготовлена з високоякісного бавовняного матеріалу. Забезпечує максимальний комфорт під час носіння. Ідеально підходить для повсякденного використання та занять.",
            price: "1299",
            gender: "Чоловіча",
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
                name: "Зима 2024",
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
                { id: "tag-1", name: "Новинка", slug: "new", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-2", name: "Хіт продажу", slug: "bestseller", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-3", name: "Знижка", slug: "sale", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-4", name: "Лімітована колекція", slug: "limited", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                { id: "tag-5", name: "Еко-матеріали", slug: "eco", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" }
            ],
            materials: [
                { id: "mat-1", name: "Бавовна", percentage: 70 },
                { id: "mat-2", name: "Катон", percentage: 30 }
            ],
            stocks: [
                {
                    id: "stock-1",
                    size: { id: "size-xs", name: "XS", slug: "xs", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Чорний", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 5
                },
                {
                    id: "stock-2",
                    size: { id: "size-s", name: "S", slug: "s", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Чорний", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 0
                },
                {
                    id: "stock-3",
                    size: { id: "size-m", name: "M", slug: "m", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Чорний", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 12
                },
                {
                    id: "stock-4",
                    size: { id: "size-l", name: "L", slug: "l", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Чорний", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 8
                },
                {
                    id: "stock-5",
                    size: { id: "size-xl", name: "XL", slug: "xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Чорний", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 3
                },
                {
                    id: "stock-6",
                    size: { id: "size-2xl", name: "2XL", slug: "2xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "black", name: "Чорний", slug: "black", hexCode: "#1A1A1A", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 7
                },
                {
                    id: "stock-7",
                    size: { id: "size-xs", name: "XS", slug: "xs", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Синій", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 3
                },
                {
                    id: "stock-8",
                    size: { id: "size-s", name: "S", slug: "s", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Синій", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 8
                },
                {
                    id: "stock-9",
                    size: { id: "size-m", name: "M", slug: "m", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Синій", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 0
                },
                {
                    id: "stock-10",
                    size: { id: "size-l", name: "L", slug: "l", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Синій", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 4
                },
                {
                    id: "stock-11",
                    size: { id: "size-xl", name: "XL", slug: "xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Синій", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 6
                },
                {
                    id: "stock-12",
                    size: { id: "size-2xl", name: "2XL", slug: "2xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "blue", name: "Синій", slug: "blue", hexCode: "#B9C1E8", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 2
                },
                {
                    id: "stock-13",
                    size: { id: "size-xs", name: "XS", slug: "xs", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Сірий", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 10
                },
                {
                    id: "stock-14",
                    size: { id: "size-s", name: "S", slug: "s", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Сірий", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 7
                },
                {
                    id: "stock-15",
                    size: { id: "size-m", name: "M", slug: "m", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Сірий", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 15
                },
                {
                    id: "stock-16",
                    size: { id: "size-l", name: "L", slug: "l", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Сірий", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 0
                },
                {
                    id: "stock-17",
                    size: { id: "size-xl", name: "XL", slug: "xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Сірий", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 5
                },
                {
                    id: "stock-18",
                    size: { id: "size-2xl", name: "2XL", slug: "2xl", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    color: { id: "gray", name: "Сірий", slug: "gray", hexCode: "#6B7280", createdAt: "2025-01-01T00:00:00Z", updatedAt: "2025-01-01T00:00:00Z" },
                    quantity: 9
                }
            ]
        },
        reviews: [
            {
                id: "review-1",
                user: {
                    id: "user-1",
                    firstName: "Максим",
                    lastName: "П.",
                    photoUrl: "https://media.gq.com/photos/55828b4f1177d66d68d528a7/master/w_1600%2Cc_limit/blogs-the-feed-2014-04-28-rapper-future-honest-album-release-music-hip-hop.jpg"
                },
                rating: 4,
                comment: "Чудова якість! Дуже задоволений покупкою.",
                createdAt: "2026-01-04T12:00:00Z"
            }
        ],
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
        questions: [
            {
                id: "question-1",
                user: {
                    id: "user-3",
                    firstName: "Каньовський",
                    lastName: "Олександр",
                    photoUrl: "https://media.gq.com/photos/55828b4f1177d66d68d528a7/master/w_1600%2Cc_limit/blogs-the-feed-2014-04-28-rapper-future-honest-album-release-music-hip-hop.jpg"
                },
                questionText: "Чі підійде на мене?",
                answers: [
                    {
                        id: "answer-1",
                        user: {
                            id: "user-4",
                            firstName: "Володимир",
                            lastName: "Іванович",
                            photoUrl: "https://media.gq.com/photos/55828b4f1177d66d68d528a7/master/w_1600%2Cc_limit/blogs-the-feed-2014-04-28-rapper-future-honest-album-release-music-hip-hop.jpg"
                        },
                        answerText: "Так, брав для своєї доньки все підійшло 'на ура'"
                    }
                ]
            }
        ]
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

    return (
        <div className={styles.pageWrapper}>
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
    );
};

export default ClotheDetailPage;