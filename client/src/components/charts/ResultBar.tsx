import { Tooltip } from "antd";
import { RESULT_SERIES } from "./chartTheme";

type Props = {
  values: { passed: number; failed: number; notRun: number };
  width?: number | string;
  height?: number;
};

// Compact part-to-whole bar (passed / not run / failed) with 2px surface gaps between segments
export default function ResultBar({ values, width = "100%", height = 8 }: Props) {
  const total = values.passed + values.failed + values.notRun;
  const segments = RESULT_SERIES.filter(s => values[s.key] > 0);

  const summary = RESULT_SERIES.map(s => `${values[s.key]} ${s.label.toLowerCase()}`).join(" · ");

  return (
    <Tooltip title={summary}>
      <div
        role="img"
        aria-label={summary}
        style={{ display: "flex", gap: 2, width, height, borderRadius: 4, overflow: "hidden", background: total ? undefined : "#e1e0d9" }}
      >
        {segments.map(s => (
          <div key={s.key} style={{ flexGrow: values[s.key], flexBasis: 0, background: s.color }} />
        ))}
      </div>
    </Tooltip>
  );
}
