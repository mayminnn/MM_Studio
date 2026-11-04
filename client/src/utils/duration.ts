// Formats a test's run time, e.g. 850 -> "0.9s", 75000 -> "1m 15s"
// Results recorded before durations were captured have 0 or no value
export const formatDuration = (ms?: number) => {
  if (!ms) return "-";

  const totalSeconds = ms / 1000;
  if (totalSeconds < 60) return `${totalSeconds.toFixed(1)}s`;

  const minutes = Math.floor(totalSeconds / 60);
  const seconds = Math.round(totalSeconds % 60);
  return `${minutes}m ${seconds}s`;
};
