// Chart colours. Validated together (dataviz validate_palette.js, light surface):
// passed/failed clear colour-blind separation (CVD ΔE 23.8), unlike green/red (ΔE 4.1).
// "Not run" is a neutral grey on purpose - it's the absence of a verdict.
// Charts always pair these with a legend, tooltip and table view, so colour is never the only cue.
export const RESULT_COLORS = {
  passed: "#2a78d6",
  failed: "#d03b3b",
  notRun: "#898781",
} as const;

export const RESULT_SERIES = [
  { key: "passed", label: "Passed", color: RESULT_COLORS.passed },
  { key: "notRun", label: "Not run", color: RESULT_COLORS.notRun },
  { key: "failed", label: "Failed", color: RESULT_COLORS.failed },
] as const;

export const CHART_INK = {
  surface: "#ffffff",
  primary: "#0b0b0b",
  secondary: "#52514e",
  muted: "#898781",
  grid: "#e1e0d9",
  baseline: "#c3c2b7",
  hover: "rgba(11, 11, 11, 0.04)",
  successText: "#006300",
  critical: "#d03b3b",
} as const;
