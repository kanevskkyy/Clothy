import {Check, MessageSquare, Trash2} from "lucide-react";
import Pagination from "../../../shared/ui/Pagination/Pagination.tsx";
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
import AdminCard from "../ui/AdminCard.tsx";
import AdminToolbar from "../ui/AdminToolbar.tsx";
import AdminEmptyState from "../ui/AdminEmptyState.tsx";
import {AdminRowsSkeleton} from "../ui/AdminSkeleton.tsx";
import styles from "./AdminReviewsPage.module.css";

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

    if (isLoading) {
        return (
            <AdminCard flush>
                <AdminRowsSkeleton rows={4} columns={3} label="Loading reviews"/>
            </AdminCard>
        );
    }

    if (!reviews || reviews.items.length === 0) {
        return (
            <>
                <Helmet>
                    <title>Admin — Reviews</title>
                </Helmet>
                <AdminCard>
                    <AdminEmptyState
                        icon={<MessageSquare size={22}/>}
                        title="No pending reviews"
                        description="All reviews have been moderated."
                    />
                </AdminCard>
            </>
        );
    }

    return (
        <div>
            <Helmet>
                <title>Admin — Reviews</title>
                <meta name="description" content="Moderate pending product reviews."/>
            </Helmet>

            <AdminToolbar
                start={<span className={styles.count}>{reviews.totalCount} pending reviews</span>}
            />

            <div className={styles.list}>
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