import ReactSelect, { type Props as SelectProps } from 'react-select';
import styles from './Select.module.css';

interface CustomSelectProps extends SelectProps {
    error?: boolean;
}

const Select = ({ error, className, ...rest }: CustomSelectProps) => {
    return (
        <ReactSelect
            className={`${styles.select} ${error ? styles.error : ''} ${className || ''}`}
            classNamePrefix="custom-select"
            {...rest}
            styles={{
                control: (base, state) => ({
                    ...base,
                    minHeight: '48px',
                    border: error
                        ? '1px solid #dc3545'
                        : state.isFocused
                            ? '1px solid #1a1a1a'
                            : '1px solid #e5e5e5',
                    borderRadius: '8px',
                    boxShadow: state.isFocused
                        ? error
                            ? '0 0 0 3px rgba(220, 53, 69, 0.1)'
                            : '0 0 0 3px rgba(26, 26, 26, 0.05)'
                        : 'none',
                    '&:hover': {
                        border: error ? '1px solid #dc3545' : '1px solid #1a1a1a',
                    },
                }),
                option: (base, state) => ({
                    ...base,
                    backgroundColor: state.isSelected
                        ? '#f5f5f5'
                        : state.isFocused
                            ? '#f5f5f5'
                            : '#fff',
                    color: state.isSelected ? '#1a1a1a' : '#1a1a1a',
                    cursor: 'pointer',
                    '&:active': {
                        backgroundColor: '#f5f5f5',
                    },
                }),
                placeholder: (base) => ({
                    ...base,
                    color: '#999',
                }),
            }}
        />
    );
};

export default Select;