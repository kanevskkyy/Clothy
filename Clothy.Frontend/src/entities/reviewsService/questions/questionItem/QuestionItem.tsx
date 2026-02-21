import { useState } from "react";
import type { IQuestionReadDTO } from "../IQuestionReadDTO.ts";
import styles from "./QuestionItem.module.css";
import { formatDate } from "../../../../shared/lib/formatDate.ts";
import { CornerDownLeft } from "lucide-react";
import AnswerCreateForm from "../../../../features/forms/answerCreateForm/AnswerCreateForm.tsx";
import type {IAnswerReadDTO} from "../../answers/IAnswerReadDTO.ts";

interface QuestionItemProps {
    question: IQuestionReadDTO;
    onInvalidate?: () => void;
}

const QuestionItem: React.FC<QuestionItemProps> = ({ question, onInvalidate }) => {
    const [isReplying, setIsReplying] = useState(false);
    const [answers, setAnswers] = useState<IAnswerReadDTO[]>(question.answers);

    const handleAnswerSuccess = (newAnswer: IAnswerReadDTO) => {
        setAnswers(prev => [...prev, newAnswer]);
        setIsReplying(false);
        onInvalidate?.();
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
                <div className={styles.questionMeta}>
                    <span className={styles.questionDate}>{formatDate(question.createdAt)}</span>
                    <button className={styles.replyBtn} onClick={() => setIsReplying(prev => !prev)}>
                        <CornerDownLeft size={16} />
                        {isReplying ? "Cancel" : "Reply"}
                    </button>
                </div>
            </div>

            {answers.map(answer => (
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
                    <div className={styles.answerDate}>{formatDate(answer.createdAt)}</div>
                </div>
            ))}

            {isReplying && (
                <AnswerCreateForm
                    questionId={question.id}
                    onSuccess={handleAnswerSuccess}
                    onCancel={() => setIsReplying(false)}
                />
            )}
        </div>
    );
};

export default QuestionItem;