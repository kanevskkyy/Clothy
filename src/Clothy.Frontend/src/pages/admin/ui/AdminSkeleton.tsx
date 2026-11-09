import styles from "./AdminSkeleton.module.css";

interface SkeletonProps {
    width?: number | string;
    height?: number | string;
    radius?: number;
}

export const AdminSkeleton = ({width = "100%", height = 14, radius}: SkeletonProps) => (
    <span className={styles.block} style={{width, height, borderRadius: radius}} aria-hidden="true"/>
);

interface AdminRowsSkeletonProps {
    rows?: number;
    columns?: number;
    label?: string;
}

/** Placeholder rows for a table or list while data loads. */
export const AdminRowsSkeleton = ({rows = 6, columns = 4, label = "Loading..."}: AdminRowsSkeletonProps) => (
    <div className={styles.rows} role="status" aria-live="polite" aria-label={label}>
        {Array.from({length: rows}, (_, r) => (
            <div key={r} className={styles.row} style={{gridTemplateColumns: `2fr repeat(${columns - 1}, 1fr)`}}>
                {Array.from({length: columns}, (_, c) => (
                    <AdminSkeleton key={c} width={c === 0 ? "70%" : "55%"}/>
                ))}
            </div>
        ))}
    </div>
);

interface AdminTilesSkeletonProps {
    count?: number;
    minWidth?: number;
    height?: number;
    label?: string;
}

/** Placeholder tiles for card grids (dashboard, products). */
export const AdminTilesSkeleton = ({count = 6, minWidth = 240, height = 120, label = "Loading..."}: AdminTilesSkeletonProps) => (
    <div
        className={styles.grid}
        style={{gridTemplateColumns: `repeat(auto-fill, minmax(min(${minWidth}px, 100%), 1fr))`}}
        role="status"
        aria-live="polite"
        aria-label={label}
    >
        {Array.from({length: count}, (_, i) => (
            <AdminSkeleton key={i} height={height} radius={10}/>
        ))}
    </div>
);
