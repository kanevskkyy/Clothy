import { useState } from "react";
import type { PagedList } from "../../../shared/pagedList";
import type { IReviewReadDTO } from "../IReviewReadDTO.ts";
import type { IReviewStatistic } from "../IReviewStatistic.ts";
import type { IQuestionReadDTO } from "../../questions/IQuestionReadDTO.ts";
import styles from "./ReviewsSection.module.css";
import ReviewItem from "../reviewItem/ReviewItem.tsx";
import QuestionItem from "../../questions/questionItem/QuestionItem.tsx";
import Pagination from "../../../shared/Pagination/Pagination.tsx";

interface ReviewsSectionProps {
    reviews: PagedList<IReviewReadDTO>;
    statistics: IReviewStatistic;
    questions: PagedList<IQuestionReadDTO>;
}

const ReviewsSection: React.FC<ReviewsSectionProps> = ({ reviews, statistics, questions }) => {
    const [activeTab, setActiveTab] = useState<'reviews' | 'questions'>('reviews');

    const calculatePercentage = (count: number) => {
        if (statistics.totalReviews === 0) return 0;
        return (count / statistics.totalReviews) * 100;
    };

    const handleReviewPageChange = (page: number) => {
        // TODO: Implement API call to fetch reviews for the selected page
        console.log('Fetching reviews for page:', page);
    };

    const handleQuestionPageChange = (page: number) => {
        // TODO: Implement API call to fetch questions for the selected page
        console.log('Fetching questions for page:', page);
    };

    return (
        <div className={styles.reviewsWrapper}>
            <div className={styles.reviewsSection}>
                <div className={styles.reviewsTabs}>
                    <button
                        className={`${styles.tabBtn} ${activeTab === 'reviews' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('reviews')}
                    >
                        Reviews
                    </button>
                    <button
                        className={`${styles.tabBtn} ${activeTab === 'questions' ? styles.activeTab : ''}`}
                        onClick={() => setActiveTab('questions')}
                    >
                        Question
                    </button>
                </div>

                {activeTab === 'reviews' && (
                    <>
                        <div className={styles.ratingSummary}>
                            <div className={styles.ratingNumber}>
                                <h2>{statistics.averageRating}</h2>
                                <span>({statistics.totalReviews} reviews)</span>
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

                        {reviews.items.map(review => (
                            <ReviewItem key={review.id} review={review} />
                        ))}

                        <Pagination
                            currentPage={reviews.currentPage}
                            totalPages={reviews.totalPages}
                            onPageChange={handleReviewPageChange}
                        />
                    </>
                )}

                {activeTab === 'questions' && (
                    <>
                        {questions.items.map(question => (
                            <QuestionItem key={question.id} question={question} />
                        ))}

                        <Pagination
                            currentPage={questions.currentPage}
                            totalPages={questions.totalPages}
                            onPageChange={handleQuestionPageChange}
                        />
                    </>
                )}
            </div>
        </div>
    );
};

export default ReviewsSection;