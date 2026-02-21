import { memo, useState, useEffect } from "react";
import { ChevronDown, X, SlidersHorizontal } from "lucide-react";
import styles from "./CatalogFilter.module.css";
import type { IFiltersResponse } from "../../../entities/catalogService/filters/IFiltersResponse.ts";
import Checkbox from "../../../shared/ui/Checkbox/Checkbox.tsx";
import PriceSlider from "../priceSlider/PriceSlider.tsx";
import { parsePrice } from "../../../shared/lib/parsePrice.ts";

interface CatalogFilterProps {
    filters: IFiltersResponse;
    initialFilters?: FilterState;
    onFilterChange: (filters: FilterState) => void;
}

export interface FilterState {
    brands: string[];
    clothingTypes: string[];
    colors: string[];
    materials: string[];
    sizes: string[];
    tags: string[];
    collections: string[];
    gender: string[];
    minPrice: number;
    maxPrice: number;
}

const CatalogFilter = memo(({ filters, initialFilters, onFilterChange }: CatalogFilterProps) => {
    const [isOpen, setIsOpen] = useState(false);
    const [expandedSections, setExpandedSections] = useState<Set<string>>(new Set(['price']));

    const [selectedFilters, setSelectedFilters] = useState<FilterState>(
        initialFilters || {
            brands: [],
            clothingTypes: [],
            colors: [],
            materials: [],
            sizes: [],
            tags: [],
            collections: [],
            gender: [],
            minPrice: parsePrice(filters.priceRange.minPrice),
            maxPrice: parsePrice(filters.priceRange.maxPrice),
        }
    );

    useEffect(() => {
        if (isOpen) {
            const scrollY = window.scrollY;
            document.body.style.position = 'fixed';
            document.body.style.top = `-${scrollY}px`;
            document.body.style.width = '100%';
            document.body.style.overflow = 'hidden';
        } else {
            const scrollY = document.body.style.top;
            document.body.style.position = '';
            document.body.style.top = '';
            document.body.style.width = '';
            document.body.style.overflow = '';
            if (scrollY) {
                window.scrollTo(0, parseInt(scrollY || '0') * -1);
            }
        }

        return () => {
            document.body.style.position = '';
            document.body.style.top = '';
            document.body.style.width = '';
            document.body.style.overflow = '';
        };
    }, [isOpen]);

    const toggleSection = (section: string) => {
        setExpandedSections(prev => {
            const newSet = new Set(prev);
            if (newSet.has(section)) {
                newSet.delete(section);
            } else {
                newSet.add(section);
            }
            return newSet;
        });
    };

    const handleCheckboxChange = (category: keyof FilterState, slug: string, checked: boolean) => {
        setSelectedFilters(prev => {
            const currentArray = prev[category] as string[];
            const newArray = checked
                ? [...currentArray, slug]
                : currentArray.filter(item => item !== slug);

            const newFilters = { ...prev, [category]: newArray };
            onFilterChange(newFilters);
            return newFilters;
        });
    };

    const handlePriceChange = (min: number, max: number) => {
        setSelectedFilters(prev => {
            const newFilters = { ...prev, minPrice: min, maxPrice: max };
            onFilterChange(newFilters);
            return newFilters;
        });
    };

    const hasActiveFilters = () => {
        return (
            selectedFilters.brands.length > 0 ||
            selectedFilters.clothingTypes.length > 0 ||
            selectedFilters.colors.length > 0 ||
            selectedFilters.materials.length > 0 ||
            selectedFilters.sizes.length > 0 ||
            selectedFilters.tags.length > 0 ||
            selectedFilters.collections.length > 0 ||
            selectedFilters.gender.length > 0 ||
            selectedFilters.minPrice !== parsePrice(filters.priceRange.minPrice) ||
            selectedFilters.maxPrice !== parsePrice(filters.priceRange.maxPrice)
        );
    };

    const resetFilters = () => {
        const resetState: FilterState = {
            brands: [],
            clothingTypes: [],
            colors: [],
            materials: [],
            sizes: [],
            tags: [],
            collections: [],
            gender: [],
            minPrice: parsePrice(filters.priceRange.minPrice),
            maxPrice: parsePrice(filters.priceRange.maxPrice),
        };
        setSelectedFilters(resetState);
        onFilterChange(resetState);
    };

    const handleCheckboxListScroll = (e: React.WheelEvent<HTMLDivElement>) => {
        const element = e.currentTarget;
        const isScrollable = element.scrollHeight > element.clientHeight;

        if (!isScrollable) {
            return;
        }

        const isAtTop = element.scrollTop === 0;
        const isAtBottom = element.scrollTop + element.clientHeight >= element.scrollHeight - 1;

        if ((isAtTop && e.deltaY < 0) || (isAtBottom && e.deltaY > 0)) {
            return;
        }

        e.stopPropagation();
    };

    const handleFilterContainerScroll = (e: React.WheelEvent<HTMLDivElement>) => {
        const element = e.currentTarget;
        const isScrollable = element.scrollHeight > element.clientHeight;

        if (!isScrollable) {
            return;
        }

        const isAtTop = element.scrollTop === 0;
        const isAtBottom = Math.abs(element.scrollHeight - element.scrollTop - element.clientHeight) < 2;

        if (!((isAtTop && e.deltaY < 0) || (isAtBottom && e.deltaY > 0))) {
            e.stopPropagation();
        }
    };

    return (
        <>
            <button className={styles.mobileFilterButton} onClick={() => setIsOpen(true)}>
                <SlidersHorizontal size={24} />
            </button>

            {isOpen && <div className={styles.overlay} onClick={() => setIsOpen(false)} />}

            <div
                className={`${styles.filterContainer} ${isOpen ? styles.open : ''}`}
                onWheel={handleFilterContainerScroll}
            >
                <div className={styles.filterHeader}>
                    <h2 className={styles.filterMainTitle}>Filters</h2>
                    <button className={styles.closeButton} onClick={() => setIsOpen(false)}>
                        <X size={24} />
                    </button>
                </div>

                <div className={styles.filterContent}>
                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('brand')}>
                            <h2 className={styles.filterTitle}>Brand</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('brand') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('brand') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    {filters.brands.map(brand => (
                                        <Checkbox
                                            key={brand.id}
                                            id={`brand-${brand.id}`}
                                            label={brand.name}
                                            count={brand.clotheItemCount}
                                            checked={selectedFilters.brands.includes(brand.slug)}
                                            onChange={(checked) => handleCheckboxChange('brands', brand.slug, checked)}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('type')}>
                            <h2 className={styles.filterTitle}>Clothing Type</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('type') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('type') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    {filters.clothingTypes.map(type => (
                                        <Checkbox
                                            key={type.id}
                                            id={`type-${type.id}`}
                                            label={type.name}
                                            count={type.clotheItemCount}
                                            checked={selectedFilters.clothingTypes.includes(type.slug)}
                                            onChange={(checked) => handleCheckboxChange('clothingTypes', type.slug, checked)}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('color')}>
                            <h2 className={styles.filterTitle}>Colors</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('color') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('color') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    {filters.colors.map(color => (
                                        <Checkbox
                                            key={color.id}
                                            id={`color-${color.id}`}
                                            label={color.name}
                                            count={color.clotheItemCount}
                                            checked={selectedFilters.colors.includes(color.slug)}
                                            onChange={(checked) => handleCheckboxChange('colors', color.slug, checked)}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('material')}>
                            <h2 className={styles.filterTitle}>Materials</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('material') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('material') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    {filters.materials.map(material => (
                                        <Checkbox
                                            key={material.id}
                                            id={`material-${material.id}`}
                                            label={material.name}
                                            count={material.clotheItemCount}
                                            checked={selectedFilters.materials.includes(material.slug)}
                                            onChange={(checked) => handleCheckboxChange('materials', material.slug, checked)}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('size')}>
                            <h2 className={styles.filterTitle}>Sizes</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('size') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('size') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    {filters.sizes.map(size => (
                                        <Checkbox
                                            key={size.id}
                                            id={`size-${size.id}`}
                                            label={size.name}
                                            count={size.clotheItemCount}
                                            checked={selectedFilters.sizes.includes(size.slug)}
                                            onChange={(checked) => handleCheckboxChange('sizes', size.slug, checked)}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('tags')}>
                            <h2 className={styles.filterTitle}>Tags</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('tags') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('tags') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    {filters.tags.map(tag => (
                                        <Checkbox
                                            key={tag.id}
                                            id={`tag-${tag.id}`}
                                            label={tag.name}
                                            count={tag.clotheItemCount}
                                            checked={selectedFilters.tags.includes(tag.slug)}
                                            onChange={(checked) => handleCheckboxChange('tags', tag.slug, checked)}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('collections')}>
                            <h2 className={styles.filterTitle}>Collections</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('collections') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('collections') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    {filters.collections.map(collection => (
                                        <Checkbox
                                            key={collection.id}
                                            id={`collection-${collection.id}`}
                                            label={collection.name}
                                            count={collection.clotheItemCount}
                                            checked={selectedFilters.collections.includes(collection.slug)}
                                            onChange={(checked) => handleCheckboxChange('collections', collection.slug, checked)}
                                        />
                                    ))}
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('gender')}>
                            <h2 className={styles.filterTitle}>Gender</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('gender') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('gender') && (
                            <div className={styles.sectionContent}>
                                <div className={styles.checkboxList} onWheel={handleCheckboxListScroll}>
                                    <Checkbox
                                        id="gender-male"
                                        label="Male"
                                        count={filters.gender.maleCount}
                                        checked={selectedFilters.gender.includes('Male')}
                                        onChange={(checked) => handleCheckboxChange('gender', 'Male', checked)}
                                    />
                                    <Checkbox
                                        id="gender-female"
                                        label="Female"
                                        count={filters.gender.femaleCount}
                                        checked={selectedFilters.gender.includes('Female')}
                                        onChange={(checked) => handleCheckboxChange('gender', 'Female', checked)}
                                    />
                                    <Checkbox
                                        id="gender-unisex"
                                        label="Unisex"
                                        count={filters.gender.unisexCount}
                                        checked={selectedFilters.gender.includes('Unisex')}
                                        onChange={(checked) => handleCheckboxChange('gender', 'Unisex', checked)}
                                    />
                                </div>
                            </div>
                        )}
                    </div>

                    <div className={styles.filterSection}>
                        <div className={styles.sectionHeader} onClick={() => toggleSection('price')}>
                            <h2 className={styles.filterTitle}>Price</h2>
                            <ChevronDown
                                className={`${styles.toggleIcon} ${expandedSections.has('price') ? styles.expanded : ''}`}
                                size={20}
                            />
                        </div>
                        {expandedSections.has('price') && (
                            <div className={styles.sectionContent}>
                                <PriceSlider
                                    min={parsePrice(filters.priceRange.minPrice)}
                                    max={parsePrice(filters.priceRange.maxPrice)}
                                    currentMin={selectedFilters.minPrice}
                                    currentMax={selectedFilters.maxPrice}
                                    onChange={handlePriceChange}
                                />
                            </div>
                        )}
                    </div>

                    {hasActiveFilters() && (
                        <div className={styles.resetButtonWrapper}>
                            <button className={styles.resetButton} onClick={resetFilters}>
                                <X size={16} />
                                Clear filters
                            </button>
                        </div>
                    )}
                </div>
            </div>
        </>
    );
});

export default CatalogFilter;