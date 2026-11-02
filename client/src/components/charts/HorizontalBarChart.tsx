import { useState } from "react";
import ChartTooltip from "./ChartTooltip";
import { CHART_INK } from "./chartTheme";
import { useElementWidth } from "./StackedColumnChart";

export type BarDatum = {
  key: string;
  label: string;         // left label (e.g. test ID)
  sublabel?: string;     // muted second line (e.g. test name)
  value: number;
  tooltip: string;       // detail shown on hover/focus
};

type Props = {
  data: BarDatum[];
  color: string;
  valueLabel: string;    // e.g. "failures"
  ariaLabel: string;
};

const ROW = 40;
const BAR = 14;          // well under the 24px cap
const LABEL_WIDTH = 190;
const VALUE_SPACE = 36;  // room for the value at the bar tip
const RADIUS = 4;

// Rounded data-end on the right, square at the baseline
const roundedEnd = (x: number, y: number, w: number, h: number) => {
  const r = Math.min(RADIUS, w, h / 2);
  return `M${x},${y} H${x + w - r} Q${x + w},${y} ${x + w},${y + r} V${y + h - r} Q${x + w},${y + h} ${x + w - r},${y + h} H${x} Z`;
};

// Single series: no legend box - the card title names it
export default function HorizontalBarChart({ data, color, valueLabel, ariaLabel }: Props) {
  const [containerRef, width] = useElementWidth<HTMLDivElement>();
  const [active, setActive] = useState<number | null>(null);

  const max = Math.max(...data.map(d => d.value), 1);
  const plotWidth = Math.max(0, width - LABEL_WIDTH - VALUE_SPACE);
  const height = data.length * ROW;

  return (
    <div ref={containerRef} style={{ position: "relative", width: "100%" }}>
      {width > 0 && (
        <svg width={width} height={height} role="img" aria-label={ariaLabel} style={{ display: "block" }}>
          {/* Baseline */}
          <line
            x1={LABEL_WIDTH} x2={LABEL_WIDTH} y1={0} y2={height}
            stroke={CHART_INK.baseline} strokeWidth={1} shapeRendering="crispEdges"
          />

          {data.map((d, i) => {
            const rowY = i * ROW;
            const barY = rowY + (ROW - BAR) / 2;
            const w = Math.max(2, (d.value / max) * plotWidth);

            return (
              <g key={d.key}>
                {active === i && <rect x={0} y={rowY} width={width} height={ROW} fill={CHART_INK.hover} />}

                <text x={0} y={rowY + (d.sublabel ? 16 : ROW / 2)} dy="0.32em" fontSize={12} fill={CHART_INK.primary}>
                  {d.label}
                </text>
                {d.sublabel && (
                  <text x={0} y={rowY + 30} dy="0.32em" fontSize={11} fill={CHART_INK.muted}>
                    {d.sublabel.length > 28 ? d.sublabel.slice(0, 27) + "…" : d.sublabel}
                  </text>
                )}

                <path d={roundedEnd(LABEL_WIDTH, barY, w, BAR)} fill={color} />

                {/* Value at the tip, in text ink - never the series colour */}
                <text
                  x={LABEL_WIDTH + w + 6} y={rowY + ROW / 2} dy="0.32em"
                  fontSize={12} fill={CHART_INK.secondary} style={{ fontVariantNumeric: "tabular-nums" }}
                >
                  {d.value}
                </text>

                <rect
                  x={0} y={rowY} width={width} height={ROW} fill="transparent" tabIndex={0}
                  aria-label={`${d.label}: ${d.tooltip}`}
                  onPointerEnter={() => setActive(i)}
                  onPointerLeave={() => setActive(null)}
                  onFocus={() => setActive(i)}
                  onBlur={() => setActive(null)}
                  style={{ outline: "none" }}
                />
              </g>
            );
          })}
        </svg>
      )}

      {active !== null && data[active] && (
        <ChartTooltip
          title={data[active].sublabel ? `${data[active].label} · ${data[active].sublabel}` : data[active].label}
          x={LABEL_WIDTH + Math.max(2, (data[active].value / max) * plotWidth) + VALUE_SPACE / 2}
          y={active * ROW + ROW / 2}
          alignRight={LABEL_WIDTH + (data[active].value / max) * plotWidth > width * 0.6}
          rows={[{ label: `${valueLabel} — ${data[active].tooltip}`, value: data[active].value, color }]}
        />
      )}
    </div>
  );
}
