import { useState, useEffect } from "react";
import {useSearchParams, useNavigate, useOutletContext} from "react-router-dom";
import { Plus, PackageSearch } from "lucide-react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { Helmet } from "react-helmet";
import styles from "./AdminClothesPage.module.css";
import {getCurrentPage, handlePageChange as handlePageChangeUtil} from "../../../../shared/lib/paginationUtils";
import type { PagedList } from "../../../../shared/lib/pagedList";
import type { IClotheSummaryDTO } from "../../../../entities/catalogService/interfaces/clothe/IClotheSummaryDTO";
import { catalogApi } from "../../../../app/api/catalogApi";
import {getErrorMessage} from "../../../../shared/lib/errorHandler.ts";
import type { SortOption } from "../../../../features/catalog/sortSelect/SortSelect.tsx";
import type { FilterState } from "../../../../features/catalog/catalogFilter/CatalogFilter.tsx";
import { parsePrice } from "../../../../shared/lib/parsePrice.ts";
import Loader from "../../../../shared/ui/Loader/Loader.tsx";
import Container from "../../../../shared/layout/Container/Container.tsx";
import Button from "../../../../shared/ui/Button/Button.tsx";
import SortSelect from "../../../../features/catalog/sortSelect/SortSelect.tsx";
import CatalogFilter from "../../../../features/catalog/catalogFilter/CatalogFilter.tsx";
import EmptyState from "../../../../shared/ui/EmptyState/EmptyState.tsx";
import AdminProductList from "../productList/AdminProductList.tsx";
import Pagination from "../../../../shared/ui/Pagination/Pagination.tsx";
import type {AdminLayoutContext} from "../../../../features/auth/admin/adminLayout/AdminLayout.tsx";

const AdminClothesPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const [sortBy, setSortBy] = useState(searchParams.get("sort") || "newest");
    const currentPage = getCurrentPage(searchParams);
    const navigate = useNavigate();
    const { setPageHeader } = useOutletContext<AdminLayoutContext>();

    const [pagedClothes, setPagedClothes] = useState<PagedList<IClotheSummaryDTO> | null>(null);
    const [loading, setLoading] = useState(true);

    const { data: filters } = useQuery({
        queryKey: ["catalog-filters"],
        queryFn: () => catalogApi.getFiltersAsync(),
        staleTime: 1000 * 60 * 15,
        throwOnError: (error) => {
            toast.error(getErrorMessage(error));
            return false;
        },
    });

    const sortOptions: SortOption[] = [
        { value: "newest", label: "Creation Date: Newest First" },
        { value: "price-asc", label: "Price: Low to High" },
        { value: "price-desc", label: "Price: High to Low" },
        { value: "name-asc", label: "Name: A-Z" },
        { value: "name-desc", label: "Name: Z-A" },
    ];

    const getInitialFilters = (): FilterState => {
        if (!filters) {
            return {
                brands: [], clothingTypes: [], colors: [], materials: [],
                sizes: [], tags: [], collections: [], gender: [],
                minPrice: 0, maxPrice: 0,
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

    const convertToIds = (
        values: string[],
        items: Array<{ id: string; slug?: string; name?: string }>,
        key: "slug" | "name" = "slug"
    ): string[] =>
        values
            .map((val) => items.find((i) => i[key] === val)?.id)
            .filter((id): id is string => id !== undefined);

    const fetchClothes = async () => {
        if (!filters) return;
        setLoading(true);
        try {
            const selectedFilters = getInitialFilters();

            const brandIds = convertToIds(selectedFilters.brands, filters.brands);
            const clothingTypeIds = convertToIds(selectedFilters.clothingTypes, filters.clothingTypes);
            const colorIds = convertToIds(selectedFilters.colors, filters.colors);
            const materialIds = convertToIds(selectedFilters.materials, filters.materials);
            const sizeIds = convertToIds(selectedFilters.sizes, filters.sizes, "name");
            const tagIds = convertToIds(selectedFilters.tags, filters.tags);
            const collectionIds = convertToIds(selectedFilters.collections, filters.collections);

            let sortByParam: "price" | "name" | undefined;
            let sortDescending = false;
            if (sortBy === "price-asc") sortByParam = "price";
            else if (sortBy === "price-desc") { sortByParam = "price"; sortDescending = true; }
            else if (sortBy === "name-asc") sortByParam = "name";
            else if (sortBy === "name-desc") { sortByParam = "name"; sortDescending = true; }

            const defaultMinPrice = parsePrice(filters.priceRange.minPrice);
            const defaultMaxPrice = parsePrice(filters.priceRange.maxPrice);

            const data = await catalogApi.getClothesPagedAsync({
                pageNumber: currentPage,
                pageSize: 12,
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
                sortDescending: sortDescending || undefined,
            });
            setPagedClothes(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (filters) fetchClothes();
    }, [filters, searchParams, sortBy, currentPage]);

    useEffect(() => {
        setPageHeader({ title: "Clothes", description: "Mange your clothes here"});
    }, []);

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
        newSort === "newest" ? params.delete("sort") : params.set("sort", newSort);
        setSearchParams(params);
    };

    const handlePageChange = (page: number) => {
        handlePageChangeUtil(page, searchParams, setSearchParams);
    };

    if (!filters || !pagedClothes) return <Loader />;

    return (
        <Container paddingY={30} paddingX={10}>
            <Helmet>
                <title>{`Admin — Clothes Management | Page ${currentPage}`}</title>
            </Helmet>

            <div className={styles.pageHeader}>
                <h1 className={styles.pageTitle}>Clothes</h1>
                <Button
                    variant="primary"
                    size="sm"
                    icon={<Plus size={16} />}
                    onClick={() => navigate("/admin/clothes/create")}
                >
                    New clothe
                </Button>
            </div>

            <div className={styles.catalogContainer}>
                <aside className={styles.filterSidebar}>
                    <CatalogFilter
                        backgroundColor="#f5f5f0"
                        filters={filters}
                        initialFilters={getInitialFilters()}
                        onFilterChange={handleFilterChange}
                    />
                </aside>

                <main className={styles.catalogMain}>
                    <div className={styles.catalogHeader}>
                        <div className={styles.desktopSort}>
                            <SortSelect value={sortBy} options={sortOptions} onChange={handleSortChange} />
                        </div>
                        <div className={styles.mobileFiltersRow}>
                            <CatalogFilter
                                backgroundColor="#f5f5f0"
                                filters={filters}
                                initialFilters={getInitialFilters()}
                                onFilterChange={handleFilterChange}
                            />
                            <SortSelect value={sortBy} options={sortOptions} onChange={handleSortChange} />
                        </div>
                    </div>

                    {loading ? (
                        <Loader />
                    ) : pagedClothes.items.length === 0 ? (
                        <EmptyState
                            icon={<PackageSearch size={28} color="#6B6B6B" />}
                            title="No items found"
                            description="Try adjusting your filters or create a new clothe."
                            buttons={[
                                {
                                    label: "Reset Filters",
                                    onClick: () => setSearchParams(new URLSearchParams()),
                                    variant: "primary",
                                    size: "md",
                                },
                            ]}
                        />
                    ) : (
                        <>
                            <div className={styles.productWrapper}>
                                <AdminProductList products={pagedClothes.items} />
                            </div>
                            {pagedClothes.totalPages > 1 && (
                                <Pagination
                                    currentPage={currentPage}
                                    totalPages={pagedClothes.totalPages}
                                    onPageChange={handlePageChange}
                                />
                            )}
                        </>
                    )}
                </main>
            </div>
        </Container>
    );
};

export default AdminClothesPage;