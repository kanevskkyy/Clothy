import { useState } from "react";
import { useSearchParams } from "react-router-dom";
import PageWrapper from "../../shared/PageWrapper/PageWrapper";
import CatalogFilter, { type FilterState } from "../../features/catalogFilter/CatalogFilter.tsx";
import SortSelect, { type SortOption } from "../../features/sortSelect/SortSelect";
import Pagination from "../../shared/Pagination/Pagination.tsx";
import ProductList from "../../features/productList/ProductList.tsx";
import type { IFiltersResponse } from "../../entities/filters/interfaces/IFiltersResponse";
import type { IClotheSummaryDTO } from "../../entities/clotheItem/interfaces/IClotheSummaryDTO";
import styles from "./CatalogPage.module.css";
import type {PagedList} from "../../shared/pagedList.ts";
import { Helmet } from "react-helmet";

const CatalogPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const [sortBy, setSortBy] = useState(searchParams.get("sort") || "newest");
    const currentPage = Number(searchParams.get("page")) || 1;

    const sortOptions: SortOption[] = [
        { value: "newest", label: 'Creation Date: Newest First' },
        { value: 'price-asc', label: 'Price: Low to High' },
        { value: 'price-desc', label: 'Price: High to Low' },
        { value: 'name-asc', label: 'Name: A-Z' },
        { value: 'name-desc', label: 'Name: Z-A' },
    ];

    const mockFilters: IFiltersResponse = {
        priceRange: { minPrice: "349", maxPrice: "47890" },
        gender: { maleCount: 78, femaleCount: 82, unisexCount: 54 },

        brands: [
            { id: "b1", name: "Nike", slug: "nike", clotheItemCount: 846 },
            { id: "b2", name: "Adidas", slug: "adidas", clotheItemCount: 131 },
            { id: "b3", name: "Puma", slug: "puma", clotheItemCount: 77 },
        ],

        clothingTypes: [
            { id: "t1", name: "T-Shirts", slug: "tees", clotheItemCount: 124 },
            { id: "t2", name: "Hoodies", slug: "hoodies", clotheItemCount: 58 },
        ],

        colors: [
            { id: "c1", name: "Dark", slug: "black", clotheItemCount: 86 },
            { id: "c2", name: "White", slug: "white", clotheItemCount: 74 },
        ],

        materials: [
            { id: "m1", name: "Cotton", slug: "cotton", clotheItemCount: 92 },
            { id: "m2", name: "Polyester", slug: "polyester", clotheItemCount: 67 },
        ],

        sizes: [
            { id: "s1", name: "S", slug: "s", clotheItemCount: 38 },
            { id: "s2", name: "M", slug: "m", clotheItemCount: 64 },
        ],

        tags: [
            { id: "tag1", name: "New", slug: "new", clotheItemCount: 28 },
            { id: "tag2", name: "Sale", slug: "sale", clotheItemCount: 35 },
        ],

        collections: [
            { id: "col1", name: "Spring–Summer 2025", slug: "ss25", clotheItemCount: 64 },
            { id: "col2", name: "Essentials", slug: "essentials", clotheItemCount: 73 },
        ],
    };

    const mockPagedClothes: PagedList<IClotheSummaryDTO> = {
        currentPage: 1,
        pageSize: 27,
        totalCount: 200,
        totalPages: 8,
        hasPrevious: false,
        hasNext: true,

        items: [
            {
                id: "1",
                name: "T-Shirt Base",
                slug: "basic-tee",
                price: 499,
                oldPrice: 699,
                discountPercent: 29,
                brand: {
                    id: "b1",
                    name: "Nike",
                    slug: "nike",
                    photoURL: "https://res-console.cloudinary.com/dkdljnfja/thumbnails/v1/image/upload/v1769175662/bmlrZUxvZ29feWkyczR2/template_primary/ZV9iYWNrZ3JvdW5kX3JlbW92YWwvZl9wbmc=",
                    createdAt: "2024-01-01",
                    updatedAt: "2024-01-01"
                },
                colors: [
                    {
                        id: "c1",
                        photoUrl: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769944208/ajnj_5_y96hah.webp",
                        colorId: "black",
                        hexCode: "#000",
                        colorSlug: "black"
                    }
                ],
                isAvailable: true
            },

            {
                id: "2",
                name: "Hoodie oversize",
                slug: "oversize-hoodie",
                price: 1199,
                brand: {
                    id: "b2",
                    name: "Adidas",
                    slug: "adidas",
                    photoURL: "https://res-console.cloudinary.com/dkdljnfja/thumbnails/v1/image/upload/v1769175662/bmlrZUxvZ29feWkyczR2/template_primary/ZV9iYWNrZ3JvdW5kX3JlbW92YWwvZl9wbmc=",
                    createdAt: "2024-01-01",
                    updatedAt: "2024-01-01"
                },
                colors: [
                    {
                        id: "c2",
                        photoUrl: "https://yesoriginal.com.ua/media/cache/catalog/products/c2/d5/cf/90256904-1-1340x1410_-jpeg-84.webp",
                        colorId: "white",
                        hexCode: "#fff",
                        colorSlug: "white"
                    }
                ],
                isAvailable: true
            },

            {
                id: "3",
                name: "Jacket leather",
                slug: "leather-jacket",
                price: 3499,
                brand: {
                    id: "b3",
                    name: "Zara",
                    slug: "zara",
                    photoURL: "https://res-console.cloudinary.com/dkdljnfja/thumbnails/v1/image/upload/v1769175662/bmlrZUxvZ29feWkyczR2/template_primary/ZV9iYWNrZ3JvdW5kX3JlbW92YWwvZl9wbmc=",
                    createdAt: "2024-01-01",
                    updatedAt: "2024-01-01"
                },
                colors: [
                    {
                        id: "c3",
                        photoUrl: "https://res.cloudinary.com/dkdljnfja/image/upload/v1769944046/%D1%84%D0%BE%D1%82%D0%BE_1_ykfs8f.webp",
                        colorId: "black",
                        hexCode: "#000",
                        colorSlug: "black"
                    }
                ],
                isAvailable: true
            }
        ]
    };

    const handleFilterChange = (filters: FilterState) => {
        const params = new URLSearchParams(searchParams);

        const setOrDelete = (key: string, arr: string[]) =>
            arr.length ? params.set(key, arr.join(",")) : params.delete(key);

        setOrDelete("brands", filters.brands);
        setOrDelete("clothingTypes", filters.clothingTypes);
        setOrDelete("colors", filters.colors);
        setOrDelete("materials", filters.materials);
        setOrDelete("sizes", filters.sizes);
        setOrDelete("tags", filters.tags);
        setOrDelete("collections", filters.collections);
        setOrDelete("gender", filters.gender);

        if (filters.minPrice !== Number(mockFilters.priceRange.minPrice)) {
            params.set("minPrice", filters.minPrice.toString());
        } else {
            params.delete("minPrice");
        }

        if (filters.maxPrice !== Number(mockFilters.priceRange.maxPrice)) {
            params.set("maxPrice", filters.maxPrice.toString());
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
        const params = new URLSearchParams(searchParams);
        params.set("page", page.toString());
        setSearchParams(params);
        window.scrollTo({ top: 0, behavior: "smooth" });
    };

    return (
        <PageWrapper>
            <Helmet>
                <title>{`Clothy — Clothing Catalog | Page ${currentPage} • ${mockPagedClothes.totalCount} items`}</title>
                <meta
                    name="description"
                    content={`Clothy clothing catalog: ${mockPagedClothes.totalCount}+ items. Filter by brand, size, and price — fast and convenient online shopping.`}
                />
                <meta property="og:title" content="Clothy — Clothing Catalog" />
                <meta
                    property="og:description"
                    content="Wide selection of clothing in the Clothy catalog. Discounts, new arrivals, and popular brands."
                />
            </Helmet>

            <div className={styles.catalogContainer}>
                <aside className={styles.filterSidebar}>
                    <CatalogFilter
                        filters={mockFilters}
                        onFilterChange={handleFilterChange}
                    />
                </aside>

                <main className={styles.catalogMain}>
                    <div className={styles.catalogHeader}>
                        <div className={styles.resultsCount}>
                            Items found: {mockPagedClothes.totalCount}
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
                                filters={mockFilters}
                                onFilterChange={handleFilterChange}
                            />
                            <SortSelect
                                value={sortBy}
                                options={sortOptions}
                                onChange={handleSortChange}
                            />
                        </div>
                    </div>

                    <div className={styles.productWrapper}>
                        <ProductList products={mockPagedClothes.items} />
                    </div>

                    <Pagination
                        currentPage={currentPage}
                        totalPages={mockPagedClothes.totalPages}
                        onPageChange={handlePageChange}
                    />
                </main>
            </div>
        </PageWrapper>
    );
};

export default CatalogPage;