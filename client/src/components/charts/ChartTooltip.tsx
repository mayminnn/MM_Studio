import { CHART_INK } from "./chartTheme";

export type TooltipRow = {
  label: string;
  value: string | number;
  color: string;
};

type Props = {
  title: string;
  rows: TooltipRow[];
  x: number;
  y: number;
  // Flip to the left of the pointer when near the right edge
  alignRight?: boolean;
};

// Value leads, label follows; series keyed with a short line, not a box
export default function ChartTooltip({ title, rows, x, y, alignRight }: Props) {
  return (
    <div
      role="tooltip"
      style={{
        position: "absolute",
        left: x,
        top: y,
        transform: alignRight ? "translate(calc(-100% - 12px), -50%)" : "translate(12px, -50%)",
        pointerEvents: "none",
        background: CHART_INK.surface,
        border: "1px solid rgba(11, 11, 11, 0.10)",
        borderRadius: 6,
        boxShadow: "0 4px 12px rgba(0, 0, 0, 0.08)",
        padding: "8px 10px",
        fontSize: 12,
        whiteSpace: "nowrap",
        zIndex: 10,
      }}
    >
      <div style={{ color: CHART_INK.secondary, marginBottom: 4 }}>{title}</div>
      {rows.map(row => (
        <div key={row.label} style={{ display: "flex", alignItems: "center", gap: 6, lineHeight: "18px" }}>
          <span style={{ width: 10, height: 2, borderRadius: 1, background: row.color }} />
          <b style={{ color: CHART_INK.primary, fontVariantNumeric: "tabular-nums" }}>{row.value}</b>
          <span style={{ color: CHART_INK.secondary }}>{row.label}</span>
        </div>
      ))}
    </div>
  );
}
