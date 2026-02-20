import { MessageSquare } from "lucide-react";
import EmptyState from "../../shared/EmptyState/EmptyState";
import ReviewCard from "../../entities/reviewsService/reviews/reviewCard/ReviewCard.tsx";
import Pagination from "../../shared/Pagination/Pagination.tsx";
import Loader from "../../shared/Loader/Loader.tsx";
import { Helmet } from "react-helmet";
import { useSearchParams } from "react-router-dom";
import { useEffect, useState } from "react";
import { reviewApi } from "../../app/api/reviewApi.ts";
import { decodeJwt } from "../../shared/utils/decodeJwt.ts";
import { useAuthStore } from "../../app/api/stores/authStore.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../shared/utils/errorHandler.ts";
import { getCurrentPage, handlePageChange } from "../../shared/utils/paginationUtils.ts";
import type { IReviewReadDTO } from "../../entities/reviewsService/reviews/IReviewReadDTO";
import type { PagedList } from "../../shared/utils/pagedList.ts";
import styles from "./AccountReviewsPage.module.css";

const AccountReviewsPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);

    const [reviews, setReviews] = useState<PagedList<IReviewReadDTO>>();
    const [isLoading, setIsLoading] = useState<boolean>(false);
    const accessToken = useAuthStore.getState().accessToken;

    useEffect(() => {
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

        fetchUsersReviews();
    }, [currentPage]);

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    if (isLoading) return <Loader />;

    return (
        <div className={styles.container}>
            <Helmet>
                <title>My Reviews | Account</title>
                <meta
                    name="description"
                    content="View and manage your product reviews in your account."
                />
            </Helmet>

            {!reviews || reviews.items.length === 0 ? (
                <EmptyState
                    icon={<MessageSquare size={28} color="#6B6B6B" />}
                    title="No reviews yet"
                    description="You haven't written any reviews. Share your opinion on products you've purchased."
                    buttons={[
                        {
                            label: "Browse Products",
                            to: "/catalog",
                            variant: "primary",
                            size: "md"
                        }
                    ]}
                />
            ) : (
                <>
                    <div className={styles.reviewsList}>
                        {reviews.items.map((review) => (
                            <ReviewCard key={review.id} review={review} />
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