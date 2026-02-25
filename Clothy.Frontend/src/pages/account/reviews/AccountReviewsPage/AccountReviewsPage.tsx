import { MessageSquare, Trash2, Pencil } from "lucide-react";
import EmptyState from "../../../../shared/ui/EmptyState/EmptyState.tsx";
import Pagination from "../../../../shared/ui/Pagination/Pagination.tsx";
import Loader from "../../../../shared/ui/Loader/Loader.tsx";
import Modal from "../../../../shared/layout/Modal/Modal.tsx";
import { Helmet } from "react-helmet";
import { useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { reviewApi } from "../../../../app/api/reviewApi.ts";
import { decodeJwt } from "../../../../shared/lib/decodeJwt.ts";
import { useAuthStore } from "../../../../app/api/stores/authStore.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../../shared/lib/errorHandler.ts";
import { getCurrentPage, handlePageChange } from "../../../../shared/lib/paginationUtils.ts";
import type { IReviewReadDTO } from "../../../../entities/reviewsService/interfaces/IReviewReadDTO.ts";
import type { PagedList } from "../../../../shared/lib/pagedList.ts";
import styles from "./AccountReviewsPage.module.css";
import ReviewForm from "../../../../features/forms/reviewForm/ReviewForm.tsx";
import ReviewRowCard from "../../../../entities/reviewsService/reviewRow/ReviewRowCard.tsx";
import { useQueryClient } from "@tanstack/react-query";

const AccountReviewsPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);
    const queryClient = useQueryClient();

    const [reviews, setReviews] = useState<PagedList<IReviewReadDTO>>();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [editingReview, setEditingReview] = useState<IReviewReadDTO | null>(null);
    const [deletingId, setDeletingId] = useState<string | null>(null);

    const accessToken = useAuthStore.getState().accessToken;

    const fetchUsersReviews = async () => {
        try {
            setIsLoading(true);
            const decodeJwtData = decodeJwt(accessToken!);
            const userId = decodeJwtData.sub;
            const data = await reviewApi.getReviewsAsync({ pageNumber: currentPage, userId });
            setReviews(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchUsersReviews();
    }, [currentPage]);

    const handleDelete = async (id: string) => {
        try {
            setDeletingId(id);
            await reviewApi.deleteReviewAsync(id);
            toast.success("Review deleted.");
            await queryClient.invalidateQueries({ queryKey: ["clothe"] });
            fetchUsersReviews();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setDeletingId(null);
        }
    };

    const handleEditClick = async (id: string) => {
        try {
            const review = await reviewApi.getReviewByIdAsync(id);
            setEditingReview(review);
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
    };

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    if (isLoading) return <Loader />;

    return (
        <div className={styles.container}>
            <Helmet>
                <title>My Reviews | Account</title>
                <meta name="description" content="View and manage your product reviews in your account." />
            </Helmet>

            {editingReview && (
                <Modal title="Edit review" onClose={() => setEditingReview(null)}>
                    <ReviewForm
                        method="update"
                        reviewId={editingReview.id}
                        clotheId={editingReview.clotheInfo?.clotheItemId}
                        initialData={{
                            rating: editingReview.rating,
                            comment: editingReview.comment,
                        }}
                        onSuccess={async () => {
                            setEditingReview(null);
                            await queryClient.invalidateQueries({ queryKey: ["clothe"] });
                            fetchUsersReviews();
                        }}
                    />
                </Modal>
            )}

            {!reviews || reviews.items.length === 0 ? (
                <EmptyState
                    icon={<MessageSquare size={28} color="#6B6B6B" />}
                    title="No reviews yet"
                    description="You haven't written any reviews. Share your opinion on products you've purchased."
                    buttons={[{ label: "Browse Products", to: "/catalog", variant: "primary", size: "md" }]}
                />
            ) : (
                <>
                    <div className={styles.list}>
                        {reviews.items.map((review) => (
                            <ReviewRowCard
                                key={review.id}
                                review={review}
                                actions={[
                                    {
                                        icon: <Pencil size={15} />,
                                        title: "Edit",
                                        onClick: () => handleEditClick(review.id),
                                    },
                                    {
                                        icon: <Trash2 size={15} />,
                                        title: "Delete",
                                        danger: true,
                                        disabled: deletingId === review.id,
                                        onClick: () => handleDelete(review.id),
                                    },
                                ]}
                            />
                        ))}
                    </div>

                    <Pagination
                        currentPage={reviews.currentPage}
                        totalPages={reviews.totalPages}
                        onPageChange={onPageChange}
                    />
                </>
            )}
        </div>
    );
};

export default AccountReviewsPage;