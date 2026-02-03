import { useState } from "react";
import type { IReviewReadDTO } from "../interfaces/IReviewReadDTO";
import type { IReviewStatistic } from "../interfaces/IReviewStatistic";
import type { IQuestionReadDTO } from "../../questions/interfaces/IQuestionReadDTO";
import styles from "./ReviewsSection.module.css";
import ReviewItem from "../reviewItem/ReviewItem.tsx";
import QuestionItem from "../../questions/questionItem/QuestionItem.tsx";

interface ReviewsSectionProps {
    reviews: IReviewReadDTO[];
    statistics: IReviewStatistic;
    questions: IQuestionReadDTO[];
}

const ReviewsSection: React.FC<ReviewsSectionProps> = ({ reviews, statistics, questions }) => {
    const [activeTab, setActiveTab] = useState<'reviews' | 'questions'>('reviews');

    const calculatePercentage = (count: number) => {
        if (statistics.totalReviews === 0) return 0;
        return (count / statistics.totalReviews) * 100;
    };

    return (
        <div className={styles.reviewsWrapper}>
            <div className={styles.reviewsSection}>
                <div className={styles.reviewsTabs}>
                    <button
                        className={`${styles.tabBtn} ${activeTab === 'reviews' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('reviews')}
                    >
                        Відгуки
                    </button>
                    <button
                        className={`${styles.tabBtn} ${activeTab === 'questions' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('questions')}
                    >
                        Питання
                    </button>
                </div>

                {activeTab === 'reviews' && (
                    <>
                        <div className={styles.ratingSummary}>
                            <div className={styles.ratingNumber}>
                                <h2>{statistics.averageRating}</h2>
                                <span>({statistics.totalReviews} відгуків)</span>
                            </div>

                            <div className={styles.ratingBars}>
                                <div className={styles.ratingBarItem}>
                                    <span className={styles.ratingLabel}>5 <span className={styles.star}>★</span></span>
                                    <div className={styles.barContainer}>
                                        <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.fiveStars)}%` }}></div>
                                    </div>
                                    <span className={styles.ratingCount}>{statistics.fiveStars}</span>
                                </div>
                                <div className={styles.ratingBarItem}>
                                    <span className={styles.ratingLabel}>4 <span className={styles.star}>★</span></span>
                                    <div className={styles.barContainer}>
                                        <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.fourStars)}%` }}></div>
                                    </div>
                                    <span className={styles.ratingCount}>{statistics.fourStars}</span>
                                </div>
                                <div className={styles.ratingBarItem}>
                                    <span className={styles.ratingLabel}>3 <span className={styles.star}>★</span></span>
                                    <div className={styles.barContainer}>
                                        <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.threeStars)}%` }}></div>
                                    </div>
                                    <span className={styles.ratingCount}>{statistics.threeStars}</span>
                                </div>
                                <div className={styles.ratingBarItem}>
                                    <span className={styles.ratingLabel}>2 <span className={styles.star}>★</span></span>
                                    <div className={styles.barContainer}>
                                        <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.twoStars)}%` }}></div>
                                    </div>
                                    <span className={styles.ratingCount}>{statistics.twoStars}</span>
                                </div>
                                <div className={styles.ratingBarItem}>
                                    <span className={styles.ratingLabel}>1 <span className={styles.star}>★</span></span>
                                    <div className={styles.barContainer}>
                                        <div className={styles.barFill} style={{ width: `${calculatePercentage(statistics.oneStars)}%` }}></div>
                                    </div>
                                    <span className={styles.ratingCount}>{statistics.oneStars}</span>
                                </div>
                            </div>
                        </div>

                        {reviews.map(review => (
                            <ReviewItem key={review.id} review={review} />
                        ))}
                    </>
                )}

                {activeTab === 'questions' && questions.map(question => (
                    <QuestionItem key={question.id} question={question} />
                ))}
            </div>
        </div>
    );
};

export default ReviewsSection;