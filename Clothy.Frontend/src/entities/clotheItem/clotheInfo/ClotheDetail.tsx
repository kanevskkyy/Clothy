import type React from "react";
import type { IClotheDetailDTO } from "../IClotheDetailDTO.ts";
import { useEffect, useMemo, useState } from "react";
import styles from "./ClotheDetail.module.css";
import type {IColorReadDTO} from "../../colors/IColorReadDTO.ts";
import type {ISizeReadDTO} from "../../sizes/ISizeReadDTO.ts";
import Button from "../../../shared/Button/Button.tsx";

interface ProductInfoProps {
    clotheDetail: IClotheDetailDTO;
    selectedColor: { id: string; name: string; hexCode: string };
    onColorChange: (color: { id: string; name: string; hexCode: string }) => void;
}

const ClotheDetail: React.FC<ProductInfoProps> = ({ clotheDetail, selectedColor, onColorChange }) => {
    const uniqueColors = useMemo(() => {
        const colorMap = new Map<string, IColorReadDTO>();
        clotheDetail.stocks.forEach(stock => {
            if (!colorMap.has(stock.color.id)) colorMap.set(stock.color.id, stock.color);
        });
        return Array.from(colorMap.values());
    }, [clotheDetail.stocks]);

    const uniqueSizes = useMemo(() => {
        const sizeMap = new Map<string, ISizeReadDTO>();
        clotheDetail.stocks.forEach(stock => {
            if (!sizeMap.has(stock.size.id)) sizeMap.set(stock.size.id, stock.size);
        });
        return Array.from(sizeMap.values());
    }, [clotheDetail.stocks]);

    const [selectedSize, setSelectedSize] = useState<ISizeReadDTO | null>(null);
    const [quantity, setQuantity] = useState(1);

    const sizeAvailability = useMemo(() => {
        return uniqueSizes.map(size => {
            const stock = clotheDetail.stocks.find(
                s => s.size.id === size.id && s.color.id === selectedColor?.id
            );
            return {
                size,
                available: stock ? stock.quantity > 0 : false,
                quantity: stock?.quantity || 0
            };
        });
    }, [selectedColor, clotheDetail.stocks, uniqueSizes]);

    useEffect(() => {
        if (!sizeAvailability.length) return;

        const firstAvailable = sizeAvailability.find(s => s.available);
        setSelectedSize(prev => {
            if (firstAvailable && prev?.id !== firstAvailable.size.id) {
                setQuantity(1);
                return firstAvailable.size;
            }
            if (!firstAvailable && !prev) {
                setQuantity(1);
                return sizeAvailability[0]?.size || null;
            }
            return prev;
        });
    }, [selectedColor.id, sizeAvailability]);

    const maxQuantity = useMemo(() => {
        if (!selectedSize || !selectedColor) return 0;
        const stock = clotheDetail.stocks.find(
            s => s.size.id === selectedSize.id && s.color.id === selectedColor.id
        );
        return stock?.quantity || 0;
    }, [selectedSize, selectedColor, clotheDetail.stocks]);

    const isAvailable = maxQuantity > 0;

    const handleAddToCart = () => console.log('Added to cart:', { selectedColor, selectedSize, quantity });
    const handleSubscribe = () => console.log('Subscription:', { selectedColor, selectedSize });
    const handleTryOnYourself = () => console.log('Try on yourself');

    return (
        <div className={styles.clotheInfo}>
            <div className={styles.clotheInfoHeader}>
                <h2>{clotheDetail.name}</h2>
                <div className={styles.priceInfo}>
                    <p className={styles.price}>{clotheDetail.price}₴</p>
                    {clotheDetail.hasOldPrice && <p className={styles.oldPrice}>{clotheDetail.oldPrice}₴</p>}
                </div>
                <div className={styles.brandInfo}>
                    <h4>Brand: <span className={styles.brandName}>{clotheDetail.brand.name}</span></h4>
                    <img src={clotheDetail.brand.photoURL} alt="brand logo" width={50} height={50} />
                </div>
                <h4>Collection: <span className={styles.collectionName}>{clotheDetail.collection.name}</span></h4>
                <h4>Gender: <span className={styles.genderName}>{clotheDetail.gender}</span></h4>
                <div className={styles.tagsInfo}>
                    <h4>Tags: </h4>
                    <div className={styles.tagsList}>
                        {clotheDetail.tags.map(tag => <div key={tag.id} className={styles.tagItem}>{tag.name}</div>)}
                    </div>
                </div>
                <div className={styles.colors}>
                    <h4>Color: <span className={styles.colorName}>{selectedColor?.name}</span></h4>
                    <div className={styles.colorsContainer}>
                        {uniqueColors.map(color => (
                            <div
                                key={color.id}
                                className={`${styles.color} ${selectedColor?.id === color.id ? styles.activeColor : ''}`}
                                style={{ backgroundColor: color.hexCode }}
                                onClick={() => onColorChange(color)}
                            />
                        ))}
                    </div>
                </div>
                <div className={styles.sizes}>
                    <h4>Size</h4>
                    <div className={styles.sizeContainer}>
                        {sizeAvailability.map(({ size, available }) => (
                            <div
                                key={size.id}
                                className={`${styles.size} ${!available ? styles.sizeNotAvailable : ''} ${selectedSize?.id === size.id ? styles.activeSize : ''}`}
                                onClick={() => {
                                    setSelectedSize(size);
                                    setQuantity(1);
                                }}
                            >
                                {size.name}
                            </div>
                        ))}
                    </div>
                </div>
                <div className={styles.cart}>
                    {isAvailable && (
                        <div className={styles.itemOptions}>
                            <button
                                className={styles.removeCount}
                                onClick={() => quantity > 1 && setQuantity(q => q - 1)}
                                disabled={quantity <= 1}
                            >
                                -
                            </button>
                            <div className={styles.productCount}>{quantity}</div>
                            <button
                                className={styles.addCount}
                                onClick={() => quantity < maxQuantity && setQuantity(q => q + 1)}
                                disabled={quantity >= maxQuantity}
                            >
                                +
                            </button>
                        </div>
                    )}
                    <Button
                        variant="primary"
                        size="lg"
                        fullWidth
                        onClick={isAvailable ? handleAddToCart : handleSubscribe}
                    >
                        {isAvailable ? 'Add to cart' : 'Subscribe to updates'}
                    </Button>
                </div>
                <div className={styles.buttonWrapper}>
                    <Button
                        variant="outline"
                        size="lg"
                        fullWidth
                        onClick={handleTryOnYourself}
                    >
                        Try it on (AI)
                    </Button>
                </div>
            </div>
            <div className={styles.clotheDescription}>
                <h3>Product description</h3>
                <p>{clotheDetail.description}</p>
            </div>
            <div className={styles.clotheMaterials}>
                <h3>Materials</h3>
                <p>{clotheDetail.materials.map((mat, idx) => `${mat.name} (${mat.percentage}%)${idx < clotheDetail.materials.length - 1 ? ', ' : ''}`)}</p>
            </div>
        </div>
    );
};

export default ClotheDetail;