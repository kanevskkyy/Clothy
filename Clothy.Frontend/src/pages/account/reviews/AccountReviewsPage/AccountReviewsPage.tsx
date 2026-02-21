import { MessageSquare } from "lucide-react";
import EmptyState from "../../../../shared/ui/EmptyState/EmptyState.tsx";
import ReviewCard from "../../../../entities/reviewsService/reviews/reviewCard/ReviewCard.tsx";
import Pagination from "../../../../shared/ui/Pagination/Pagination.tsx";
import Loader from "../../../../shared/ui/Loader/Loader.tsx";
import Modal from "../../../../shared/layout/Modal/Modal.tsx";
import Button from "../../../../shared/ui/Button/Button.tsx";
import { Helmet } from "react-helmet";
import { useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { reviewApi } from "../../../../app/api/reviewApi.ts";
import { decodeJwt } from "../../../../shared/lib/decodeJwt.ts";
import { useAuthStore } from "../../../../app/api/stores/authStore.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../../shared/lib/errorHandler.ts";
import { getCurrentPage, handlePageChange } from "../../../../shared/lib/paginationUtils.ts";
import type { IReviewReadDTO } from "../../../../entities/reviewsService/reviews/IReviewReadDTO.ts";
import type { PagedList } from "../../../../shared/lib/pagedList.ts";
import styles from "./AccountReviewsPage.module.css";
import ReviewForm from "../../../../features/forms/reviewForm/ReviewForm.tsx";

const AccountReviewsPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);

    const [reviews, setReviews] = useState<PagedList<IReviewReadDTO>>();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const [editingReview, setEditingReview] = useState<IReviewReadDTO | null>(null);

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
            await reviewApi.deleteReviewAsync(id);
            toast.success("Review deleted.");
            fetchUsersReviews();
        } catch (error) {
            toast.error(getErrorMessage(error));
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
                        initialData={{
                            rating: editingReview.rating,
                            comment: editingReview.comment,
                        }}
                        onSuccess={() => {
                            setEditingReview(null);
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
                    <div className={styles.reviewsList}>
                        {reviews.items.map((review) => (
                            <div key={review.id} className={styles.reviewWrapper}>
                                <ReviewCard review={review} />
                                <div className={styles.actions}>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => handleEditClick(review.id)}
                                    >
                                        Edit
                                    </Button>
                                    <Button
                                        variant="outline"
                                        size="sm"
                                        onClick={() => handleDelete(review.id)}
                                    >
                                        Delete
                                    </Button>
                                </div>
                            </div>
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