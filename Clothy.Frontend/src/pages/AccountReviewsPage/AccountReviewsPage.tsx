import styles from "./AccountReviewsPage.module.css";
import type { IReviewReadDTO } from "../../entities/reviewsService/reviews/IReviewReadDTO";
import ReviewCard from "../../entities/reviewsService/reviews/reviewCard/ReviewCard.tsx";
import Pagination from "../../shared/Pagination/Pagination.tsx";
import { Helmet } from "react-helmet";
import { useSearchParams } from "react-router-dom";
import type { PagedList } from "../../shared/utils/pagedList.ts";
import {getCurrentPage, handlePageChange} from "../../shared/utils/paginationUtils.ts";

const AccountReviewsPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const currentPage = getCurrentPage(searchParams);

    // TODO: Connect to API and fetch paged reviews based on currentPage
    const mockPagedReviews: PagedList<IReviewReadDTO> = {
        currentPage: currentPage,
        totalPages: 1,
        pageSize: 10,
        totalCount: 2,
        hasPrevious: false,
        hasNext: false,
        items: [
            {
                id: "1",
                clotheInfo: {
                    clotheItemId: "c1",
                    clotheName: "Classic White T-Shirt",
                    clothePhotoURL: "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?w=400",
                },
                user: {
                    id: "u1",
                    firstName: "John",
                    lastName: "Doe",
                    photoUrl: "https://i.pravatar.cc/150?img=1",
                },
                rating: 5,
                comment: "Great quality! Very comfortable and fits perfectly.",
                status: "approved",
                createdAt: "2024-02-10T10:30:00Z",
                updatedAt: "2024-02-10T10:30:00Z",
            },
            {
                id: "2",
                clotheInfo: {
                    clotheItemId: "c2",
                    clotheName: "Blue Denim Jeans",
                    clothePhotoURL: "https://images.unsplash.com/photo-1542272604-787c3835535d?w=400",
                },
                user: {
                    id: "u1",
                    firstName: "John",
                    lastName: "Doe",
                    photoUrl: "https://i.pravatar.cc/150?img=1",
                },
                rating: 4,
                comment: "Nice jeans, but the sizing runs a bit small.",
                status: "approved",
                createdAt: "2024-02-08T14:20:00Z",
                updatedAt: "2024-02-08T14:20:00Z",
            },
        ],
    };

    const onPageChange = (page: number) => {
        handlePageChange(page, searchParams, setSearchParams);
    };

    return (
        <div className={styles.container}>
            <Helmet>
                <title>My Reviews | Account</title>
                <meta
                    name="description"
                    content="View and manage your product reviews in your account."
                />
            </Helmet>

            <h1 className={styles.title}>My Reviews</h1>

            {mockPagedReviews.items.length === 0 ? (
                <p className={styles.empty}>You haven't written any reviews yet.</p>
            ) : (
                <>
                    <div className={styles.reviewsList}>
                        {mockPagedReviews.items.map((review) => (
                            <ReviewCard key={review.id} review={review} />
                        ))}
                    </div>

                    <Pagination
                        currentPage={mockPagedReviews.currentPage}
                        totalPages={mockPagedReviews.totalPages}
                        onPageChange={onPageChange}
                    />
                </>
            )}
        </div>
    );
};

export default AccountReviewsPage;