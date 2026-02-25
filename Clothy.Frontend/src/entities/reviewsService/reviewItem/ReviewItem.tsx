import styles from "./ReviewItem.module.css";
import {formatDate} from "../../../shared/lib/formatDate.ts";
import type {IReviewReadDTO} from "../interfaces/IReviewReadDTO.ts";
import StarRating from "../starRating/StarRating.tsx";

interface ReviewItemProps {
    review: IReviewReadDTO;
}

const ReviewItem: React.FC<ReviewItemProps> = ({ review }) => {
    return (
        <div className={styles.reviewItem}>
            <div className={styles.reviewHeader}>
                <img
                    src={review.user.photoUrl}
                    className={styles.reviewAvatar}
                    alt={`${review.user.firstName} ${review.user.lastName}`}
                />
                <div className={styles.reviewInfo}>
                    <p className={styles.reviewAuthor}>{review.user.firstName} {review.user.lastName}</p>
                    <StarRating rating={review.rating} />
                </div>
                <span className={styles.reviewDate}>{formatDate(review.createdAt)}</span>
            </div>
            <p className={styles.reviewText}>{review.comment}</p>
        </div>
    );
};

export default ReviewItem;