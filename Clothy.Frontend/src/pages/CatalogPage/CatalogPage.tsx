import {useState, useEffect} from "react";
import {useSearchParams} from "react-router-dom";
import PageWrapper from "../../shared/PageWrapper/PageWrapper";
import CatalogFilter, {type FilterState} from "../../features/catalog/catalogFilter/CatalogFilter.tsx";
import SortSelect, {type SortOption} from "../../features/catalog/sortSelect/SortSelect";
import Pagination from "../../shared/Pagination/Pagination.tsx";
import ProductList from "../../features/catalog/productList/ProductList.tsx";
import type {IFiltersResponse} from "../../entities/catalogService/filters/IFiltersResponse.ts";
import type {IClotheSummaryDTO} from "../../entities/catalogService/clotheItem/IClotheSummaryDTO.ts";
import styles from "./CatalogPage.module.css";
import type {PagedList} from "../../shared/utils/pagedList.ts";
import {Helmet} from "react-helmet";
import {getCurrentPage, handlePageChange as handlePageChangeUtil} from "../../shared/utils/paginationUtils.ts";
import {catalogApi} from "../../app/api/catalogApi.ts";
import Loader from "../../shared/Loader/Loader.tsx";
import {parsePrice} from "../../shared/utils/parsePrice.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../shared/utils/errorHandler.ts";

const CatalogPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const [sortBy, setSortBy] = useState(searchParams.get("sort") || "newest");
    const currentPage = getCurrentPage(searchParams);

    const [filters, setFilters] = useState<IFiltersResponse | null>(null);
    const [pagedClothes, setPagedClothes] = useState<PagedList<IClotheSummaryDTO> | null>(null);
    const [loading, setLoading] = useState(true);

    const sortOptions: SortOption[] = [
        {value: "newest", label: 'Creation Date: Newest First'},
        {value: 'price-asc', label: 'Price: Low to High'},
        {value: 'price-desc', label: 'Price: High to Low'},
        {value: 'name-asc', label: 'Name: A-Z'},
        {value: 'name-desc', label: 'Name: Z-A'},
    ];

    const getInitialFilters = (): FilterState => {
        if (!filters) {
            return {
                brands: [],
                clothingTypes: [],
                colors: [],
                materials: [],
                sizes: [],
                tags: [],
                collections: [],
                gender: [],
                minPrice: 0,
                maxPrice: 0,
            };
        }

        return {
            brands: searchParams.get("brands")?.split(",").filter(Boolean) || [],
            clothingTypes: searchParams.get("clothingTypes")?.split(",").filter(Boolean) || [],
            colors: searchParams.get("colors")?.split(",").filter(Boolean) || [],
            materials: searchParams.get("materials")?.split(",").filter(Boolean) || [],
            sizes: searchParams.get("sizes")?.split(",").filter(Boolean) || [],
            tags: searchParams.get("tags")?.split(",").filter(Boolean) || [],
            collections: searchParams.get("collections")?.split(",").filter(Boolean) || [],
            gender: searchParams.get("gender")?.split(",").filter(Boolean) || [],
            minPrice: Number(searchParams.get("minPrice")) || parsePrice(filters.priceRange.minPrice),
            maxPrice: Number(searchParams.get("maxPrice")) || parsePrice(filters.priceRange.maxPrice),
        };
    };

    const convertSlugsToIds = (slugs: string[], items: Array<{ id: string; slug: string }>): string[] => {
        return slugs
            .map(slug => {
                const item = items.find(i => i.slug === slug);
                return item?.id;
            })
            .filter((id): id is string => id !== undefined);
    };

    const fetchClothes = async () => {
        if (!filters) return;

        setLoading(true);
        try {
            const selectedFilters = getInitialFilters();

            const brandIds = convertSlugsToIds(selectedFilters.brands, filters.brands);
            const clothingTypeIds = convertSlugsToIds(selectedFilters.clothingTypes, filters.clothingTypes);
            const colorIds = convertSlugsToIds(selectedFilters.colors, filters.colors);
            const materialIds = convertSlugsToIds(selectedFilters.materials, filters.materials);
            const sizeIds = convertSlugsToIds(selectedFilters.sizes, filters.sizes);
            const tagIds = convertSlugsToIds(selectedFilters.tags, filters.tags);
            const collectionIds = convertSlugsToIds(selectedFilters.collections, filters.collections);

            let sortByParam: 'price' | 'name' | undefined;
            let sortDescending = false;

            if (sortBy === 'price-asc') sortByParam = 'price';
            else if (sortBy === 'price-desc') {
                sortByParam = 'price';
                sortDescending = true;
            }
            else if (sortBy === 'name-asc') {
                sortByParam = 'name';
            }
            else if (sortBy === 'name-desc') {
                sortByParam = 'name';
                sortDescending = true;
            }

            const defaultMinPrice = parsePrice(filters.priceRange.minPrice);
            const defaultMaxPrice = parsePrice(filters.priceRange.maxPrice);

            const data = await catalogApi.getClothesPagedAsync({
                pageNumber: currentPage,
                pageSize: 27,
                brands: brandIds.length > 0 ? brandIds : undefined,
                clothingTypes: clothingTypeIds.length > 0 ? clothingTypeIds : undefined,
                colors: colorIds.length > 0 ? colorIds : undefined,
                materials: materialIds.length > 0 ? materialIds : undefined,
                sizes: sizeIds.length > 0 ? sizeIds : undefined,
                tags: tagIds.length > 0 ? tagIds : undefined,
                collections: collectionIds.length > 0 ? collectionIds : undefined,
                gender: selectedFilters.gender.length > 0 ? selectedFilters.gender : undefined,
                minPrice: selectedFilters.minPrice !== defaultMinPrice ? selectedFilters.minPrice : undefined,
                maxPrice: selectedFilters.maxPrice !== defaultMaxPrice ? selectedFilters.maxPrice : undefined,
                sortBy: sortByParam,
                sortDescending: sortDescending ? true : undefined,
            });

            setPagedClothes(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        const loadFilters = async () => {
            try {
                const data = await catalogApi.getFiltersAsync();
                setFilters(data);
            } catch (error) {
                toast.error(getErrorMessage(error));
            }
        };

        loadFilters();
    }, []);

    useEffect(() => {
        if (filters) {
            fetchClothes();
        }
    }, [filters, searchParams, sortBy, currentPage]);

    const handleFilterChange = (filterState: FilterState) => {
        const params = new URLSearchParams(searchParams);

        const setOrDelete = (key: string, arr: string[]) =>
            arr.length ? params.set(key, arr.join(",")) : params.delete(key);

        setOrDelete("brands", filterState.brands);
        setOrDelete("clothingTypes", filterState.clothingTypes);
        setOrDelete("colors", filterState.colors);
        setOrDelete("materials", filterState.materials);
        setOrDelete("sizes", filterState.sizes);
        setOrDelete("tags", filterState.tags);
        setOrDelete("collections", filterState.collections);
        setOrDelete("gender", filterState.gender);

        if (filters && filterState.minPrice !== parsePrice(filters.priceRange.minPrice)) {
            params.set("minPrice", filterState.minPrice.toString());
        } else {
            params.delete("minPrice");
        }

        if (filters && filterState.maxPrice !== parsePrice(filters.priceRange.maxPrice)) {
            params.set("maxPrice", filterState.maxPrice.toString());
        } else {
            params.delete("maxPrice");
        }

        params.delete("page");
        setSearchParams(params);
    };

    const handleSortChange = (newSort: string) => {
        setSortBy(newSort);
        const params = new URLSearchParams(searchParams);

        if (newSort === "newest") {
            params.delete("sort");
        } else {
            params.set("sort", newSort);
        }

        setSearchParams(params);
    };

    const handlePageChange = (page: number) => {
        handlePageChangeUtil(page, searchParams, setSearchParams);
    };

    if (!filters || !pagedClothes) {
        return <Loader marginTop="75px"/>;
    }

    return (
        <PageWrapper>
            <Helmet>
                <title>{`Clothy — Clothing Catalog | Page ${currentPage} • ${pagedClothes.totalCount} items`}</title>
                <meta
                    name="description"
                    content={`Clothy clothing catalog: ${pagedClothes.totalCount}+ items. Filter by brand, size, and price — fast and convenient online shopping.`}
                />
                <meta property="og:title" content="Clothy — Clothing Catalog"/>
                <meta
                    property="og:description"
                    content="Wide selection of clothing in the Clothy catalog. Discounts, new arrivals, and popular brands."
                />
            </Helmet>

            <div className={styles.catalogContainer}>
                <aside className={styles.filterSidebar}>
                    <CatalogFilter
                        filters={filters}
                        initialFilters={getInitialFilters()}
                        onFilterChange={handleFilterChange}
                    />
                </aside>

                <main className={styles.catalogMain}>
                    <div className={styles.catalogHeader}>
                        <div className={styles.resultsCount}>
                            Items found: {pagedClothes.totalCount}
                        </div>

                        <div className={styles.desktopSort}>
                            <SortSelect
                                value={sortBy}
                                options={sortOptions}
                                onChange={handleSortChange}
                            />
                        </div>

                        <div className={styles.mobileFiltersRow}>
                            <CatalogFilter
                                filters={filters}
                                initialFilters={getInitialFilters()}
                                onFilterChange={handleFilterChange}
                            />
                            <SortSelect
                                value={sortBy}
                                options={sortOptions}
                                onChange={handleSortChange}
                            />
                        </div>
                    </div>

                    {loading ? (
                        <Loader/>
                    ) : (
                        <>
                            <div className={styles.productWrapper}>
                                <ProductList products={pagedClothes.items}/>
                            </div>

                            <Pagination
                                currentPage={currentPage}
                                totalPages={pagedClothes.totalPages}
                                onPageChange={handlePageChange}
                            />
                        </>
                    )}
                </main>
            </div>
        </PageWrapper>
    );
};

export default CatalogPage;