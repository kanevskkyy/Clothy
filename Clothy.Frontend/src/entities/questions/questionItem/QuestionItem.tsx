import type { IQuestionReadDTO } from "../interfaces/IQuestionReadDTO";
import styles from "./QuestionItem.module.css";

interface QuestionItemProps {
    question: IQuestionReadDTO;
}

const QuestionItem: React.FC<QuestionItemProps> = ({ question }) => {
    const formatDate = (dateString: string) => {
        const date = new Date(dateString);
        return date.toLocaleDateString('uk-UA');
    };

    return (
        <div className={styles.questionItem}>
            <div className={styles.questionSection}>
                <div className={styles.questionWrapper}>
                    <img
                        src={question.user.photoUrl}
                        alt={`${question.user.firstName} ${question.user.lastName}`}
                        className={styles.questionAvatar}
                    />
                    <div className={styles.questionInfo}>
                        <p className={styles.questionAuthor}>{question.user.firstName} {question.user.lastName}</p>
                        <p className={styles.questionText}>{question.questionText}</p>
                    </div>
                </div>
                <div className={styles.questionDate}>{formatDate(new Date().toISOString())}</div>
            </div>
            {question.answers.map(answer => (
                <div key={answer.id} className={styles.answerSection}>
                    <div className={styles.answerWrapper}>
                        <img
                            src={answer.user.photoUrl}
                            alt={`${answer.user.firstName} ${answer.user.lastName}`}
                            className={styles.answerAvatar}
                        />
                        <div className={styles.questionInfo}>
                            <p className={styles.answerAuthor}>{answer.user.firstName} {answer.user.lastName}</p>
                            <p className={styles.answerText}>{answer.answerText}</p>
                        </div>
                    </div>
                    <div className={styles.answerDate}>{formatDate(new Date().toISOString())}</div>
                </div>
            ))}
        </div>
    );
};

export default QuestionItem;
