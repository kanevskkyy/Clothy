import type React from "react";
import type { IClotheDetailDTO } from "../IClotheDetailDTO.ts";
import { useEffect, useMemo, useState, useRef } from "react";
import styles from "./ClotheDetail.module.css";
import type { IColorReadDTO } from "../../colors/IColorReadDTO.ts";
import type { ISizeReadDTO } from "../../sizes/ISizeReadDTO.ts";
import Button from "../../../../shared/ui/Button/Button.tsx";
import { toast } from "sonner";
import Loader from "../../../../shared/ui/Loader/Loader.tsx";
import { tryOnService } from "../../../../app/api/tryOnApi.ts";
import { catalogApi } from "../../../../app/api/catalogApi.ts";
import { getErrorMessage } from "../../../../shared/lib/errorHandler.ts";
import { basketApi, type IBasketItemCreateDTO } from "../../../../app/api/basketApi.ts";
import {Tag} from "lucide-react";
import Badge from "../../../../features/catalog/badge/Badge.tsx";

interface ProductInfoProps {
    clotheDetail: IClotheDetailDTO;
    selectedColor: IColorReadDTO;
    onColorChange: (color: IColorReadDTO) => void;
}

const ClotheDetail: React.FC<ProductInfoProps> = ({ clotheDetail, selectedColor, onColorChange }) => {
    const fileInputRef = useRef<HTMLInputElement>(null);
    const downloadLinkRef = useRef<HTMLAnchorElement>(null);
    const containerRef = useRef<HTMLDivElement>(null);

    const [selectedSize, setSelectedSize] = useState<ISizeReadDTO | null>(null);
    const [quantity, setQuantity] = useState(1);
    const [tryOnLoading, setTryOnLoading] = useState(false);
    const [tryOnResult, setTryOnResult] = useState<string | undefined>(undefined);
    const [isAddingToCart, setIsAddingToCart] = useState(false);
    const [isSubscribing, setIsSubscribing] = useState(false);

    useEffect(() => {
        const el = containerRef.current;
        if (!el) return;
        const observer = new IntersectionObserver(
            ([entry]) => {
                if (entry.isIntersecting) {
                    el.classList.add(styles.visible);
                    observer.disconnect();
                }
            },
            { threshold: 0.05 }
        );
        observer.observe(el);
        return () => observer.disconnect();
    }, []);

    const uniqueColors = useMemo(() => {
        const colorMap = new Map<string, IColorReadDTO>();
        clotheDetail.stocks.forEach((stock) => {
            if (!colorMap.has(stock.color.id)) colorMap.set(stock.color.id, stock.color);
        });
        return Array.from(colorMap.values());
    }, [clotheDetail.stocks]);

    const uniqueSizes = useMemo(() => {
        const sizeMap = new Map<string, ISizeReadDTO>();
        clotheDetail.stocks.forEach((stock) => {
            if (!sizeMap.has(stock.size.id)) sizeMap.set(stock.size.id, stock.size);
        });
        return Array.from(sizeMap.values());
    }, [clotheDetail.stocks]);

    const sizeAvailability = useMemo(() => {
        return uniqueSizes.map((size) => {
            const stock = clotheDetail.stocks.find(
                (s) => s.size.id === size.id && s.color.id === selectedColor?.id
            );
            return { size, available: stock ? stock.quantity > 0 : false };
        });
    }, [selectedColor, clotheDetail.stocks, uniqueSizes]);

    const maxQuantity = useMemo(() => {
        if (!selectedSize || !selectedColor) return 0;
        const stock = clotheDetail.stocks.find(
            (s) => s.size.id === selectedSize.id && s.color.id === selectedColor.id
        );
        return stock?.quantity || 0;
    }, [selectedSize, selectedColor, clotheDetail.stocks]);

    const currentClotheImage = useMemo(() => {
        const photo = clotheDetail.additionalPhotos.find(
            (p) => p.colorId === selectedColor.id && p.isMain
        );
        return photo?.photoUrl || clotheDetail.additionalPhotos[0]?.photoUrl || "";
    }, [selectedColor.id, clotheDetail.additionalPhotos]);

    const isAvailable = maxQuantity > 0;

    useEffect(() => {
        if (!sizeAvailability.length) return;
        const firstAvailable = sizeAvailability.find((s) => s.available);
        setSelectedSize((prev) => {
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

    const handleAddToCart = async () => {
        setIsAddingToCart(true);
        try {
            const dto: IBasketItemCreateDTO = {
                clotheId: clotheDetail.id,
                sizeId: selectedSize?.id ?? "",
                colorId: selectedColor.id,
                quantity,
            };
            await basketApi.addToCartAsync(dto);
            toast.success("Successfully added");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsAddingToCart(false);
        }
    };

    const handleSubscribe = async () => {
        if (!selectedSize || !selectedColor) { toast.error("Please select size and color"); return; }
        const stock = clotheDetail.stocks.find(
            (s) => s.size.id === selectedSize.id && s.color.id === selectedColor.id
        );
        if (!stock) return;
        setIsSubscribing(true);
        try {
            await catalogApi.subscribeOnUpdatesAsync(stock.id);
            toast.success("Subscribed! We'll email you when it's back in stock.");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubscribing(false);
        }
    };

    const handleTryOnYourself = () => fileInputRef.current?.click();

    const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
        const file = e.target.files?.[0];
        if (!file) return;
        setTryOnLoading(true);
        setTryOnResult(undefined);
        toast.info("AI is processing your photo. Please wait 15–30 seconds…");
        try {
            const response = await tryOnService.tryOn(file, currentClotheImage);
            setTryOnResult(response.outputImageUrl);
            toast.success("Try-on ready! You can download your photo now.");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setTryOnLoading(false);
            if (fileInputRef.current) fileInputRef.current.value = '';
        }
    };

    const handleDownloadResult = () => {
        if (tryOnResult && downloadLinkRef.current) {
            downloadLinkRef.current.click();
            setTryOnResult(undefined);
            if (fileInputRef.current) fileInputRef.current.value = '';
        }
    };

    return (
        <div className={styles.clotheInfo} ref={containerRef}>
            <p className={`${styles.brandName} ${styles.animItem}`}>{clotheDetail.brand.name}</p>

            <h1 className={`${styles.clotheName} ${styles.animItem}`}>{clotheDetail.name}</h1>

            <div className={`${styles.priceInfo} ${styles.animItem}`}>
                <span className={styles.price}>${clotheDetail.price}</span>
                {clotheDetail.hasOldPrice && <span className={styles.oldPrice}>${clotheDetail.oldPrice}</span>}
            </div>

            <p className={`${styles.description} ${styles.animItem}`}>{clotheDetail.description}</p>

            <div className={styles.divider} />

            <div className={`${styles.section} ${styles.animItem}`}>
                <p className={styles.sectionLabel}>Collection</p>
                <div className={styles.pillList}>
                    <Badge
                        label={clotheDetail.collection.name}
                        color="#2a2622"
                        fontSize="13px"
                        background="#F3F4F6"
                        borderColor="#E5E7EB"
                    />
                </div>
            </div>

            <div className={`${styles.section} ${styles.animItem}`}>
                <p className={styles.sectionLabel}>Tags</p>
                <div className={styles.pillList}>
                    {clotheDetail.tags.map((tag) => (
                        <Badge
                            key={tag.id}
                            label={tag.name}
                            icon={<Tag size={12} />}
                            color="#161412"
                            fontSize="13px"
                            background="transparent"
                            borderColor="#D1D5DB"
                        />
                    ))}
                </div>
            </div>

            <div className={`${styles.section} ${styles.animItem}`}>
                <p className={styles.sectionLabel}>Materials</p>
                <div className={styles.pillList}>
                    {clotheDetail.materials.map((mat) => (
                        <Badge
                            key={mat.name}
                            label={`${mat.name} — ${mat.percentage}%`}
                            color="#161412"
                            background="#F3F4F6"
                            borderColor="transparent"
                            fontSize="13px"
                        />
                    ))}
                </div>
            </div>

            <div className={`${styles.section} ${styles.animItem}`}>
                <p className={styles.sectionLabel}>Gender</p>
                <div className={styles.pillList}>
                    <Badge
                        label={clotheDetail.gender}
                        color="#161412"
                        background="#F3F4F6"
                        fontSize="13px"
                        borderColor="#E5E7EB"
                    />
                </div>
            </div>

            <div className={`${styles.section} ${styles.animItem}`}>
                <p className={styles.sectionLabel}>Size</p>
                <div className={styles.sizeContainer}>
                    {sizeAvailability.map(({ size, available }) => (
                        <div
                            key={size.id}
                            className={`${styles.size} ${!available ? styles.sizeNotAvailable : ""} ${selectedSize?.id === size.id ? styles.activeSize : ""}`}
                            onClick={() => { setSelectedSize(size); setQuantity(1); }}
                        >
                            {size.name}
                        </div>
                    ))}
                </div>
            </div>

            <div className={`${styles.section} ${styles.animItem}`}>
                <p className={styles.sectionLabel}>
                    Color —<span className={styles.colorNameInline}> {selectedColor?.name}</span>
                </p>
                <div className={styles.colorsContainer}>
                    {uniqueColors.map((color) => (
                        <div
                            key={color.id}
                            className={`${styles.color} ${selectedColor?.id === color.id ? styles.activeColor : ""}`}
                            style={{ backgroundColor: color.hexCode }}
                            onClick={() => onColorChange(color)}
                        />
                    ))}
                </div>
            </div>

            <div className={styles.divider} />

            <div className={`${styles.cart} ${styles.animItem}`}>
                {isAvailable && (
                    <div className={styles.itemOptions}>
                        <button className={styles.removeCount} onClick={() => quantity > 1 && setQuantity((q) => q - 1)} disabled={quantity <= 1}>−</button>
                        <div className={styles.productCount}>{quantity}</div>
                        <button className={styles.addCount} onClick={() => quantity < maxQuantity && setQuantity((q) => q + 1)} disabled={quantity >= maxQuantity}>+</button>
                    </div>
                )}
                <Button variant="primary" size="lg" fullWidth onClick={isAvailable ? handleAddToCart : handleSubscribe} disabled={isAddingToCart || isSubscribing}>
                    {isAvailable ? "Add to cart" : "Subscribe to updates"}
                </Button>
            </div>

            <div className={`${styles.buttonWrapper} ${styles.animItem}`}>
                <input ref={fileInputRef} type="file" accept="image/jpeg,image/jpg,image/png,image/webp,image/gif" onChange={handleFileChange} style={{ display: "none" }} />
                <Button variant="outline" size="lg" fullWidth onClick={handleTryOnYourself}>Try it on (AI)</Button>
            </div>

            {tryOnLoading && !tryOnResult && <Loader />}

            {tryOnResult && (
                <>
                    <a ref={downloadLinkRef} href={tryOnResult} download={`try-on-result-${Date.now()}.jpg`} style={{ display: "none" }} />
                    <div className={styles.buttonWrapper}>
                        <Button variant="outline" size="lg" fullWidth onClick={handleDownloadResult}>Download result</Button>
                    </div>
                </>
            )}
        </div>
    );
};

export default ClotheDetail;