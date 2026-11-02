import { useEffect, useRef, useState } from "react";
import ChartTooltip from "./ChartTooltip";
import { CHART_INK } from "./chartTheme";

export type ColumnSeries = {
  key: string;
  label: string;
  color: string;
};

export type ColumnDatum = {
  label: string;           // full label, used in the tooltip
  shortLabel: string;      // axis label
  values: Record<string, number>;
};

type Props = {
  data: ColumnDatum[];
  series: readonly ColumnSeries[];   // bottom-to-top stacking order
  height?: number;
  ariaLabel: string;
};

const MARGIN = { top: 12, right: 8, bottom: 26, left: 36 };
const MAX_BAR = 24;     // bars never fill the slot
const GAP = 2;          // surface gap between stacked segments
const RADIUS = 4;       // rounded data-end, square at the baseline

// Clean axis ticks: 0, step, 2*step... covering max
const niceTicks = (max: number, count = 4) => {
  if (max <= 0) return [0, 1];
  const raw = max / count;
  const magnitude = Math.pow(10, Math.floor(Math.log10(raw)));
  const step = [1, 2, 5, 10].map(m => m * magnitude).find(s => s >= raw) ?? raw;
  const ticks: number[] = [];
  for (let t = 0; t <= max + step * 0.001; t += step) ticks.push(Math.round(t));
  if (ticks[ticks.length - 1] < max) ticks.push(ticks[ticks.length - 1] + step);
  return ticks;
};

// Top segment: rounded top corners only
const roundedTop = (x: number, y: number, w: number, h: number) => {
  const r = Math.min(RADIUS, w / 2, h);
  return `M${x},${y + h} V${y + r} Q${x},${y} ${x + r},${y} H${x + w - r} Q${x + w},${y} ${x + w},${y + r} V${y + h} Z`;
};

export function useElementWidth<T extends HTMLElement>() {
  const ref = useRef<T>(null);
  const [width, setWidth] = useState(0);

  useEffect(() => {
    if (!ref.current) return;
    const observer = new ResizeObserver(entries => setWidth(entries[0].contentRect.width));
    observer.observe(ref.current);
    return () => observer.disconnect();
  }, []);

  return [ref, width] as const;
}

export default function StackedColumnChart({ data, series, height = 240, ariaLabel }: Props) {
  const [containerRef, width] = useElementWidth<HTMLDivElement>();
  const [active, setActive] = useState<number | null>(null);

  const innerWidth = Math.max(0, width - MARGIN.left - MARGIN.right);
  const innerHeight = height - MARGIN.top - MARGIN.bottom;

  const totals = data.map(d => series.reduce((sum, s) => sum + (d.values[s.key] ?? 0), 0));
  const ticks = niceTicks(Math.max(...totals, 0));
  const yMax = ticks[ticks.length - 1];
  const y = (v: number) => MARGIN.top + innerHeight - (v / yMax) * innerHeight;

  const band = data.length ? innerWidth / data.length : 0;
  const barWidth = Math.max(2, Math.min(MAX_BAR, band * 0.6));

  // Thin out x labels so they never collide (~48px per label)
  const labelEvery = Math.max(1, Math.ceil(data.length / Math.max(1, Math.floor(innerWidth / 48))));

  const activeDatum = active !== null ? data[active] : null;
  const activeCenter = active !== null ? MARGIN.left + band * active + band / 2 : 0;

  return (
    <div ref={containerRef} style={{ position: "relative", width: "100%" }}>
      {width > 0 && (
        <svg width={width} height={height} role="img" aria-label={ariaLabel} style={{ display: "block" }}>
          {/* Grid + y ticks */}
          {ticks.map(t => (
            <g key={t}>
              <line
                x1={MARGIN.left} x2={width - MARGIN.right} y1={y(t)} y2={y(t)}
                stroke={t === 0 ? CHART_INK.baseline : CHART_INK.grid} strokeWidth={1}
                shapeRendering="crispEdges"
              />
              <text
                x={MARGIN.left - 8} y={y(t)} dy="0.32em" textAnchor="end"
                fontSize={11} fill={CHART_INK.muted} style={{ fontVariantNumeric: "tabular-nums" }}
              >
                {t.toLocaleString()}
              </text>
            </g>
          ))}

          {data.map((d, i) => {
            const bandX = MARGIN.left + band * i;
            const x = bandX + (band - barWidth) / 2;

            // Visible (non-zero) segments, bottom to top
            let base = 0;
            const segments = series
              .map(s => ({ s, value: d.values[s.key] ?? 0 }))
              .filter(seg => seg.value > 0)
              .map(seg => {
                const seg0 = base;
                base += seg.value;
                return { ...seg, from: seg0, to: base };
              });

            return (
              <g key={d.label}>
                {/* Hover lift: the whole band responds */}
                {active === i && (
                  <rect x={bandX} y={MARGIN.top} width={band} height={innerHeight} fill={CHART_INK.hover} />
                )}

                {segments.map((seg, k) => {
                  const top = y(seg.to);
                  // Gap sits on top of each segment below the topmost one
                  const isTop = k === segments.length - 1;
                  const h = Math.max(0, y(seg.from) - top - (k > 0 ? GAP : 0));
                  return isTop
                    ? <path key={seg.s.key} d={roundedTop(x, top, barWidth, h)} fill={seg.s.color} />
                    : <rect key={seg.s.key} x={x} y={top} width={barWidth} height={h} fill={seg.s.color} />;
                })}

                {i % labelEvery === 0 && (
                  <text
                    x={bandX + band / 2} y={height - 8} textAnchor="middle"
                    fontSize={11} fill={CHART_INK.muted}
                  >
                    {d.shortLabel}
                  </text>
                )}

                {/* Hit target: the full band, bigger than the mark; keyboard reachable */}
                <rect
                  x={bandX} y={MARGIN.top} width={band} height={innerHeight}
                  fill="transparent" tabIndex={0}
                  aria-label={`${d.label}: ${series.map(s => `${d.values[s.key] ?? 0} ${s.label}`).join(", ")}`}
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

      {activeDatum && (
        <ChartTooltip
          title={activeDatum.label}
          x={activeCenter}
          y={MARGIN.top + innerHeight / 2}
          alignRight={activeCenter > width * 0.65}
          rows={[...series].reverse().map(s => ({
            label: s.label,
            value: activeDatum.values[s.key] ?? 0,
            color: s.color,
          }))}
        />
      )}
    </div>
  );
}
