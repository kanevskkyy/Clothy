import ReactSelect, { type Props as SelectProps } from 'react-select';
import selectStyles from './Select.module.css';

interface CustomSelectProps extends SelectProps {
    error?: boolean;
}

type AnyStyles = Record<string, (base: any, state: any) => any>;

const Select = ({ error, className, styles: externalStyles, ...rest }: CustomSelectProps) => {
    const internalStyles: AnyStyles = {
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
            backgroundColor: state.isSelected || state.isFocused ? '#f5f5f5' : '#fff',
            color: '#1a1a1a',
            cursor: 'pointer',
            '&:active': {
                backgroundColor: '#f5f5f5',
            },
        }),
        placeholder: (base) => ({
            ...base,
            color: '#999',
        }),
    };

    const external = externalStyles as AnyStyles | undefined;

    const mergedStyles: AnyStyles = { ...internalStyles };

    if (external) {
        Object.keys(external).forEach((key) => {
            const int = internalStyles[key];
            const ext = external[key];
            mergedStyles[key] = int
                ? (base, state) => ({ ...int(base, state), ...ext(base, state) })
                : ext;
        });
    }

    return (
        <ReactSelect
            className={`${selectStyles.select} ${error ? selectStyles.error : ''} ${className || ''}`}
            classNamePrefix="custom-select"
            {...rest}
            styles={mergedStyles as SelectProps['styles']}
        />
    );
};

export default Select;