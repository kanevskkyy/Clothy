import type { ReactNode } from "react";
import type { IReviewReadDTO } from "../interfaces/IReviewReadDTO.ts";
import StarRating from "../starRating/StarRating.tsx";
import { formatDate } from "../../../shared/lib/formatDate.ts";
import styles from "./ReviewRowCard.module.css";

export interface ReviewRowAction {
    icon: ReactNode;
    onClick: () => void;
    title?: string;
    disabled?: boolean;
    danger?: boolean;
}

interface ReviewRowCardProps {
    review: IReviewReadDTO;
    actions?: ReviewRowAction[];
}

const ReviewRowCard = ({ review, actions }: ReviewRowCardProps) => {
    return (
        <div className={styles.row}>
            <img
                src={review?.clotheInfo?.clothePhotoURL}
                alt={review?.clotheInfo?.clotheName}
                className={styles.image}
            />

            <div className={styles.content}>
                <p className={styles.name}>{review?.clotheInfo?.clotheName}</p>
                <StarRating rating={review.rating} />
                <p className={styles.comment}>{review.comment}</p>
                <p className={styles.date}>{formatDate(review.createdAt)}</p>
            </div>

            {actions && actions.length > 0 && (
                <div className={styles.actions}>
                    {actions.map((action, i) => (
                        <button
                            key={i}
                            className={`${styles.btn} ${action.danger ? styles.danger : ""}`}
                            onClick={action.onClick}
                            disabled={action.disabled}
                            title={action.title}
                        >
                            {action.icon}
                        </button>
                    ))}
                </div>
            )}
        </div>
    );
};

export default ReviewRowCard;