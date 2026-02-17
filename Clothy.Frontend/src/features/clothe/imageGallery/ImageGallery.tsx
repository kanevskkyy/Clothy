import React, { useState, useMemo, useEffect } from 'react';
import { ChevronLeft, ChevronRight } from 'lucide-react';
import styles from './ImageGallery.module.css';
import type { IClothePhotos } from "../../../entities/catalogService/photos/IClothePhotos.ts";

interface ImageGalleryProps {
    additionalPhotos: IClothePhotos[];
    selectedColor: {
        id: string;
        name: string;
        hexCode: string
    };
}

const ImageGallery: React.FC<ImageGalleryProps> = ({ additionalPhotos, selectedColor }) => {
    const sortedPhotos: IClothePhotos[] = useMemo(() => {
        const colorPhotos: IClothePhotos[] = additionalPhotos.filter(
            p => p.colorId === selectedColor.id
        );
        if (!colorPhotos.length) return [];

        const mainPhoto: IClothePhotos | undefined = colorPhotos.find(p => p.isMain);
        const rest: IClothePhotos[] = colorPhotos.filter(p => p.id !== mainPhoto?.id);
        return mainPhoto ? [mainPhoto, ...rest] : colorPhotos;
    }, [additionalPhotos, selectedColor.id]);

    const [activeIndex, setActiveIndex] = useState(0);

    useEffect(() => {
        setActiveIndex(0);
    }, [selectedColor.id]);

    const safeActiveIndex: number = Math.min(activeIndex, sortedPhotos.length - 1);
    const activePhoto: IClothePhotos = sortedPhotos[safeActiveIndex];

    const handlePrevious = () => {
        setActiveIndex((prev) => (prev === 0 ? sortedPhotos.length - 1 : prev - 1));
    };

    const handleNext = () => {
        setActiveIndex((prev) => (prev === sortedPhotos.length - 1 ? 0 : prev + 1));
    };

    if (!sortedPhotos.length || !activePhoto) {
        return (
            <div className={styles.imageContainer}>
                <div className={styles.mainImageContainer}>
                    <div>No photos available</div>
                </div>
            </div>
        );
    }

    return (
        <div className={styles.imageContainer} key={selectedColor.id}>
            <div className={styles.mainImageContainer}>
                <img
                    src={activePhoto.photoUrl}
                    className={styles.mainImage}
                    alt="Product"
                />
                {sortedPhotos.length > 1 && (
                    <>
                        <button
                            className={`${styles.arrowButton} ${styles.arrowLeft}`}
                            onClick={handlePrevious}
                            aria-label="Previous photo"
                        >
                            <ChevronLeft size={24} />
                        </button>
                        <button
                            className={`${styles.arrowButton} ${styles.arrowRight}`}
                            onClick={handleNext}
                            aria-label="Next photo"
                        >
                            <ChevronRight size={24} />
                        </button>
                    </>
                )}
            </div>
            <div className={styles.additionalImages}>
                {sortedPhotos.map((photo, index) => (
                    <img
                        key={photo.id}
                        src={photo.photoUrl}
                        className={`${styles.additionalImage} ${
                            index === safeActiveIndex ? styles.active : ''
                        }`}
                        onClick={() => setActiveIndex(index)}
                        alt={`Preview ${index + 1}`}
                    />
                ))}
            </div>
        </div>
    );
};

export default ImageGallery;