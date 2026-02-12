import type {SetURLSearchParams} from "react-router-dom";

export const handlePageChange = (
    page: number,
    searchParams: URLSearchParams,
    setSearchParams: SetURLSearchParams
) => {
    const params = new URLSearchParams(searchParams);
    params.set("page", page.toString());
    setSearchParams(params);
    window.scrollTo({ top: 0, behavior: "smooth" });
};

export const getCurrentPage = (searchParams: URLSearchParams): number => {
    return Number(searchParams.get("page")) || 1;
};