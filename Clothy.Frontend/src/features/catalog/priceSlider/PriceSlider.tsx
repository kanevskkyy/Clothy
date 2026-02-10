import { memo, useEffect, useRef, useState } from "react";
import styles from "./PriceSlider.module.css";

interface PriceSliderProps {
    min: number;
    max: number;
    currentMin: number;
    currentMax: number;
    onChange: (min: number, max: number) => void;
}

const PriceSlider = memo(({ min, max, currentMin, currentMax, onChange }: PriceSliderProps) => {
    const [tempMin, setTempMin] = useState(currentMin);
    const [tempMax, setTempMax] = useState(currentMax);
    const [activeThumb, setActiveThumb] = useState<'min' | 'max' | null>(null);
    const sliderRef = useRef<HTMLDivElement>(null);

    useEffect(() => {
        setTempMin(currentMin);
        setTempMax(currentMax);
    }, [currentMin, currentMax]);

    const formatPrice = (price: number) => {
        return Math.round(price).toLocaleString('uk-UA').replace(/\s/g, '\u00A0') + '\u00A0₴';
    };

    const handleMouseMove = (e: MouseEvent) => {
        if (!activeThumb || !sliderRef.current) return;

        const rect = sliderRef.current.getBoundingClientRect();
        let percent = ((e.clientX - rect.left) / rect.width) * 100;
        percent = Math.max(0, Math.min(100, percent));

        const value = min + (percent / 100) * (max - min);

        if (activeThumb === 'min') {
            const newMin = Math.min(value, tempMax - 100);
            setTempMin(newMin);
        } else {
            const newMax = Math.max(value, tempMin + 100);
            setTempMax(newMax);
        }
    };

    const handleMouseUp = () => {
        if (activeThumb) {
            onChange(tempMin, tempMax);
            setActiveThumb(null);
        }
    };

    const handleTouchMove = (e: TouchEvent) => {
        if (!activeThumb || !sliderRef.current) return;

        const touch = e.touches[0];
        const rect = sliderRef.current.getBoundingClientRect();
        let percent = ((touch.clientX - rect.left) / rect.width) * 100;
        percent = Math.max(0, Math.min(100, percent));

        const value = min + (percent / 100) * (max - min);

        if (activeThumb === 'min') {
            const newMin = Math.min(value, tempMax - 100);
            setTempMin(newMin);
        } else {
            const newMax = Math.max(value, tempMin + 100);
            setTempMax(newMax);
        }
    };

    const handleTouchEnd = () => {
        if (activeThumb) {
            onChange(tempMin, tempMax);
            setActiveThumb(null);
        }
    };

    useEffect(() => {
        if (activeThumb) {
            document.addEventListener('mousemove', handleMouseMove);
            document.addEventListener('mouseup', handleMouseUp);
            document.addEventListener('touchmove', handleTouchMove);
            document.addEventListener('touchend', handleTouchEnd);

            return () => {
                document.removeEventListener('mousemove', handleMouseMove);
                document.removeEventListener('mouseup', handleMouseUp);
                document.removeEventListener('touchmove', handleTouchMove);
                document.removeEventListener('touchend', handleTouchEnd);
            };
        }
    }, [activeThumb, tempMin, tempMax]);

    const minPercent = ((tempMin - min) / (max - min)) * 100;
    const maxPercent = ((tempMax - min) / (max - min)) * 100;

    return (
        <div>
            <div className={styles.priceInputs}>
                <div className={styles.priceInputWrapper}>
                    <span className={styles.priceLabel}>from</span>
                    <input
                        type="text"
                        className={styles.priceInput}
                        value={formatPrice(tempMin)}
                        readOnly
                    />
                </div>
                <span className={styles.priceSeparator}>-</span>
                <div className={styles.priceInputWrapper}>
                    <span className={styles.priceLabel}>to</span>
                    <input
                        type="text"
                        className={styles.priceInput}
                        value={formatPrice(tempMax)}
                        readOnly
                    />
                </div>
            </div>

            <div className={styles.priceSliderContainer}>
                <div className={styles.priceSlider} ref={sliderRef}>
                    <div className={styles.sliderTrack}></div>
                    <div
                        className={styles.sliderRange}
                        style={{
                            left: `${minPercent}%`,
                            width: `${maxPercent - minPercent}%`
                        }}
                    ></div>
                    <div
                        className={styles.sliderThumb}
                        style={{ left: `${minPercent}%` }}
                        onMouseDown={() => setActiveThumb('min')}
                        onTouchStart={() => setActiveThumb('min')}
                    ></div>
                    <div
                        className={styles.sliderThumb}
                        style={{ left: `${maxPercent}%` }}
                        onMouseDown={() => setActiveThumb('max')}
                        onTouchStart={() => setActiveThumb('max')}
                    ></div>
                </div>
            </div>

            <div className={styles.priceRangeLabels}>
                <span>{formatPrice(min)}</span>
                <span>{formatPrice(max)}</span>
            </div>
        </div>
    );
});

export default PriceSlider;