import {Check, MessageSquare, Trash2} from "lucide-react";
import EmptyState from "../../../shared/ui/EmptyState/EmptyState.tsx";
import Pagination from "../../../shared/ui/Pagination/Pagination.tsx";
import Loader from "../../../shared/ui/Loader/Loader.tsx";
import {Helmet} from "react-helmet";
import {useOutletContext, useSearchParams} from "react-router-dom";
import {useEffect, useState} from "react";
import {reviewApi} from "../../../app/api/reviewApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import {getCurrentPage, handlePageChange} from "../../../shared/lib/paginationUtils.ts";
import type {IReviewReadDTO} from "../../../entities/reviewsService/interfaces/IReviewReadDTO.ts";
import type {PagedList} from "../../../shared/lib/pagedList.ts";
import ReviewRowCard from "../../../entities/reviewsService/reviewRow/ReviewRowCard.tsx";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import { useQueryClient } from "@tanstack/react-query";

const AdminReviewsPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);
    const {setPageHeader} = useOutletContext<AdminLayoutContext>();
    const queryClient = useQueryClient();

    const [reviews, setReviews] = useState<PagedList<IReviewReadDTO> | null>(null);
    const [isLoading, setIsLoading] = useState(true);
    const [confirmingId, setConfirmingId] = useState<string | null>(null);
    const [deletingId, setDeletingId] = useState<string | null>(null);

    useEffect(() => {
        setPageHeader({title: "Reviews", description: "Pending reviews awaiting moderation"});
    }, []);

    const fetchReviews = async () => {
        try {
            setIsLoading(true);
            const data = await reviewApi.getReviewsAsync({pageNumber: currentPage, status: "Pending"});
            setReviews(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchReviews();
    }, [currentPage]);

    const handleConfirm = async (id: string) => {
        try {
            setConfirmingId(id);
            await reviewApi.confirmReviewAsync(id);
            toast.success("Review confirmed.");
            await queryClient.invalidateQueries({ queryKey: ["clothe"] });
            fetchReviews();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setConfirmingId(null);
        }
    };

    const handleDelete = async (id: string) => {
        try {
            setDeletingId(id);
            await reviewApi.deleteReviewAsync(id);
            toast.success("Review deleted.");
            fetchReviews();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setDeletingId(null);
        }
    };

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    if (isLoading) return <Loader/>;

    if (!reviews || reviews.items.length === 0) {
        return (
            <>
                <Helmet>
                    <title>Admin — Reviews</title>
                </Helmet>
                <EmptyState
                    icon={<MessageSquare size={28} color="#6B6B6B"/>}
                    title="No pending reviews"
                    description="All reviews have been moderated."
                    buttons={[]}
                />
            </>
        );
    }

    return (
        <div>
            <Helmet>
                <title>Admin — Reviews</title>
                <meta name="description" content="Moderate pending product reviews."/>
            </Helmet>

            <div>
                {reviews.items.map((review) => (
                    <ReviewRowCard
                        key={review.id}
                        review={review}
                        actions={[
                            {
                                icon: <Check size={15}/>,
                                title: "Confirm",
                                disabled: confirmingId === review.id,
                                onClick: () => handleConfirm(review.id),
                            },
                            {
                                icon: <Trash2 size={15}/>,
                                title: "Delete",
                                danger: true,
                                disabled: deletingId === review.id,
                                onClick: () => handleDelete(review.id),
                            },
                        ]}
                    />
                ))}
            </div>

            {reviews.totalPages > 1 && (
                <Pagination
                    currentPage={reviews.currentPage}
                    totalPages={reviews.totalPages}
                    onPageChange={onPageChange}
                />
            )}
        </div>
    );
};

export default AdminReviewsPage;